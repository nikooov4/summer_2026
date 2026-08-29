from __future__ import annotations

from pathlib import Path

from docx import Document
from docx.enum.section import WD_SECTION
from docx.enum.table import WD_CELL_VERTICAL_ALIGNMENT, WD_TABLE_ALIGNMENT
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_BREAK
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Cm, Pt


ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / "Готовые_материалы"


def configure_document(document: Document) -> None:
    section = document.sections[0]
    section.page_width = Cm(21)
    section.page_height = Cm(29.7)
    section.left_margin = Cm(2)
    section.right_margin = Cm(1.5)
    section.top_margin = Cm(1.5)
    section.bottom_margin = Cm(1.5)

    normal = document.styles["Normal"]
    normal.font.name = "Times New Roman"
    normal.font.size = Pt(12)
    normal._element.rPr.rFonts.set(qn("w:eastAsia"), "Times New Roman")


def format_run(run, size: float = 12, bold: bool = False, underline: bool = False) -> None:
    run.font.name = "Times New Roman"
    run._element.rPr.rFonts.set(qn("w:eastAsia"), "Times New Roman")
    run.font.size = Pt(size)
    run.font.bold = bold
    run.font.underline = underline


def add_centered(document: Document, text: str = "", size: float = 12, bold: bool = False, before: float = 0, after: float = 0):
    paragraph = document.add_paragraph()
    paragraph.alignment = WD_ALIGN_PARAGRAPH.CENTER
    paragraph.paragraph_format.space_before = Pt(before)
    paragraph.paragraph_format.space_after = Pt(after)
    run = paragraph.add_run(text)
    format_run(run, size, bold)
    return paragraph


def add_heading(document: Document, text: str) -> None:
    paragraph = document.add_paragraph()
    paragraph.paragraph_format.space_before = Pt(4)
    paragraph.paragraph_format.space_after = Pt(4)
    run = paragraph.add_run(text)
    format_run(run, 13, True)


def add_field(document: Document, number: int, label: str, value: str = "", size: float = 10.5) -> None:
    paragraph = document.add_paragraph()
    paragraph.paragraph_format.space_after = Pt(1)
    paragraph.paragraph_format.line_spacing = 1.0
    label_run = paragraph.add_run(f"{number}. {label} ")
    format_run(label_run, size)
    if value:
        value_run = paragraph.add_run(value)
        format_run(value_run, size, underline=True)
    else:
        blank = paragraph.add_run("____________________________________________")
        format_run(blank, size)


def shade_cell(cell, color: str) -> None:
    properties = cell._tc.get_or_add_tcPr()
    shade = OxmlElement("w:shd")
    shade.set(qn("w:fill"), color)
    properties.append(shade)


def set_cell(cell, text: str, size: float = 8.5, bold: bool = False, center: bool = False) -> None:
    cell.text = ""
    cell.vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.CENTER
    paragraph = cell.paragraphs[0]
    paragraph.alignment = WD_ALIGN_PARAGRAPH.CENTER if center else WD_ALIGN_PARAGRAPH.LEFT
    paragraph.paragraph_format.space_after = Pt(0)
    paragraph.paragraph_format.line_spacing = 1.0
    run = paragraph.add_run(text)
    format_run(run, size, bold)


def set_cell_width(cell, width_cm: float) -> None:
    cell.width = Cm(width_cm)
    properties = cell._tc.get_or_add_tcPr()
    width = properties.first_child_found_in("w:tcW")
    if width is None:
        width = OxmlElement("w:tcW")
        properties.append(width)
    width.set(qn("w:w"), str(int(width_cm * 567)))
    width.set(qn("w:type"), "dxa")


def add_assignment_table(document: Document, tasks: list[tuple[str, str, str]]) -> None:
    table = document.add_table(rows=1, cols=5)
    table.style = "Table Grid"
    table.alignment = WD_TABLE_ALIGNMENT.CENTER
    table.autofit = False
    widths = [0.8, 8.0, 2.7, 2.1, 3.9]
    headers = ["№\nп/п", "Содержание работы", "Форма\nотчетности", "Срок\nисполнения", "Отметка о выполнении,\nдата и подпись рук."]
    for index, (cell, header) in enumerate(zip(table.rows[0].cells, headers)):
        set_cell_width(cell, widths[index])
        set_cell(cell, header, 8, True, True)
        shade_cell(cell, "E7E9ED")

    for number, (content, form, deadline) in enumerate(tasks, start=1):
        row = table.add_row()
        values = [str(number), content, form, deadline, ""]
        for index, (cell, value) in enumerate(zip(row.cells, values)):
            set_cell_width(cell, widths[index])
            set_cell(cell, value, 8.2, False, index in (0, 2, 3, 4))


