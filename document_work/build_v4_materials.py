from __future__ import annotations

from pathlib import Path

from docx import Document
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml import OxmlElement
from docx.shared import Cm
from pptx import Presentation
from pptx.enum.text import MSO_ANCHOR, PP_ALIGN
from pptx.util import Inches

import build_efimenko_v2 as base
import build_v3_materials as v3


ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / "Готовые_материалы" / "Летняя_практика" / "v4"
EF_OUTPUT = OUTPUT / "Ефименко"
LOK_OUTPUT = OUTPUT / "Локтев"
ASSETS = ROOT / "document_work" / "generated_assets"
SCREENS = ROOT / "document_work" / "screenshots"


COMMON_SOURCES = [
    "Соммервилл И. Инженерия программного обеспечения. 10-е изд. М.: Вильямс, 2021.",
    "Pressman R. S., Maxim B. R. Software Engineering: A Practitioner's Approach. 9th ed. New York: McGraw-Hill Education, 2020.",
    "Fowler M. Refactoring: Improving the Design of Existing Code. 2nd ed. Boston: Addison-Wesley, 2018.",
    "Bourque P., Fairley R. E. Guide to the Software Engineering Body of Knowledge (SWEBOK Guide). Version 4.0. IEEE Computer Society, 2024.",
    "ISO/IEC/IEEE 12207:2017. Systems and software engineering - Software life cycle processes. Geneva: ISO, 2017.",
    "ISO/IEC/IEEE 14764:2022. Software Engineering - Software Life Cycle Processes - Maintenance. Geneva: ISO, 2022.",
    "ASP.NET Core Blazor [Электронный ресурс] // Microsoft Learn. URL: https://learn.microsoft.com/aspnet/core/blazor/ (дата обращения: 30.08.2026).",
    "ASP.NET Core Razor components [Электронный ресурс] // Microsoft Learn. URL: https://learn.microsoft.com/aspnet/core/blazor/components/ (дата обращения: 30.08.2026).",
    "Dependency injection in ASP.NET Core [Электронный ресурс] // Microsoft Learn. URL: https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection/ (дата обращения: 30.08.2026).",
    "ASP.NET Core Blazor state management overview [Электронный ресурс] // Microsoft Learn. URL: https://learn.microsoft.com/aspnet/core/blazor/state-management/ (дата обращения: 30.08.2026).",
    "C# documentation [Электронный ресурс] // Microsoft Learn. URL: https://learn.microsoft.com/dotnet/csharp/ (дата обращения: 30.08.2026).",
]

EF_SOURCES = COMMON_SOURCES + [
    "Bray T. The JavaScript Object Notation (JSON) Data Interchange Format // RFC 8259. URL: https://www.rfc-editor.org/info/rfc8259/ (дата обращения: 30.08.2026).",
    "Jones M., Bradley J., Sakimura N. JSON Web Token (JWT) // RFC 7519. URL: https://www.rfc-editor.org/info/rfc7519/ (дата обращения: 30.08.2026).",
    "Authorization Cheat Sheet [Электронный ресурс] // OWASP Cheat Sheet Series. URL: https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html (дата обращения: 30.08.2026).",
    "Нильсен Я. Веб-дизайн: удобство использования веб-сайтов. М.: Вильямс, 2006. 368 с.",
]

LOK_SOURCES = COMMON_SOURCES + [
    "Bray T. The JavaScript Object Notation (JSON) Data Interchange Format // RFC 8259. URL: https://www.rfc-editor.org/info/rfc8259/ (дата обращения: 30.08.2026).",
    "Shafranovich Y. Common Format and MIME Type for Comma-Separated Values (CSV) Files // RFC 4180. URL: https://www.rfc-editor.org/info/rfc4180/ (дата обращения: 30.08.2026).",
    "Authorization Cheat Sheet [Электронный ресурс] // OWASP Cheat Sheet Series. URL: https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html (дата обращения: 30.08.2026).",
    "Нильсен Я. Веб-дизайн: удобство использования веб-сайтов. М.: Вильямс, 2006. 368 с.",
]


