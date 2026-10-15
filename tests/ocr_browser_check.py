"""Run with the optional Playwright dependency; use installed Microsoft Edge."""
import uuid
from pathlib import Path

from playwright.sync_api import sync_playwright, expect
from browser_support import browser_login as login

ROOT = Path(__file__).resolve().parents[1]
BASE = "http://localhost:5293"
PHOTO = ROOT / "attendance-ocr-service/examples/demo_attendance_sheet.png"
OUTPUT = ROOT / "document_work/browser_profiles/ocr-checks"
OUTPUT.mkdir(parents=True, exist_ok=True)


def scan(page):
    panel = page.locator(".ocr-panel")
    panel.locator(":scope > summary").click()
    panel.locator('input[type="file"]').set_input_files(str(PHOTO))
    panel.locator(".ocr-controls select").select_option("demo")
    panel.get_by_role("button", name="Распознать", exact=True).click()
    expect(panel.locator(".ocr-table tbody tr").first).to_be_visible(timeout=30000)
    return panel


with sync_playwright() as pw:
    browser = pw.chromium.launch(channel="msedge", headless=True)
    teacher = browser.new_context(viewport={"width": 1440, "height": 1000})
    page = teacher.new_page()
    failures = []
    page.on("pageerror", lambda error: failures.append(str(error)))
    login(page, "teacher", "/attendance")
    page.locator(".attendance-lesson-list button").filter(has_text="Компонентная модель Blazor").click()
    if page.get_by_role("button", name="Возобновить журнал", exact=True).count():
        page.get_by_role("button", name="Возобновить журнал", exact=True).click()
        expect(page.locator(".attendance-table tbody tr select").first).to_be_enabled()
    page.locator(".attendance-table tbody tr").filter(has_text="Иван Иванов").locator("select").select_option("Unmarked")
    save_button = page.locator(".attendance-actions-row button.statement-primary-button")
    if save_button.is_enabled():
        save_button.click()
        expect(page.get_by_text("Журнал сохранён в базе данных.", exact=True)).to_be_visible()
    before = page.locator(".attendance-table tbody tr select").evaluate_all("els => els.map(el => el.value)")
    panel = scan(page)
    review_rows = panel.locator(".ocr-table tbody tr")
    for index in range(review_rows.count()):
        row = review_rows.nth(index)
        if index == 0:
            row.locator("select").select_option(label="Иван Иванов (student)")
        else:
            row.locator('input[type="checkbox"]').uncheck()
    panel.get_by_role("button", name="Отметить присутствие", exact=True).click()
    expect(panel.get_by_role("status").filter(has_text="Изменено отметок")).to_be_visible(timeout=30000)
    after = page.locator(".attendance-table tbody tr select").evaluate_all("els => els.map(el => el.value)")
    assert sum(a != b for a, b in zip(before, after)) == 1
    page.locator(".attendance-actions-row button.statement-primary-button").click()
    expect(page.get_by_text("Журнал сохранён в базе данных.", exact=True)).to_be_visible()
    page.reload()
    page.wait_for_selector(".attendance-lesson-list button")
    page.locator(".attendance-lesson-list button").filter(has_text="Компонентная модель Blazor").click()
    expect(page.locator(".attendance-table tbody tr").filter(has_text="Иван Иванов").locator("select")).to_have_value("Present")
    page.evaluate("window.scrollTo(0, 0)")
    page.screenshot(path=str(OUTPUT / "teacher-desktop.png"), full_page=True)
    print("PASS: teacher OCR review and journal persistence after reload")
    page.set_viewport_size({"width": 390, "height": 844})
    page.screenshot(path=str(OUTPUT / "teacher-mobile.png"), full_page=True)
    assert page.evaluate("document.documentElement.scrollWidth <= innerWidth + 2")
    teacher.close()

    admin = browser.new_context(viewport={"width": 1440, "height": 1000})
    page = admin.new_page()
    page.on("pageerror", lambda error: failures.append(str(error)))
    login(page, "admin", "/admin")
    panel = scan(page)
    test_login = "ocr_browser_" + uuid.uuid4().hex[:10]
    review_rows = panel.locator(".ocr-table tbody tr")
    for index in range(review_rows.count()):
        row = review_rows.nth(index)
        if index == 0:
            row.locator("select").select_option("__new")
            row.get_by_label("ФИО нового студента").fill("Иван Соколов")
            row.get_by_label("Логин нового студента").fill(test_login)
        else:
            row.locator('input[type="checkbox"]').uncheck()
    panel.get_by_role("button", name="Создать и зачислить", exact=True).click()
    expect(panel.get_by_role("status").filter(has_text="Создано пользователей: 1")).to_be_visible()
    expect(page.locator(".admin-grid").get_by_text(test_login, exact=True).first).to_be_visible()
    page.reload()
    expect(page.locator(".admin-grid").get_by_text(test_login, exact=True).first).to_be_visible(timeout=30000)
    panel = page.locator(".ocr-panel")
    page.evaluate("window.scrollTo(0, 0)")
    page.screenshot(path=str(OUTPUT / "admin-desktop.png"), full_page=True)
    print("PASS: administrator OCR user and enrollment persist after reload")
    page.set_viewport_size({"width": 390, "height": 844})
    page.screenshot(path=str(OUTPUT / "admin-mobile.png"), full_page=True)
    assert page.evaluate("document.documentElement.scrollWidth <= innerWidth + 2")
    panel.scroll_into_view_if_needed()
    page.screenshot(path=str(OUTPUT / "admin-mobile-viewport.png"))
    print("PASS: mobile page has no horizontal overflow")
    admin.close()

    student = browser.new_context(viewport={"width": 390, "height": 844})
    page = student.new_page()
    login(page, "student", "/attendance")
    expect(page.locator(".ocr-panel")).to_have_count(0)
    expect(page.locator(".attendance-personal-card h3")).to_be_visible()
    assert page.locator(".attendance-personal-card").evaluate("""card => {
        const title = card.querySelector('h3').getBoundingClientRect();
        const badge = card.querySelector('.attendance-status-badge').getBoundingClientRect();
        return badge.top >= title.bottom && badge.height < 60;
    }""")
    page.screenshot(path=str(OUTPUT / "student-mobile.png"), full_page=True)
    assert not failures, failures
    print("PASS: student cannot access OCR; no browser errors")
    student.close()
    browser.close()
