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


ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / "Готовые_материалы" / "Летняя_практика" / "v3"
EF_OUTPUT = OUTPUT / "Ефименко"
LOK_OUTPUT = OUTPUT / "Локтев"
ASSETS = ROOT / "document_work" / "generated_assets"
SCREENS = ROOT / "document_work" / "screenshots"


SOURCES = [
    "Microsoft. ASP.NET Core Blazor documentation [Электронный ресурс]. URL: https://learn.microsoft.com/aspnet/core/blazor/.",
    "Microsoft. ASP.NET Core Razor components [Электронный ресурс]. URL: https://learn.microsoft.com/aspnet/core/blazor/components/.",
    "Microsoft. Dependency injection in ASP.NET Core [Электронный ресурс]. URL: https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection.",
    "Microsoft. ASP.NET Core Blazor state management [Электронный ресурс]. URL: https://learn.microsoft.com/aspnet/core/blazor/state-management.",
    "Microsoft. C# documentation [Электронный ресурс]. URL: https://learn.microsoft.com/dotnet/csharp/.",
    "Bray T. The JavaScript Object Notation (JSON) Data Interchange Format. RFC 8259. IETF, 2017.",
    "Shafranovich Y. Common Format and MIME Type for Comma-Separated Values (CSV) Files. RFC 4180. IETF, 2005.",
    "Fowler M. Refactoring: Improving the Design of Existing Code. 2nd ed. Addison-Wesley, 2018.",
    "Соммервилл И. Инженерия программного обеспечения. 10-е изд. М.: Вильямс, 2021.",
    "ISO/IEC/IEEE 12207:2017. Systems and software engineering - Software life cycle processes. Geneva: ISO, 2017.",
]


EF_TASKS = [
    ("Анализ исходного прототипа учета посещаемости и структуры omelea-dev-lms.", "Аналитический обзор", "27.06.2026"),
    ("Перенос моделей занятий, студентов и отметок посещаемости в целевое Blazor-приложение.", "Исходный код", "29.06.2026"),
    ("Разработка журнала занятия преподавателя со статусами, минутами опоздания и комментариями.", "Интерфейс и код", "02.07.2026"),
    ("Реализация правил валидации, сохранения, отмены и завершения журнала.", "Сценарии проверки", "04.07.2026"),
    ("Разработка личного просмотра студента и разграничения поведения интерфейса.", "Интерфейс и код", "06.07.2026"),
    ("Создание тестовой ролевой модели и проверка основных пользовательских маршрутов.", "Протокол проверки", "09.07.2026"),
]


LOK_TASKS = [
    ("Анализ показателей, необходимых для контроля заполнения журналов посещаемости.", "Аналитический обзор", "27.06.2026"),
    ("Разработка сводки посещаемости по занятиям и фильтрации по периоду и состоянию.", "Интерфейс и код", "30.06.2026"),
    ("Разработка списка студентов, требующих внимания, и детальной карточки выбранного студента.", "Интерфейс и код", "03.07.2026"),
    ("Создание административной панели пользователей, курсов и зачислений.", "Интерфейс и код", "06.07.2026"),
    ("Реализация импорта и экспорта пользователей в форматах JSON и CSV.", "Исходный код", "08.07.2026"),
    ("Проверка расчетов, административных операций и оформление отчетных материалов.", "Протокол проверки", "09.07.2026"),
]