EF_TASKS = [
    (
        "Проведение сравнительного анализа архитектуры исходного прототипа uir-main и целевого Blazor WebAssembly-приложения omelea-dev-lms с определением переносимых компонентов, зависимостей и границ интеграции без изменения базового конструктора ведомостей.",
        "Аналитический обзор, схема интеграции, раздел отчета",
        "28.06.2026",
    ),
    (
        "Проектирование предметной модели электронного учета посещаемости и механизма управления состоянием занятий, студентов и отметок с обеспечением согласованности интерфейса и подтвержденных данных текущего сеанса.",
        "Модель данных, исходный код, раздел отчета",
        "30.06.2026",
    ),
    (
        "Разработка и интеграция журнала занятия преподавателя, включающего переключение контекста дисциплины, статусы посещения, фиксацию длительности опоздания, комментарии, поиск, фильтрацию и групповые операции.",
        "Razor-компонент, сервис, стили, иллюстрации",
        "03.07.2026",
    ),
    (
        "Разработка правил предметной валидации и жизненного цикла журнала, персонализированного режима обучающегося и тестового механизма разграничения доступных дисциплин и пользовательских операций.",
        "Исходный код, матрица доступа, сценарии проверки",
        "07.07.2026",
    ),
    (
        "Проведение апробации реализованного модуля на сценариях преподавателя и студента, анализ ограничений клиентского прототипа и подготовка отчета, презентации и иллюстративных материалов.",
        "Протокол апробации, отчет, презентация",
        "09.07.2026",
    ),
]

LOK_TASKS = [
    (
        "Проведение анализа предметных требований к контролю заполнения электронных журналов и определение состава агрегированных показателей, необходимых для оценки посещаемости по занятиям, периодам и обучающимся.",
        "Аналитический обзор, перечень показателей, раздел отчета",
        "28.06.2026",
    ),
    (
        "Проектирование клиентского контура мониторинга с использованием общего состояния модуля посещаемости, включая модели сводки, фильтрацию исходной выборки и правила интерпретации завершенных и незаполненных журналов.",
        "Схема потока данных, модели, исходный код",
        "01.07.2026",
    ),
    (
        "Разработка интерфейса сводного контроля занятий и механизма выявления студентов, требующих внимания, с автоматическим выбором приоритетной записи, поиском и переходом к детализированной истории.",
        "Razor-компонент, алгоритмы расчета, иллюстрации",
        "04.07.2026",
    ),
    (
        "Разработка административного каталога пользователей, курсов и зачислений с синхронизацией состава учебных групп, а также реализация текстового импорта и экспорта пользователей в форматах JSON и CSV.",
        "Сервис каталога, админ-панель, форматы обмена",
        "08.07.2026",
    ),
    (
        "Проведение апробации аналитических расчетов и административных операций на контрольных сценариях, формулирование ограничений прототипа и подготовка отчета и презентационных материалов.",
        "Протокол апробации, отчет, презентация",
        "09.07.2026",
    ),
]