def add_signature_line(document: Document, role: str) -> None:
    table = document.add_table(rows=1, cols=3)
    table.alignment = WD_TABLE_ALIGNMENT.CENTER
    table.autofit = False
    widths = [6.3, 5.0, 6.0]
    values = [role, "________________", "____________________________"]
    for index, (cell, value) in enumerate(zip(table.rows[0].cells, values)):
        set_cell_width(cell, widths[index])
        set_cell(cell, value, 10, False, index != 0)
        cell._tc.get_or_add_tcPr().append(OxmlElement("w:tcBorders"))


def add_blank_lines(document: Document, count: int, width: int = 102) -> None:
    for _ in range(count):
        paragraph = document.add_paragraph("_" * width)
        paragraph.paragraph_format.space_after = Pt(1)
        paragraph.paragraph_format.line_spacing = 0.8
        for run in paragraph.runs:
            format_run(run, 9)


def add_page_break(document: Document) -> None:
    paragraph = document.add_paragraph()
    paragraph.add_run().add_break(WD_BREAK.PAGE)


def build_diary(
    short_name: str,
    full_name: str,
    title_name: str,
    tasks: list[tuple[str, str, str]],
    diploma_topic: str,
    conclusion: str,
) -> Path:
    document = Document()
    configure_document(document)
    document.core_properties.title = f"Дневник преддипломной практики {short_name}"
    document.core_properties.author = short_name

    add_centered(document, "МИНИСТЕРСТВО ОБРАЗОВАНИЯ И НАУКИ", 11, True)
    add_centered(document, "РОССИЙСКОЙ ФЕДЕРАЦИИ", 11, True)
    add_centered(document, "Федеральное государственное автономное образовательное учреждение", 10, False, before=10)
    add_centered(document, "высшего образования", 10)
    add_centered(document, "«Национальный исследовательский ядерный университет «МИФИ»", 11, True)

    add_centered(document, "ДНЕВНИК", 22, True, before=85, after=8)
    add_centered(document, "преддипломной практики", 16, True, after=28)
    add_centered(document, "студента группы Б23-504", 14)
    add_centered(document, title_name, 15, True, before=5)
    add_centered(document, "(фамилия, инициалы)", 9)
    add_centered(document, "Москва 2026 г.", 12, False, before=120)

    add_page_break(document)
    add_heading(document, "1. Общие сведения")
    surname, name, patronymic = full_name.split(maxsplit=2)
    add_field(document, 1, "Фамилия", surname)
    add_field(document, 2, "Имя, отчество", f"{name} {patronymic}")
    add_field(document, 3, "Группа", "Б23-504")
    add_field(document, 4, "Специальность (код)", "09.03.04")
    add_field(document, 5, "Предприятие", "НИЯУ МИФИ")
    add_field(document, 6, "Руководитель практики", "Рословцев Владимир Владимирович, старший преподаватель")
    add_field(document, 7, "Ответственный за производственную практику на кафедре")
    add_field(document, 8, "Сроки практики по учебному плану", "с 26.06.2026 по 09.07.2026")
    add_field(document, 9, "Дата выезда из НИЯУ МИФИ")
    add_field(document, 10, "Дата прибытия на место прохождения практики")
    add_field(document, 11, "Назначен на должность и приступил к работе")
    add_field(document, 12, "Переведен на должность")
    add_field(document, 13, "Дата выезда с места прохождения практики")
    add_field(document, 14, "Дата прибытия в НИЯУ МИФИ")

    add_heading(document, "2. Индивидуальное задание студента по производственной практике")
    add_assignment_table(document, tasks)

    add_page_break(document)
    add_heading(document, "Ориентировочная тема дипломного проекта")
    paragraph = document.add_paragraph()
    paragraph.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
    run = paragraph.add_run(diploma_topic)
    format_run(run, 12, False, True)
    add_blank_lines(document, 2)
    add_signature_line(document, "Руководитель практики")
    add_centered(document, "«_____» __________________ 2026 г.", 10, before=6, after=8)

    add_heading(document, "3. Заключение студента по итогам практики и его предложения по содержанию практики")
    paragraph = document.add_paragraph()
    paragraph.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
    paragraph.paragraph_format.first_line_indent = Cm(1.25)
    paragraph.paragraph_format.line_spacing = 1.15
    run = paragraph.add_run(conclusion)
    format_run(run, 11)
    add_centered(document, "Подпись ____________________    «_____» __________________ 2026 г.", 10, before=8, after=8)

    add_heading(document, "4. Производственная характеристика студента")
    add_blank_lines(document, 6)
    paragraph = document.add_paragraph("Указывается степень теоретической и практической подготовки студента, качество выполненной работы, трудовая дисциплина и недостатки, если они имели место; в конце характеристики дается оценка за практику.")
    paragraph.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
    for run in paragraph.runs:
        format_run(run, 9)
    add_signature_line(document, "Руководитель практики")
    add_centered(document, "«_____» __________________ 2026 г.", 10, before=4)

    add_page_break(document)
    add_heading(document, "5. Заключение комиссии по результатам защиты по практике")
    add_blank_lines(document, 10)

    table = document.add_table(rows=5, cols=2)
    table.alignment = WD_TABLE_ALIGNMENT.CENTER
    table.autofit = False
    roles = ["Председатель комиссии", "Член комиссии", "Член комиссии", "Член комиссии", "Член комиссии"]
    for row, role in zip(table.rows, roles):
        set_cell_width(row.cells[0], 6.0)
        set_cell_width(row.cells[1], 11.0)
        set_cell(row.cells[0], role, 11)
        set_cell(row.cells[1], "________________ / ________________________________", 11, False, True)

    add_centered(document, "Итоговая оценка ____________________", 12, True, before=20, after=12)
    add_centered(document, "«_____» __________________ 2026 г.", 11)

    output = OUTPUT / f"Дневник_летней_практики_{short_name.replace(' ', '')}.docx"
    document.save(output)
    return output


