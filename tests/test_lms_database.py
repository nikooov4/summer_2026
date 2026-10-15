"""Integration checks against the running local PostgreSQL database bridge."""
import copy
import json
import os
import uuid

import pytest
from lms_test_support import BASE, ANONYMOUS, send, close_sessions



@pytest.fixture(scope="module", autouse=True)
def close_http_client():
    yield
    close_sessions()


def get(actor="admin"):
    response = send("GET", "/api/bootstrap", actor)
    assert response.status_code == 200, response.text
    return response.json()


def command(payload, actor="admin", expected=200):
    response = send("POST", "/api/catalog/commands", actor, json=payload)
    assert response.status_code == expected, response.text
    return response.json()


@pytest.fixture
def course():
    suffix = uuid.uuid4().hex[:10]
    state = command({"kind": "add-course", "code": "DB-" + suffix, "name": "Тест базы данных", "groupName": "Тестовая группа", "defaultRoom": "Т-101"})["snapshot"]
    selected = next(row for row in state["courses"] if row["code"] == "DB-" + suffix)
    teacher = next(row for row in state["users"] if row["username"] == "teacher")
    command({"kind": "add-enrollment", "userId": teacher["id"], "courseId": selected["id"], "role": 2})
    login = "db_test_" + suffix
    state = command({"kind": "add-user", "username": login, "fullName": "Иван Тестов", "recordBookNumber": "Т-01"})["snapshot"]
    student = next(row for row in state["users"] if row["username"] == login)
    command({"kind": "add-enrollment", "userId": student["id"], "courseId": selected["id"], "role": 1})
    response = send("POST", "/api/attendance/lessons", "teacher", json={"courseId": selected["id"], "topic": "Занятие для проверки", "room": "Т-101", "startsAt": "2026-10-15T10:00:00", "revision": 0})
    assert response.status_code == 200, response.text
    yield selected, student
    command({"kind": "remove-course", "id": selected["id"]})
    if any(row["id"] == student["id"] for row in get()["users"]):
        command({"kind": "remove-user", "id": student["id"]})


def lesson_for(course_id, actor="teacher"):
    return next(row for row in get(actor)["attendance"]["lessons"] if row["courseId"] == course_id)


def save(lesson, actor="teacher", expected=200):
    response = send("PUT", f"/api/attendance/lessons/{lesson['id']}/entries", actor, json={"revision": lesson["revision"], "entries": lesson["entries"]})
    assert response.status_code == expected, response.text
    return response.json()


def test_schema_seed_is_persistent_and_has_guid_keys():
    state = get()
    assert len(state["users"]) >= 9 and len(state["courses"]) >= 3
    for row in state["users"] + state["courses"] + state["enrollments"] + state["attendance"]["lessons"]:
        uuid.UUID(row["id"])
    again = get()
    assert [row["id"] for row in state["courses"]] == [row["id"] for row in again["courses"]]


def test_authenticated_access_is_filtered_and_read_only_for_student():
    state = get("student")
    assert len(state["users"]) == 1 and state["users"][0]["username"] == "student"
    student_id = state["users"][0]["id"]
    assert all(entry["studentId"] == student_id for lesson in state["attendance"]["lessons"] for entry in lesson["entries"])
    command({"kind": "add-user", "username": "forbidden", "fullName": "Чужой Пользователь"}, actor="student", expected=403)
    save(state["attendance"]["lessons"][0], actor="student", expected=403)
    assert ANONYMOUS.get(BASE + "/api/bootstrap").status_code == 401


def test_journal_save_validation_and_optimistic_lock(course):
    selected, student = course
    lesson = lesson_for(selected["id"])
    assert lesson["entries"][0]["studentId"] == student["id"]
    invalid = copy.deepcopy(lesson)
    invalid["entries"][0].update(status=2, lateMinutes=0)
    save(invalid, expected=400)
    invalid["entries"][0].update(status=3, lateMinutes=None, comment="")
    save(invalid, expected=400)
    invalid["entries"][0].update(status=99)
    save(invalid, expected=400)
    lesson["entries"][0].update(status=2, lateMinutes=9, comment="Задержка транспорта")
    saved = save(lesson)
    assert saved["revision"] == lesson["revision"] + 1
    loaded = lesson_for(selected["id"])
    assert loaded["entries"][0]["lateMinutes"] == 9
    save(lesson, expected=409)
    loaded["entries"][0].update(studentId=get()["users"][0]["id"])
    save(loaded, expected=409)


def test_journal_completion_and_reopening_persist(course):
    selected, _ = course
    lesson = lesson_for(selected["id"])
    path = BASE + f"/api/attendance/lessons/{lesson['id']}"
    response = send("POST", path + "/complete", "teacher", json={"revision": lesson["revision"]})
    assert response.status_code == 400
    lesson["entries"][0].update(status=1)
    saved = save(lesson)
    closed = send("POST", path + "/complete", "teacher", json={"revision": saved["revision"]}).json()
    assert closed["completedAt"] is not None
    save(closed, expected=409)
    reopened = send("POST", path + "/reopen", "teacher", json={"revision": closed["revision"]}).json()
    assert reopened["completedAt"] is None