def add_title_page(
    document: Document,
    student_short: str,
    topic: str,
) -> None:
    blocks = [
        ("МИНИСТЕРСТВО НАУКИ И ВЫСШЕГО ОБРАЗОВАНИЯ РОССИЙСКОЙ ФЕДЕРАЦИИ", 11, True, 0, 5),
        ("Федеральное государственное автономное образовательное учреждение высшего образования", 10, False, 0, 0),
        ("«Национальный исследовательский ядерный университет «МИФИ»", 12, True, 0, 20),
        ("Институт интеллектуальных кибернетических систем", 12, False, 0, 2),
        ("Кафедра №22 «Кибернетика»", 12, False, 0, 2),
        ("Направление подготовки 09.03.04 «Программная инженерия»", 12, False, 0, 28),
        ("ОТЧЕТ", 16, True, 0, 2),
        ("по летней практике", 16, True, 0, 10),
        ("на тему:", 14, False, 0, 4),
        (f"«{topic}»", 15, True, 0, 30),
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

    table = document.add_table(rows=3, cols=3)
    table.alignment = WD_TABLE_ALIGNMENT.CENTER
    table.autofit = False
    values = [
        ("Группа", "Б23-504", ""),
        ("Студент", "________________", student_short),
        ("Руководитель", "________________", "Рословцев В.В."),
    ]
    widths = [4.0, 5.0, 7.0]
    for row, row_values in zip(table.rows, values):
        for index, (cell, value) in enumerate(zip(row.cells, row_values)):
            cell.width = Cm(widths[index])
            base.set_table_cell(cell, value, 12, False, index == 1)
            cell._tc.get_or_add_tcPr().append(OxmlElement("w:tcBorders"))

    base.add_report_paragraph(
        document,
        "Москва 2026",
        align=WD_ALIGN_PARAGRAPH.CENTER,
        indent=False,
        size=12,
        before=56,
        spacing=1.0,
    )


def add_assignment_pages(
    document: Document,
    student_full: str,
    student_short: str,
    topic: str,
    tasks: list[tuple[str, str, str]],
) -> None:
    base.add_page_break(document)
    base.add_report_paragraph(document, "НИЯУ МИФИ", align=WD_ALIGN_PARAGRAPH.CENTER, indent=False, size=13, after=2, spacing=1.0).runs[0].font.bold = True
    base.add_report_paragraph(document, "ИНСТИТУТ ИНТЕЛЛЕКТУАЛЬНЫХ КИБЕРНЕТИЧЕСКИХ СИСТЕМ", align=WD_ALIGN_PARAGRAPH.CENTER, indent=False, size=11, spacing=1.0)
    base.add_report_paragraph(document, "КАФЕДРА №22 «КИБЕРНЕТИКА»", align=WD_ALIGN_PARAGRAPH.CENTER, indent=False, size=12, after=18, spacing=1.0).runs[0].font.bold = True
    base.add_report_paragraph(document, "ЗАДАНИЕ НА ЛЕТНЮЮ ПРАКТИКУ", align=WD_ALIGN_PARAGRAPH.CENTER, indent=False, size=15, after=14, spacing=1.0).runs[0].font.bold = True
    base.add_report_paragraph(document, f"Студенту группы Б23-504 {student_full}", indent=False, size=12, after=8, spacing=1.0)
    base.add_report_paragraph(document, f"Тема практики: «{topic}»", indent=False, size=12, after=8, spacing=1.0)
    base.add_report_table(
        document,
        ["№", "Содержание работы", "Форма отчетности", "Срок"],
        [[str(index), *task] for index, task in enumerate(tasks, 1)],
        [0.8, 9.1, 3.5, 2.4],
        8.5,
    )

    base.add_page_break(document)
    base.add_report_heading(document, "Литература к заданию", 2)
    for index, source in enumerate(SOURCES[:7], 1):
        base.add_report_paragraph(document, f"{index}. {source}", indent=False, size=11, after=3, spacing=1.05)
    base.add_report_paragraph(document, "Дата выдачи задания: 26 июня 2026 г.", indent=False, size=12, before=16, after=14, spacing=1.0)
    table = document.add_table(rows=2, cols=3)
    table.alignment = WD_TABLE_ALIGNMENT.CENTER
    for row, row_values in zip(table.rows, [("Руководитель", "________________", "Рословцев В.В."), ("Студент", "________________", student_short)]):
        for index, (cell, value) in enumerate(zip(row.cells, row_values)):
            base.set_table_cell(cell, value, 12, False, index == 1)
            cell._tc.get_or_add_tcPr().append(OxmlElement("w:tcBorders"))


def add_abstract(
    document: Document,
    page_count: int,
    table_count: int,
    figure_count: int,
    keywords: str,
    paragraphs: list[str],
) -> None:
    base.add_page_break(document)
    base.add_report_heading(document, "Реферат", 1)
    base.add_report_paragraph(
        document,
        f"Отчет по летней практике содержит {page_count} страниц, {table_count} таблиц, {figure_count} рисунков, {len(SOURCES)} использованных источников и 2 приложения.",
    )
    base.add_report_paragraph(document, f"Ключевые слова: {keywords}.")
    for text in paragraphs:
        base.add_report_paragraph(document, text)


def add_contents(document: Document, entries: list[tuple[str, str]]) -> None:
    base.add_page_break(document)
    base.add_report_heading(document, "Содержание", 1)
    for title, page in entries:
        paragraph = document.add_paragraph()
        paragraph.paragraph_format.first_line_indent = Cm(0)
        paragraph.paragraph_format.line_spacing = 1.0
        paragraph.paragraph_format.space_after = base.Pt(2)
        paragraph.paragraph_format.tab_stops.add_tab_stop(Cm(15.5))
        base.font_run(paragraph.add_run(title), 11.2)
        base.font_run(paragraph.add_run("\t" + page), 11.2)


def add_sources_and_appendices(
    document: Document,
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
    base.add_report_heading(document, "Список использованных источников", 1)
    for index, source in enumerate(SOURCES, 1):
        base.add_report_paragraph(document, f"{index}. {source}", indent=False, size=12, after=4, spacing=1.05)

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


def add_efimenko_body(document: Document) -> None:
    base.add_page_break(document)
    base.add_report_heading(document, "Введение", 1)
    base.add_report_paragraph(document, "Учет посещаемости является регулярной операцией в образовательной системе: преподаватель отмечает присутствие на каждом занятии, уточняет причины пропусков и при необходимости анализирует историю конкретного студента. Если журнал существует отдельно от LMS, сведения о дисциплинах и составе группы приходится дублировать, а доступ обучающегося к собственным данным организуется дополнительными средствами.")
    base.add_report_paragraph(document, "До начала практики в проекте uir-main имелся рабочий прототип посещаемости, а проект omelea-dev-lms содержал готовый интерфейс учебной системы и самостоятельный конструктор ведомостей. Задача состояла в переносе предметной логики посещаемости в целевой проект без существенной переработки чужого конструктора.")
    base.add_report_paragraph(document, "Цель работы - реализовать в omelea-dev-lms модуль учета посещаемости с журналом преподавателя, личным просмотром студента и тестовым разграничением поведения интерфейса. Для достижения цели перенесены модели и сервис состояния, разработаны статусы и правила ввода, добавлены операции сохранения и завершения занятия, а также тестовая ролевая модель.")
    base.add_report_paragraph(document, "Объектом работы является пользовательская часть LMS. Предметом работы являются способы представления и проверки отметок посещаемости в Blazor WebAssembly-приложении.")

    base.add_page_break(document)
    base.add_report_heading(document, "1 Анализ задачи и исходных решений", 1)
    base.add_report_heading(document, "1.1 Состав исходного прототипа", 2)
    base.add_report_paragraph(document, "В uir-main были выделены основные сценарии: выбор занятия, редактирование строк студентов, использование нескольких статусов, фиксация опозданий, добавление пояснений и просмотр истории. Переносился не визуальный стиль исходного прототипа, а его предметная модель и порядок действий пользователя.")
    base.add_table_caption(document, "Таблица 1 - Элементы переносимого прототипа")
    base.add_report_table(document, ["Элемент", "Назначение", "Результат переноса"], [
        ["Занятие", "Контекст дисциплины, группы, темы и времени", "Отдельный выбираемый журнал"],
        ["Строка студента", "Связь обучающегося с отметкой", "Редактируемая строка преподавателя"],
        ["Статус", "Фиксация результата посещения", "Пять состояний, включая неотмеченное"],
        ["Комментарий", "Пояснение отметки", "Обязателен для уважительной причины"],
        ["История", "Последовательность отметок", "Личный режим студента"],
    ], [3.2, 6.0, 6.5], 9.8)
    base.add_report_paragraph(document, "Часть наработок uir-main не использовалась, поскольку проект содержал экспериментальные ветви интерфейса. В целевую реализацию включались только элементы, необходимые для целостного демонстрационного сценария.")

    base.add_page_break(document)
    base.add_report_heading(document, "1.2 Целевой проект и граница изменений", 2)
    base.add_report_paragraph(document, "omelea-dev-lms реализован как Blazor WebAssembly-приложение с боковой навигацией, страницами курсов, форм и ведомостей. Новый раздел должен был использовать существующие размеры элементов, цвета, поля ввода и общую компоновку. Поэтому Razor-разметка журнала адаптирована к стилям LMS.")
    base.add_report_paragraph(document, "Конструктор ведомостей сохранен как самостоятельный раздел. Его файлы StatementConstructor.razor и StatementService.cs не перерабатывались в журнал посещаемости. Новая функциональность размещена рядом в маршруте /attendance и использует отдельные модели.")
    base.add_report_figure(document, ASSETS / "statement_constructor.png", "Рисунок 1 - Сохраненный конструктор ведомостей omelea-dev-lms", 15.5)

    base.add_page_break(document)
    base.add_report_heading(document, "1.3 Требования к модулю", 2)
    base.add_report_paragraph(document, "Требования сформированы на основе сценариев преподавателя и студента. Особое внимание уделено тому, чтобы выбор занятия менял не только заголовок, но и строки журнала, показатели и доступные команды.")
    base.add_table_caption(document, "Таблица 2 - Функциональные требования")
    base.add_report_table(document, ["Код", "Требование", "Критерий выполнения"], [
        ["R1", "Выбор занятия", "Отображается соответствующий состав и отметки"],
        ["R2", "Четыре итоговых статуса", "Статус выбирается для каждой строки"],
        ["R3", "Валидация", "Некорректная строка блокирует сохранение"],
        ["R4", "Личный просмотр", "Студент видит только собственную историю"],
        ["R5", "Разграничение интерфейса", "Редактирование скрыто у студента"],
        ["R6", "Тестовые роли", "Доступные дисциплины зависят от пользователя"],
    ], [1.2, 6.0, 8.5], 9.5)
    base.add_report_paragraph(document, "Нефункциональные требования включали сохранение дизайна LMS, отсутствие изменений в алгоритме конструктора ведомостей и возможность запуска демонстрации без внешней базы данных.")

    base.add_page_break(document)
    base.add_report_heading(document, "2 Проектирование модуля посещаемости", 1)
    base.add_report_heading(document, "2.1 Архитектура переноса", 2)
    base.add_report_paragraph(document, "Модуль разделен на модели предметной области, singleton-сервис AttendanceService и Razor-страницу Attendance. Сервис хранит занятия и отметки, выполняет выборку строк, рассчитывает сводку, проверяет ввод и создает подтвержденные копии. Компонент страницы отвечает за представление и пользовательские события.")
    base.add_report_figure(document, ASSETS / "attendance_integration.png", "Рисунок 2 - Размещение модуля посещаемости в LMS", 15.7)
    base.add_report_paragraph(document, "Каталог LMS передает сведения о доступных дисциплинах и составе групп. Благодаря этому один экземпляр журнала работает в разных пользовательских контекстах, не дублируя демонстрационные списки внутри страницы.", size=12.4, spacing=1.15)

    base.add_page_break(document)
    base.add_report_heading(document, "2.2 Модель данных", 2)
    base.add_report_paragraph(document, "Модель AttendanceDataModel объединяет студентов и занятия. Занятие содержит идентификатор курса, название дисциплины, группу, тему, аудиторию, время начала, признак завершения и коллекцию отметок. Отметка связывает студента со статусом, минутами опоздания, комментарием и временем последнего изменения.")
    base.add_table_caption(document, "Таблица 3 - Основные модели данных")
    base.add_report_table(document, ["Модель", "Ключевые поля", "Назначение"], [
        ["AttendanceStudentModel", "Id, FullName, RecordBookNumber", "Идентификация студента"],
        ["AttendanceLessonModel", "CourseId, StartsAt, CompletedAt, Entries", "Контекст журнала занятия"],
        ["AttendanceEntryModel", "StudentId, Status, LateMinutes, Comment, UpdatedAt", "Результат посещения"],
        ["AttendanceRowModel", "Student, Entry, IsDirty, ValidationError", "Строка интерфейса"],
        ["AttendanceSummaryModel", "Total и счетчики статусов", "Оперативная сводка"],
    ], [3.8, 7.2, 4.7], 9.5)
    base.add_report_paragraph(document, "Клонирование AttendanceEntryModel используется для хранения подтвержденного состояния. Это позволяет отделить текущие изменения преподавателя от последнего сохранения в рамках сеанса.")

    base.add_page_break(document)
    base.add_report_heading(document, "2.3 Статусы посещения", 2)
    base.add_report_paragraph(document, "В модели предусмотрены четыре итоговых статуса и служебное состояние «Не отмечен». Служебное состояние необходимо, чтобы преподаватель видел незаполненные строки и не мог случайно завершить неполный журнал.")
    base.add_table_caption(document, "Таблица 4 - Правила использования статусов")
    base.add_report_table(document, ["Статус", "Смысл", "Дополнительные данные"], [
        ["Присутствовал", "Студент участвовал в занятии", "Не требуются"],
        ["Опоздал", "Студент пришел после начала", "Положительное число минут"],
        ["Отсутствовал", "Студент не присутствовал", "Комментарий необязателен"],
        ["Уважительная причина", "Пропуск имеет подтверждаемую причину", "Обязательный комментарий"],
        ["Не отмечен", "Преподаватель еще не обработал строку", "Блокирует завершение"],
    ], [4.0, 6.5, 5.2], 9.8)
    base.add_report_paragraph(document, "При смене статуса с «Опоздал» поле минут очищается, если новый статус не требует этого значения. Комментарий сохраняется как пояснение, но пустая строка преобразуется в null.")

    base.add_page_break(document)
    base.add_report_heading(document, "2.4 Тестовая ролевая модель", 2)
    base.add_report_paragraph(document, "Для демонстрации созданы учетные записи администратора, преподавателя и студента. Имя пользователя извлекается из демонстрационного токена, после чего LmsCatalogService возвращает связанные курсы и тип участия. Для редактирования занятия допускаются администратор, преподаватель и ассистент; студент получает только чтение собственных данных.")
    base.add_report_paragraph(document, "Модель является клиентской и тестовой: она управляет поведением компонентов, но не заменяет серверную авторизацию. Ее назначение - воспроизводимо проверить маршруты и состав интерфейса до подключения защищенного API.")
    base.add_report_figure(document, ASSETS / "role_model.png", "Рисунок 3 - Тестовое разграничение пользовательских режимов", 15.7)

    base.add_page_break(document)
    base.add_report_heading(document, "3 Реализация пользовательских сценариев", 1)
    base.add_report_heading(document, "3.1 Журнал занятия преподавателя", 2)
    base.add_report_paragraph(document, "В верхней части страницы преподаватель выбирает доступное занятие. Обработчик SelectLesson изменяет SelectedLessonId в сервисе, после чего заново формируются строки, счетчики статусов, тема, аудитория и состояние журнала. Таким образом, кнопки занятий переключают полный контекст, а не только название.")
    base.add_report_figure(document, ASSETS / "teacher_attendance.png", "Рисунок 4 - Журнал посещаемости преподавателя", 15.7)
    base.add_report_paragraph(document, "В таблице доступны поиск по ФИО или номеру зачетной книжки, фильтр по статусу, сортировка по имени, статусу и длительности опоздания. Команда массовой отметки переводит только неотмеченные видимые строки в состояние «Присутствовал».", size=12.2, spacing=1.1)

    base.add_page_break(document)
    base.add_report_heading(document, "3.2 Ввод отметок и комментариев", 2)
    base.add_report_paragraph(document, "Для каждой строки преподаватель выбирает статус из списка. Поле минут активно только при опоздании, а подпись поля комментария меняется для уважительной причины. Измененная строка сравнивается с подтвержденной копией и визуально отмечается как несохраненная.")
    base.add_report_paragraph(document, "Сводные кнопки над таблицей одновременно показывают количество отметок каждого типа и служат фильтрами. Повторное нажатие на активный статус возвращает полный список. Это сокращает число элементов управления и позволяет быстро проверить пропуски или неотмеченные строки.")
    base.add_report_bullets(document, [
        "изменение статуса сразу пересчитывает сводку занятия;",
        "для опоздания открывается числовое поле минут;",
        "комментарий очищается от пробелов по краям;",
        "завершенный журнал отображается в режиме чтения;",
        "число несохраненных строк выводится рядом с командами.",
    ])

    base.add_page_break(document)
    base.add_report_heading(document, "3.3 Валидация данных", 2)
    base.add_report_paragraph(document, "Проверка выполняется для каждой AttendanceEntryModel. Ошибка возвращается в строку интерфейса и учитывается перед сохранением или завершением. Такой подход размещает правило рядом с предметной моделью и исключает расхождение между визуальной подсказкой и фактическим разрешением операции.")
    base.add_table_caption(document, "Таблица 5 - Условия валидации")
    base.add_report_table(document, ["Условие", "Сообщение", "Результат"], [
        ["Опоздание без минут или значение 0", "Укажите количество минут", "Сохранение блокируется"],
        ["Уважительная причина без комментария", "Добавьте комментарий", "Сохранение блокируется"],
        ["Есть несохраненные изменения", "Сначала сохраните журнал", "Завершение блокируется"],
        ["Остались неотмеченные строки", "Не отмечено студентов: N", "Завершение блокируется"],
    ], [5.7, 5.4, 4.6], 9.6)
    base.add_report_paragraph(document, "Отрицательное число минут нормализуется до нуля и затем считается ошибочным. Это предотвращает сохранение формально числового, но предметно некорректного значения.")

    base.add_page_break(document)
    base.add_report_heading(document, "3.4 Сохранение и жизненный цикл журнала", 2)
    base.add_report_paragraph(document, "Команда сохранения проверяет все строки выбранного занятия. Для измененных отметок записывается UpdatedAt, а коллекция клонируется в словарь confirmedEntries. Команда отмены восстанавливает последнюю подтвержденную копию. Механизм действует при переходах между страницами в пределах текущего сеанса приложения.")
    base.add_report_paragraph(document, "Завершение возможно только после сохранения, устранения ошибок и заполнения всех строк. После завершения поля блокируются. Команда «Возобновить» снимает признак завершения и возвращает преподавателю возможность редактирования.")
    base.add_report_bullets(document, [
        "«Сохранить» фиксирует корректное состояние текущего занятия;",
        "«Отменить» возвращает последнюю подтвержденную версию;",
        "«Завершить» закрывает полностью заполненный журнал;",
        "«Возобновить» повторно открывает завершенное занятие.",
    ])

    base.add_page_break(document)
    base.add_report_heading(document, "3.5 Личный просмотр посещаемости студента", 2)
    base.add_report_paragraph(document, "После входа тестового студента AttendanceService связывает имя пользователя с AttendanceStudentModel. На странице отображаются только дисциплины, на которые студент зачислен, собственная сводка и история отметок. Выбор другого студента, массовые операции и поля редактирования отсутствуют.")
    base.add_report_figure(document, ASSETS / "student_attendance.png", "Рисунок 5 - Личный просмотр посещаемости студента", 15.7)
    base.add_report_paragraph(document, "В истории выводятся дата, дисциплина, тема, итоговый статус и пояснение. Для опоздания текст объединяет количество минут и комментарий. Переключение занятия изменяет фактическую выборку истории и сводные значения.", size=12.2, spacing=1.1)

    base.add_page_break(document)
    base.add_report_heading(document, "3.6 Разграничение поведения интерфейса", 2)
    base.add_report_paragraph(document, "Разграничение реализовано не отдельной копией страницы, а вычислением режима на основании текущего пользователя и доступного занятия. Это уменьшает дублирование разметки и позволяет поддерживать единые подписи статусов.")
    base.add_table_caption(document, "Таблица 6 - Поведение пользовательских режимов")
    base.add_report_table(document, ["Операция", "Преподаватель", "Студент"], [
        ["Просмотр доступных дисциплин", "Назначенные курсы", "Курсы зачисления"],
        ["Просмотр списка группы", "Разрешен", "Недоступен"],
        ["Изменение статуса", "Разрешено", "Недоступно"],
        ["Сохранение и завершение", "Разрешено", "Недоступно"],
        ["Личная история", "Можно открыть в рабочем сценарии", "Только собственная"],
    ], [6.2, 4.7, 4.8], 9.7)
    base.add_report_paragraph(document, "Клиентская проверка CanEditLesson используется как средство управления прототипом. Для промышленной эксплуатации аналогичная проверка должна повторяться сервером для каждого запроса изменения.")

    base.add_page_break(document)
    base.add_report_heading(document, "4 Проверка и оценка результата", 1)
    base.add_report_heading(document, "4.1 Проверка пользовательских сценариев", 2)
    base.add_report_paragraph(document, "Проверка выполнялась после сборки проекта путем последовательного прохождения маршрутов с демонстрационными учетными записями. Отдельно проверялись переключение занятий, блокировка некорректных данных и отсутствие элементов редактирования у студента.")
    base.add_report_figure(document, ASSETS / "login.png", "Рисунок 6 - Выбор тестового пользователя", 15.2)
    base.add_table_caption(document, "Таблица 7 - Результаты ручной проверки")
    base.add_report_table(document, ["Сценарий", "Ожидаемый результат", "Статус"], [
        ["Выбор другого занятия", "Меняются тема, состав и отметки", "Выполнено"],
        ["Опоздание без минут", "Сохранение заблокировано", "Выполнено"],
        ["Уважительная причина без текста", "Сохранение заблокировано", "Выполнено"],
        ["Отмена изменений", "Возвращена подтвержденная копия", "Выполнено"],
        ["Вход студента", "Показана только личная история", "Выполнено"],
    ], [6.0, 7.3, 2.4], 9.2)

    base.add_page_break(document)
    base.add_report_heading(document, "4.2 Ограничения и дальнейшее развитие", 2)
    base.add_report_paragraph(document, "Прототип хранит AttendanceDataModel и подтвержденные копии в памяти браузера. После полной перезагрузки страницы создается начальный демонстрационный набор. Тестовый токен не имеет проверяемой подписи, а ограничение операций выполняется на клиенте.")
    base.add_report_paragraph(document, "Следующий этап должен включать серверный API, постоянное хранилище и проверку прав на сервере. Полезными функциональными расширениями являются создание занятий по расписанию, журнал корректировок, прикрепление подтверждающих документов и уведомления о незаполненных журналах.")
    base.add_report_bullets(document, [
        "хранить занятия и отметки в PostgreSQL;",
        "подключить JWT-авторизацию исходного dev-lms;",
        "добавить аудит изменений и автора каждой отметки;",
        "реализовать согласование исправлений после завершения;",
        "добавить автоматические и браузерные тесты.",
    ])

    base.add_page_break(document)
    base.add_report_heading(document, "Заключение", 1)
    base.add_report_paragraph(document, "В ходе летней практики рабочий прототип учета посещаемости перенесен в интерфейс omelea-dev-lms. Реализован журнал занятия преподавателя, который переключает полный контекст занятия, поддерживает поиск, фильтрацию, сортировку и массовую отметку неотмеченных студентов.")
    base.add_report_paragraph(document, "Для отметок предусмотрены статусы «Присутствовал», «Опоздал», «Отсутствовал» и «Уважительная причина», а также служебное состояние «Не отмечен». Реализованы минуты опоздания, комментарии, построчная валидация, сохранение, отмена, завершение и возобновление журнала.")
    base.add_report_paragraph(document, "Студент получает личный режим без доступа к изменению журнала и просмотру других обучающихся. Тестовая ролевая модель ограничивает список дисциплин и поведение интерфейса для администратора, преподавателя и студента.")
    base.add_report_paragraph(document, "Исходный конструктор ведомостей сохранен самостоятельным модулем. Цель практики достигнута на уровне рабочего клиентского прототипа; для эксплуатации требуется серверное хранение и полноценная авторизация.")


def add_loktev_body(document: Document) -> None:
    base.add_page_break(document)
    base.add_report_heading(document, "Введение", 1)
    base.add_report_paragraph(document, "Электронный журнал решает задачу фиксации отдельных отметок, однако для регулярного контроля недостаточно просматривать каждое занятие вручную. Преподавателю необходимы сведения о заполненности журналов, распределении статусов и студентах с устойчиво низкой посещаемостью. Администратору требуется управлять справочниками, от которых зависит состав групп и доступность дисциплин.")
    base.add_report_paragraph(document, "Цель работы - разработать в составе omelea-dev-lms клиентский модуль мониторинга посещаемости и административные средства управления пользователями, курсами и зачислениями. Дополнительно требовалось реализовать перенос списка пользователей через JSON и CSV.")
    base.add_report_paragraph(document, "Для достижения цели определены показатели мониторинга, реализованы сводка по занятиям, фильтры периода и состояния, список студентов, требующих внимания, и карточка выбранного студента. Административная часть объединяет операции со справочниками и синхронизирует изменения с данными посещаемости текущего сеанса.")
    base.add_report_paragraph(document, "Объектом работы является информационная поддержка контроля учебной активности. Предметом работы являются методы агрегирования отметок и управления клиентскими справочниками LMS.")

    base.add_page_break(document)
    base.add_report_heading(document, "1 Анализ задачи мониторинга", 1)
    base.add_report_heading(document, "1.1 Исходные данные и целевой проект", 2)
    base.add_report_paragraph(document, "В проекте uir-main присутствовали экспериментальные элементы мониторинга, которые позволяли оценить общую посещаемость и перейти к сведениям о студенте. Целевой проект omelea-dev-lms предоставлял готовый дизайн LMS и навигационный каркас, но не содержал связанной страницы аналитики и административного каталога для демонстрационного модуля.")
    base.add_report_paragraph(document, "В качестве источника данных используется AttendanceService целевого Blazor-приложения. Он предоставляет занятия, отметки, признаки завершения и время последнего изменения. Административный LmsCatalogService предоставляет пользователей, курсы и зачисления. Мониторинг не создает отдельную копию журнала, а рассчитывает показатели над общим состоянием.")
    base.add_report_figure(document, ASSETS / "statement_constructor.png", "Рисунок 1 - Исходный раздел ведомостей целевой LMS", 15.4)

    base.add_page_break(document)
    base.add_report_heading(document, "1.2 Требования к мониторингу и администрированию", 2)
    base.add_report_paragraph(document, "Страница мониторинга должна одновременно отвечать на два вопроса: все ли занятия обработаны и какие обучающиеся требуют дополнительного внимания. Административная страница должна позволять изменять исходные справочники без редактирования демонстрационного кода.")
    base.add_table_caption(document, "Таблица 1 - Функциональные требования")
    base.add_report_table(document, ["Код", "Требование", "Критерий выполнения"], [
        ["M1", "Сводка по занятиям", "Показаны заполненность и распределение отметок"],
        ["M2", "Фильтр периода", "Расчеты ограничиваются выбранным интервалом"],
        ["M3", "Группа внимания", "Список отсортирован по показателю посещаемости"],
        ["A1", "Пользователи", "Доступны добавление, изменение признака и удаление"],
        ["A2", "Курсы и зачисления", "Состав группы изменяется из интерфейса"],
        ["A3", "JSON/CSV", "Список пользователей импортируется и экспортируется"],
    ], [1.2, 6.1, 8.4], 9.4)
    base.add_report_paragraph(document, "Интерфейс должен соответствовать визуальным правилам omelea-dev-lms и сохранять данные при переходах между страницами в пределах текущего запуска приложения.")

    base.add_page_break(document)
    base.add_report_heading(document, "2 Проектирование аналитического модуля", 1)
    base.add_report_heading(document, "2.1 Архитектура и поток данных", 2)
    base.add_report_paragraph(document, "Компонент AttendanceMonitoring получает выборки через методы AttendanceService. Сервис фильтрует занятия по периоду, строит AttendanceLessonMonitoringModel для каждой записи и формирует общую AttendanceMonitoringSummaryModel. Для обучающихся создается AttendanceStudentMonitoringModel со счетчиками итоговых статусов.")
    base.add_report_figure(document, ASSETS / "monitoring_integration.png", "Рисунок 2 - Поток данных мониторинга и администрирования", 15.7)
    base.add_report_paragraph(document, "Изменение пользователей, курсов или зачислений вызывает событие Changed. AttendanceService синхронизирует студентов и строки занятий с каталогом, а компоненты повторно вычисляют отображаемые наборы.", size=12.3, spacing=1.15)

    base.add_page_break(document)
    base.add_report_heading(document, "2.2 Система аналитических показателей", 2)
    base.add_report_paragraph(document, "Сводка строится по занятиям, дата которых уже наступила. Для итогового распределения используются отметки завершенных журналов, а число неотмеченных строк вычисляется по всем попавшим в период занятиям. Такой подход отделяет подтвержденную статистику от контроля текущего заполнения.")
    base.add_table_caption(document, "Таблица 2 - Показатели мониторинга")
    base.add_report_table(document, ["Показатель", "Способ вычисления", "Назначение"], [
        ["Всего занятий", "Количество занятий периода", "Оценка объема контроля"],
        ["Завершено", "CompletedAt не равен null", "Контроль закрытых журналов"],
        ["Не завершено", "Разность общего и завершенного", "Поиск незакрытых занятий"],
        ["Распределение статусов", "Счетчики отметок завершенных занятий", "Структура посещаемости"],
        ["Не отмечено", "Счетчик Unmarked во всех занятиях", "Контроль полноты ввода"],
        ["Процент студента", "(присутствовал + опоздал) / учтенные отметки", "Выявление риска"],
    ], [4.0, 7.0, 4.7], 9.3)

    base.add_page_break(document)
    base.add_report_heading(document, "2.3 Состояния занятия и фильтры", 2)
    base.add_report_paragraph(document, "Для занятия отображаются дата, дисциплина, тема, группа, количество отмеченных строк, распределение статусов и время последнего изменения. Состояние определяется по признаку завершения и наличию неотмеченных строк.")
    base.add_table_caption(document, "Таблица 3 - Интерпретация состояния журнала")
    base.add_report_table(document, ["Состояние", "Условие", "Действие пользователя"], [
        ["Завершен", "CompletedAt заполнен", "Использовать в итоговой сводке"],
        ["Требует заполнения", "Есть строки Unmarked", "Перейти к журналу"],
        ["Заполнен, не завершен", "Все отмечены, CompletedAt пуст", "Проверить и завершить"],
        ["Вне периода", "Дата раньше выбранной границы", "Не показывать в выборке"],
    ], [4.2, 6.5, 5.0], 9.7)
    base.add_report_paragraph(document, "Пользователь может выбрать период и состояние журнала. Фильтры применяются до формирования таблицы, поэтому счетчики и список остаются согласованными с выбранной областью анализа.")

    base.add_page_break(document)
    base.add_report_heading(document, "3 Реализация мониторинга", 1)
    base.add_report_heading(document, "3.1 Сводка посещаемости по занятиям", 2)
    base.add_report_paragraph(document, "В верхней части страницы размещены компактные показатели количества занятий, завершенных и незавершенных журналов, отметок по типам и незаполненных строк. Ниже расположена таблица занятий с переходом к конкретному журналу.")
    base.add_report_figure(document, ASSETS / "monitoring_main.png", "Рисунок 3 - Сводка посещаемости и таблица занятий", 15.8)
    base.add_report_paragraph(document, "Для каждого занятия сводка рассчитывается методом GetSummary, а время последнего изменения определяется по максимальному UpdatedAt среди его отметок. Это позволяет увидеть давно не обновлявшиеся записи.", size=12.2, spacing=1.1)

    base.add_page_break(document)
    base.add_report_heading(document, "3.2 Контроль заполнения журналов", 2)
    base.add_report_paragraph(document, "Контроль заполнения основан на сочетании признака завершения и числа строк Unmarked. Пользователь может оставить на экране только незавершенные журналы или занятия с пропущенными отметками. В таблице явно показывается количество необработанных студентов.")
    base.add_report_bullets(document, [
        "период ограничивает исходный набор занятий;",
        "фильтр состояния выделяет незавершенные журналы;",
        "счетчик «Не отмечено» показывает общий объем незаполненных строк;",
        "ссылка на занятие открывает рабочий журнал в нужном контексте;",
        "команды «Завершить» и «Возобновить» имеют явные подписи.",
    ])
    base.add_report_paragraph(document, "Сочетание агрегированной карточки и детальной таблицы позволяет сначала оценить масштаб проблемы, а затем перейти к конкретному занятию.")

    base.add_page_break(document)
    base.add_report_heading(document, "3.3 Студенты, требующие внимания", 2)
    base.add_report_paragraph(document, "Для каждого студента учитываются отметки завершенных занятий за выбранный период. Записи со статусом «Не отмечен» исключаются из знаменателя. Список сортируется по возрастанию процента посещаемости, затем по ФИО.")
    base.add_table_caption(document, "Таблица 4 - Признаки для списка внимания")
    base.add_report_table(document, ["Признак", "Интерпретация", "Отображение"], [
        ["Низкий процент", "Мало присутствий и опозданий", "Верхние позиции списка"],
        ["Повторные отсутствия", "Несколько статусов Absent", "Счетчик пропусков"],
        ["Опоздания", "Систематическое нарушение начала", "Отдельный счетчик"],
        ["Нет данных", "Нет учтенных завершенных занятий", "Нулевой объем наблюдений"],
    ], [4.2, 6.5, 5.0], 9.7)
    base.add_report_figure(document, ASSETS / "monitoring_attention.png", "Рисунок 4 - Список студентов, требующих внимания", 15.0)

    base.add_page_break(document)
    base.add_report_heading(document, "3.4 Карточка выбранного студента", 2)
    base.add_report_paragraph(document, "После расчета списка первый студент выбирается автоматически, поэтому информационная область не остается пустой. Пользователь может выбрать другую строку или найти студента по ФИО и номеру зачетной книжки. Для выбранного элемента показываются процент, счетчики статусов и ссылка на детальную историю.")
    base.add_report_paragraph(document, "Автовыбор выполняется повторно, если фильтр периода исключил ранее выбранного студента. Это устраняет ситуацию, при которой список изменился, но карточка продолжает показывать данные элемента, отсутствующего в текущей выборке.")
    base.add_report_bullets(document, [
        "автоматически выбран первый элемент отсортированного списка;",
        "поиск сокращает набор без потери выбранного контекста;",
        "выпадающий список позволяет быстро сменить студента;",
        "ссылка открывает детальную историю в отдельном маршруте.",
    ])

    base.add_page_break(document)
    base.add_report_heading(document, "4 Реализация административного каталога", 1)
    base.add_report_heading(document, "4.1 Структура административной панели", 2)
    base.add_report_paragraph(document, "Административная страница объединяет три связанных справочника: пользователей, курсы и зачисления. Операции выполняются через LmsCatalogService, который хранит коллекции текущего сеанса и уведомляет связанные компоненты об изменениях.")
    base.add_report_figure(document, ASSETS / "admin_main.png", "Рисунок 5 - Административная панель пользователей", 15.8)
    base.add_table_caption(document, "Таблица 5 - Сущности административного каталога")
    base.add_report_table(document, ["Сущность", "Основные поля", "Операции"], [
        ["Пользователь", "Логин, ФИО, номер зачетной книжки, признак администратора", "Добавление, изменение признака, удаление"],
        ["Курс", "Код, название, группа, аудитория", "Добавление и удаление"],
        ["Зачисление", "Пользователь, курс, тип участия", "Назначение и удаление"],
    ], [3.2, 7.5, 5.0], 9.2)

    base.add_page_break(document)
    base.add_report_heading(document, "4.2 Управление пользователями", 2)
    base.add_report_paragraph(document, "При создании пользователя проверяется наличие логина и ФИО, а также уникальность логина без учета регистра. Для студента может быть указан номер зачетной книжки. Признак администратора переключается в карточке пользователя.")
    base.add_report_paragraph(document, "Удаление пользователя каскадно удаляет его зачисления из клиентского каталога. Сервис не позволяет удалить последнего администратора, что сохраняет возможность продолжить демонстрацию административного сценария.")
    base.add_report_bullets(document, [
        "добавление пользователя с проверкой обязательных полей;",
        "предотвращение повторяющихся логинов;",
        "изменение административного признака;",
        "удаление связанных зачислений;",
        "синхронизация списков студентов в занятиях.",
    ])

    base.add_page_break(document)
    base.add_report_heading(document, "4.3 Курсы и зачисления", 2)
    base.add_report_paragraph(document, "Курс содержит короткий код, полное название, учебную группу и аудиторию по умолчанию. При добавлении проверяются обязательные поля и уникальность кода. Для нового курса AttendanceService создает демонстрационное вводное занятие.")
    base.add_report_figure(document, ASSETS / "admin_courses.png", "Рисунок 6 - Управление курсами и зачислениями", 15.7)
    base.add_report_paragraph(document, "Зачисление связывает пользователя и курс с типом участия: студент, преподаватель, ассистент или куратор. Повторяющаяся связь отклоняется. Добавление студента приводит к появлению его строки в занятиях курса, а удаление связи исключает строку после синхронизации.", size=12.2, spacing=1.1)

    base.add_page_break(document)
    base.add_report_heading(document, "4.4 Импорт и экспорт JSON/CSV", 2)
    base.add_report_paragraph(document, "Для обмена списком пользователей реализованы два текстовых формата. Экспорт JSON использует JsonSerializer с форматированием, экспорт CSV формирует заголовок и экранирует кавычки, запятые и переводы строк. Результат отображается в текстовом поле административной страницы.")
    base.add_table_caption(document, "Таблица 6 - Поля обмена пользователями")
    base.add_report_table(document, ["Поле", "JSON", "CSV", "Назначение"], [
        ["username", "Строка", "Колонка 1", "Уникальный логин"],
        ["fullName", "Строка", "Колонка 2", "Отображаемое ФИО"],
        ["recordBookNumber", "Строка", "Колонка 3", "Номер зачетной книжки"],
        ["isAdmin", "Логическое", "Колонка 4", "Административный признак"],
    ], [3.2, 3.0, 3.0, 6.5], 9.7)
    base.add_report_paragraph(document, "При импорте применяется операция upsert по логину: существующий пользователь обновляется, отсутствующий добавляется. Пустые или некорректные строки пропускаются. Если импорт снял признак со всех администраторов, он возвращается первому пользователю.")

    base.add_page_break(document)
    base.add_report_heading(document, "5 Проверка и оценка результата", 1)
    base.add_report_heading(document, "5.1 Проверка сценариев", 2)
    base.add_report_paragraph(document, "Проверка проводилась после сборки Blazor-приложения. Значения в карточках сравнивались со строками завершенных занятий, затем выполнялись операции добавления пользователя, курса и зачисления, а также импорт и экспорт тестового набора.")
    base.add_table_caption(document, "Таблица 7 - Результаты ручной проверки")
    base.add_report_table(document, ["Сценарий", "Ожидаемый результат", "Статус"], [
        ["Изменение периода", "Сводка и таблица пересчитаны", "Выполнено"],
        ["Фильтр незавершенных", "Оставлены соответствующие занятия", "Выполнено"],
        ["Открытие списка внимания", "Первый студент выбран автоматически", "Выполнено"],
        ["Добавление зачисления", "Студент появляется в курсе", "Выполнено"],
        ["Импорт JSON и CSV", "Пользователи добавлены или обновлены", "Выполнено"],
        ["Экспорт CSV со спецсимволами", "Значения заключены в кавычки", "Выполнено"],
    ], [6.1, 7.2, 2.4], 9.1)
    base.add_report_paragraph(document, "Проект собирается без ошибок. Проверка имеет характер ручной апробации и не заменяет автоматизированные модульные и браузерные тесты.")

    base.add_page_break(document)
    base.add_report_heading(document, "5.2 Ограничения и дальнейшее развитие", 2)
    base.add_report_paragraph(document, "Каталог и данные мониторинга хранятся в оперативной памяти браузера. Обновление страницы возвращает исходный набор. Импорт и экспорт работают с текстом в интерфейсе: загрузка файла и серверное хранилище в текущей версии отсутствуют.")
    base.add_report_paragraph(document, "Дальнейшее развитие связано с переносом расчетов и справочников на сервер. Для мониторинга полезны настраиваемые пороги риска, сравнение групп, динамика по неделям, уведомления и экспорт аналитической выборки в XLSX или PDF.")
    base.add_report_bullets(document, [
        "сохранять каталог и посещаемость в PostgreSQL;",
        "выполнять агрегирование через серверный API;",
        "добавить настраиваемые критерии группы внимания;",
        "реализовать загрузку и скачивание файлов импорта;",
        "добавить аудит административных операций;",
        "покрыть расчеты модульными тестами.",
    ])

    base.add_page_break(document)
    base.add_report_heading(document, "Заключение", 1)
    base.add_report_paragraph(document, "В ходе летней практики разработан клиентский модуль мониторинга посещаемости в интерфейсе omelea-dev-lms. Реализованы показатели количества и состояния занятий, распределение статусов, контроль неотмеченных строк и фильтрация по периоду.")
    base.add_report_paragraph(document, "Создан список студентов, требующих внимания, с сортировкой по проценту посещаемости, поиском, автоматическим выбором первого элемента и детальной карточкой выбранного студента.")
    base.add_report_paragraph(document, "Административная панель позволяет добавлять и удалять пользователей и курсы, формировать зачисления и синхронизировать состав занятий. Реализован текстовый импорт и экспорт пользователей в форматах JSON и CSV с обновлением записей по логину.")
    base.add_report_paragraph(document, "Цель практики достигнута на уровне рабочего клиентского прототипа. Для промышленной эксплуатации требуется постоянное серверное хранение, защищенный API и автоматизированная проверка расчетов.")


def build_efimenko_report() -> Path:
    EF_OUTPUT.mkdir(parents=True, exist_ok=True)
    path = EF_OUTPUT / "Отчет_по_летней_практике_ЕфименкоНП_v3.docx"
    document = Document()
    base.configure_report(document)
    document.core_properties.title = "Модуль учета посещаемости и тестовая ролевая модель"
    document.core_properties.author = "Ефименко Н.П."
    add_title_page(document, "Ефименко Н.П.", "Разработка модуля учета посещаемости и тестовой ролевой модели в LMS")
    add_assignment_pages(document, "Ефименко Николаю Павловичу", "Ефименко Н.П.", "Разработка модуля учета посещаемости и тестовой ролевой модели в LMS", EF_TASKS)
    add_abstract(document, 25, 7, 8, "УЧЕТ ПОСЕЩАЕМОСТИ, LMS, BLAZOR, ЭЛЕКТРОННЫЙ ЖУРНАЛ, ВАЛИДАЦИЯ, СТУДЕНТ, ПРЕПОДАВАТЕЛЬ, РОЛЕВАЯ МОДЕЛЬ", [
        "Цель практики состояла в переносе рабочего прототипа посещаемости в omelea-dev-lms и разработке журнала занятия, личного просмотра студента и тестового разграничения интерфейса.",
        "Реализованы статусы присутствия, опоздания, отсутствия и уважительной причины, правила ввода минут и комментариев, сохранение и отмена изменений, завершение и возобновление журнала.",
        "Тестовая ролевая модель связывает пользователя с дисциплинами и определяет доступные операции. Студент видит только собственные сведения без возможности изменять журнал.",
        "Прототип выполняется на стороне клиента и хранит состояние в памяти браузера. Полноценная серверная авторизация и постоянное хранение относятся к дальнейшему развитию.",
    ])
    add_contents(document, [
        ("Введение", "6"), ("1 Анализ задачи и исходных решений", "7"), ("1.1 Состав исходного прототипа", "7"),
        ("1.2 Целевой проект и граница изменений", "8"), ("1.3 Требования к модулю", "9"),
        ("2 Проектирование модуля посещаемости", "10"), ("2.1 Архитектура переноса", "10"),
        ("2.2 Модель данных", "11"), ("2.3 Статусы посещения", "12"), ("2.4 Тестовая ролевая модель", "13"),
        ("3 Реализация пользовательских сценариев", "14"), ("3.1 Журнал занятия преподавателя", "14"),
        ("3.2 Ввод отметок и комментариев", "15"), ("3.3 Валидация данных", "16"),
        ("3.4 Сохранение и жизненный цикл журнала", "17"), ("3.5 Личный просмотр студента", "18"),
        ("3.6 Разграничение поведения интерфейса", "19"), ("4 Проверка и оценка результата", "20"),
        ("4.1 Проверка пользовательских сценариев", "20"), ("4.2 Ограничения и дальнейшее развитие", "21"),
        ("Заключение", "22"), ("Список использованных источников", "23"),
        ("Приложение А. Журнал преподавателя", "24"), ("Приложение Б. Личный режим студента", "25"),
    ])
    add_efimenko_body(document)
    add_sources_and_appendices(
        document,
        "Журнал преподавателя",
        SCREENS / "02-teacher-attendance.png",
        "Рисунок А.1 - Полная страница журнала преподавателя",
        "Страница объединяет выбор занятия, сводку статусов, фильтры и редактируемые строки группы.",
        "Личный режим студента",
        SCREENS / "03-student-attendance.png",
        "Рисунок Б.1 - Полная страница личной посещаемости",
        "В личном режиме отсутствуют список группы и операции изменения отметок.",
    )
    document.save(path)
    return path


def build_loktev_report() -> Path:
    LOK_OUTPUT.mkdir(parents=True, exist_ok=True)
    path = LOK_OUTPUT / "Отчет_по_летней_практике_ЛоктевМС_v3.docx"
    document = Document()
    base.configure_report(document)
    document.core_properties.title = "Мониторинг и административный каталог посещаемости"
    document.core_properties.author = "Локтев М.С."
    add_title_page(document, "Локтев М.С.", "Разработка мониторинга и административных средств модуля посещаемости")
    add_assignment_pages(document, "Локтеву Максиму Сергеевичу", "Локтев М.С.", "Разработка мониторинга и административных средств модуля посещаемости", LOK_TASKS)
    add_abstract(document, 25, 7, 8, "МОНИТОРИНГ, АНАЛИТИЧЕСКИЕ ПОКАЗАТЕЛИ, LMS, BLAZOR, АДМИНИСТРАТИВНАЯ ПАНЕЛЬ, КУРС, ЗАЧИСЛЕНИЕ, JSON, CSV", [
        "Цель практики состояла в разработке мониторинга посещаемости и административных средств управления клиентскими справочниками omelea-dev-lms.",
        "Реализованы сводка по занятиям, контроль завершения и заполненности журналов, фильтры периода и состояния, распределение статусов и список студентов, требующих внимания.",
        "Административная панель поддерживает пользователей, курсы и зачисления. Для списка пользователей реализованы импорт и экспорт в форматах JSON и CSV.",
        "Реализация является клиентским прототипом: данные сохраняются в памяти браузера, а файловый и серверный обмен относятся к следующему этапу.",
    ])
    add_contents(document, [
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
        ("Заключение", "22"), ("Список использованных источников", "23"),
        ("Приложение А. Мониторинг", "24"), ("Приложение Б. Административная панель", "25"),
    ])
    add_loktev_body(document)
    add_sources_and_appendices(
        document,
        "Мониторинг посещаемости",
        ASSETS / "monitoring_main.png",
        "Рисунок А.1 - Основная область мониторинга",
        "Страница содержит сводные показатели, фильтры, список занятий и область студентов, требующих внимания.",
        "Административный каталог",
        ASSETS / "admin_main.png",
        "Рисунок Б.1 - Основная область административной панели",
        "Административная страница объединяет пользователей, обмен данными, курсы и зачисления.",
    )
    document.save(path)
    return path


def ppt_header(slide, title: str, number: int, student: str, accent: str) -> None:
    base.ppt_add_rect(slide, 0, 0, 13.333, 0.1, accent, accent)
    base.ppt_add_text(slide, 0.55, 0.25, 11.6, 0.55, title, 27, base.NAVY, True)
    base.ppt_add_text(slide, 12.2, 0.25, 0.55, 0.5, str(number), 15, base.GRAY, True, PP_ALIGN.RIGHT)
    base.ppt_add_rect(slide, 0.55, 7.12, 12.2, 0.02, base.MID, base.MID)
    base.ppt_add_text(slide, 0.55, 7.15, 8.0, 0.22, f"{student} · Б23-504 · летняя практика", 9, base.GRAY)


def title_slide(slide, title: str, student_full: str, image: Path, accent: str) -> None:
    base.ppt_set_background(slide, base.NAVY)
    base.ppt_add_picture_cover(slide, image, 5.3, 0, 8.033, 7.5, False)
    base.ppt_add_rect(slide, 0.55, 0.48, 0.12, 0.9, accent, accent)
    base.ppt_add_text(slide, 0.85, 0.46, 3.9, 0.42, "НИЯУ МИФИ · КАФЕДРА №22", 13, base.WHITE, True)
    base.ppt_add_text(slide, 0.55, 1.65, 4.25, 2.45, title, 29, base.WHITE, True, valign=MSO_ANCHOR.TOP)
    base.ppt_add_text(slide, 0.55, 4.35, 4.0, 0.45, "Отчет по летней практике", 18, "C9D4DF")
    base.ppt_add_text(slide, 0.55, 5.38, 4.1, 0.9, f"{student_full}\nБ23-504", 18, base.WHITE, True, valign=MSO_ANCHOR.TOP)
    base.ppt_add_text(slide, 0.55, 6.75, 4.1, 0.3, "Руководитель: Рословцев В.В. · Москва, 2026", 10, "C9D4DF")


def normalize_presentation_text_boxes(presentation: Presentation) -> None:
    for slide in presentation.slides:
        for shape in slide.shapes:
            if not shape.has_text_frame:
                continue
            shape.text_frame.margin_top = Inches(0)
            shape.text_frame.margin_bottom = Inches(0)


def add_goal_tasks_slide(slide, number: int, student: str, accent: str, goal: str, tasks: list[str]) -> None:
    base.ppt_set_background(slide)
    ppt_header(slide, "Цель и задачи", number, student, accent)
    base.ppt_add_rect(slide, 0.65, 1.05, 12.0, 1.0, base.NAVY, base.NAVY)
    base.ppt_add_text(slide, 0.95, 1.12, 11.4, 0.82, goal, 18, base.WHITE, True, PP_ALIGN.CENTER)
    colors = [accent, base.GREEN, base.PURPLE, base.ORANGE, base.RED]
    for index, task in enumerate(tasks):
        x = 0.65 + index * 2.43
        base.ppt_add_rect(slide, x, 2.62, 2.05, 2.65, base.LIGHT, base.MID)
        base.ppt_add_rect(slide, x + 0.15, 2.82, 0.55, 0.55, colors[index], colors[index])
        base.ppt_add_text(slide, x + 0.15, 2.82, 0.55, 0.55, str(index + 1), 19, base.WHITE, True, PP_ALIGN.CENTER)
        base.ppt_add_text(slide, x + 0.16, 3.48, 1.73, 1.5, task, 14, base.INK, True, PP_ALIGN.CENTER)


def build_efimenko_presentation() -> Path:
    EF_OUTPUT.mkdir(parents=True, exist_ok=True)
    path = EF_OUTPUT / "Презентация_ЕфименкоНП_v3.pptx"
    presentation = Presentation()
    presentation.slide_width = Inches(13.333)
    presentation.slide_height = Inches(7.5)
    blank = presentation.slide_layouts[6]

    slide = presentation.slides.add_slide(blank)
    title_slide(slide, "Модуль учета\nпосещаемости и тестовая\nролевая модель", "Ефименко Николай Павлович", SCREENS / "02-teacher-attendance.png", base.BLUE)

    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide)
    ppt_header(slide, "Постановка задачи", 2, "Ефименко Н.П.", base.BLUE)
    base.ppt_add_card(slide, 0.55, 1.1, 3.75, 4.95, "Исходный прототип", "Журнал и история посещаемости существовали в uir-main как отдельная экспериментальная часть.", base.PURPLE, 20, 18)
    base.ppt_add_card(slide, 4.78, 1.1, 3.75, 4.95, "Целевой проект", "omelea-dev-lms задавал дизайн, навигацию и самостоятельный конструктор ведомостей.", base.BLUE, 20, 18)
    base.ppt_add_card(slide, 9.0, 1.1, 3.75, 4.95, "Требуемый результат", "Перенести предметную логику, добавить личный режим и разграничить поведение интерфейса.", base.ORANGE, 20, 18)

    slide = presentation.slides.add_slide(blank)
    add_goal_tasks_slide(slide, 3, "Ефименко Н.П.", base.BLUE, "Цель: реализовать рабочий клиентский модуль учета посещаемости в omelea-dev-lms", ["Перенести модели и сервис", "Собрать журнал занятия", "Добавить статусы и проверки", "Создать личный режим", "Настроить тестовые роли"])

    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide)
    ppt_header(slide, "Граница изменений", 4, "Ефименко Н.П.", base.BLUE)
    base.ppt_add_picture_contain(slide, ASSETS / "statement_constructor.png", 0.55, 1.05, 7.4, 5.7)
    base.ppt_add_card(slide, 8.28, 1.05, 4.45, 2.35, "Конструктор сохранен", "StatementConstructor и StatementService не перерабатывались. Посещаемость размещена отдельным маршрутом.", base.GREEN, 20, 17)
    base.ppt_add_card(slide, 8.28, 3.72, 4.45, 2.35, "Интеграция", "Общие навигация, дизайн, каталог пользователей, курсов и зачислений.", base.BLUE, 20, 17)

    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide)
    ppt_header(slide, "Архитектура модуля", 5, "Ефименко Н.П.", base.BLUE)
    base.ppt_add_picture_contain(slide, ASSETS / "attendance_integration.png", 0.65, 1.0, 12.0, 5.8)

    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide)
    ppt_header(slide, "Модель данных и статусы", 6, "Ефименко Н.П.", base.BLUE)
    base.ppt_add_card(slide, 0.55, 1.05, 3.75, 4.95, "Занятие", "Дисциплина · группа\nТема · аудитория\nВремя · завершение\nКоллекция отметок", base.BLUE, 20, 18)
    base.ppt_add_card(slide, 4.78, 1.05, 3.75, 4.95, "Отметка", "Студент · статус\nМинуты опоздания\nКомментарий\nВремя изменения", base.PURPLE, 20, 18)
    base.ppt_add_card(slide, 9.0, 1.05, 3.75, 4.95, "Статусы", "Присутствовал\nОпоздал\nОтсутствовал\nУважительная причина\nНе отмечен", base.GREEN, 20, 18)

    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide)
    ppt_header(slide, "Журнал преподавателя", 7, "Ефименко Н.П.", base.BLUE)
    base.ppt_add_picture_contain(slide, ASSETS / "teacher_attendance.png", 0.55, 1.02, 8.25, 5.72)
    base.ppt_add_card(slide, 9.12, 1.02, 3.63, 5.72, "Рабочий сценарий", base.ppt_bullet_text(["выбор занятия", "поиск и фильтр", "сортировка строк", "массовая отметка", "минуты и комментарии", "сохранение и отмена"]), base.BLUE, 20, 16)

    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide)
    ppt_header(slide, "Валидация отметок", 8, "Ефименко Н.П.", base.BLUE)
    base.ppt_add_card(slide, 0.6, 1.1, 3.7, 4.9, "Опоздание", "Статус требует положительного числа минут. Ноль и пустое значение считаются ошибкой.", base.RED, 20, 18)
    base.ppt_add_card(slide, 4.82, 1.1, 3.7, 4.9, "Уважительная причина", "Для статуса обязателен непустой комментарий с пояснением.", base.ORANGE, 20, 18)
    base.ppt_add_card(slide, 9.04, 1.1, 3.7, 4.9, "Результат", "Ошибка показывается в строке и блокирует сохранение или завершение журнала.", base.GREEN, 20, 18)

    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide)
    ppt_header(slide, "Жизненный цикл журнала", 9, "Ефименко Н.П.", base.BLUE)
    for index, (title, body, color) in enumerate([
        ("1. Изменение", "Строка сравнивается с подтвержденной копией", base.BLUE),
        ("2. Сохранение", "Проверка данных, UpdatedAt и новая копия", base.GREEN),
        ("3. Завершение", "Только после заполнения всех строк", base.PURPLE),
        ("4. Возобновление", "Повторное открытие для корректировки", base.ORANGE),
    ]):
        x = 0.62 + index * 3.08
        base.ppt_add_card(slide, x, 1.35, 2.72, 4.65, title, body, color, 18, 17)

    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide)
    ppt_header(slide, "Личный режим студента", 10, "Ефименко Н.П.", base.BLUE)
    base.ppt_add_picture_contain(slide, ASSETS / "student_attendance.png", 0.55, 1.02, 8.25, 5.72)
    base.ppt_add_card(slide, 9.12, 1.02, 3.63, 5.72, "Только личные данные", "Доступны собственные дисциплины, сводка и история. Нет выбора другого студента, списка группы и операций редактирования.", base.GREEN, 20, 17)

    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide)
    ppt_header(slide, "Тестовая ролевая модель", 11, "Ефименко Н.П.", base.BLUE)
    base.ppt_add_picture_contain(slide, ASSETS / "role_model.png", 0.65, 1.0, 12.0, 5.8)

    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide)
    ppt_header(slide, "Проверка сценариев", 12, "Ефименко Н.П.", base.BLUE)
    checks = [
        ("Занятие", "Меняется весь контекст журнала", base.BLUE),
        ("Валидация", "Некорректные строки блокируются", base.RED),
        ("Черновик", "Сохранение и отмена работают", base.GREEN),
        ("Студент", "Редактирование отсутствует", base.PURPLE),
        ("Сборка", "Проект собирается без ошибок", base.ORANGE),
    ]
    for index, (title, body, color) in enumerate(checks):
        y = 1.05 + index * 1.05
        base.ppt_add_rect(slide, 0.75, y, 2.0, 0.78, color, color)
        base.ppt_add_text(slide, 0.85, y + 0.05, 1.8, 0.66, title, 17, base.WHITE, True, PP_ALIGN.CENTER)
        base.ppt_add_rect(slide, 2.75, y, 9.8, 0.78, base.LIGHT, base.MID)
        base.ppt_add_text(slide, 3.05, y + 0.05, 9.15, 0.66, body + " — выполнено", 17, base.INK)

    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide)
    ppt_header(slide, "Ограничения", 13, "Ефименко Н.П.", base.BLUE)
    base.ppt_add_card(slide, 0.6, 1.05, 5.75, 5.3, "Текущая версия", base.ppt_bullet_text(["состояние в памяти браузера", "демонстрационные токены", "проверки на стороне клиента", "нет серверного аудита", "ручная апробация"]), base.RED, 22, 18)
    base.ppt_add_card(slide, 6.95, 1.05, 5.75, 5.3, "Следующий этап", base.ppt_bullet_text(["PostgreSQL и API", "JWT-авторизация", "серверная проверка доступа", "история корректировок", "автоматические тесты"]), base.GREEN, 22, 18)

    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide)
    ppt_header(slide, "Результаты", 14, "Ефименко Н.П.", base.BLUE)
    base.ppt_add_rect(slide, 0.7, 1.05, 11.95, 1.0, base.NAVY, base.NAVY)
    base.ppt_add_text(slide, 1.0, 1.18, 11.35, 0.7, "Модуль посещаемости встроен в интерфейс omelea-dev-lms", 23, base.WHITE, True, PP_ALIGN.CENTER)
    results = [
        ("Журнал", "Полный контекст занятия", base.BLUE),
        ("Статусы", "Правила минут и комментариев", base.ORANGE),
        ("Студент", "Личная история без редактирования", base.GREEN),
        ("Доступ", "Три тестовых пользовательских режима", base.PURPLE),
    ]
    for index, (title, body, color) in enumerate(results):
        x = 0.7 + (index % 2) * 6.15
        y = 2.45 + (index // 2) * 1.72
        base.ppt_add_card(slide, x, y, 5.8, 1.35, title, body, color, 19, 16)

    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide, base.NAVY)
    base.ppt_add_text(slide, 0.7, 1.5, 11.95, 1.2, "Спасибо за внимание", 38, base.WHITE, True, PP_ALIGN.CENTER)
    base.ppt_add_rect(slide, 5.35, 3.05, 2.65, 0.08, base.ORANGE, base.ORANGE)
    base.ppt_add_text(slide, 0.7, 3.55, 11.95, 0.55, "Ефименко Николай Павлович", 22, base.WHITE, True, PP_ALIGN.CENTER)
    base.ppt_add_text(slide, 0.7, 4.2, 11.95, 0.45, "Б23-504 · 09.03.04 «Программная инженерия»", 17, "C9D4DF", False, PP_ALIGN.CENTER)

    normalize_presentation_text_boxes(presentation)
    presentation.core_properties.author = "Ефименко Н.П."
    presentation.core_properties.title = "Модуль учета посещаемости и тестовая ролевая модель"
    presentation.save(path)
    return path


