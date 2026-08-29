from __future__ import annotations

from copy import deepcopy
from pathlib import Path
from typing import Callable, Iterable

from docx import Document
from docx.document import Document as DocxDocument
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Cm, Pt
from PIL import Image, ImageDraw, ImageFont
from pptx import Presentation
from pptx.dml.color import RGBColor
from pptx.enum.shapes import MSO_SHAPE
from pptx.enum.text import MSO_ANCHOR, PP_ALIGN
from pptx.util import Inches, Pt as PptPt


ROOT = Path(__file__).resolve().parents[1]
DESKTOP = ROOT.parent
WORK = ROOT / "document_work"
SCREENS = WORK / "screenshots"
ASSETS = WORK / "generated_assets"
OUTPUT = ROOT / "Готовые_материалы"

EF_REPORT_SOURCE = ROOT / "uir-main" / "deliverables" / "Пояснительная_записка_УИР_Ефименко_актуализированная.docx"
LOK_REPORT_SOURCE = DESKTOP / "ПЗ_ЛоктевМС.docx"
EF_PPT_SOURCE = DESKTOP / "Презентация_ЕфименкоНП.pptx"
LOK_PPT_SOURCE = DESKTOP / "Презентация_ЛоктевМС.pptx"


NAVY = "1D334A"
BLUE = "6279EA"
PURPLE = "7851AD"
ORANGE = "F47738"
GREEN = "2E9B62"
RED = "D94B4B"
LIGHT = "F4F6FA"
MID = "DDE3EC"
GRAY = "6F7D8A"
WHITE = "FFFFFF"


def font(size: int, bold: bool = False) -> ImageFont.FreeTypeFont:
    name = "arialbd.ttf" if bold else "arial.ttf"
    return ImageFont.truetype(str(Path("C:/Windows/Fonts") / name), size)


def rounded_box(draw: ImageDraw.ImageDraw, box: tuple[int, int, int, int], fill: str, outline: str | None = None) -> None:
    draw.rounded_rectangle(box, radius=20, fill=f"#{fill}", outline=f"#{outline}" if outline else None, width=3)


def draw_centered(draw: ImageDraw.ImageDraw, box: tuple[int, int, int, int], text: str, used_font: ImageFont.FreeTypeFont, fill: str) -> None:
    left, top, right, bottom = box
    bounds = draw.multiline_textbbox((0, 0), text, font=used_font, spacing=8, align="center")
    width = bounds[2] - bounds[0]
    height = bounds[3] - bounds[1]
    draw.multiline_text(((left + right - width) / 2, (top + bottom - height) / 2), text, font=used_font, fill=f"#{fill}", spacing=8, align="center")


def create_role_diagram(path: Path) -> None:
    image = Image.new("RGB", (1600, 900), "white")
    draw = ImageDraw.Draw(image)
    draw.text((70, 45), "Тестовая ролевая модель LMS", font=font(45, True), fill=f"#{NAVY}")
    draw.text((70, 105), "Права определяются учетной записью и назначением пользователя на курс", font=font(25), fill=f"#{GRAY}")

    cards = [
        (70, 190, 520, 610, BLUE, "АДМИНИСТРАТОР", "Пользователи и курсы\nЗачисления и назначения\nВсе журналы\nJSON/CSV обмен"),
        (575, 190, 1025, 610, PURPLE, "ПРЕПОДАВАТЕЛЬ", "Только назначенные курсы\nРедактирование журнала\nЗавершение и возобновление\nМониторинг групп"),
        (1080, 190, 1530, 610, GREEN, "СТУДЕНТ", "Только свои дисциплины\nЛичная история\nЛичные показатели\nБез редактирования"),
    ]
    for left, top, right, bottom, color, title, body in cards:
        rounded_box(draw, (left, top, right, bottom), LIGHT, color)
        rounded_box(draw, (left + 20, top + 20, right - 20, top + 95), color)
        draw_centered(draw, (left + 20, top + 20, right - 20, top + 95), title, font(24, True), WHITE)
        draw.multiline_text((left + 38, top + 135), body, font=font(25), fill=f"#{NAVY}", spacing=22)

    rounded_box(draw, (160, 690, 720, 820), NAVY)
    rounded_box(draw, (880, 690, 1440, 820), NAVY)
    draw_centered(draw, (160, 690, 720, 820), "AuthSessionService\nдемо-токен и текущий пользователь", font(25, True), WHITE)
    draw_centered(draw, (880, 690, 1440, 820), "LmsCatalogService\nкурсы, роли и разрешения", font(25, True), WHITE)
    draw.line((720, 755, 880, 755), fill=f"#{ORANGE}", width=8)
    image.save(path)


def create_integration_diagram(path: Path, monitoring_focus: bool = False) -> None:
    image = Image.new("RGB", (1600, 900), "white")
    draw = ImageDraw.Draw(image)
    title = "Интеграция мониторинга и администрирования" if monitoring_focus else "Интеграция учета посещений в существующую LMS"
    draw.text((70, 45), title, font=font(42, True), fill=f"#{NAVY}")

    rounded_box(draw, (80, 170, 550, 530), LIGHT, BLUE)
    draw.text((120, 205), "Существующий модуль", font=font(27, True), fill=f"#{BLUE}")
    draw.multiline_text((120, 270), "Конструктор ведомостей\nStatementConstructor\nStatementService\nРасчет оценок и ECTS", font=font(27), fill=f"#{NAVY}", spacing=20)

    rounded_box(draw, (610, 170, 1520, 530), LIGHT, PURPLE)
    draw.text((650, 205), "Добавленный функционал", font=font(27, True), fill=f"#{PURPLE}")
    if monitoring_focus:
        body = "AttendanceMonitoring.razor\nКонтроль заполнения и показатели\nСтуденты, требующие внимания\nAdmin.razor: пользователи, курсы, зачисления"
    else:
        body = "Attendance.razor\nЖурнал преподавателя\nЛичный режим студента\nПроверка операций по роли и курсу"
    draw.multiline_text((650, 270), body, font=font(27), fill=f"#{NAVY}", spacing=18)

    draw.line((550, 350, 610, 350), fill=f"#{ORANGE}", width=10)
    draw.polygon([(610, 350), (585, 335), (585, 365)], fill=f"#{ORANGE}")

    rounded_box(draw, (160, 650, 760, 810), NAVY)
    rounded_box(draw, (840, 650, 1440, 810), NAVY)
    draw_centered(draw, (160, 650, 760, 810), "Общий каркас LMS\nнавигация и дизайн", font(28, True), WHITE)
    catalog_text = "Каталог LMS\nпользователи, курсы, зачисления" if monitoring_focus else "Общий контекст\nпользователи, курсы, роли"
    draw_centered(draw, (840, 650, 1440, 810), catalog_text, font(28, True), WHITE)
    draw.line((760, 730, 840, 730), fill=f"#{ORANGE}", width=8)
    image.save(path)


def crop_image(source: Path, target: Path, box: tuple[int, int, int, int]) -> None:
    with Image.open(source) as image:
        image.crop(box).save(target)


def prepare_assets() -> None:
    ASSETS.mkdir(parents=True, exist_ok=True)
    create_role_diagram(ASSETS / "role_model.png")
    create_integration_diagram(ASSETS / "attendance_integration.png")
    create_integration_diagram(ASSETS / "monitoring_integration.png", monitoring_focus=True)

    crop_specs = {
        "login.png": (SCREENS / "01-login.png", (0, 0, 1440, 810)),
        "teacher_attendance.png": (SCREENS / "02-teacher-attendance.png", (0, 80, 1440, 890)),
        "student_attendance.png": (SCREENS / "03-student-attendance.png", (0, 80, 1440, 890)),
        "monitoring_main.png": (SCREENS / "09-teacher-monitoring-full.png", (0, 80, 1440, 890)),
        "monitoring_attention.png": (SCREENS / "09-teacher-monitoring-full.png", (0, 1120, 1440, 1930)),
        "student_monitoring.png": (SCREENS / "07-student-monitoring.png", (0, 80, 1440, 890)),
        "admin_main.png": (SCREENS / "08-admin-catalog-full.png", (0, 80, 1440, 890)),
        "admin_courses.png": (SCREENS / "08-admin-catalog-full.png", (0, 1450, 1440, 2260)),
        "statement_constructor.png": (SCREENS / "06-statement-constructor.png", (0, 80, 1440, 890)),
    }
    for name, (source, box) in crop_specs.items():
        crop_image(source, ASSETS / name, box)


def all_paragraphs(document: DocxDocument) -> Iterable:
    for paragraph in document.paragraphs:
        yield paragraph
    for table in document.tables:
        for row in table.rows:
            for cell in row.cells:
                for paragraph in cell.paragraphs:
                    yield paragraph
    for section in document.sections:
        for part in (section.header, section.footer):
            for paragraph in part.paragraphs:
                yield paragraph


def set_paragraph_text(paragraph, text: str) -> None:
    if paragraph.runs:
        paragraph.runs[0].text = text
        for run in paragraph.runs[1:]:
            run.text = ""
    else:
        paragraph.add_run(text)


def replace_everywhere(document: DocxDocument, old: str, new: str) -> None:
    for paragraph in all_paragraphs(document):
        if old in paragraph.text:
            set_paragraph_text(paragraph, paragraph.text.replace(old, new))


def set_first(document: DocxDocument, predicate: Callable[[str], bool], text: str) -> None:
    for paragraph in document.paragraphs:
        if predicate(paragraph.text):
            set_paragraph_text(paragraph, text)
            return
    raise ValueError(f"Paragraph was not found for: {text[:80]}")