def add_title_page(document: Document, student_short: str, topic: str) -> None:
    blocks = [
        ("МИНИСТЕРСТВО НАУКИ И ВЫСШЕГО ОБРАЗОВАНИЯ РОССИЙСКОЙ ФЕДЕРАЦИИ", 10.5, True, 0, 3),
        ("ФЕДЕРАЛЬНОЕ ГОСУДАРСТВЕННОЕ АВТОНОМНОЕ ОБРАЗОВАТЕЛЬНОЕ УЧРЕЖДЕНИЕ ВЫСШЕГО ОБРАЗОВАНИЯ", 9.5, False, 0, 1),
        ("«Национальный исследовательский ядерный университет «МИФИ»", 11.5, True, 0, 11),
        ("Институт интеллектуальных кибернетических систем", 11.5, False, 0, 1),
        ("Кафедра №22 «Кибернетика»", 11.5, False, 0, 1),
        ("Направление подготовки 09.03.04 «Программная инженерия»", 11.5, False, 0, 15),
        ("ОТЧЕТ", 15, True, 0, 1),
        ("по летней практике студента на тему:", 14, True, 0, 5),
        (f"«{topic}»", 14, True, 0, 13),
    ]
    for text, size, bold, before, after in blocks:
        paragraph = base.add_report_paragraph(
            document,
            text,
            align=WD_ALIGN_PARAGRAPH.CENTER,
            indent=False,
            size=size,
            before=before,
            after=after,
            spacing=1.0,
        )
        paragraph.runs[0].font.bold = bold

    rows = [
        ("Группа", "Б23-504", ""),
        ("Студент", "________________", student_short),
        ("Руководитель", "________________", "Рословцев В.В."),
        ("Научный консультант", "________________", ""),
        ("Оценка руководителя", "________________", "0-30 баллов"),
        ("Оценка консультанта", "________________", "0-30 баллов"),
        ("Итоговая оценка", "________________", "0-100 баллов"),
        ("ECTS", "________________", ""),
        ("Комиссия", "", ""),
        ("Председатель", "________________", "Рословцев В.В."),
        ("Члены комиссии", "________________", ""),
        ("", "________________", ""),
    ]
    table = document.add_table(rows=len(rows), cols=3)
    table.alignment = WD_TABLE_ALIGNMENT.CENTER
    table.autofit = False
    widths = [5.0, 4.2, 6.2]
    for row, values in zip(table.rows, rows):
        for index, (cell, value) in enumerate(zip(row.cells, values)):
            cell.width = Cm(widths[index])
            base.set_table_cell(cell, value, 9.4, False, index == 1)
            cell._tc.get_or_add_tcPr().append(OxmlElement("w:tcBorders"))

    base.add_report_paragraph(document, "Москва 2026", align=WD_ALIGN_PARAGRAPH.CENTER, indent=False, size=11, before=8, spacing=1.0)


def add_assignment_pages(
    document: Document,
    student_full: str,
    student_short: str,
    topic: str,
    tasks: list[tuple[str, str, str]],
    sources: list[str],
) -> None:
    base.add_page_break(document)
    base.add_report_paragraph(document, "МИНИСТЕРСТВО НАУКИ И ВЫСШЕГО ОБРАЗОВАНИЯ РОССИЙСКОЙ ФЕДЕРАЦИИ", align=WD_ALIGN_PARAGRAPH.CENTER, indent=False, size=9.5, spacing=1.0).runs[0].font.bold = True
    base.add_report_paragraph(document, "Национальный исследовательский ядерный университет «МИФИ»", align=WD_ALIGN_PARAGRAPH.CENTER, indent=False, size=11.5, after=5, spacing=1.0).runs[0].font.bold = True
    base.add_report_paragraph(document, "Институт интеллектуальных кибернетических систем", align=WD_ALIGN_PARAGRAPH.CENTER, indent=False, size=10.5, spacing=1.0)
    base.add_report_paragraph(document, "КАФЕДРА №22 «КИБЕРНЕТИКА»", align=WD_ALIGN_PARAGRAPH.CENTER, indent=False, size=11.5, after=9, spacing=1.0).runs[0].font.bold = True
    base.add_report_paragraph(document, "ЗАДАНИЕ НА ЛЕТНЮЮ ПРАКТИКУ", align=WD_ALIGN_PARAGRAPH.CENTER, indent=False, size=14.5, after=8, spacing=1.0).runs[0].font.bold = True
    base.add_report_paragraph(document, f"Студенту гр. Б23-504 {student_full}", indent=False, size=11, after=5, spacing=1.0)
    base.add_report_paragraph(document, f"ТЕМА ПРАКТИКИ: {topic}", indent=False, size=11, after=6, spacing=1.0).runs[0].font.bold = True
    base.add_report_paragraph(document, "ЗАДАНИЕ", align=WD_ALIGN_PARAGRAPH.CENTER, indent=False, size=12, after=4, spacing=1.0).runs[0].font.bold = True

    headers = ["№ п/п", "Содержание работы", "Форма отчетности", "Срок", "Отметка", "Дата, подпись рук."]
    rows = [[str(index), task, form, deadline, "", ""] for index, (task, form, deadline) in enumerate(tasks, 1)]
    base.add_report_table(document, headers, rows, [0.7, 7.7, 3.0, 1.7, 1.4, 2.2], 7.1)

    base.add_page_break(document)
    base.add_report_heading(document, "Литература к заданию", 2)
    for index, source in enumerate(sources[:7], 1):
        base.add_report_paragraph(document, f"{index}. {source}", indent=False, size=10.2, after=2, spacing=1.0)
    base.add_report_paragraph(document, "Дата выдачи задания: 26 июня 2026 г.", indent=False, size=11.5, before=10, after=9, spacing=1.0)
    table = document.add_table(rows=2, cols=3)
    table.alignment = WD_TABLE_ALIGNMENT.CENTER
    for row, values in zip(table.rows, [("Руководитель", "________________", "Рословцев В.В."), ("Студент", "________________", student_short)]):
        for index, (cell, value) in enumerate(zip(row.cells, values)):
            base.set_table_cell(cell, value, 11.5, False, index == 1)
            cell._tc.get_or_add_tcPr().append(OxmlElement("w:tcBorders"))