def build_loktev_presentation() -> Path:
    LOK_OUTPUT.mkdir(parents=True, exist_ok=True)
    path = LOK_OUTPUT / "Презентация_ЛоктевМС_v3.pptx"
    presentation = Presentation()
    presentation.slide_width = Inches(13.333)
    presentation.slide_height = Inches(7.5)
    blank = presentation.slide_layouts[6]

    slide = presentation.slides.add_slide(blank)
    title_slide(slide, "Мониторинг и\nадминистративные средства\nмодуля посещаемости", "Локтев Максим Сергеевич", SCREENS / "09-teacher-monitoring-full.png", base.PURPLE)

    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide)
    ppt_header(slide, "Постановка задачи", 2, "Локтев М.С.", base.PURPLE)
    base.ppt_add_card(slide, 0.55, 1.1, 3.75, 4.95, "Проблема контроля", "Отдельные журналы не дают быстрой картины заполненности и динамики посещаемости.", base.RED, 20, 18)
    base.ppt_add_card(slide, 4.78, 1.1, 3.75, 4.95, "Проблема данных", "Состав групп и дисциплин нельзя удобно менять в демонстрационном интерфейсе.", base.ORANGE, 20, 18)
    base.ppt_add_card(slide, 9.0, 1.1, 3.75, 4.95, "Решение", "Общий мониторинг и административный каталог с JSON/CSV обменом.", base.PURPLE, 20, 18)

    slide = presentation.slides.add_slide(blank)
    add_goal_tasks_slide(slide, 3, "Локтев М.С.", base.PURPLE, "Цель: разработать мониторинг и административные средства модуля посещаемости", ["Определить показатели", "Собрать сводку занятий", "Выделить группу внимания", "Создать админ-панель", "Добавить JSON/CSV"])

    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide)
    ppt_header(slide, "Архитектура и данные", 4, "Локтев М.С.", base.PURPLE)
    base.ppt_add_picture_contain(slide, ASSETS / "monitoring_integration.png", 0.65, 1.0, 12.0, 5.8)

    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide)
    ppt_header(slide, "Аналитические показатели", 5, "Локтев М.С.", base.PURPLE)
    cards = [
        ("Занятия", "Всего\nЗавершено\nНе завершено", base.BLUE),
        ("Отметки", "Присутствовал\nОпоздал\nУважительная\nОтсутствовал", base.GREEN),
        ("Заполнение", "Неотмеченные строки\nПоследнее обновление", base.ORANGE),
        ("Студент", "Учтенные занятия\nПроцент посещаемости\nСчетчики статусов", base.PURPLE),
    ]
    for index, (title, body, color) in enumerate(cards):
        x = 0.62 + index * 3.08
        base.ppt_add_card(slide, x, 1.15, 2.72, 4.9, title, body, color, 20, 17)

    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide)
    ppt_header(slide, "Сводка по занятиям", 6, "Локтев М.С.", base.PURPLE)
    base.ppt_add_picture_contain(slide, ASSETS / "monitoring_main.png", 0.55, 1.02, 8.25, 5.72)
    base.ppt_add_card(slide, 9.12, 1.02, 3.63, 5.72, "На одном экране", base.ppt_bullet_text(["объем периода", "состояние журналов", "распределение статусов", "неотмеченные строки", "таблица занятий", "последнее изменение"]), base.PURPLE, 20, 16)

    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide)
    ppt_header(slide, "Фильтры и контроль заполнения", 7, "Локтев М.С.", base.PURPLE)
    base.ppt_add_card(slide, 0.6, 1.1, 3.7, 4.9, "Период", "Ограничивает занятия по дате начала и пересчитывает все показатели.", base.BLUE, 20, 18)
    base.ppt_add_card(slide, 4.82, 1.1, 3.7, 4.9, "Состояние", "Позволяет оставить завершенные, незавершенные или требующие заполнения журналы.", base.ORANGE, 20, 18)
    base.ppt_add_card(slide, 9.04, 1.1, 3.7, 4.9, "Переход", "Строка занятия открывает соответствующий журнал для проверки и заполнения.", base.GREEN, 20, 18)

    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide)
    ppt_header(slide, "Студенты, требующие внимания", 8, "Локтев М.С.", base.PURPLE)
    base.ppt_add_picture_contain(slide, ASSETS / "monitoring_attention.png", 0.55, 1.02, 8.25, 5.72)
    base.ppt_add_card(slide, 9.12, 1.02, 3.63, 5.72, "Приоритет", "Список сортируется по проценту посещаемости. Сразу выбран первый студент; другого можно найти или выбрать вручную.", base.RED, 20, 17)

    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide)
    ppt_header(slide, "Карточка выбранного студента", 9, "Локтев М.С.", base.PURPLE)
    base.ppt_add_card(slide, 0.7, 1.1, 3.7, 4.95, "Автовыбор", "Информационная область сразу показывает первый элемент актуального списка.", base.GREEN, 20, 18)
    base.ppt_add_card(slide, 4.82, 1.1, 3.7, 4.95, "Показатели", "Процент, число учтенных занятий, присутствия, опоздания и пропуски.", base.PURPLE, 20, 18)
    base.ppt_add_card(slide, 8.94, 1.1, 3.7, 4.95, "Детализация", "Переход к истории позволяет проверить занятия и комментарии выбранного студента.", base.BLUE, 20, 18)

    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide)
    ppt_header(slide, "Административная панель", 10, "Локтев М.С.", base.PURPLE)
    base.ppt_add_picture_contain(slide, ASSETS / "admin_main.png", 0.55, 1.02, 8.25, 5.72)
    base.ppt_add_card(slide, 9.12, 1.02, 3.63, 5.72, "Каталог", base.ppt_bullet_text(["пользователи", "курсы", "зачисления", "синхронизация групп", "JSON/CSV обмен"]), base.ORANGE, 20, 17)

    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide)
    ppt_header(slide, "Управление пользователями", 11, "Локтев М.С.", base.PURPLE)
    base.ppt_add_card(slide, 0.6, 1.1, 3.7, 4.9, "Добавление", "Логин, ФИО и номер зачетной книжки. Обязательные поля проверяются.", base.BLUE, 20, 18)
    base.ppt_add_card(slide, 4.82, 1.1, 3.7, 4.9, "Изменение", "Можно переключить административный признак; последний администратор защищен.", base.ORANGE, 20, 18)
    base.ppt_add_card(slide, 9.04, 1.1, 3.7, 4.9, "Удаление", "Связанные зачисления удаляются, а состав занятий синхронизируется.", base.RED, 20, 18)

    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide)
    ppt_header(slide, "Курсы и зачисления", 12, "Локтев М.С.", base.PURPLE)
    base.ppt_add_picture_contain(slide, ASSETS / "admin_courses.png", 0.55, 1.02, 8.25, 5.72)
    base.ppt_add_card(slide, 9.12, 1.02, 3.63, 5.72, "Связанные данные", "Курс задает код, название, группу и аудиторию. Зачисление связывает пользователя с курсом и типом участия.", base.PURPLE, 20, 17)

    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide)
    ppt_header(slide, "Импорт и экспорт JSON/CSV", 13, "Локтев М.С.", base.PURPLE)
    base.ppt_add_card(slide, 0.6, 1.05, 5.75, 5.3, "Экспорт", base.ppt_bullet_text(["четыре поля пользователя", "форматированный JSON", "CSV с заголовком", "экранирование спецсимволов", "результат в текстовом поле"]), base.BLUE, 22, 18)
    base.ppt_add_card(slide, 6.95, 1.05, 5.75, 5.3, "Импорт", base.ppt_bullet_text(["разбор JSON или CSV", "проверка непустого набора", "добавление новых записей", "обновление по логину", "пропуск некорректных строк"]), base.GREEN, 22, 18)

    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide)
    ppt_header(slide, "Результаты и ограничения", 14, "Локтев М.С.", base.PURPLE)
    base.ppt_add_card(slide, 0.6, 1.05, 5.75, 5.3, "Реализовано", base.ppt_bullet_text(["сводка по занятиям", "фильтры и контроль заполнения", "группа внимания", "административный каталог", "JSON/CSV обмен", "синхронизация данных"]), base.PURPLE, 22, 17)
    base.ppt_add_card(slide, 6.95, 1.05, 5.75, 5.3, "Ограничения", base.ppt_bullet_text(["данные в памяти браузера", "нет серверного API", "обмен через текстовое поле", "нет файлового хранилища", "ручная апробация"]), base.RED, 22, 18)

    slide = presentation.slides.add_slide(blank)
    base.ppt_set_background(slide, base.NAVY)
    base.ppt_add_text(slide, 0.7, 1.5, 11.95, 1.2, "Спасибо за внимание", 38, base.WHITE, True, PP_ALIGN.CENTER)
    base.ppt_add_rect(slide, 5.35, 3.05, 2.65, 0.08, base.PURPLE, base.PURPLE)
    base.ppt_add_text(slide, 0.7, 3.55, 11.95, 0.55, "Локтев Максим Сергеевич", 22, base.WHITE, True, PP_ALIGN.CENTER)
    base.ppt_add_text(slide, 0.7, 4.2, 11.95, 0.45, "Б23-504 · 09.03.04 «Программная инженерия»", 17, "C9D4DF", False, PP_ALIGN.CENTER)

    normalize_presentation_text_boxes(presentation)
    presentation.core_properties.author = "Локтев М.С."
    presentation.core_properties.title = "Мониторинг и административные средства модуля посещаемости"
    presentation.save(path)
    return path


