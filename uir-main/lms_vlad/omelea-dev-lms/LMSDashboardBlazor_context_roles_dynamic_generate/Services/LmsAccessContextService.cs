using LMSDashboardBlazor.Models;

namespace LMSDashboardBlazor.Services;

public class LmsAccessContextService
{
    public IReadOnlyList<CourseAccessContext> GetAccessContexts(string? username)
    {
        username ??= "svm011";

        return new List<CourseAccessContext>
        {
            new()
            {
                CourseCode = "СТИС",
                CourseName = "Системы и технологии интеллектуальных систем",
                Role = LmsCourseRole.Student,
                Permissions = new[]
                {
                    "Просмотр материалов",
                    "Сдача заданий",
                    "Просмотр собственных оценок"
                }
            },
            new()
            {
                CourseCode = "ПП",
                CourseName = "Параллельное программирование",
                Role = LmsCourseRole.Teacher,
                Permissions = new[]
                {
                    "Редактирование материалов",
                    "Проверка работ",
                    "Выставление оценок"
                }
            },
            new()
            {
                CourseCode = "BICA",
                CourseName = "Biologically Inspired Cognitive Architectures",
                Role = LmsCourseRole.Assistant,
                Permissions = new[]
                {
                    "Просмотр списка группы",
                    "Консультации по заданиям",
                    "Проверка лабораторных"
                }
            }
        };
    }
}
