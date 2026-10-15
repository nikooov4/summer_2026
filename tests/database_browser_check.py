"""Check the original statement save path and recovery from a database API outage."""
import uuid
from pathlib import Path

from playwright.sync_api import expect, sync_playwright
from browser_support import browser_login
from lms_test_support import send, close_sessions

BASE = "http://localhost:5293"
API = "http://localhost:8085"
OUTPUT = Path(__file__).resolve().parents[1] / "document_work/browser_profiles/database-checks"
OUTPUT.mkdir(parents=True, exist_ok=True)


def login(page):
    browser_login(page, "teacher", "/statements/constructor")
    expect(page.get_by_role("button", name="Создать ведомость")).to_be_visible(timeout=30000)


before_response = send("GET", "/api/bootstrap", "teacher")
before_response.raise_for_status()
before = before_response.json()
try:
    with sync_playwright() as pw:
        browser = pw.chromium.launch(channel="msedge", headless=True)
        context = browser.new_context(viewport={"width": 1440, "height": 1000})
        page = context.new_page()
        errors = []
        page.on("pageerror", lambda error: errors.append(str(error)))
        login(page)
        title = "Проверка сохранения " + uuid.uuid4().hex[:8]
        page.get_by_label("Количество студентов").fill("1")
        page.get_by_label("Количество разделов").fill("2")
        page.get_by_label("Название экзамена").fill(title)
        page.get_by_role("button", name="Создать ведомость").click()
        row = page.locator(".statement-table tbody tr").first
        row.locator('input[type="number"]').nth(0).fill("30")
        row.locator('input[type="number"]').nth(1).fill("20")
        row.locator('input[type="number"]').nth(2).fill("40")
        row.locator("input.wide").fill("Иван Петров")
        page.get_by_role("button", name="Сохранить ведомость", exact=True).click()
        expect(page.get_by_text("Ведомость сохранена в базе данных.", exact=True)).to_be_visible()
        page.goto(BASE + "/statements/view")
        page.reload()
        expect(page.get_by_role("heading", name="Ведомость: " + title, exact=True)).to_be_visible()
        expect(page.locator(".statement-table tbody tr").filter(has_text="Иван Петров")).to_contain_text("90")
        page.screenshot(path=str(OUTPUT / "statement-desktop.png"), full_page=True)
        page.set_viewport_size({"width": 390, "height": 844})
        page.screenshot(path=str(OUTPUT / "statement-mobile.png"), full_page=True)
        assert page.evaluate("document.documentElement.scrollWidth <= innerWidth + 2")
        print("PASS: original statement saves and restores calculated scores after reload")

        page.route(API + "/**", lambda route: route.abort())
        page.reload()
        expect(page.get_by_role("heading", name="Данные LMS недоступны", exact=True)).to_be_visible(timeout=30000)
        expect(page.get_by_role("alert")).to_contain_text("Нет связи с БД")
        page.unroute(API + "/**")
        page.get_by_role("button", name="Повторить").click()
        expect(page.get_by_role("heading", name="Ведомость: " + title, exact=True)).to_be_visible(timeout=30000)
        print("PASS: API outage is visible; retry restores the database view")
        assert not errors, errors
        browser.close()
finally:
    if before["statement"] is not None:
        response = send("GET", "/api/bootstrap", "teacher")
        response.raise_for_status()
        send("PUT", "/api/statements/current", "teacher",
                  json={"statement": before["statement"], "revision": response.json()["statementRevision"]}).raise_for_status()
    close_sessions()