def add_abstract(document: Document, keywords: str, paragraphs: list[str], source_count: int) -> None:
    base.add_page_break(document)
    base.add_report_heading(document, "Реферат", 1)
    base.add_report_paragraph(document, f"Пояснительная записка содержит 25 страниц. Количество использованных источников - {source_count}. Количество таблиц - 7. Количество рисунков - 8.")
    base.add_report_paragraph(document, f"Ключевые слова: {keywords}.")
    for text in paragraphs:
        base.add_report_paragraph(document, text)


def append_citation(paragraph, citation: str) -> None:
    if citation in paragraph.text or not paragraph.runs:
        return
    run = paragraph.runs[-1]
    text = run.text.rstrip()
    if text.endswith((".", ";", ":")):
        run.text = text[:-1] + f" {citation}" + text[-1]
    else:
        run.text = text + f" {citation}"


def apply_citations(document: Document, citation_map: list[tuple[str, str]]) -> None:
    matched: set[str] = set()
    for paragraph in document.paragraphs:
        for prefix, citation in citation_map:
            if paragraph.text.startswith(prefix):
                append_citation(paragraph, citation)
                matched.add(prefix)
    missing = [prefix for prefix, _ in citation_map if prefix not in matched]
    if missing:
        raise RuntimeError(f"Не найдены абзацы для внутритекстовых ссылок: {missing}")


EF_CITATIONS = [
    ("До начала практики в проекте uir-main", "[1]"),
    ("В uir-main были выделены основные сценарии", "[2, 3]"),
    ("Часть наработок uir-main не использовалась", "[4]"),
    ("omelea-dev-lms реализован как Blazor WebAssembly", "[7, 8]"),
    ("Конструктор ведомостей сохранен как самостоятельный раздел", "[5, 6]"),
    ("Требования сформированы на основе сценариев", "[15]"),
    ("Модуль разделен на модели предметной области", "[8, 9]"),
    ("Каталог LMS передает сведения", "[10]"),
    ("Модель AttendanceDataModel объединяет", "[11]"),
    ("Для демонстрации созданы учетные записи", "[12, 13]"),
    ("Модель является клиентской и тестовой", "[14]"),
    ("Проверка выполнялась после сборки проекта", "[4, 5]"),
    ("Прототип хранит AttendanceDataModel", "[10]"),
    ("Следующий этап должен включать серверный API", "[6]"),
]

