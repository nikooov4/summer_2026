"""Real cookie login, permissions and attendance lifecycle against local PostgreSQL."""
import copy
import uuid

import httpx
import pytest

from lms_test_support import BASE, ANONYMOUS, csrf, login, send, session, close_sessions
from test_lms_database import course, get, command, lesson_for, save


@pytest.fixture(scope="module", autouse=True)
def cleanup_sessions():
    yield
    close_sessions()


@pytest.fixture
def account():
    name = "auth_test_" + uuid.uuid4().hex[:10]
    state = command({"kind": "add-user", "username": name, "fullName": "Анна Иванова"})["snapshot"]
    user = next(row for row in state["users"] if row["username"] == name)
    password = "Test-" + uuid.uuid4().hex
    response = send("POST", f"/api/users/{user['id']}/password", json={"password": password, "mustChangePassword": False})
    assert response.status_code == 204, response.text
    yield user, password
    command({"kind": "remove-user", "id": user["id"]})


def test_demo_header_and_unsigned_bearer_cannot_authenticate():
    response = ANONYMOUS.get(BASE + "/api/bootstrap", headers={"X-Lms-Demo-User": "admin", "Authorization": "Bearer eyJhbGciOiJub25lIn0.eyJ1c2VybmFtZSI6ImFkbWluIn0.demo"})
    assert response.status_code == 401
    assert "location" not in response.headers


def test_local_demo_buttons_issue_real_sessions_and_preserve_roles():
    assert ANONYMOUS.get(BASE + "/api/auth/options").json()["demoLoginEnabled"]
    for username in ("admin", "teacher", "student"):
        with httpx.Client(trust_env=False, timeout=30) as client:
            response = client.post(BASE + "/api/auth/demo/" + username, headers=csrf(client))
            assert response.status_code == 200, response.text
            assert response.json()["username"] == username
            assert response.json()["isAdmin"] == (username == "admin")
            snapshot = client.get(BASE + "/api/bootstrap")
            assert snapshot.status_code == 200
            if username == "student":
                assert len(snapshot.json()["users"]) == 1
                assert client.post(BASE + "/api/catalog/commands", headers=csrf(client), json={"kind": "add-user", "username": "denied", "fullName": "Иван Иванов"}).status_code == 403
            assert client.post(BASE + "/api/auth/logout", headers=csrf(client)).status_code == 204
            assert client.get(BASE + "/api/bootstrap").status_code == 401


def test_local_demo_login_requires_csrf_and_rejects_other_accounts():
    with httpx.Client(trust_env=False, timeout=30) as client:
        assert client.post(BASE + "/api/auth/demo/admin").status_code == 400
        assert client.post(BASE + "/api/auth/demo/assistant", headers=csrf(client)).status_code == 404
        assert client.get(BASE + "/api/bootstrap").status_code == 401


def test_cookie_is_http_only_and_mutations_require_csrf():
    teacher = session("teacher")
    assert any(cookie.name == "Lms.Attendance.Session" and "httponly" in {key.lower(): value for key, value in cookie._rest.items()} and {key.lower(): value for key, value in cookie._rest.items()}.get("samesite") == "strict" for cookie in teacher.cookies.jar)
    assert teacher.post(BASE + "/api/auth/logout").status_code == 400
    assert teacher.get(BASE + "/api/auth/me").status_code == 200
    other = httpx.Client(trust_env=False)
    try:
        wrong_token = csrf(other)
        assert teacher.post(BASE + "/api/auth/logout", headers=wrong_token).status_code == 400
    finally:
        other.close()
    preflight = ANONYMOUS.options(BASE + "/api/catalog/commands", headers={"Origin": "https://untrusted.invalid", "Access-Control-Request-Method": "POST", "Access-Control-Request-Headers": "X-CSRF-TOKEN"})
    assert "access-control-allow-origin" not in preflight.headers


def test_password_reset_logout_disable_and_forced_change_revoke_sessions(account):
    user, password = account
    first, second = login(user["username"], password), login(user["username"], password)
    try:
        cookie = first.cookies.get("Lms.Attendance.Session")
        response = first.post(BASE + "/api/auth/logout", headers=csrf(first))
        assert response.status_code == 204
        replay = httpx.Client(trust_env=False)
        replay.cookies.set("Lms.Attendance.Session", cookie)
        try:
            assert replay.get(BASE + "/api/bootstrap").status_code == 401
        finally:
            replay.close()
        assert second.get(BASE + "/api/bootstrap").status_code == 200
        temporary = "Temporary-" + uuid.uuid4().hex
        assert send("POST", f"/api/users/{user['id']}/password", json={"password": temporary, "mustChangePassword": True}).status_code == 204
        assert second.get(BASE + "/api/bootstrap").status_code == 401
        changed = login(user["username"], temporary)
        try:
            assert changed.get(BASE + "/api/bootstrap").status_code == 403
            new_password = "Updated-" + uuid.uuid4().hex
            response = changed.post(BASE + "/api/auth/change-password", headers=csrf(changed), json={"currentPassword": temporary, "newPassword": new_password})
            assert response.status_code == 204, response.text
            assert changed.get(BASE + "/api/bootstrap").status_code == 401
            active = login(user["username"], new_password)
            try:
                assert active.get(BASE + "/api/bootstrap").status_code == 200
                assert send("POST", f"/api/users/{user['id']}/access", json={"enabled": False}).status_code == 204
                assert active.get(BASE + "/api/bootstrap").status_code == 401
                failed = active.post(BASE + "/api/auth/login", headers=csrf(active), json={"username": user["username"], "password": new_password})
                assert failed.status_code == 401
            finally:
                active.close()
        finally:
            changed.close()
    finally:
        first.close()
        second.close()