def write_memo() -> Path:
    OUTPUT.mkdir(parents=True, exist_ok=True)
    path = OUTPUT / "Памятка_v3_по_содержанию_и_расхождениям.md"
    path.write_text(
        """# Памятка к материалам v3

## Разделение работ

- Ефименко Н.П.: модуль учета посещаемости, журнал преподавателя, статусы, минуты опоздания и комментарии, валидация, сохранение и завершение журнала, личный режим студента, разграничение интерфейса и тестовая ролевая модель.
- Локтев М.С.: мониторинг, показатели и сводка по занятиям, список студентов, требующих внимания, административная панель пользователей, курсов и зачислений, импорт и экспорт JSON/CSV.

В персональных отчетах фамилия второго участника не упоминается. В отчете Локтева тестовая ролевая модель не заявлена как результат работы.

## Где формулировки слегка сильнее реализации

- «Апробация» означает ручное прохождение основных маршрутов и успешную сборку, а не полный набор автоматизированных тестов.
- «Сохранение журнала» фиксирует подтвержденную копию в памяти браузера. После F5 данные возвращаются к демонстрационному набору.
- Тестовая ролевая модель Ефименко управляет клиентским интерфейсом и доступными дисциплинами, но не является защищенной серверной авторизацией.
- Импорт и экспорт Локтева работают с JSON/CSV-текстом в административной странице. Автоматическое чтение или скачивание файла и серверная база данных не реализованы.
- «Административный каталог» хранится в памяти текущего сеанса и синхронизирует демонстрационные данные приложения.
- Процент посещаемости рассчитывается по завершенным и учтенным занятиям; уважительная причина не входит в числитель присутствия.

## Что еще можно добавить в модуль посещаемости

1. Постоянное хранение журналов в PostgreSQL и серверный API.
2. Создание занятий из расписания и автоматическое формирование состава группы на дату занятия.
3. Журнал аудита: кто, когда и с какого значения изменил отметку.
4. Согласование исправлений после завершения занятия с причиной и подтверждающим файлом.
5. QR-регистрация или самостоятельная отметка студента в ограниченное время с подтверждением преподавателя.
6. Уведомления о незаполненных журналах и серии пропусков.
7. Настраиваемые правила: пороги риска, допустимое опоздание, веса статусов.
8. Экспорт журнала и аналитики в XLSX/PDF, а также загрузка отметок из CSV.
9. Связь посещаемости с ведомостью: отдельный показатель или условие допуска без изменения базового конструктора.
10. Офлайн-черновик, синхронизация и обработка конфликтов одновременного редактирования.
11. Прикрепление документов для уважительной причины и срок их действия.
12. Автоматические модульные и браузерные тесты ключевых сценариев.
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