def test_ocr_import_atomicity_and_idempotent_enrollment(course):
    selected, student = course
    suffix = uuid.uuid4().hex[:10]
    rows = [{"include": True, "studentId": "__new", "fullName": "Анна Тестова", "username": "ocr_atomic_" + suffix},
            {"include": True, "studentId": "__new", "fullName": "Мария Тестова", "username": "admin"}]
    command({"kind": "ocr-import", "courseId": selected["id"], "rows": rows}, expected=400)
    assert not any(user["username"] == "ocr_atomic_" + suffix for user in get()["users"])
    result = command({"kind": "ocr-import", "courseId": selected["id"], "rows": [{"include": True, "studentId": student["id"]}]})
    assert result["created"] == 0 and result["enrolled"] == 0


def test_roster_is_frozen_and_new_enrollments_only_affect_new_lessons(course):
    selected, student = course
    lesson = lesson_for(selected["id"])
    lesson["entries"][0].update(status=1)
    saved = save(lesson)
    response = send("POST", f"/api/attendance/lessons/{lesson['id']}/complete", "teacher", json={"revision": saved["revision"]})
    response.raise_for_status()
    closed = response.json()
    another = next(row for row in get()["users"] if row["username"] == "student")
    state = command({"kind": "add-enrollment", "userId": another["id"], "courseId": selected["id"], "role": 1})["snapshot"]
    changed = lesson_for(selected["id"])
    assert changed["completedAt"] == closed["completedAt"] and changed["revision"] == closed["revision"]
    assert next(row for row in changed["entries"] if row["studentId"] == student["id"])["status"] == 1
    assert all(row["studentId"] != another["id"] for row in changed["entries"])
    future = send("POST", "/api/attendance/lessons", "teacher", json={"courseId": selected["id"], "topic": "Следующее занятие", "room": "Т-101", "startsAt": "2026-10-16T10:00:00", "revision": 0})
    assert future.status_code == 200, future.text
    assert len(future.json()["entries"]) == 2
    save(closed, expected=409)
    enrollment = next(row for row in state["enrollments"] if row["userId"] == another["id"] and row["courseId"] == selected["id"])
    command({"kind": "remove-enrollment", "id": enrollment["id"]})
    changed_again = next(row for row in get("teacher")["attendance"]["lessons"] if row["id"] == lesson["id"])
    assert changed_again["revision"] == changed["revision"]
    assert len(changed_again["entries"]) == 1


def test_last_admin_cannot_be_demoted_or_removed():
    state = get()
    admin = next(row for row in state["users"] if row["username"] == "admin")
    command({"kind": "toggle-admin", "id": admin["id"]}, expected=400)
    command({"kind": "remove-user", "id": admin["id"]}, expected=400)
    command({"kind": "import-users", "format": "json", "payload": '[{"username":"admin","fullName":"Изменённое Имя","isAdmin":false}]'}, expected=400)
    assert next(row for row in get()["users"] if row["id"] == admin["id"])["fullName"] == admin["fullName"]


def test_statement_draft_is_saved_per_author_and_recalculated():
    before = get("teacher")
    statement = {"name": "Тестовая ведомость БД", "subjects": ["Раздел 1", "Раздел 2"], "students": [{"fullName": "Иван Иванов", "sections": [30, 20], "exam": 40, "total": -100}]}
    response = send("PUT", "/api/statements/current", "teacher", json={"statement": statement, "revision": before["statementRevision"]})
    assert response.status_code == 200, response.text
    saved = response.json()
    assert saved["statement"]["students"][0]["total"] == 90
    assert get("teacher")["statement"]["name"] == statement["name"]
    assert get("student")["statement"] is None
    assert send("PUT", "/api/statements/current", "student", json={"statement": statement, "revision": 0}).status_code == 403
    assert send("PUT", "/api/statements/current", "teacher", json={"statement": statement, "revision": before["statementRevision"]}).status_code == 409
    if before["statement"] is not None:
        send("PUT", "/api/statements/current", "teacher", json={"statement": before["statement"], "revision": saved["revision"]}).raise_for_status()


def test_json_and_csv_import_updates_same_persistent_user():
    login = "db_import_" + uuid.uuid4().hex[:10]
    payload = json.dumps([{"username": login, "fullName": "Анна Иванова", "recordBookNumber": "Т-01", "isAdmin": False}])
    result = command({"kind": "import-users", "format": "json", "payload": payload})
    assert result["count"] == 1
    user = next(row for row in get()["users"] if row["username"] == login)
    try:
        csv = f'username,fullName,recordBookNumber,isAdmin\n{login},Анна Петрова,"Т,02",false\n'
        result = command({"kind": "import-users", "format": "csv", "payload": csv})
        assert result["count"] == 1
        updated = next(row for row in get()["users"] if row["username"] == login)
        assert updated["id"] == user["id"] and updated["fullName"] == "Анна Петрова"
        assert updated["recordBookNumber"] == "Т,02" and not updated["isAdmin"]
    finally:
        command({"kind": "remove-user", "id": user["id"]})
