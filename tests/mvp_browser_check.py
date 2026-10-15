"""Exercise real sign-in, lesson CRUD, journal save and history in Edge."""
import uuid
from pathlib import Path

from playwright.sync_api import expect, sync_playwright
from browser_support import BASE, browser_login
from lms_test_support import send, close_sessions

OUTPUT = Path(__file__).resolve().parents[1] / "document_work/browser_profiles/mvp-checks"
OUTPUT.mkdir(parents=True, exist_ok=True)
suffix = uuid.uuid4().hex[:8]
code = "UI-" + suffix
response = send("POST", "/api/catalog/commands", json={"kind": "add-course", "code": code, "name": "Проверка интерфейса", "groupName": "Тестовая группа", "defaultRoom": "Т-101"})
response.raise_for_status()
state = response.json()["snapshot"]
course = next(row for row in state["courses"] if row["code"] == code)
try:
    for username, role in (("teacher", 2), ("student", 1)):
        user = next(row for row in state["users"] if row["username"] == username)
        send("POST", "/api/catalog/commands", json={"kind": "add-enrollment", "courseId": course["id"], "userId": user["id"], "role": role}).raise_for_status()
    with sync_playwright() as pw:
        browser = pw.chromium.launch(channel="msedge", headless=True)
        context = browser.new_context(viewport={"width": 1440, "height": 1000})
        page = context.new_page()
        failures = []
        page.on("pageerror", lambda error: failures.append(str(error)))
        browser_login(page, "teacher", "/attendance")
        page.get_by_role("button", name="Создать занятие").click()
        form = page.locator(".lesson-manager-form")
        expect(form).to_be_visible(timeout=10000)
        form.get_by_label("Курс").select_option(course["id"])
        title = "Занятие " + suffix
        form.get_by_label("Тема", exact=True).fill(title)
        form.get_by_label("Аудитория", exact=True).fill("Т-202")
        form.get_by_label("Дата и время", exact=True).fill("2026-10-15T15:00")
        with page.expect_response(lambda response: response.url.endswith("/api/attendance/lessons") and response.request.method == "POST") as creation:
            form.get_by_role("button", name="Сохранить занятие").click()
        assert creation.value.json()["startsAt"].startswith("2026-10-15T15:00"), creation.value.json()["startsAt"]
        expect(page.get_by_role("heading", name=title, exact=True)).to_be_visible(timeout=30000)
        row = page.locator(".attendance-table tbody tr").filter(has_text="Иван Иванов")
        row.locator("select").select_option("Late")
        row.locator('input[type="number"]').fill("7")
        row.locator("input.attendance-comment").fill("Задержка транспорта")
        page.locator(".attendance-actions-row button.statement-primary-button").click()
        expect(page.get_by_text("Журнал сохранён в базе данных.", exact=True)).to_be_visible()
        page.get_by_role("button", name="История изменений", exact=True).click()
        expect(page.locator(".lesson-audit-table tbody tr").first).to_contain_text("Иван Иванов")
        page.evaluate("window.scrollTo(0, 0)")
        page.screenshot(path=str(OUTPUT / "teacher-history-desktop.png"), full_page=True)
        page.get_by_role("button", name="Изменить занятие").click()
        form.get_by_label("Аудитория", exact=True).fill("Т-303")
        form.get_by_role("button", name="Сохранить занятие").click()
        expect(page.get_by_text("Занятие изменено.", exact=True)).to_be_visible()
        page.set_viewport_size({"width": 390, "height": 844})
        page.evaluate("window.scrollTo(0, 0)")
        page.screenshot(path=str(OUTPUT / "teacher-mobile.png"), full_page=True)
        assert page.evaluate("document.documentElement.scrollWidth <= innerWidth + 2")
        page.on("dialog", lambda dialog: dialog.accept())
        page.get_by_role("button", name="Отменить занятие", exact=True).click()
        expect(page.get_by_text("Занятие отменено", exact=True)).to_be_visible()
        page.get_by_role("button", name="Выйти", exact=True).click()
        expect(page.get_by_label("Логин", exact=True)).to_be_visible()
        page.goto(BASE + "/attendance")
        expect(page.get_by_label("Логин", exact=True)).to_be_visible()
        print("PASS: password sign-in, create/edit/cancel, save, audit, mobile and logout")
        assert not failures, failures
        browser.close()
finally:
    send("POST", "/api/catalog/commands", json={"kind": "remove-course", "id": course["id"]}).raise_for_status()
    close_sessions()