def set_starts(document: DocxDocument, prefix: str, text: str) -> None:
    set_first(document, lambda value: value.startswith(prefix), text)


def set_exact(document: DocxDocument, value: str, text: str) -> None:
    set_first(document, lambda current: current == value, text)


def insert_after(paragraph, text: str):
    element = OxmlElement("w:p")
    paragraph._p.addnext(element)
    inserted = paragraph._parent.add_paragraph()
    inserted._p.getparent().remove(inserted._p)
    element.addnext(inserted._p)
    inserted.style = paragraph.style
    inserted.add_run(text)
    inserted.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
    inserted.paragraph_format.first_line_indent = Cm(1.25)
    inserted.paragraph_format.line_spacing = 1.5
    for run in inserted.runs:
        run.font.name = "Times New Roman"
        run.font.size = Pt(14)
    return inserted


def replace_image(document: DocxDocument, relation_id: str, image_path: Path) -> None:
    relation = document.part.rels[relation_id]
    relation.target_part._blob = image_path.read_bytes()


def set_table(document: DocxDocument, index: int, rows: list[list[str]]) -> None:
    table = document.tables[index]
    if len(rows) > len(table.rows):
        raise ValueError(f"Table {index} has only {len(table.rows)} rows")
    for row_index, values in enumerate(rows):
        if len(values) > len(table.columns):
            raise ValueError(f"Table {index} has only {len(table.columns)} columns")
        for column_index, value in enumerate(values):
            cell = table.cell(row_index, column_index)
            cell.text = value
            for paragraph in cell.paragraphs:
                for run in paragraph.runs:
                    run.font.name = "Times New Roman"
                    run.font.size = Pt(10)


def set_table_cell(document: DocxDocument, table_index: int, row_index: int, column_index: int, text: str) -> None:
    cell = document.tables[table_index].cell(row_index, column_index)
    set_paragraph_text(cell.paragraphs[0], text)
    for paragraph in cell.paragraphs:
        for run in paragraph.runs:
            run.font.name = "Times New Roman"
            run.font.size = Pt(10)


