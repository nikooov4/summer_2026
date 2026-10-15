"""Check the local quick-login buttons without exposing account passwords."""
from pathlib import Path

from playwright.sync_api import expect, sync_playwright

BASE = "http://localhost:5293"
OUTPUT = Path(__file__).resolve().parents[1] / "document_work/browser_profiles/demo-login"
OUTPUT.mkdir(parents=True, exist_ok=True)

with sync_playwright() as pw:
    browser = pw.chromium.launch(channel="msedge", headless=True)
    for username, label in (("admin", "Админ"), ("teacher", "Преподаватель"), ("student", "Студент")):
        context = browser.new_context(viewport={"width": 390, "height": 844} if username == "student" else {"width": 1280, "height": 900})
        page = context.new_page()
        failures = []
        page.on("pageerror", lambda error: failures.append(str(error)))
        page.goto(BASE + "/login")
        expect(page.get_by_role("button", name=label, exact=True)).to_be_visible(timeout=30000)
        page.screenshot(path=str(OUTPUT / ("login-mobile.png" if username == "student" else "login-desktop.png")), full_page=True)
        assert page.evaluate("document.documentElement.scrollWidth <= innerWidth + 2")
        page.get_by_role("button", name=label, exact=True).click()
        page.wait_for_url(BASE + "/", timeout=30000)
        expect(page.locator(".user-name")).to_have_text(username)
        page.goto(BASE + ("/admin" if username == "admin" else "/attendance"))
        if username == "admin":
            expect(page.get_by_role("heading", name="Каталог LMS", exact=True)).to_be_visible()
        elif username == "student":
            expect(page.get_by_role("heading", name="Моя посещаемость", exact=True)).to_be_visible()
            expect(page.locator(".ocr-panel")).to_have_count(0)
        else:
            expect(page.locator(".ocr-panel")).to_be_visible()
        page.get_by_role("button", name="Выйти", exact=True).click()
        expect(page.get_by_label("Логин", exact=True)).to_be_visible()
        assert not failures, failures
        print(f"PASS: {username} quick login, role-specific view and logout")
        context.close()
    browser.close()
