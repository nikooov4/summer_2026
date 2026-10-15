from playwright.sync_api import expect
from lms_test_support import credentials

BASE = "http://localhost:5293"


def browser_login(page, username, route):
    page.goto(BASE + "/login")
    expect(page.get_by_label("Логин", exact=True)).to_be_visible(timeout=30000)
    page.get_by_label("Логин", exact=True).fill(username)
    page.get_by_label("Пароль", exact=True).fill(credentials()[username])
    page.get_by_role("button", name="Войти", exact=True).click()
    page.wait_for_url(BASE + "/", timeout=30000)
    page.goto(BASE + route)
    page.wait_for_selector(".lms-page-shell")