def update_efimenko_report() -> Path:
    document = Document(EF_REPORT_SOURCE)
    document.core_properties.title = "Интеграция учета посещений и тестовой ролевой модели в LMS"
    document.core_properties.author = "Ефименко Н.П."

    for paragraph in all_paragraphs(document):
        normalized = " ".join(paragraph.text.split())
        if "Разработка пользовательского веб-интерфейса" in normalized and "учета посещений" in normalized:
            set_paragraph_text(paragraph, "Разработка пользовательского веб-интерфейса учета посещений и тестовой ролевой модели в LMS")

    set_starts(document, "Общий объем пояснительной", "Общий объем пояснительной записки без учета листов задания составляет 42 страницы. Работа содержит 15 таблиц, 9 рисунков, 20 использованных источников и 2 приложения.")
    set_starts(document, "Ключевые слова:", "Ключевые слова: УЧЕТ ПОСЕЩЕНИЙ, LMS, ТЕСТОВАЯ РОЛЕВАЯ МОДЕЛЬ, BLAZOR WEBASSEMBLY, RAZOR-КОМПОНЕНТЫ, C#, РАЗГРАНИЧЕНИЕ ДОСТУПА, АДАПТИВНЫЙ ИНТЕРФЕЙС.")
    set_starts(document, "Цель работы заключается", "Цель работы заключается в интеграции модуля учета посещений в существующий проект LMS и реализации тестовой ролевой модели. Прототип поддерживает журнал преподавателя, личный режим студента, выбор доступных дисциплин и ограничение операций по учетной записи и назначению на курс.")
    set_starts(document, "Инженерная часть содержит", "Инженерная часть содержит компонентную архитектуру, навигацию и перспективные контракты REST API. Практическая часть описывает сохранение исходного конструктора ведомостей, добавление журнала посещений, личной истории, тестовых входов администратора, преподавателя и студента, а также связь страниц с каталогом пользователей и курсов.")

    set_starts(document, "Цель работы состоит в разработке", "Цель работы состоит в разработке и интеграции пользовательского веб-интерфейса учета посещений в LMS с помощью Blazor WebAssembly. Для достижения цели необходимо исследовать предметную область, определить статусы и сценарии, реализовать журнал и личный просмотр, сформировать тестовую модель ролей и проверить ограничение данных по пользователю и курсу.")
    set_starts(document, "Практическая значимость связана", "Практическая значимость связана с сокращением времени заполнения журнала и разделением рабочих режимов преподавателя и студента. Новый функционал встроен рядом с исходным конструктором ведомостей, использует общий дизайн LMS и получает контекст пользователей, курсов и назначений из LmsCatalogService.")
    intro_anchor = next(p for p in document.paragraphs if p.text.startswith("Практическая значимость связана"))
    insert_after(intro_anchor, "Личный вклад автора включает реализацию журнала посещаемости, статусов и правил ввода, личного режима студента, демонстрационных входов и тестовой ролевой модели с фильтрацией доступных дисциплин. Модуль мониторинга показателей и административные операции со справочниками разработаны в смежной части проекта М.С. Локтева и использованы при совместной интеграции.")

    set_starts(document, "Администратор управляет справочниками", "Администратор в тестовой модели управляет пользователями, курсами и зачислениями, а также открывает журналы всех дисциплин. Интерфейс позволяет добавлять и удалять записи, менять признак администратора и проверять доступность страниц для разных учетных записей.")
    set_starts(document, "Клиентское ограничение повышает", "В прототипе реализовано клиентское разграничение доступа. AuthSessionService извлекает имя пользователя из демонстрационного токена, LmsCatalogService определяет роль на конкретном курсе, а AttendanceService фильтрует занятия и разрешает изменение только администратору, преподавателю или ассистенту. Такая модель предназначена для испытаний и не заменяет серверную авторизацию.")
    set_starts(document, "MudBlazor выбран", "MudBlazor рассматривался как вариант библиотеки визуальных элементов, однако при переносе в omelea-dev-lms применены стандартные Razor-компоненты и существующие CSS-стили проекта. Это позволило сохранить внешний вид LMS и не добавлять новую зависимость.")
    set_starts(document, "Анализ показал, что основным рабочим", "Анализ показал, что основным рабочим экраном преподавателя должен быть журнал выбранного занятия, дополненный поиском, фильтрами, массовой операцией и сводными показателями. Для реализации выбран Blazor WebAssembly и существующее оформление omelea-dev-lms. Тестовые роли и административный раздел включены в интегрированный прототип; REST API и серверная авторизация оставлены дальнейшим развитием.")
    set_starts(document, "В режиме студента выбирается", "После входа студента система автоматически связывает учетную запись с его данными. Пользователь видит только назначенные дисциплины, собственную историю и личные показатели; выбор другого студента и элементы изменения журнала недоступны. Администратор и преподаватель получают расширенные режимы в соответствии с назначениями.")
    set_starts(document, "Демонстрационные данные хранятся", "Демонстрационные пользователи, курсы и зачисления формируются LmsCatalogService, а журналы и черновики обслуживает AttendanceService в памяти клиентского приложения. Для пользователей реализован обмен JSON/CSV через административную форму; промышленная база данных в прототип не входит.")
    set_starts(document, "Общее состояние вынесено", "Общее состояние вынесено в AttendanceService. Сервис хранит выбранное занятие, подтвержденный снимок, локальные изменения, правила проверки, сводные показатели и операции сохранения. Razor-страница передает сервису команды изменения статуса, минут и комментария.")

    set_starts(document, "Инженерная часть описывает структуру", "Инженерная часть описывает структуру программных компонентов после переноса в omelea-dev-lms. Razor-страницы отвечают за отображение и события, AttendanceService - за журналы и показатели, LmsCatalogService - за пользователей, курсы, назначения и разрешения, AuthSessionService - за тестовый сеанс, а StatementService продолжает обслуживать исходный конструктор ведомостей.")
    set_starts(document, "На уровне представления находятся", "На уровне представления находятся страницы Attendance, AttendanceMonitoring, Admin, Login и сохраненные страницы ведомостей. Attendance объединяет выбор занятия, сводные показатели, поиск, фильтр, таблицу преподавателя и личную историю студента.")
    set_starts(document, "Разметка журнала получает", "Разметка журнала получает подготовленные строки из AttendanceService. Преподавательское и студенческое представления используют одни модели AttendanceLessonModel, AttendanceEntryModel и AttendanceStudentModel, но набор доступных элементов определяется текущей учетной записью.")
    set_starts(document, "AttendanceWorkspace координирует", "AttendanceService координирует выбор занятия, одиночные и массовые изменения, сохранение и отмену черновика, завершение и возобновление журнала, историю студента и расчет показателей. Перед выдачей данных сервис сверяет курс с назначениями в LmsCatalogService.")
    set_starts(document, "Текущая версия не использует HTTP", "Текущая версия не использует промышленный HTTP API. Состояние прототипа хранится в singleton-сервисах WebAssembly-приложения, а JSON/CSV применяется для обмена списком пользователей. Выделение сервисов позволяет позднее подключить серверное хранилище без полной переработки страниц.")
    set_exact(document, "3.2 Навигация и макеты основных экранов", "3.2 Навигация и тестовая ролевая модель")
    set_starts(document, "Прототип доступен без входа", "Маршрут /login содержит демонстрационные входы администратора, преподавателя и студента. После входа имя пользователя сохраняется в локальном токене, а основная навигация формируется с учетом доступных разделов.")
    set_starts(document, "Боковая панель страницы /attendance", "Страница /attendance показывает только занятия назначенных пользователю курсов. Преподаватель и администратор могут редактировать строки, завершать и возобновлять журналы; студент автоматически переводится в личный режим без возможности выбрать другого обучающегося.")
    set_starts(document, "Страница журнала включает", "Страница журнала включает заголовок занятия, поиск, фильтры, групповые действия, таблицу студентов и сообщения проверки. Страница /attendance/monitoring для преподавателя показывает состояние журналов, а для студента - только его показатели и историю.")
    set_starts(document, "Раздел концептов по маршруту", "Исходный конструктор ведомостей сохранен по маршруту /statements/constructor и не подвергался существенной переработке. Модуль посещаемости встроен рядом с ним как отдельная подсистема в общем каркасе и дизайне LMS.")
    set_exact(document, "Рисунок 3.2 - Карта навигации веб-интерфейса", "Рисунок 3.2 - Тестовая админ-панель пользователей и обмена данными")
    set_exact(document, "Рисунок 3.3 - Реализованный режим личной истории студента", "Рисунок 3.3 - Личный режим студента без элементов редактирования")
    set_exact(document, "Рисунок 3.4 - Реализованный журнал посещений преподавателя", "Рисунок 3.4 - Журнал преподавателя для назначенной дисциплины")
    set_starts(document, "Ниже приведены проектируемые контракты", "Ниже приведены проектируемые контракты будущей серверной интеграции. В текущей версии страницы обращаются к AttendanceService и LmsCatalogService; вызовы REST API, серверная проверка токена и база данных относятся к следующему этапу.")
    set_starts(document, "Диаграмма последовательности показывает", "Диаграмма последовательности показывает целевой сценарий серверного сохранения. В реализованном прототипе Razor-компонент передает команду AttendanceService, выполняется проверка строки и обновляется состояние текущего сеанса; вызов API и обработка конфликтов являются будущим расширением.")

    set_starts(document, "Практическая часть описывает автономный", "Практическая часть описывает интегрированный прототип Blazor WebAssembly. Исходный конструктор ведомостей сохранен как отдельный модуль, а рядом добавлены журнал посещений, мониторинг, тестовые входы и админ-панель.")
    set_starts(document, "В качестве технологической основы используются", "В качестве технологической основы используются .NET 8, C# и Blazor WebAssembly. Визуальные элементы выполнены стандартной Razor-разметкой и существующими CSS-стилями omelea-dev-lms; сервисы зарегистрированы во встроенном контейнере зависимостей.")
    set_starts(document, "Структура проекта разделена", "Структура проекта разделена на Pages, Models, Services, Layout и wwwroot. Модели посещаемости и доступа отделены от Razor-разметки. AttendanceService содержит состояние журналов и расчеты, LmsCatalogService - пользователей, курсы и назначения, AuthSessionService - текущий тестовый сеанс.")
    set_starts(document, "При запуске как singleton", "LmsCatalogService и AttendanceService зарегистрированы как singleton-сервисы в пределах клиентского приложения. Это сохраняет изменения при переходах между страницами текущего сеанса; постоянная запись в серверную базу данных пока не выполняется.")
    set_starts(document, "Режим студента содержит записи", "Режим студента содержит записи в обратном хронологическом порядке и личные показатели. Изменяющие элементы отсутствуют, выбор другого студента заблокирован. Административная страница доступна только тестовой учетной записи admin.")
    set_starts(document, "Команда отмены возвращает", "Команда отмены возвращает строки к последнему подтвержденному снимку текущего сеанса. После сохранения состояние становится новым подтвержденным снимком, а признаки черновика снимаются.")
    set_exact(document, "Рисунок 4.1 - Редактор структуры ведомости и вычисляемых столбцов", "Рисунок 4.1 - Исходный конструктор ведомостей в составе обновленной LMS")
    set_exact(document, "4.5 Локальное сохранение и обработка ошибок", "4.5 Интеграция с LMS и тестовый доступ")
    set_starts(document, "AttendanceWorkspace сериализует", "AttendanceService хранит журналы и подтвержденные снимки в памяти клиентского приложения. LmsCatalogService синхронизирует состав студентов занятий с назначениями на курсы, поэтому изменение каталога отражается в доступных списках и журналах.")
    set_starts(document, "Ошибки предметных правил", "Ошибки предметных правил отображаются рядом со строкой и блокируют сохранение. Дополнительно проверяется право текущего пользователя изменять выбранный курс; студент получает только чтение собственной истории.")
    set_starts(document, "Операции локального файла", "Операции текущего сеанса выполняются без перезагрузки страницы. Кнопки сохранения и отмены блокируются при отсутствии изменений, после записи интерфейс подтверждает результат статусным сообщением.")
    set_starts(document, "Клиентская проверка обеспечивает", "Клиентская проверка обеспечивает корректность демонстрационных данных, но не является промышленным механизмом защиты. Тестовые токены не подписаны, а серверная проверка разрешений, аудит и база данных должны быть добавлены при дальнейшем внедрении.")
    set_starts(document, "Проверка проводилась на демонстрационной", "Проверка проводилась на демонстрационных учетных записях admin, teacher и student. Проверены фильтрация дисциплин, запрет редактирования для студента, обязательные минуты и комментарий, массовая отметка, отмена, завершение журнала и переход к мониторингу.")
    set_starts(document, "Автоматизированный smoke-тест", "Выполнены сборка проекта, ручная проверка основных сценариев и визуальная проверка экранов преподавателя, студента и администратора. Отдельно проверено, что исходный конструктор ведомостей продолжает открываться и рассчитывать итоговые оценки.")
    set_starts(document, "Сценарии частичного успеха", "Сценарии частичного успеха, конфликта версии и потери сети относятся к следующему этапу с REST API. Текущая реализация сосредоточена на ролевом поведении интерфейса и согласованной работе сервисов текущего сеанса.")
    set_starts(document, "В практической части реализован", "В практической части реализован прототип на Blazor WebAssembly без внешней UI-библиотеки: журнал, личная история, поиск, фильтры, массовая операция, валидация и тестовая ролевая модель. Исходный конструктор ведомостей сохранен. REST API, серверная авторизация, аудит и постоянная база данных оставлены дальнейшим развитием.")

    set_starts(document, "В ходе учебно-исследовательской", "В ходе учебно-исследовательской работы модуль учета посещений перенесен в существующий проект omelea-dev-lms и приведен к его визуальному стилю. Журнал посещений реализован как отдельная электронная ведомость рядом с исходным конструктором оценок.")
    set_starts(document, "Сформированы модели занятий", "Сформированы модели занятий, студентов и отметок. Реализованы выбор занятия, фиксация и корректировка посещения, поиск, фильтрация, массовая отметка, завершение журнала, личная история и сводные показатели.")
    set_starts(document, "Архитектура включает Razor", "Архитектура включает Razor-страницы, AttendanceService, LmsCatalogService, AuthSessionService и StatementService. Тестовая ролевая модель связывает учетную запись с курсами и определяет, какие журналы доступны для чтения и изменения.")
    set_starts(document, "Прототип построен на .NET", "Прототип построен на .NET 8, C# и Blazor WebAssembly с собственными CSS-стилями LMS. Администратор, преподаватель и студент получают разные рабочие режимы; студенческий интерфейс не содержит операций изменения чужих данных.")
    set_starts(document, "Таким образом, создан", "Таким образом, создан работоспособный интегрированный прототип. Личный вклад автора охватывает журнал посещаемости, личный режим студента и тестовую модель ролей. Дальнейшее развитие связано с серверным API, подписанными токенами, базой данных, аудитом и прямой синхронизацией посещаемости с конструктором ведомостей.")
    set_starts(document, "Сохранить изменения, перезапустить", "Сохранить изменения, перейти на другую страницу и вернуться в журнал, проверив состояние текущего сеанса.")

    set_table(document, 7, [
        ["Модель", "Назначение", "Ключевые поля"],
        ["LmsUserModel", "Пользователь LMS", "Username, FullName, RecordBookNumber, IsAdmin"],
        ["LmsCourseModel", "Дисциплина и группа", "Code, Name, GroupName, DefaultRoom"],
        ["LmsEnrollmentModel", "Назначение на курс", "UserId, CourseId, Role"],
        ["AttendanceLessonModel", "Занятие и журнал", "CourseId, StartsAt, Topic, Entries"],
        ["AttendanceSummaryModel", "Сводные показатели", "Present, Late, Excused, Absent, Unmarked"],
    ])
    set_table(document, 10, [
        ["Состояние", "Признак", "Доступные действия"],
        ["Не выбран контекст", "Нет доступного занятия", "Выбрать дисциплину или проверить назначения"],
        ["Сохранено", "Черновик отсутствует", "Фильтровать и редактировать при наличии права"],
        ["Черновик", "Есть локальные изменения", "Сохранить или отменить"],
        ["Ошибка проверки", "Не заполнены зависимые поля", "Исправить строку"],
        ["Завершен", "CompletedAt задан", "Просматривать или возобновить"],
        ["Только чтение", "Текущий пользователь - студент", "Открыть личную историю"],
        ["Нет доступа", "Курс не назначен пользователю", "Вернуться к доступным дисциплинам"],
    ])

    set_table(document, 12, [
        ["Маршрут", "Назначение", "Доступ"],
        ["/login", "Демонстрационный вход", "Все пользователи"],
        ["/", "Главная страница LMS", "После входа"],
        ["/statements", "Список и просмотр ведомостей", "После входа"],
        ["/statements/constructor", "Исходный конструктор оценок", "После входа"],
        ["/attendance", "Журнал или личная история", "По роли на курсе"],
        ["/attendance/monitoring", "Мониторинг или личные показатели", "По учетной записи"],
        ["/admin", "Каталоги и назначения", "Только администратор"],
    ])
    set_table(document, 14, [
        ["Каталог", "Содержимое"],
        ["Pages", "Журнал, мониторинг, админ-панель, вход и ведомости"],
        ["Models/Attendance", "Студенты, занятия, отметки и сводки"],
        ["Models", "Пользователи, курсы, назначения и роли"],
        ["Services/Attendance", "Журналы, черновики, история и показатели"],
        ["Services/Admin", "Каталог LMS, права и JSON/CSV обмен"],
        ["Services/Statements", "Исходная логика конструктора ведомостей"],
        ["Layout и wwwroot", "Навигация и общие стили omelea-dev-lms"],
    ])
    set_table(document, 15, [
        ["Компонент", "Назначение", "Основные события"],
        ["Attendance.razor", "Журнал и личная история", "Выбор, изменение, сохранение"],
        ["AttendanceMonitoring.razor", "Сводные показатели", "Период, фильтр, переход"],
        ["Login.razor", "Тестовые входы", "SetTokenAsync"],
        ["Admin.razor", "Каталоги LMS", "Добавление и удаление"],
        ["AttendanceService", "Состояние посещаемости", "Save, Complete, Reopen"],
        ["LmsCatalogService", "Пользователи, курсы, роли", "Import, Export, Enrollment"],
        ["StatementConstructor.razor", "Исходная ведомость оценок", "Generate, Save"],
    ])
    set_table(document, 16, [
        ["№", "Сценарий", "Ожидаемый результат"],
        ["1", "Вход преподавателя", "Видны только назначенные дисциплины"],
        ["2", "Вход студента", "Нет редактирования и выбора другого студента"],
        ["3", "Опоздание без минут", "Строка помечается ошибкой"],
        ["4", "Уважительная причина без комментария", "Сохранение блокируется"],
        ["5", "Массовая отметка", "Меняются только неотмеченные строки"],
        ["6", "Завершение журнала", "Неотмеченные строки блокируют действие"],
        ["7", "Вход администратора", "Доступна страница /admin"],
        ["8", "Смена назначения на курс", "Меняется список доступных дисциплин"],
        ["9", "Открытие конструктора", "Исходная ведомость работает без изменений"],
    ])

    replace_image(document, "rId13", ASSETS / "role_model.png")
    replace_image(document, "rId17", ASSETS / "attendance_integration.png")
    replace_image(document, "rId18", ASSETS / "admin_main.png")
    replace_image(document, "rId19", ASSETS / "student_attendance.png")
    replace_image(document, "rId20", ASSETS / "teacher_attendance.png")
    replace_image(document, "rId22", ASSETS / "statement_constructor.png")

    output = OUTPUT / "ПЗ_ЕфименкоНП_обновленная.docx"
    document.save(output)
    return output