LOK_CITATIONS = [
    ("Электронный журнал решает задачу", "[1, 2]"),
    ("В проекте uir-main присутствовали", "[3, 4]"),
    ("В качестве источника данных используется", "[7, 8]"),
    ("Интерфейс должен соответствовать", "[15]"),
    ("Компонент AttendanceMonitoring получает", "[8, 9]"),
    ("Изменение пользователей, курсов или зачислений", "[10]"),
    ("Сводка строится по занятиям", "[11]"),
    ("Для каждого занятия сводка рассчитывается", "[10]"),
    ("Административная страница объединяет", "[9]"),
    ("Для обмена списком пользователей реализованы", "[12, 13]"),
    ("Проверка проводилась после сборки", "[4, 5]"),
    ("Каталог и данные мониторинга хранятся", "[10]"),
    ("Дальнейшее развитие связано с переносом", "[6]"),
    ("Цель практики достигнута на уровне рабочего клиентского прототипа", "[14]"),
]


def add_sources_and_appendices(
    document: Document,
    sources: list[str],
    appendix_a_title: str,
    appendix_a_image: Path,
    appendix_a_caption: str,
    appendix_a_text: str,
    appendix_b_title: str,
    appendix_b_image: Path,
    appendix_b_caption: str,
    appendix_b_text: str,
) -> None:
    base.add_page_break(document)
    base.add_report_heading(document, "Список литературы", 1)
    for index, source in enumerate(sources, 1):
        base.add_report_paragraph(document, f"{index}. {source}", indent=False, size=11.5, after=3, spacing=1.05)

    base.add_page_break(document)
    base.add_report_heading(document, "Приложение А", 1)
    base.add_report_paragraph(document, appendix_a_title, align=WD_ALIGN_PARAGRAPH.CENTER, indent=False, size=14, after=8)
    base.add_report_figure(document, appendix_a_image, appendix_a_caption, 16.0)
    base.add_report_paragraph(document, appendix_a_text, size=12.2, spacing=1.1)

    base.add_page_break(document)
    base.add_report_heading(document, "Приложение Б", 1)
    base.add_report_paragraph(document, appendix_b_title, align=WD_ALIGN_PARAGRAPH.CENTER, indent=False, size=14, after=8)
    base.add_report_figure(document, appendix_b_image, appendix_b_caption, 16.0)
    base.add_report_paragraph(document, appendix_b_text, size=12.2, spacing=1.1)


EF_CONTENTS = [
    ("Введение", "6"), ("1 Анализ задачи и исходных решений", "7"), ("1.1 Состав исходного прототипа", "7"),
    ("1.2 Целевой проект и граница изменений", "8"), ("1.3 Требования к модулю", "9"),
    ("2 Проектирование модуля посещаемости", "10"), ("2.1 Архитектура переноса", "10"),
    ("2.2 Модель данных", "11"), ("2.3 Статусы посещения", "12"), ("2.4 Тестовая ролевая модель", "13"),
    ("3 Реализация пользовательских сценариев", "14"), ("3.1 Журнал занятия преподавателя", "14"),
    ("3.2 Ввод отметок и комментариев", "15"), ("3.3 Валидация данных", "16"),
    ("3.4 Сохранение и жизненный цикл журнала", "17"), ("3.5 Личный просмотр студента", "18"),
    ("3.6 Разграничение поведения интерфейса", "19"), ("4 Проверка и оценка результата", "20"),
    ("4.1 Проверка пользовательских сценариев", "20"), ("4.2 Ограничения и дальнейшее развитие", "21"),
    ("Заключение", "22"), ("Список литературы", "23"),
    ("Приложение А. Журнал преподавателя", "24"), ("Приложение Б. Личный режим студента", "25"),
]