def test_failed_passwords_lock_only_the_target_account(account):
    user, password = account
    with httpx.Client(trust_env=False, timeout=30) as client:
        for _ in range(5):
            assert client.post(BASE + "/api/auth/login", headers=csrf(client), json={"username": user["username"], "password": "incorrect-password"}).status_code == 401
        assert client.post(BASE + "/api/auth/login", headers=csrf(client), json={"username": user["username"], "password": password}).status_code == 401
    assert send("GET", "/api/bootstrap").status_code == 200


def test_student_cannot_provision_passwords_or_change_access():
    student = get("student")["users"][0]
    assert send("POST", f"/api/users/{student['id']}/password", "student", json={"password": "Another-password-123"}).status_code == 403
    assert send("POST", f"/api/users/{student['id']}/access", "student", json={"enabled": False}).status_code == 403


def test_lesson_edit_cancel_history_and_historical_roster(course):
    selected, student = course
    lesson = lesson_for(selected["id"])
    path = f"/api/attendance/lessons/{lesson['id']}"
    request = {"courseId": selected["id"], "topic": "Обновлённое занятие", "room": "Т-202", "startsAt": "2026-10-15T11:00:00", "revision": lesson["revision"]}
    assert send("PUT", path, "student", json=request).status_code == 403
    edited = send("PUT", path, "teacher", json=request)
    assert edited.status_code == 200, edited.text
    assert edited.json()["room"] == "Т-202" and edited.json()["revision"] == lesson["revision"] + 1
    assert send("PUT", path, "teacher", json=request).status_code == 409
    marked = edited.json()
    marked["entries"][0].update(status=2, lateMinutes=5, comment="Транспорт")
    saved = save(marked)
    history = send("GET", path + "/history", "teacher")
    assert history.status_code == 200, history.text
    assert history.json()[0]["studentId"] == student["id"] and history.json()[0]["newStatus"] == 2
    assert send("GET", path + "/history", "assistant").status_code == 403
    command({"kind": "remove-user", "id": student["id"]})
    snapshot = get("teacher")
    preserved = next(row for row in snapshot["attendance"]["lessons"] if row["id"] == lesson["id"])
    assert preserved["entries"][0]["status"] == 2
    assert preserved["roster"][0]["fullName"] == student["fullName"]
    cancelled = send("POST", path + "/cancel", "teacher", json={"revision": saved["revision"]})
    assert cancelled.status_code == 200 and cancelled.json()["isCancelled"]
    save(cancelled.json(), expected=409)
    assert send("POST", path + "/complete", "teacher", json={"revision": cancelled.json()["revision"]}).status_code == 409


def test_ocr_gateway_rejects_students_and_invalid_files(course):
    selected, _ = course
    lesson = lesson_for(selected["id"])
    path = f"/api/ocr/recognize?courseId={selected['id']}&lessonId={lesson['id']}&mode=demo"
    assert send("POST", path, "student", files={"file": ("image.png", b"not an image", "image/png")}).status_code == 403
    assert send("POST", path, "teacher", files={"file": ("image.png", b"not an image", "image/png")}).status_code == 400
    assert send("POST", "/api/ocr/jobs/" + str(uuid.uuid4()) + "/confirm", "student", json=[]).status_code in (400, 403, 404)


def test_teacher_in_one_course_does_not_gain_peer_access_in_student_course(course):
    selected, _ = course
    actor = get("student")["users"][0]
    command({"kind": "add-enrollment", "userId": actor["id"], "courseId": selected["id"], "role": 2})
    state = get("student")
    own_lessons = [row for row in state["attendance"]["lessons"] if row["courseId"] != selected["id"]]
    assert own_lessons
    for lesson in own_lessons:
        assert all(row["studentId"] == actor["id"] for row in lesson["entries"])
        assert all(row["id"] == actor["id"] for row in lesson["roster"])
    assert all(row["userId"] == actor["id"] for row in state["enrollments"] if row["courseId"] != selected["id"])


def test_last_working_admin_cannot_be_replaced_by_account_without_credentials():
    name = "no_password_" + uuid.uuid4().hex[:8]
    state = command({"kind": "add-user", "username": name, "fullName": "Иван Петров", "isAdmin": True})["snapshot"]
    candidate = next(row for row in state["users"] if row["username"] == name)
    admin = next(row for row in state["users"] if row["username"] == "admin")
    try:
        command({"kind": "toggle-admin", "id": admin["id"]}, expected=400)
        assert next(row for row in get()["users"] if row["id"] == admin["id"])["isAdmin"]
    finally:
        command({"kind": "remove-user", "id": candidate["id"]})


def test_user_and_course_edits_persist_without_rewriting_roster(course):
    selected, student = course
    command({"kind": "update-user", "id": student["id"], "username": student["username"], "fullName": "Иван Петров", "recordBookNumber": "Т-02", "isAdmin": False})
    command({"kind": "update-course", "id": selected["id"], "code": selected["code"], "name": "Обновлённый курс", "groupName": selected["groupName"], "defaultRoom": "Т-303"})
    state = get()
    assert next(row for row in state["users"] if row["id"] == student["id"])["fullName"] == "Иван Петров"
    assert next(row for row in state["courses"] if row["id"] == selected["id"])["defaultRoom"] == "Т-303"
    assert lesson_for(selected["id"])["roster"][0]["fullName"] == student["fullName"]