def update_loktev_report() -> Path:
    document = Document(LOK_REPORT_SOURCE)
    document.core_properties.title = "Модуль мониторинга и администрирования посещаемости в LMS"
    document.core_properties.author = "Локтев М.С."

    replace_everywhere(document, "Создание клиентского модуля мониторинга посещаемости учебных занятий", "Создание клиентского модуля мониторинга и администрирования посещаемости учебных занятий")
    set_starts(document, "Общий объем пояснительной", "Общий объем пояснительной записки без учета листов задания составляет 36 страниц. Работа содержит 16 таблиц, 9 рисунков, 20 использованных источников и 2 приложения.")
    set_starts(document, "Ключевые слова:", "Ключевые слова: МОНИТОРИНГ ПОСЕЩАЕМОСТИ, АДМИН-ПАНЕЛЬ, BLAZOR WEBASSEMBLY, RAZOR-КОМПОНЕНТЫ, ПОКАЗАТЕЛИ, JSON, CSV, LMS.")
    set_starts(document, "Цель работы -", "Цель работы - разработать и встроить в LMS клиентский модуль мониторинга посещаемости и тестовые административные операции. Решение сфокусировано на контроле заполнения журналов, объяснимых показателях, списке студентов, требующих внимания, и управлении пользователями, курсами и зачислениями.")
    set_starts(document, "Практическая часть описывает построение", "Практическая часть описывает страницы мониторинга и администрирования на Blazor WebAssembly, фильтрацию периода и состояния журналов, расчет показателей, выбор студента, а также обмен списком пользователей в JSON/CSV.")

    set_starts(document, "В качестве технологической основы применяется", "В качестве технологической основы применяется Blazor WebAssembly. Компонентный подход позволяет описать рабочие экраны на C# и Razor, а интерфейс использует стандартные элементы и существующие CSS-стили omelea-dev-lms без подключения MudBlazor.")
    set_starts(document, "Объектом исследования является", "Объектом исследования является пользовательский процесс мониторинга посещаемости и ведения справочников LMS. Предмет исследования - способы организации экранных форм, сводных показателей и административных операций в клиентском приложении.")
    set_starts(document, "Цель работы состоит в создании", "Цель работы состоит в создании прототипа мониторинга и администрирования посещаемости на Blazor WebAssembly. Для достижения цели необходимо определить показатели, реализовать фильтры и переходы к журналам, сформировать список студентов, требующих внимания, и добавить операции со справочниками пользователей, курсов и зачислений.")
    set_starts(document, "Методы работы включают", "Методы работы включают анализ пользовательских сценариев, моделирование сводных показателей, прототипирование Razor-страниц и ручную проверку на демонстрационных данных. Полученный результат встроен в существующий интерфейс omelea-dev-lms.")
    intro_anchor = next(p for p in document.paragraphs if p.text.startswith("Методы работы включают"))
    insert_after(intro_anchor, "Личный вклад автора включает страницу мониторинга, показатели полноты и посещаемости, фильтры журналов, список студентов, требующих внимания, переход к выбранному студенту, а также тестовую админ-панель и обмен пользователями в JSON/CSV.")

    set_starts(document, "Аутентификация подтверждает", "В интегрированном прототипе мониторинг получает подготовленные данные о дисциплинах и занятиях из AttendanceService. Промышленное серверное хранение, обработка кодов HTTP и аудит остаются направлениями дальнейшего развития.")
    set_starts(document, "MudBlazor предоставляет", "MudBlazor рассматривался как возможная библиотека компонентов, однако фактическая реализация построена на Razor-разметке и собственном CSS проекта omelea-dev-lms. Прикладные расчеты и фильтрация вынесены в сервисы и не зависят от конкретной UI-библиотеки.")
    set_starts(document, "Анализ показал, что центральной", "Анализ показал, что центральной функцией работы является объяснимый мониторинг: пользователь видит полноту журналов, посещаемость, незавершенные занятия и отклонения, а затем переходит к исходному журналу или выбранному студенту. Административная часть поддерживает подготовку каталога для этих расчетов.")
    set_starts(document, "Требования разделяются на функциональные", "Требования разделяются на функциональные и качественные. Функциональные описывают доступные операции, качественные - скорость, понятность, устойчивость и доступность. Для прототипа выполнена приоритизация: наивысший приоритет получили понятность показателей и скорость перехода к исходным данным. Сводные показатели не должны блокировать основной сценарий работы с журналом.")
    set_starts(document, "UserContext содержит", "MonitoringContext содержит выбранный период, состояние фильтров и идентификаторы открытых объектов. Он используется для построения страницы мониторинга и переходов к журналу или карточке студента. Постоянное хранение и проверка операций на сервере относятся к дальнейшему развитию.")
    set_starts(document, "Навигация начинается с рабочей области", "Навигация начинается с рабочей области LMS и включает страницы журналов, мониторинга и администрирования. Параметры курса, группы, занятия и выбранного студента передаются через маршрут или контекст страницы и используются при загрузке данных.")

    set_starts(document, "Уровень сценариев представлен", "Уровень сценариев представлен AttendanceService. Он предоставляет отфильтрованные занятия, рассчитывает сводки, формирует историю и список студентов, требующих внимания, а также выполняет завершение и возобновление журналов.")
    set_starts(document, "Интеграционный уровень включает", "Интеграционный уровень прототипа включает AttendanceService и LmsCatalogService. Первый обслуживает посещаемость и показатели, второй предоставляет пользователей, курсы и назначения. REST API и серверное хранилище описываются как следующий этап.")
    set_starts(document, "Спроектирована и уточнена архитектура", "Спроектирована и уточнена архитектура, соответствующая исходному коду: AttendanceMonitoring.razor и Admin.razor обращаются к AttendanceService и LmsCatalogService, используют общий каркас LMS и не изменяют логику исходного конструктора ведомостей.")
    set_starts(document, "Практическая часть описывает прототип", "Практическая часть описывает прототип на .NET 8, C# и Blazor WebAssembly. Реализация использует демонстрационные данные в памяти клиентского приложения и позволяет проверить мониторинг и административные сценарии до подключения промышленного API.")

    set_starts(document, "Маршрут /workspace", "Маршрут /attendance/monitoring открывает сводную страницу. Пользователь выбирает период 7 дней, 30 дней или все время и фильтрует журналы по состоянию. Строка занятия показывает заполнение, посещаемость, время обновления и доступное действие.")
    set_starts(document, "Страница JournalPage", "Из мониторинга можно открыть выбранный журнал по маршруту /attendance с идентификатором занятия. Для преподавателя доступны завершение и возобновление, а студент вместо общего списка получает личные показатели и историю.")
    set_starts(document, "В прототипе предусмотрена отмена", "Данные на странице пересчитываются из общего AttendanceService без перезагрузки. Изменение состояния журнала сразу отражается в полноте, числе незавершенных занятий и списке отклонений.")
    set_starts(document, "Массовая операция «Отметить", "Мониторинг использует итоговое состояние строк журнала. Кнопки «Завершить журнал», «Возобновить журнал» и «Открыть» связывают сводный показатель с исходными данными занятия.")
    set_starts(document, "Завершенный журнал переводится", "Завершенный журнал отображается отдельным состоянием и учитывается в показателе полноты. Доступное действие определяется текущим режимом страницы.")
    set_starts(document, "Навигация фактического приложения", "Навигация приложения включает ведомости, посещения, мониторинг и административный раздел. Исходный конструктор ведомостей остается самостоятельным соседним модулем; прямого обмена строками между ним и посещаемостью пока нет.")
    set_starts(document, "Переход к журналу возможен", "Переход к журналу возможен со страницы мониторинга. Для студента маршрут /attendance/monitoring автоматически выводит только его показатели и историю, а выбор другого студента и административные действия отсутствуют.")
    set_exact(document, "Рисунок 4.1 - Режим просмотра истории студента", "Рисунок 4.1 - Личный мониторинг студента")
    set_exact(document, "4.4 Модуль мониторинга посещаемости", "4.4 Модуль мониторинга и административные операции")
    set_starts(document, "Маршрут /monitoring", "Маршрут /attendance/monitoring открывает отдельную страницу мониторинга. Преподаватель выбирает период и состояние журналов, а студент получает упрощенный личный экран без общих показателей группы.")
    set_starts(document, "Страница показывает полноту", "Страница показывает полноту журналов, посещаемость, число незавершенных занятий и отклонений. Таблица контроля заполнения содержит процент, время обновления и команды перехода, завершения или возобновления.")
    set_starts(document, "Дополнительно отображаются", "Ниже отображается список студентов, требующих внимания. При открытии страницы сразу показана карточка выбранного студента; другого обучающегося можно выбрать в списке. Показатели объясняются количеством занятий, пропусков и опозданий.")
    set_starts(document, "Обновление без перезагрузки", "Тестовая админ-панель позволяет добавлять и удалять пользователей и курсы, создавать назначения на курс, а также формировать и принимать данные пользователей в JSON/CSV.")
    set_starts(document, "Проверка проводилась как ручной", "Проверка проводилась как ручной набор пользовательских и административных сценариев. Проверены фильтры периода, пересчет показателей, переходы к журналу и студенту, а также добавление справочных данных и JSON/CSV обмен.")
    set_starts(document, "Сценарии с HTTP", "Сценарии с HTTP 401/403/409, оптимистической конкуренцией, сетевыми отказами и промышленным аудитом перенесены в направления дальнейшего развития.")
    set_starts(document, "Описан демонстрационный клиентский", "Описан интегрированный клиентский модуль на Blazor WebAssembly: мониторинг периода, контроль заполненности журналов, личные показатели студента, список студентов, требующих внимания, админ-панель и обмен пользователями JSON/CSV. REST API, постоянная база данных и серверная авторизация отнесены к будущему внедрению.")

    set_starts(document, "В работе уточнен проект", "В работе разработан и встроен в omelea-dev-lms модуль мониторинга посещаемости и тестовых административных операций. Исходной точкой стали реализованные журналы; результат превращает их данные в сводное состояние учебного процесса.")
    set_starts(document, "В аналитической части сформулированы", "В аналитической части сформулированы требования к показателям, фильтрам и административным операциям. Выбор Blazor WebAssembly обоснован единой платформой .NET и возможностью использовать визуальный стиль существующей LMS.")
    set_starts(document, "В теоретической части построена", "В теоретической части построена предметная модель студентов, занятий, отметок и сводных показателей. Разделены посещаемость и полнота журналов, определены критерии списка студентов, требующих внимания.")
    set_starts(document, "В инженерной части описана", "В инженерной части описана архитектура Pages, Models, Services, Layout и wwwroot. AttendanceMonitoring.razor использует AttendanceService, а Admin.razor - LmsCatalogService; серверные REST-контракты представлены как проектное развитие.")
    set_starts(document, "Практическая часть описывает структуру", "Практическая часть описывает мониторинг учебных показателей, переходы к журналам, выбор и карточку студента, пользователей, курсы, зачисления и обмен JSON/CSV.")
    set_starts(document, "Дальнейшее развитие включает", "Дальнейшее развитие включает промышленный REST API, серверную базу данных, подписанные токены, аудит административных действий, файловую загрузку CSV/JSON и автоматизированные тесты.")
    set_starts(document, "10. Открыть /monitoring", "10. Открыть /attendance/monitoring, сменить период 7 дней / 30 дней / все время и перейти в конкретный журнал.")

    set_table_cell(document, 4, 4, 1, "Определение требований к обмену данными с сервером, защите пользовательских сессий и проверке доступности операций для выбранного объекта")
    set_table_cell(document, 7, 0, 0, "Пользователь")
    set_table_cell(document, 9, 2, 0, "Доступ к данным")
    set_table_cell(document, 9, 2, 1, "Отображение доступных действий и проверка контекста страницы")
    set_table_cell(document, 9, 2, 2, "Проверка учетной записи, назначения и объекта операции")
    set_table_cell(document, 22, 1, 2, "Участники и границы описаны; расширенные административные операции вынесены в развитие")
    set_table_cell(document, 22, 7, 2, "Проверки статусов и обязательных полей реализованы; версии данных оставлены дальнейшему развитию")

    set_exact(document, "Рисунок 3.1 - Многоуровневая архитектура клиентского модуля", "Рисунок 3.1 - Интеграция мониторинга и администрирования в LMS")
    set_exact(document, "Рисунок 3.2 - Навигационная схема клиентского модуля", "Рисунок 3.2 - Связь существующего конструктора и новых модулей")
    set_exact(document, "Рисунок 3.3 - Прототип сводной панели преподавателя", "Рисунок 3.3 - Реализованный мониторинг преподавателя")
    set_exact(document, "Рисунок 3.4 - Прототип рабочего журнала занятия", "Рисунок 3.4 - Тестовая админ-панель пользователей")

    set_table(document, 19, [
        ["Каталог / файл", "Назначение"],
        ["Pages/AttendanceMonitoring.razor", "Мониторинг преподавателя и студента"],
        ["Pages/Admin.razor", "Пользователи, курсы, зачисления, обмен"],
        ["Services/Attendance", "Расчет сводок и выборок"],
        ["Services/Admin", "Каталог LMS и операции со справочниками"],
        ["Models/Attendance", "Занятия, отметки и показатели"],
        ["Models/LmsAccessModels.cs", "Пользователи, курсы и назначения"],
        ["wwwroot/css", "Общие стили omelea-dev-lms"],
    ])
    set_table(document, 11, [
        ["Модель", "Ключевые данные", "Использование в интерфейсе"],
        ["AttendanceDataModel", "Students, Lessons", "Общее состояние посещаемости текущего сеанса"],
        ["AttendanceStudentModel", "Id, FullName, RecordBookNumber", "Поиск, журнал и история"],
        ["AttendanceLessonModel", "CourseId, Topic, StartsAt, CompletedAt, Entries", "Контроль заполнения и переходы"],
        ["AttendanceEntryModel", "StudentId, Status, LateMinutes, Comment", "Исходные данные показателей"],
        ["AttendanceSummaryModel", "Present, Late, Excused, Absent, Unmarked", "Сводка занятия"],
        ["AttendanceMonitoringSummaryModel", "HeldLessons, CompletedLessons, статусы", "Показатели мониторинга"],
    ])
    set_table(document, 12, [
        ["Состояние", "Признак", "Доступные действия"],
        ["Пустая выборка", "Нет занятий за период", "Сменить период или проверить назначения"],
        ["Готово", "Показатели рассчитаны", "Фильтровать и открывать журнал"],
        ["Черновик", "Журнал не завершен", "Открыть или завершить при наличии права"],
        ["Требует внимания", "Есть неотмеченные строки", "Перейти к журналу"],
        ["Завершен", "Журнал подтвержден", "Просмотреть или возобновить"],
        ["Личный режим", "Текущий пользователь - студент", "Просматривать свою историю"],
        ["Будущая интеграция", "Сетевой API недоступен", "Предусмотрено для серверной версии"],
    ])
    set_table(document, 15, [
        ["Уровень", "Компоненты", "Ответственность"],
        ["Представление", "Pages, Layout, wwwroot/css", "Маршруты, таблицы, фильтры и адаптивность"],
        ["Посещаемость", "AttendanceService", "Журналы, история и расчет показателей"],
        ["Каталоги", "LmsCatalogService", "Пользователи, курсы, зачисления и обмен"],
        ["Модели", "AttendanceModels, LmsAccessModels", "Данные журналов и каталога LMS"],
        ["Будущая интеграция", "REST API, серверная авторизация, БД", "Постоянное хранение и промышленная защита"],
    ])
    set_table(document, 17, [
        ["Предлагаемый метод и маршрут", "Назначение", "Статус в работе"],
        ["GET /api/workspace/lessons", "Получение доступных занятий", "Спроектировано; сейчас AttendanceService"],
        ["GET /api/lessons/{id}/journal", "Получение снимка журнала", "Спроектировано; сейчас состояние в памяти"],
        ["PATCH /api/lessons/{id}/attendance/batch", "Пакетное сохранение", "Спроектировано; сейчас SaveSelectedLesson"],
        ["POST /api/lessons/{id}/complete", "Завершение журнала", "Спроектировано; сейчас CompleteLesson"],
        ["GET /api/attendance/summary", "Сводка за период", "Спроектировано; сейчас AttendanceService"],
    ])
    set_table(document, 20, [
        ["Страница / элемент", "Данные", "Действия"],
        ["/attendance/monitoring", "Журналы и показатели", "Период, фильтр, открыть"],
        ["Контроль заполнения", "Полнота и посещаемость", "Завершить, возобновить"],
        ["Студенты, требующие внимания", "История и отклонения", "Выбрать, открыть карточку"],
        ["Личный мониторинг", "История текущего студента", "Только просмотр"],
        ["/admin", "Пользователи и курсы", "Добавить, удалить"],
        ["Импорт и экспорт", "JSON/CSV пользователей", "Сформировать, загрузить"],
    ])
    set_table(document, 21, [
        ["№", "Проверка", "Ожидаемый результат", "Результат"],
        ["1", "Период 7/30 дней", "Меняется выборка", "Выполнено"],
        ["2", "Фильтр журналов", "Показывается нужное состояние", "Выполнено"],
        ["3", "Завершение/возобновление", "Показатели пересчитываются", "Выполнено"],
        ["4", "Открытие мониторинга", "Сразу выбран студент", "Выполнено"],
        ["5", "Выбор другого студента", "Обновляется карточка", "Выполнено"],
        ["6", "Вход студента", "Только личные показатели", "Выполнено"],
        ["7", "Добавление пользователя", "Запись появляется в каталоге", "Выполнено"],
        ["8", "Добавление курса и назначения", "Данные доступны в списках", "Выполнено"],
        ["9", "JSON/CSV обмен", "Пользователи выгружаются и принимаются", "Выполнено"],
    ])

    replace_image(document, "rId14", ASSETS / "monitoring_integration.png")
    replace_image(document, "rId15", ASSETS / "attendance_integration.png")
    replace_image(document, "rId16", ASSETS / "monitoring_main.png")
    replace_image(document, "rId17", ASSETS / "admin_main.png")
    replace_image(document, "rId19", ASSETS / "student_monitoring.png")

    output = OUTPUT / "ПЗ_ЛоктевМС_обновленная.docx"
    document.save(output)
    return output