LOK_CONTENTS = [
    ("Введение", "6"), ("1 Анализ задачи мониторинга", "7"), ("1.1 Исходные данные и целевой проект", "7"),
    ("1.2 Требования к мониторингу и администрированию", "8"), ("2 Проектирование аналитического модуля", "9"),
    ("2.1 Архитектура и поток данных", "9"), ("2.2 Система аналитических показателей", "10"),
    ("2.3 Состояния занятия и фильтры", "11"), ("3 Реализация мониторинга", "12"),
    ("3.1 Сводка посещаемости по занятиям", "12"), ("3.2 Контроль заполнения журналов", "13"),
    ("3.3 Студенты, требующие внимания", "14"), ("3.4 Карточка выбранного студента", "15"),
    ("4 Реализация административного каталога", "16"), ("4.1 Структура административной панели", "16"),
    ("4.2 Управление пользователями", "17"), ("4.3 Курсы и зачисления", "18"),
    ("4.4 Импорт и экспорт JSON/CSV", "19"), ("5 Проверка и оценка результата", "20"),
    ("5.1 Проверка сценариев", "20"), ("5.2 Ограничения и дальнейшее развитие", "21"),
    ("Заключение", "22"), ("Список литературы", "23"),
    ("Приложение А. Мониторинг", "24"), ("Приложение Б. Административная панель", "25"),
]


def build_efimenko_report() -> Path:
    EF_OUTPUT.mkdir(parents=True, exist_ok=True)
    path = EF_OUTPUT / "Отчет_по_летней_практике_ЕфименкоНП_v4.docx"
    document = Document()
    base.configure_report(document)
    document.core_properties.title = "Модуль учета посещаемости и тестовая ролевая модель"
    document.core_properties.author = "Ефименко Н.П."
    topic = "Разработка и интеграция модуля учета посещаемости и тестового механизма разграничения пользовательских сценариев в LMS"
    add_title_page(document, "Ефименко Н.П.", topic)
    add_assignment_pages(document, "Ефименко Николаю Павловичу", "Ефименко Н.П.", topic, EF_TASKS, EF_SOURCES)
    add_abstract(document, "учет посещаемости, LMS, Blazor, электронный журнал, валидация, студент, преподаватель, ролевая модель", [
        "Целью настоящей работы является перенос рабочего прототипа учета посещаемости в omelea-dev-lms и его доработка для поддержки журнала преподавателя, личного режима студента и тестового разграничения интерфейса.",
        "В первой главе рассматриваются исходный прототип, особенности целевого Blazor-приложения и требования к интегрируемому модулю.",
        "Вторая глава посвящена архитектуре переноса, предметной модели занятий и отметок, статусам посещения и тестовому механизму определения доступных операций.",
        "В третьей главе описываются журнал преподавателя, валидация опозданий и комментариев, сохранение состояния и личный просмотр студента. Четвертая глава содержит результаты ручной апробации и ограничения клиентского прототипа.",
    ], len(EF_SOURCES))
    v3.add_contents(document, EF_CONTENTS)
    v3.add_efimenko_body(document)
    apply_citations(document, EF_CITATIONS)
    add_sources_and_appendices(
        document, EF_SOURCES,
        "Журнал преподавателя", SCREENS / "02-teacher-attendance.png", "Рисунок А.1 - Полная страница журнала преподавателя", "Страница объединяет выбор занятия, сводку статусов, фильтры и редактируемые строки группы.",
        "Личный режим студента", SCREENS / "03-student-attendance.png", "Рисунок Б.1 - Полная страница личной посещаемости", "В личном режиме отсутствуют список группы и операции изменения отметок.",
    )
    document.save(path)
    return path