def main() -> None:
    OUTPUT.mkdir(parents=True, exist_ok=True)

    efimenko_tasks = [
        (
            "Анализ исходных проектов uir-main и omelea-dev-lms, определение границ переноса журнала посещаемости и требований к ролевому отображению.",
            "Аналитический обзор, разделы отчета",
            "28.06.2026",
        ),
        (
            "Интеграция страницы журнала посещаемости в дизайн LMS; реализация выбора занятия, статусов, минут опоздания, комментариев и проверок.",
            "Исходный код, интерфейс",
            "02.07.2026",
        ),
        (
            "Реализация личного режима студента и тестовой ролевой модели для администратора, преподавателя и студента с ограничением доступных дисциплин.",
            "Исходный код, сценарии",
            "06.07.2026",
        ),
        (
            "Проверка сценариев входа, редактирования и просмотра; контроль работоспособности исходного конструктора ведомостей после интеграции.",
            "Протокол проверки",
            "08.07.2026",
        ),
        (
            "Оформление отчета, дневника и иллюстративных материалов для доклада.",
            "Отчет, дневник, презентация",
            "09.07.2026",
        ),
    ]
    efimenko_conclusion = (
        "В ходе практики был выполнен анализ проектов uir-main и omelea-dev-lms и определен способ переноса учета посещаемости без существенной переработки исходного конструктора ведомостей. "
        "В общий интерфейс LMS встроен журнал преподавателя с выбором занятия, статусами, минутами опоздания, комментариями, поиском, фильтрацией и массовой отметкой. "
        "Реализован личный режим студента без доступа к редактированию, а также тестовая ролевая модель для администратора, преподавателя и студента с учетом назначенных дисциплин. "
        "Проведена ручная проверка основных сценариев и подготовлены отчетные материалы. Полученный прототип может быть развит подключением серверной авторизации, REST API и постоянной базы данных."
    )
    build_diary(
        "ЕфименкоНП",
        "Ефименко Николай Павлович",
        "Ефименко Н.П.",
        efimenko_tasks,
        "Разработка подсистемы учета посещаемости и разграничения доступа в образовательной LMS",
        efimenko_conclusion,
    )

    loktev_tasks = [
        (
            "Анализ требований к мониторингу посещаемости, показателям полноты журналов и административным операциям в существующей LMS.",
            "Аналитический обзор, разделы отчета",
            "28.06.2026",
        ),
        (
            "Реализация страницы мониторинга: периоды, фильтры состояния журналов, показатели полноты, посещаемости и отклонений.",
            "Исходный код, интерфейс",
            "02.07.2026",
        ),
        (
            "Реализация списка студентов, требующих внимания, карточки выбранного студента и личного экрана показателей.",
            "Исходный код, сценарии",
            "05.07.2026",
        ),
        (
            "Разработка тестовой админ-панели пользователей, курсов и зачислений; добавление обмена пользователями в JSON/CSV.",
            "Исходный код, контрольные данные",
            "08.07.2026",
        ),
        (
            "Проверка мониторинга и административных сценариев; оформление отчета, дневника и презентации.",
            "Протокол проверки, отчетные материалы",
            "09.07.2026",
        ),
    ]
    loktev_conclusion = (
        "В ходе практики были проанализированы сценарии контроля посещаемости и требования к объяснимым показателям учебного процесса. "
        "В существующий проект LMS встроена страница мониторинга с выбором периода, фильтрами журналов, показателями полноты и посещаемости, а также переходами к исходным данным. "
        "Добавлены список студентов, требующих внимания, карточка выбранного студента и личный экран показателей. "
        "Разработана тестовая админ-панель для управления пользователями, курсами и зачислениями, предусмотрен обмен списком пользователей в JSON/CSV. "
        "Проведена ручная проверка основных сценариев и подготовлены отчетные материалы."
    )
    build_diary(
        "ЛоктевМС",
        "Локтев Максим Сергеевич",
        "Локтева М.С.",
        loktev_tasks,
        "Разработка модуля мониторинга и администрирования посещаемости в образовательной LMS",
        loktev_conclusion,
    )


if __name__ == "__main__":
    main()