def rgb(value: str) -> RGBColor:
    return RGBColor.from_string(value)


def clear_slide(slide) -> None:
    for shape in list(slide.shapes):
        element = shape._element
        element.getparent().remove(element)
    slide.background.fill.solid()
    slide.background.fill.fore_color.rgb = rgb(WHITE)


def add_text(slide, x: float, y: float, w: float, h: float, text: str, size: int = 20, color: str = NAVY, bold: bool = False, align=PP_ALIGN.LEFT, valign=MSO_ANCHOR.MIDDLE, margin: float = 0.08):
    box = slide.shapes.add_textbox(Inches(x), Inches(y), Inches(w), Inches(h))
    frame = box.text_frame
    frame.clear()
    frame.word_wrap = True
    frame.margin_left = Inches(margin)
    frame.margin_right = Inches(margin)
    frame.margin_top = Inches(margin)
    frame.margin_bottom = Inches(margin)
    frame.vertical_anchor = valign
    paragraph = frame.paragraphs[0]
    paragraph.alignment = align
    paragraph.text = text
    for run in paragraph.runs:
        run.font.name = "Arial"
        run.font.size = PptPt(size)
        run.font.bold = bold
        run.font.color.rgb = rgb(color)
    return box


def add_title(slide, title: str, number: int) -> None:
    accent = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, 0, 0, Inches(13.333), Inches(0.14))
    accent.fill.solid()
    accent.fill.fore_color.rgb = rgb(BLUE)
    accent.line.fill.background()
    add_text(slide, 0.65, 0.35, 11.8, 0.6, title, 27, NAVY, True)
    line = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0.65), Inches(1.03), Inches(0.75), Inches(0.06))
    line.fill.solid()
    line.fill.fore_color.rgb = rgb(ORANGE)
    line.line.fill.background()
    add_text(slide, 12.45, 7.02, 0.35, 0.24, str(number), 11, GRAY, False, PP_ALIGN.RIGHT)