def build_loktev_report() -> Path:
    LOK_OUTPUT.mkdir(parents=True, exist_ok=True)
    path = LOK_OUTPUT / "Отчет_по_летней_практике_ЛоктевМС_v4.docx"
    document = Document()
    base.configure_report(document)
    document.core_properties.title = "Мониторинг и административные средства модуля посещаемости"
    document.core_properties.author = "Локтев М.С."
    topic = "Разработка аналитического мониторинга и административного каталога модуля учета посещаемости в LMS"
    add_title_page(document, "Локтев М.С.", topic)
    add_assignment_pages(document, "Локтеву Максиму Сергеевичу", "Локтев М.С.", topic, LOK_TASKS, LOK_SOURCES)
    add_abstract(document, "мониторинг, аналитические показатели, LMS, Blazor, административная панель, курс, зачисление, JSON, CSV", [
        "Целью настоящей работы является разработка мониторинга посещаемости и административных средств управления клиентскими справочниками omelea-dev-lms.",
        "В первой главе рассматриваются исходные данные, задачи контроля заполнения журналов и требования к административному каталогу.",
        "Вторая глава посвящена потоку данных, системе аналитических показателей и интерпретации состояния занятий.",
        "В третьей главе описываются сводка по занятиям и список студентов, требующих внимания. Четвертая глава содержит административную панель пользователей, курсов и зачислений, а также импорт и экспорт JSON/CSV. Пятая глава содержит результаты ручной апробации.",
    ], len(LOK_SOURCES))
    v3.add_contents(document, LOK_CONTENTS)
    v3.add_loktev_body(document)
    apply_citations(document, LOK_CITATIONS)
    add_sources_and_appendices(
        document, LOK_SOURCES,
        "Мониторинг посещаемости", ASSETS / "monitoring_main.png", "Рисунок А.1 - Основная область мониторинга", "Страница содержит сводные показатели, фильтры, список занятий и область студентов, требующих внимания.",
        "Административный каталог", ASSETS / "admin_main.png", "Рисунок Б.1 - Основная область административной панели", "Административная страница объединяет пользователей, обмен данными, курсы и зачисления.",
    )
    document.save(path)
    return path


def add_slide_reference(slide, text: str) -> None:
    base.ppt_add_text(slide, 8.75, 6.78, 3.95, 0.22, text, 8, base.GRAY, False, PP_ALIGN.RIGHT, margin=0)


def add_sources_slide(presentation: Presentation, student: str, accent: str, sources: list[str]) -> None:
    blank = presentation.slide_layouts[6]
    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide)
    v3.ppt_header(slide, "Основные источники", 15, student, accent)
    left = sources[:3]
    right = sources[3:]
    for index, source in enumerate(left):
        base.ppt_add_text(slide, 0.7, 1.05 + index * 1.72, 5.85, 1.45, source, 15, base.INK, index == 0, valign=MSO_ANCHOR.TOP)
    for index, source in enumerate(right):
        base.ppt_add_text(slide, 6.8, 1.05 + index * 1.72, 5.85, 1.45, source, 15, base.INK, False, valign=MSO_ANCHOR.TOP)
    slide_ids = presentation.slides._sldIdLst
    new_slide_id = slide_ids[-1]
    slide_ids.remove(new_slide_id)
    slide_ids.insert(len(slide_ids) - 1, new_slide_id)
    thanks = presentation.slides[-1]
    base.ppt_add_text(thanks, 12.2, 0.25, 0.55, 0.5, "16", 15, "C9D4DF", True, PP_ALIGN.RIGHT, margin=0)


def build_efimenko_presentation() -> Path:
    EF_OUTPUT.mkdir(parents=True, exist_ok=True)
    source = ROOT / "Готовые_материалы" / "Летняя_практика" / "v3" / "Ефименко" / "Презентация_ЕфименкоНП_v3.pptx"
    if not source.exists():
        v3.build_efimenko_presentation()
    path = EF_OUTPUT / "Презентация_ЕфименкоНП_v4.pptx"
    presentation = Presentation(source)
    references = {
        2: "[1], [3]",
        4: "[5], [6], [15]",
        5: "[7]-[10]",
        6: "[11]",
        8: "[11]",
        11: "[12]-[14]",
        13: "[5], [6], [10]",
    }
    for number, citation in references.items():
        add_slide_reference(presentation.slides[number - 1], f"Источники: {citation}")
    add_sources_slide(presentation, "Ефименко Н.П.", base.BLUE, [
        "[1] Соммервилл И. Инженерия программного обеспечения. 2021.",
        "[7] ASP.NET Core Blazor // Microsoft Learn. learn.microsoft.com/aspnet/core/blazor/",
        "[10] Blazor state management // Microsoft Learn. learn.microsoft.com/aspnet/core/blazor/state-management/",
        "[13] Jones M. et al. JSON Web Token (JWT) // RFC 7519.",
        "[14] Authorization Cheat Sheet // OWASP Cheat Sheet Series.",
    ])
    v3.normalize_presentation_text_boxes(presentation)
    presentation.core_properties.title = "Модуль учета посещаемости и тестовая ролевая модель, v4"
    presentation.save(path)
    return path