def add_card(slide, x: float, y: float, w: float, h: float, title: str, body: str, accent: str = BLUE, title_size: int = 18, body_size: int = 15) -> None:
    shape = slide.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(x), Inches(y), Inches(w), Inches(h))
    shape.fill.solid()
    shape.fill.fore_color.rgb = rgb(LIGHT)
    shape.line.color.rgb = rgb(MID)
    shape.line.width = PptPt(1)
    band = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(x), Inches(y), Inches(0.08), Inches(h))
    band.fill.solid()
    band.fill.fore_color.rgb = rgb(accent)
    band.line.fill.background()
    add_text(slide, x + 0.2, y + 0.13, w - 0.35, 0.45, title, title_size, accent, True)
    add_text(slide, x + 0.2, y + 0.62, w - 0.35, h - 0.75, body, body_size, NAVY, False, valign=MSO_ANCHOR.TOP)


def add_picture(slide, path: Path, x: float, y: float, w: float, h: float, border: bool = True) -> None:
    slide.shapes.add_picture(str(path), Inches(x), Inches(y), Inches(w), Inches(h))
    if border:
        frame = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(x), Inches(y), Inches(w), Inches(h))
        frame.fill.background()
        frame.line.color.rgb = rgb(MID)
        frame.line.width = PptPt(1.2)


def title_slide(slide, title: str, student: str, group: str, supervisor: str) -> None:
    clear_slide(slide)
    slide._element.set("showMasterSp", "0")
    band = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, 0, 0, Inches(13.333), Inches(0.22))
    band.fill.solid()
    band.fill.fore_color.rgb = rgb(BLUE)
    band.line.fill.background()
    add_text(slide, 0.8, 0.55, 11.8, 0.48, "НИЯУ МИФИ · ИИКС · КАФЕДРА №22 «КИБЕРНЕТИКА»", 15, BLUE, True)
    add_text(slide, 0.8, 1.45, 11.5, 2.0, title, 32, NAVY, True, valign=MSO_ANCHOR.TOP)
    accent = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0.8), Inches(3.6), Inches(1.1), Inches(0.08))
    accent.fill.solid()
    accent.fill.fore_color.rgb = rgb(ORANGE)
    accent.line.fill.background()
    add_card(slide, 0.8, 4.15, 3.7, 1.35, "Студент", f"{student}\nГруппа {group}", BLUE)
    add_card(slide, 4.8, 4.15, 3.7, 1.35, "Руководитель", supervisor, PURPLE)
    add_card(slide, 8.8, 4.15, 3.7, 1.35, "Формат", ".NET 8 · Blazor WebAssembly\nИнтеграция в omelea-dev-lms", GREEN)
    add_text(slide, 0.8, 6.65, 11.7, 0.35, "Москва, 2026", 15, GRAY)


def stats_slide(slide, number: int, pages: int, tables: int, figures: int, sources: int, idea: str, stack: str) -> None:
    clear_slide(slide)
    add_title(slide, "Реферат", number)
    labels = [(str(pages), "страниц"), (str(tables), "таблиц"), (str(figures), "рисунков"), (str(sources), "источников")]
    for i, (value, label) in enumerate(labels):
        x = 0.7 + i * 3.1
        add_card(slide, x, 1.35, 2.75, 1.45, value, label, [BLUE, PURPLE, ORANGE, GREEN][i], 28, 15)
    add_card(slide, 0.7, 3.2, 5.95, 2.55, "Ключевая идея", idea, BLUE, 19, 17)
    add_card(slide, 6.85, 3.2, 5.75, 2.55, "Технологии и границы", stack, PURPLE, 19, 17)


def image_slide(slide, title: str, number: int, image_path: Path, caption: str) -> None:
    clear_slide(slide)
    add_title(slide, title, number)
    add_picture(slide, image_path, 0.7, 1.25, 11.95, 5.45)
    label = slide.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(0.95), Inches(6.18), Inches(7.8), Inches(0.42))
    label.fill.solid()
    label.fill.fore_color.rgb = rgb(NAVY)
    label.line.fill.background()
    add_text(slide, 1.08, 6.21, 7.55, 0.3, caption, 12, WHITE, True)


def build_efimenko_presentation() -> Path:
    presentation = Presentation(EF_PPT_SOURCE)
    slides = presentation.slides
    title_slide(slides[0], "Разработка интерфейса учета посещений\nи тестовой ролевой модели в LMS", "Ефименко Н.П.", "Б23-504", "Рословцев В.В.")
    stats_slide(slides[1], 2, 42, 15, 9, 20, "Журнал, личный режим студента и тестовые роли встроены в существующий проект LMS без существенной переработки конструктора ведомостей.", "Blazor WebAssembly · C# · Razor\nAttendanceService · LmsCatalogService\nREST API и БД - следующий этап")

    clear_slide(slides[2]); add_title(slides[2], "Актуальность работы", 3)
    add_card(slides[2], 0.7, 1.35, 2.85, 2.0, "Разрозненные данные", "Состав групп, дисциплины и посещение часто ведутся отдельно.", BLUE)
    add_card(slides[2], 3.75, 1.35, 2.85, 2.0, "Разные права", "Преподаватель изменяет журнал, студент видит только себя.", PURPLE)
    add_card(slides[2], 6.8, 1.35, 2.85, 2.0, "Риск ошибок", "Массовый ввод требует проверок минут, комментариев и состояния.", ORANGE)
    add_card(slides[2], 9.85, 1.35, 2.75, 2.0, "Интеграция", "Новый модуль должен сохранить стиль и структуру чужого проекта.", GREEN)
    add_card(slides[2], 0.7, 3.75, 11.9, 1.7, "Практический эффект", "Единый интерфейс связывает пользователя, назначенные дисциплины, занятие, журнал и личные показатели. Исходный конструктор ведомостей остается самостоятельным и работоспособным.", NAVY, 20, 18)

    clear_slide(slides[3]); add_title(slides[3], "Цель и задачи", 4)
    add_card(slides[3], 0.7, 1.3, 11.9, 1.25, "Цель", "Интегрировать учет посещений и тестовую ролевую модель в omelea-dev-lms.", BLUE, 22, 20)
    tasks = ["Перенести журнал в дизайн LMS", "Формализовать статусы и проверки", "Добавить личный режим студента", "Связать доступ с курсами и ролями", "Сохранить исходный конструктор"]
    for i, task in enumerate(tasks):
        add_card(slides[3], 0.7 + i * 2.42, 3.0, 2.2, 2.05, str(i + 1), task, [BLUE, PURPLE, ORANGE, GREEN, NAVY][i], 25, 15)

    clear_slide(slides[4]); add_title(slides[4], "Результаты анализа", 5)
    add_card(slides[4], 0.7, 1.35, 3.75, 1.65, "Журнал преподавателя", "Быстрые отметки, поиск, фильтр, сортировка и массовая операция.", BLUE)
    add_card(slides[4], 4.8, 1.35, 3.75, 1.65, "Личный режим", "История и показатели только текущего студента, без редактирования.", GREEN)
    add_card(slides[4], 8.9, 1.35, 3.7, 1.65, "Тестовый доступ", "Роль и назначение на курс определяют доступные данные и действия.", PURPLE)
    add_card(slides[4], 0.7, 3.45, 11.9, 1.85, "Выбранное решение", "AttendanceService хранит состояние журналов, LmsCatalogService связывает пользователей с курсами, AuthSessionService определяет текущую учетную запись. Серверная проверка остается этапом развития.", NAVY, 20, 18)

    clear_slide(slides[5]); add_title(slides[5], "Личный вклад автора", 6)
    add_card(slides[5], 0.7, 1.35, 5.75, 4.55, "Ефименко Н.П.", "• журнал посещаемости;\n• статусы, минуты и комментарии;\n• сохранение и отмена черновика;\n• личный режим студента;\n• демонстрационные входы;\n• тестовая ролевая модель;\n• фильтрация дисциплин и операций.", BLUE, 22, 19)
    add_card(slides[5], 6.85, 1.35, 5.75, 4.55, "Смежная часть проекта", "М.С. Локтев:\n• мониторинг журналов;\n• показатели и фильтры периода;\n• студенты, требующие внимания;\n• админ-панель справочников;\n• импорт и экспорт пользователей.\n\nЭти функции используются при общей интеграции.", PURPLE, 22, 18)

    image_slide(slides[6], "Тестовая ролевая модель", 7, ASSETS / "role_model.png", "Роль вычисляется для конкретного пользователя и курса")

    clear_slide(slides[7]); add_title(slides[7], "Правила обработки отметок", 8)
    add_card(slides[7], 0.7, 1.45, 3.75, 2.0, "Опоздание", "Требуется положительное число минут от 1 до 180.", ORANGE)
    add_card(slides[7], 4.8, 1.45, 3.75, 2.0, "Уважительная причина", "Требуется поясняющий комментарий.", BLUE)
    add_card(slides[7], 8.9, 1.45, 3.7, 2.0, "Завершение", "Неотмеченные строки и несохраненный черновик блокируют действие.", RED)
    add_card(slides[7], 0.7, 3.95, 11.9, 1.55, "Поведение интерфейса", "Преподаватель получает элементы редактирования и массовые команды. Студент видит те же исходные данные через личную историю, но не может изменять журнал.", NAVY, 20, 18)

    image_slide(slides[8], "Интеграция в существующую LMS", 9, ASSETS / "attendance_integration.png", "Конструктор ведомостей сохранен как соседний модуль")
    image_slide(slides[9], "Демонстрационные учетные записи", 10, ASSETS / "login.png", "Администратор · преподаватель · студент")
    image_slide(slides[10], "Журнал преподавателя", 11, ASSETS / "teacher_attendance.png", "Редактирование доступно только для назначенной дисциплины")
    image_slide(slides[11], "Личный режим студента", 12, ASSETS / "student_attendance.png", "Только собственная история и показатели")

    clear_slide(slides[12]); add_title(slides[12], "Результаты и ограничения", 13)
    add_card(slides[12], 0.7, 1.35, 5.75, 4.4, "Реализовано", "• интеграция в дизайн LMS;\n• журнал и статусы;\n• поиск, фильтры и массовые действия;\n• личный режим студента;\n• тестовые входы и роли;\n• доступ по назначенным курсам;\n• сохранение конструктора ведомостей.", GREEN, 22, 18)
    add_card(slides[12], 6.85, 1.35, 5.75, 4.4, "Следующий этап", "• серверная авторизация;\n• подписанные токены;\n• REST API и база данных;\n• аудит изменений;\n• конфликты параллельных версий;\n• прямая синхронизация с ведомостями;\n• автоматизированные тесты.", ORANGE, 22, 18)

    clear_slide(slides[13]); add_title(slides[13], "Список литературы", 14)
    sources = "1. Microsoft. ASP.NET Core Blazor documentation.\n\n2. Microsoft. ASP.NET Core Razor components.\n\n3. Microsoft. Blazor WebAssembly security.\n\n4. Bray T. The JavaScript Object Notation (JSON) Data Interchange Format. RFC 8259.\n\n5. Нильсен Я. Веб-дизайн: удобство использования веб-сайтов."
    add_card(slides[13], 0.8, 1.35, 11.75, 4.95, "Основные источники", sources, BLUE, 20, 18)

    clear_slide(slides[14])
    add_text(slides[14], 0.8, 2.35, 11.7, 1.1, "Спасибо за внимание!", 38, NAVY, True, PP_ALIGN.CENTER)
    add_text(slides[14], 0.8, 3.55, 11.7, 0.5, "Ефименко Н.П. · Б23-504", 18, BLUE, True, PP_ALIGN.CENTER)
    add_text(slides[14], 0.8, 5.85, 11.7, 0.35, "Учет посещений · тестовая ролевая модель · интеграция в LMS", 15, GRAY, False, PP_ALIGN.CENTER)

    output = OUTPUT / "Презентация_ЕфименкоНП_обновленная.pptx"
    presentation.save(output)
    return output