def build_loktev_presentation() -> Path:
    LOK_OUTPUT.mkdir(parents=True, exist_ok=True)
    source = ROOT / "Готовые_материалы" / "Летняя_практика" / "v3" / "Локтев" / "Презентация_ЛоктевМС_v3.pptx"
    if not source.exists():
        v3.build_loktev_presentation()
    path = LOK_OUTPUT / "Презентация_ЛоктевМС_v4.pptx"
    presentation = Presentation(source)
    references = {
        2: "[1]-[4]",
        4: "[7]-[10]",
        5: "[11]",
        6: "[10], [15]",
        10: "[9], [14]",
        13: "[12], [13]",
        14: "[5], [6], [10]",
    }
    for number, citation in references.items():
        add_slide_reference(presentation.slides[number - 1], f"Источники: {citation}")
    add_sources_slide(presentation, "Локтев М.С.", base.PURPLE, [
        "[1] Соммервилл И. Инженерия программного обеспечения. 2021.",
        "[7] ASP.NET Core Blazor // Microsoft Learn. learn.microsoft.com/aspnet/core/blazor/",
        "[10] Blazor state management // Microsoft Learn. learn.microsoft.com/aspnet/core/blazor/state-management/",
        "[12] Bray T. JSON Data Interchange Format // RFC 8259.",
        "[13] Shafranovich Y. CSV Files // RFC 4180.",
    ])
    v3.normalize_presentation_text_boxes(presentation)
    presentation.core_properties.title = "Мониторинг и административные средства модуля посещаемости, v4"
    presentation.save(path)
    return path


def write_memo() -> Path:
    OUTPUT.mkdir(parents=True, exist_ok=True)
    path = OUTPUT / "Памятка_v4_по_содержанию_и_расхождениям.md"
    path.write_text(
        """# Памятка к материалам v4

## Что исправлено относительно v3

- Титульные листы дополнены полями оценок, ECTS и комиссии по структуре образца `Практика.docx.pdf`.
- Индивидуальное задание содержит пять развернутых аналитико-проектных этапов и шесть колонок, включая отметку о выполнении и подпись руководителя.
- В содержательных главах добавлены внутритекстовые ссылки вида `[1]`, `[7, 8]`, `[12, 13]`.
- Списки литературы согласованы с нумерацией ссылок и содержат по 15 источников.
- В презентациях добавлены ссылки на технических слайдах и отдельный слайд «Основные источники».

## Где формулировки сильнее фактической реализации

- Часть функциональности не разработана с нуля во время практики, а перенесена из `uir-main`, отобрана и адаптирована к `omelea-dev-lms`.
- «Апробация» означает ручное прохождение контрольных маршрутов и успешную сборку, а не полный комплект автоматизированных тестов.
- Сохранение журнала фиксирует подтвержденную копию в памяти браузера; после `F5` восстанавливается демонстрационный набор.
- Тестовый механизм Ефименко разграничивает клиентский интерфейс, но не является защищенной серверной авторизацией.
- Импорт и экспорт Локтева преобразуют JSON/CSV-текст в административной странице; чтение и скачивание файлов не реализованы.
- Каталог пользователей, курсов и зачислений действует в памяти текущего сеанса и не записывается в базу данных.

В отчете Локтева не упоминаются Ефименко и тестовая ролевая модель. В отчете Ефименко не упоминается Локтев.
""",
        encoding="utf-8",
    )
    return path


def main() -> None:
    outputs = [
        build_efimenko_report(),
        build_loktev_report(),
        build_efimenko_presentation(),
        build_loktev_presentation(),
        write_memo(),
    ]
    for output in outputs:
        print(output)


if __name__ == "__main__":
    main()