def build_loktev_presentation() -> Path:
    presentation = Presentation(LOK_PPT_SOURCE)
    slides = presentation.slides
    title_slide(slides[0], "Клиентский модуль мониторинга\nи администрирования посещаемости", "Локтев М.С.", "Б23-504", "Рословцев В.В.")
    stats_slide(slides[1], 2, 36, 16, 9, 20, "Мониторинг связывает сводные показатели с исходными журналами и дополняется тестовой админ-панелью пользователей, курсов и зачислений.", "Blazor WebAssembly · C# · Razor\nAttendanceService · LmsCatalogService\nJSON/CSV · адаптивные CSS-стили")

    clear_slide(slides[2]); add_title(slides[2], "Актуальность", 3)
    add_card(slides[2], 0.7, 1.35, 2.85, 2.0, "Незаполненные журналы", "Сводные данные нельзя считать надежными без контроля полноты.", ORANGE)
    add_card(slides[2], 3.75, 1.35, 2.85, 2.0, "Нет общей картины", "Отдельные занятия не показывают динамику за период.", BLUE)
    add_card(slides[2], 6.8, 1.35, 2.85, 2.0, "Поздняя реакция", "Требуется список студентов с пропусками и опозданиями.", RED)
    add_card(slides[2], 9.85, 1.35, 2.75, 2.0, "Каталоги", "Показатели зависят от корректных курсов и зачислений.", GREEN)
    add_card(slides[2], 0.7, 3.8, 11.9, 1.65, "Требование", "Мониторинг должен объяснять каждый показатель и давать переход к журналу или карточке студента; админ-панель подготавливает справочные данные для расчетов.", NAVY, 20, 18)

    clear_slide(slides[3]); add_title(slides[3], "Цель и границы работы", 4)
    add_card(slides[3], 0.7, 1.3, 11.9, 1.25, "Цель", "Встроить в LMS мониторинг посещаемости и тестовые административные операции.", PURPLE, 22, 20)
    add_card(slides[3], 0.7, 3.0, 3.7, 2.1, "Мониторинг", "Периоды, фильтры, полнота, посещаемость, отклонения и переходы.", BLUE)
    add_card(slides[3], 4.8, 3.0, 3.7, 2.1, "Студенты", "Выбранная карточка, список требующих внимания и личные показатели.", ORANGE)
    add_card(slides[3], 8.9, 3.0, 3.7, 2.1, "Администрирование", "Пользователи, курсы, зачисления и обмен JSON/CSV.", GREEN)

    clear_slide(slides[4]); add_title(slides[4], "Личный вклад и границы", 5)
    add_card(slides[4], 0.7, 1.35, 5.75, 4.55, "Локтев М.С.", "• показатели мониторинга;\n• фильтры периода и журналов;\n• контроль заполнения;\n• студенты, требующие внимания;\n• выбор и карточка студента;\n• админ-панель каталогов;\n• JSON/CSV обмен пользователей.", PURPLE, 22, 18)
    add_card(slides[4], 6.85, 1.35, 5.75, 4.55, "Границы прототипа", "• данные хранятся в памяти клиента;\n• промышленный REST API не подключен;\n• постоянная база данных отсутствует;\n• аудит операций не реализован;\n• автоматизированные тесты оставлены дальнейшим развитием.", BLUE, 22, 18)

    clear_slide(slides[5]); add_title(slides[5], "Функции прототипа", 6)
    functions = [("Период", "7 / 30 дней / все время", BLUE), ("Состояние", "завершенные и черновики", PURPLE), ("Показатели", "полнота и посещаемость", GREEN), ("Отклонения", "пропуски и опоздания", ORANGE), ("Переходы", "к журналу и студенту", NAVY), ("Каталоги", "пользователи и курсы", BLUE)]
    for i, (title, body, color) in enumerate(functions):
        x = 0.7 + (i % 3) * 4.1
        y = 1.35 + (i // 3) * 2.35
        add_card(slides[5], x, y, 3.75, 1.85, title, body, color)

    clear_slide(slides[6]); add_title(slides[6], "Предметная модель", 7)
    add_card(slides[6], 0.7, 1.45, 2.8, 2.0, "Students", "ФИО и зачетная книжка", BLUE)
    add_card(slides[6], 3.75, 1.45, 2.8, 2.0, "Courses", "Дисциплина, группа, аудитория", PURPLE)
    add_card(slides[6], 6.8, 1.45, 2.8, 2.0, "Lessons", "Дата, тема и состояние", ORANGE)
    add_card(slides[6], 9.85, 1.45, 2.75, 2.0, "Entries", "Статус, минуты, комментарий", GREEN)
    add_card(slides[6], 0.7, 3.95, 5.75, 1.5, "MonitoringSummary", "Полнота журналов · посещаемость · незавершенные · отклонения", NAVY, 18, 17)
    add_card(slides[6], 6.85, 3.95, 5.75, 1.5, "Catalog", "Пользователи, курсы и зачисления для административной страницы", BLUE, 18, 17)

    image_slide(slides[7], "Архитектура интеграции", 8, ASSETS / "monitoring_integration.png", "Мониторинг и админ-панель используют общие сервисы LMS")
    image_slide(slides[8], "Мониторинг журналов", 9, ASSETS / "monitoring_main.png", "Полнота · посещаемость · незавершенные · отклонения")
    image_slide(slides[9], "Студенты, требующие внимания", 10, ASSETS / "monitoring_attention.png", "При открытии сразу показан выбранный студент")
    image_slide(slides[10], "Тестовая админ-панель", 11, ASSETS / "admin_main.png", "Пользователи и обмен JSON/CSV")
    image_slide(slides[11], "Курсы и зачисления", 12, ASSETS / "admin_courses.png", "Управление справочниками для демонстрационных сценариев")

    clear_slide(slides[12]); add_title(slides[12], "Проверка и развитие", 13)
    add_card(slides[12], 0.7, 1.35, 5.75, 4.4, "Проверено", "• смена периода и фильтра;\n• пересчет показателей;\n• завершение и возобновление;\n• выбор студента;\n• личный мониторинг;\n• пользователи и курсы;\n• JSON/CSV обмен.", GREEN, 22, 18)
    add_card(slides[12], 6.85, 1.35, 5.75, 4.4, "Дальнейшее развитие", "• REST API;\n• серверная база данных;\n• файловая загрузка и выгрузка;\n• аудит администратора;\n• история изменения каталогов;\n• автоматизированные тесты;\n• экспорт аналитических отчетов.", ORANGE, 22, 18)

    clear_slide(slides[13]); add_title(slides[13], "Результаты работы", 14)
    add_card(slides[13], 0.7, 1.35, 3.75, 3.9, "Мониторинг", "Реализованы показатели, фильтры, состояния журналов и переход к исходным данным.", PURPLE, 22, 19)
    add_card(slides[13], 4.8, 1.35, 3.75, 3.9, "Работа со студентами", "Сразу показана карточка выбранного студента; доступна смена выбора и личный режим.", BLUE, 22, 19)
    add_card(slides[13], 8.9, 1.35, 3.7, 3.9, "Администрирование", "Добавлены пользователи, курсы, назначения и обмен данными в JSON/CSV.", GREEN, 22, 19)
    add_text(slides[13], 0.9, 5.65, 11.4, 0.55, "Серверное хранение и промышленная авторизация остаются направлениями развития.", 17, RED, True, PP_ALIGN.CENTER)

    clear_slide(slides[14]); add_title(slides[14], "Список литературы", 15)
    sources = "1. Microsoft. ASP.NET Core Blazor documentation.\n\n2. Microsoft. ASP.NET Core Razor components.\n\n3. Microsoft. Call a web API from a Blazor app.\n\n4. Bray T. The JavaScript Object Notation (JSON) Data Interchange Format. RFC 8259.\n\n5. Нильсен Я. Веб-дизайн: удобство использования веб-сайтов."
    add_card(slides[14], 0.8, 1.35, 11.75, 4.95, "Основные источники", sources, PURPLE, 20, 18)

    clear_slide(slides[15])
    add_text(slides[15], 0.8, 2.35, 11.7, 1.1, "Спасибо за внимание!", 38, NAVY, True, PP_ALIGN.CENTER)
    add_text(slides[15], 0.8, 3.55, 11.7, 0.5, "Локтев М.С. · Б23-504", 18, PURPLE, True, PP_ALIGN.CENTER)
    add_text(slides[15], 0.8, 5.85, 11.7, 0.35, "Мониторинг · показатели · администрирование LMS", 15, GRAY, False, PP_ALIGN.CENTER)

    output = OUTPUT / "Презентация_ЛоктевМС_обновленная.pptx"
    presentation.save(output)
    return output


def write_notes() -> None:
    contributions = """# Распределение задач для отчета

## Ефименко Н.П.

- интеграция журнала посещаемости в omelea-dev-lms;
- статусы посещения, минуты опоздания, комментарии и проверки;
- поиск, фильтрация, массовая отметка, сохранение и отмена черновика;
- личный режим студента без доступа к редактированию;
- демонстрационные входы администратора, преподавателя и студента;
- тестовая ролевая модель, назначения на курсы и проверки доступных операций;
- сохранение исходного конструктора ведомостей как соседнего модуля.

## Локтев М.С.

- мониторинг полноты журналов и посещаемости за период;
- фильтрация завершенных, незавершенных и требующих внимания журналов;
- список студентов, требующих внимания, и карточка выбранного студента;
- личный экран показателей студента в мониторинге;
- тестовая админ-панель пользователей, курсов и зачислений;
- импорт и экспорт пользователей в JSON/CSV;
- переходы из мониторинга к журналу и выбранному студенту.

Распределение относится только к отчетным материалам. В исходном коде искусственное разделение по авторам не вводилось.
"""
    discrepancies = """# Расхождения отчетов с текущей реализацией

1. Ролевая модель является тестовой клиентской моделью. Демонстрационные токены не подписываются и не проверяются сервером, поэтому это не промышленная авторизация.
2. Пользователи, курсы, назначения и журналы хранятся в памяти клиентского приложения. Постоянной базы данных и восстановления после полного перезапуска нет.
3. Импорт и экспорт JSON/CSV реализован через текстовое поле админ-панели. Настоящего выбора файла, скачивания файла и серверной загрузки пока нет.
4. Конструктор ведомостей и посещаемость объединены общим интерфейсом, навигацией и каталогом LMS, но прямой передачи студентов или итогов из ведомости в журнал пока нет.
5. REST API, HTTP 401/403/409, аудит, конфликты версий и сетевое восстановление в записках описываются как проектные решения или направления развития, а не как готовый код.
6. MudBlazor анализировался как вариант, но фактический интерфейс построен на стандартных Razor-компонентах и собственном CSS omelea-dev-lms.
7. Проверка выполнена сборкой, ручными сценариями и визуальным просмотром. Полного набора автоматизированных тестов нет.
8. Админ-панель импортирует и экспортирует только пользователей. Курсы и зачисления редактируются вручную в текущем сеансе.
9. Все ФИО и показатели в интерфейсе демонстрационные и не представляют реальных студентов или реальную статистику.
"""
    (OUTPUT / "Распределение_задач.md").write_text(contributions, encoding="utf-8")
    (OUTPUT / "Памятка_по_расхождениям.md").write_text(discrepancies, encoding="utf-8")


def main() -> None:
    OUTPUT.mkdir(parents=True, exist_ok=True)
    prepare_assets()
    update_efimenko_report()
    update_loktev_report()
    build_efimenko_presentation()
    build_loktev_presentation()
    write_notes()
    print(OUTPUT)


if __name__ == "__main__":
    main()
