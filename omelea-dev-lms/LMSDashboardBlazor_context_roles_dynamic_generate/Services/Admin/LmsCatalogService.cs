using LMSDashboardBlazor.Models;
using System.Text;
using System.Text.Json;
using LMSDashboardBlazor.Models.Attendance;

namespace LMSDashboardBlazor.Services.Admin;

public sealed class LmsCatalogService
{
    private readonly List<LmsUserModel> users;
    private readonly List<LmsCourseModel> courses;
    private readonly List<LmsEnrollmentModel> enrollments;

    public LmsCatalogService()
    {
        users =
        [
            new() { Id = "u-admin", Username = "admin", FullName = "Анна Админова", IsAdmin = true },
            new() { Id = "u-teacher", Username = "teacher", FullName = "Сергей Петров" },
            new() { Id = "u-assistant", Username = "assistant", FullName = "Ольга Соколова" },
            new() { Id = "u-student", Username = "student", FullName = "Иван Иванов", RecordBookNumber = "Б23-504-01" },
            new() { Id = "u-student2", Username = "student2", FullName = "Мария Смирнова", RecordBookNumber = "Б23-504-02" },
            new() { Id = "u-student3", Username = "student3", FullName = "Алексей Сидоров", RecordBookNumber = "Б23-504-03" },
            new() { Id = "u-student4", Username = "student4", FullName = "Елена Кузнецова", RecordBookNumber = "Б23-504-04" },
            new() { Id = "u-student5", Username = "student5", FullName = "Дмитрий Попов", RecordBookNumber = "Б23-504-05" },
            new() { Id = "u-student6", Username = "student6", FullName = "Анна Волкова", RecordBookNumber = "Б23-504-06" }
        ];

        courses =
        [
            new()
            {
                Id = "c-stis",
                Code = "СТИС",
                Name = "Системы и технологии интеллектуальных систем",
                GroupName = "Б23-504",
                DefaultRoom = "Б-214"
            },
            new()
            {
                Id = "c-pp",
                Code = "ПП",
                Name = "Параллельное программирование",
                GroupName = "Б23-504",
                DefaultRoom = "А-312"
            },
            new()
            {
                Id = "c-bica",
                Code = "BICA",
                Name = "Biologically Inspired Cognitive Architectures",
                GroupName = "Б23-504",
                DefaultRoom = "Б-305"
            }
        ];

        enrollments =
        [
            new() { Id = "e-teacher-stis", UserId = "u-teacher", CourseId = "c-stis", Role = LmsCourseRole.Teacher },
            new() { Id = "e-teacher-pp", UserId = "u-teacher", CourseId = "c-pp", Role = LmsCourseRole.Teacher },
            new() { Id = "e-assistant-bica", UserId = "u-assistant", CourseId = "c-bica", Role = LmsCourseRole.Assistant },
            new() { Id = "e-student-stis", UserId = "u-student", CourseId = "c-stis", Role = LmsCourseRole.Student },
            new() { Id = "e-student-pp", UserId = "u-student", CourseId = "c-pp", Role = LmsCourseRole.Student },
            new() { Id = "e-student2-stis", UserId = "u-student2", CourseId = "c-stis", Role = LmsCourseRole.Student },
            new() { Id = "e-student2-pp", UserId = "u-student2", CourseId = "c-pp", Role = LmsCourseRole.Student },
            new() { Id = "e-student2-bica", UserId = "u-student2", CourseId = "c-bica", Role = LmsCourseRole.Student },
            new() { Id = "e-student3-stis", UserId = "u-student3", CourseId = "c-stis", Role = LmsCourseRole.Student },
            new() { Id = "e-student4-stis", UserId = "u-student4", CourseId = "c-stis", Role = LmsCourseRole.Student },
            new() { Id = "e-student5-stis", UserId = "u-student5", CourseId = "c-stis", Role = LmsCourseRole.Student },
            new() { Id = "e-student6-pp", UserId = "u-student6", CourseId = "c-pp", Role = LmsCourseRole.Student }
        ];
    }

    public event Action? Changed;

    public IReadOnlyList<LmsUserModel> Users => users;
    public IReadOnlyList<LmsCourseModel> Courses => courses;
    public IReadOnlyList<LmsEnrollmentModel> Enrollments => enrollments;

    public void Load(IEnumerable<LmsUserModel> loadedUsers, IEnumerable<LmsCourseModel> loadedCourses, IEnumerable<LmsEnrollmentModel> loadedEnrollments)
    {
        users.Clear(); users.AddRange(loadedUsers);
        courses.Clear(); courses.AddRange(loadedCourses);
        enrollments.Clear(); enrollments.AddRange(loadedEnrollments);
        Changed?.Invoke();
    }

    public LmsUserModel? GetUserByUsername(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        return users.FirstOrDefault(user =>
            string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase));
    }

    public bool IsAdmin(string? username)
    {
        return GetUserByUsername(username)?.IsAdmin == true;
    }

    public IReadOnlyList<CourseAccessContext> GetAccessContexts(string? username)
    {
        var user = GetUserByUsername(username);
        if (user is null)
        {
            return Array.Empty<CourseAccessContext>();
        }

        if (user.IsAdmin)
        {
            return courses.Select(course => new CourseAccessContext
            {
                CourseCode = course.Code,
                CourseName = course.Name,
                Role = LmsCourseRole.Admin,
                Description = $"Администрирование курса {course.GroupName}",
                Permissions =
                [
                    "Управление пользователями",
                    "Управление курсами",
                    "Назначение ролей",
                    "Просмотр всех журналов"
                ]
            }).ToList();
        }

        return enrollments
            .Where(enrollment => enrollment.UserId == user.Id)
            .Join(courses,
                enrollment => enrollment.CourseId,
                course => course.Id,
                (enrollment, course) => new CourseAccessContext
                {
                    CourseCode = course.Code,
                    CourseName = course.Name,
                    Role = enrollment.Role,
                    Description = $"{GetRoleLabel(enrollment.Role)} в группе {course.GroupName}",
                    Permissions = GetPermissions(enrollment.Role)
                })
            .OrderBy(context => context.CourseCode)
            .ToList();
    }

    public IReadOnlyList<LmsCourseModel> GetCoursesForUser(string? username)
    {
        var user = GetUserByUsername(username);
        if (user is null)
        {
            return Array.Empty<LmsCourseModel>();
        }

        if (user.IsAdmin)
        {
            return courses.OrderBy(course => course.Code).ToList();
        }

        var courseIds = enrollments
            .Where(enrollment => enrollment.UserId == user.Id)
            .Select(enrollment => enrollment.CourseId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return courses
            .Where(course => courseIds.Contains(course.Id))
            .OrderBy(course => course.Code)
            .ToList();
    }

    public IReadOnlyList<LmsUserModel> GetStudentsForCourse(string courseId)
    {
        var studentIds = enrollments
            .Where(enrollment => enrollment.CourseId == courseId && enrollment.Role == LmsCourseRole.Student)
            .Select(enrollment => enrollment.UserId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return users
            .Where(user => studentIds.Contains(user.Id))
            .OrderBy(user => user.FullName)
            .ToList();
    }

    public IReadOnlyList<LmsEnrollmentModel> GetCourseEnrollments(string courseId)
    {
        return enrollments
            .Where(enrollment => enrollment.CourseId == courseId)
            .OrderBy(enrollment => GetUser(enrollment.UserId)?.FullName)
            .ThenBy(enrollment => enrollment.Role)
            .ToList();
    }

    public LmsUserModel? GetUser(string userId)
    {
        return users.FirstOrDefault(user => user.Id == userId);
    }

    public LmsCourseModel? GetCourse(string courseId)
    {
        return courses.FirstOrDefault(course => course.Id == courseId);
    }

    public LmsCourseRole? GetCourseRole(string? username, string courseId)
    {
        var user = GetUserByUsername(username);
        if (user is null)
        {
            return null;
        }

        if (user.IsAdmin)
        {
            return LmsCourseRole.Admin;
        }

        return enrollments
            .Where(enrollment => enrollment.UserId == user.Id && enrollment.CourseId == courseId)
            .OrderBy(enrollment => enrollment.Role switch { LmsCourseRole.Teacher => 0, LmsCourseRole.Assistant => 1, LmsCourseRole.Curator => 2, _ => 3 })
            .FirstOrDefault()
            ?.Role;
    }

    public void AddUser(string username, string fullName, string recordBookNumber, bool isAdmin)
    {
        username = username.Trim();
        fullName = fullName.Trim();
        recordBookNumber = recordBookNumber.Trim();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(fullName))
        {
            throw new InvalidOperationException("Укажите логин и ФИО пользователя.");
        }

        if (users.Any(user => string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Пользователь с таким логином уже есть.");
        }

        users.Add(new LmsUserModel
        {
            Username = username,
            FullName = fullName,
            RecordBookNumber = recordBookNumber,
            IsAdmin = isAdmin
        });

        Changed?.Invoke();
    }

    public (int Created, int Enrolled) ImportOcrStudents(string? actor, string courseId, IEnumerable<OcrImportRow> rows)
    {
        if (!IsAdmin(actor)) throw new InvalidOperationException("Импорт доступен только администратору.");
        if (GetCourse(courseId) is null) throw new InvalidOperationException("Курс не найден.");
        var selected = rows.Where(row => row.Include).ToList();
        if (selected.Count == 0) throw new InvalidOperationException("Выберите хотя бы одну строку.");
        var pendingUsers = new List<LmsUserModel>();
        var pendingEnrollments = new List<LmsEnrollmentModel>();
        var ids = new HashSet<string>();
        var logins = users.Select(user => user.Username).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var newNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in selected)
        {
            LmsUserModel user;
            if (row.StudentId == "__new")
            {
                var fullName = row.FullName.Trim();
                var login = row.Username.Trim();
                if (fullName.Length < 3 || fullName.Length > 200 || login.Length < 1 || login.Length > 100 || login.Any(char.IsWhiteSpace))
                    throw new InvalidOperationException("Проверьте ФИО и логины новых студентов (логин без пробелов).");
                if (!logins.Add(login)) throw new InvalidOperationException($"Логин {login} уже используется.");
                if (!newNames.Add(fullName)) throw new InvalidOperationException("В списке есть повторяющиеся ФИО новых студентов.");
                user = new LmsUserModel { Username = login, FullName = fullName, RecordBookNumber = row.RecordBookNumber.Trim() };
                pendingUsers.Add(user);
            }
            else user = GetUser(row.StudentId) ?? throw new InvalidOperationException("Выберите пользователя для каждой строки.");
            if (!ids.Add(user.Id)) throw new InvalidOperationException("Один пользователь выбран несколько раз.");
            if (!enrollments.Any(item => item.UserId == user.Id && item.CourseId == courseId && item.Role == LmsCourseRole.Student))
                pendingEnrollments.Add(new LmsEnrollmentModel { UserId = user.Id, CourseId = courseId, Role = LmsCourseRole.Student });
        }
        // Validate the whole batch before changing the catalog or firing synchronization.
        users.AddRange(pendingUsers);
        enrollments.AddRange(pendingEnrollments);
        Changed?.Invoke();
        return (pendingUsers.Count, pendingEnrollments.Count);
    }

    public void ToggleAdmin(string userId)
    {
        var user = GetUser(userId) ?? throw new InvalidOperationException("Пользователь не найден.");
        if (user.IsAdmin && users.Count(item => item.IsAdmin) == 1)
            throw new InvalidOperationException("Нельзя снять права последнего администратора.");
        user.IsAdmin = !user.IsAdmin;
        Changed?.Invoke();
    }

    public void RemoveUser(string userId)
    {
        var user = GetUser(userId) ?? throw new InvalidOperationException("Пользователь не найден.");
        if (user.IsAdmin && users.Count(item => item.IsAdmin) == 1)
        {
            throw new InvalidOperationException("Нельзя удалить последнего администратора.");
        }

        enrollments.RemoveAll(enrollment => enrollment.UserId == userId);
        users.Remove(user);
        Changed?.Invoke();
    }

    public string ExportUsersJson()
    {
        return JsonSerializer.Serialize(
            users.Select(ToTransferModel),
            new JsonSerializerOptions { WriteIndented = true });
    }

    public string ExportUsersCsv()
    {
        var builder = new StringBuilder();
        builder.AppendLine("username,fullName,recordBookNumber,isAdmin");

        foreach (var user in users.OrderBy(item => item.Username))
        {
            builder.Append(EscapeCsv(user.Username));
            builder.Append(',');
            builder.Append(EscapeCsv(user.FullName));
            builder.Append(',');
            builder.Append(EscapeCsv(user.RecordBookNumber));
            builder.Append(',');
            builder.AppendLine(user.IsAdmin ? "true" : "false");
        }

        return builder.ToString();
    }

    public int ImportUsersJson(string payload)
    {
        var imported = JsonSerializer.Deserialize<List<UserTransferModel>>(
            payload,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (imported is null || imported.Count == 0)
        {
            throw new InvalidOperationException("JSON не содержит пользователей.");
        }

        return UpsertUsers(imported);
    }

    public int ImportUsersCsv(string payload)
    {
        var lines = payload
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        if (lines.Count < 2)
        {
            throw new InvalidOperationException("CSV должен содержать заголовок и хотя бы одну строку.");
        }

        var imported = new List<UserTransferModel>();
        foreach (var line in lines.Skip(1))
        {
            var cells = ParseCsvLine(line);
            if (cells.Count < 2)
            {
                continue;
            }

            imported.Add(new UserTransferModel
            {
                Username = cells.ElementAtOrDefault(0) ?? string.Empty,
                FullName = cells.ElementAtOrDefault(1) ?? string.Empty,
                RecordBookNumber = cells.ElementAtOrDefault(2) ?? string.Empty,
                IsAdmin = ParseBoolean(cells.ElementAtOrDefault(3))
            });
        }

        if (imported.Count == 0)
        {
            throw new InvalidOperationException("CSV не содержит корректных пользователей.");
        }

        return UpsertUsers(imported);
    }

    public void AddCourse(string code, string name, string groupName, string defaultRoom)
    {
        code = code.Trim();
        name = name.Trim();
        groupName = groupName.Trim();
        defaultRoom = defaultRoom.Trim();

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(groupName))
        {
            throw new InvalidOperationException("Укажите код, название и группу курса.");
        }

        if (courses.Any(course => string.Equals(course.Code, code, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Курс с таким кодом уже есть.");
        }

        courses.Add(new LmsCourseModel
        {
            Code = code,
            Name = name,
            GroupName = groupName,
            DefaultRoom = string.IsNullOrWhiteSpace(defaultRoom) ? "Не указана" : defaultRoom
        });

        Changed?.Invoke();
    }

    public void RemoveCourse(string courseId)
    {
        var course = GetCourse(courseId) ?? throw new InvalidOperationException("Курс не найден.");
        enrollments.RemoveAll(enrollment => enrollment.CourseId == courseId);
        courses.Remove(course);
        Changed?.Invoke();
    }

    public void AddEnrollment(string userId, string courseId, LmsCourseRole role)
    {
        if (role == LmsCourseRole.Admin)
        {
            throw new InvalidOperationException("Администратор задается в карточке пользователя, а не в зачислении.");
        }

        if (GetUser(userId) is null || GetCourse(courseId) is null)
        {
            throw new InvalidOperationException("Выберите пользователя и курс.");
        }

        if (enrollments.Any(enrollment =>
            enrollment.UserId == userId &&
            enrollment.CourseId == courseId &&
            enrollment.Role == role))
        {
            throw new InvalidOperationException("Такое зачисление уже есть.");
        }

        enrollments.Add(new LmsEnrollmentModel
        {
            UserId = userId,
            CourseId = courseId,
            Role = role
        });

        Changed?.Invoke();
    }

    public void RemoveEnrollment(string enrollmentId)
    {
        var enrollment = enrollments.FirstOrDefault(item => item.Id == enrollmentId) ??
            throw new InvalidOperationException("Зачисление не найдено.");

        enrollments.Remove(enrollment);
        Changed?.Invoke();
    }

    public static string GetRoleLabel(LmsCourseRole role)
    {
        return role switch
        {
            LmsCourseRole.Admin => "Администратор",
            LmsCourseRole.Student => "Студент",
            LmsCourseRole.Teacher => "Преподаватель",
            LmsCourseRole.Assistant => "Ассистент",
            LmsCourseRole.Curator => "Куратор",
            _ => role.ToString()
        };
    }

    public static IReadOnlyList<string> GetPermissions(LmsCourseRole role)
    {
        return role switch
        {
            LmsCourseRole.Admin =>
            [
                "Управление каталогом LMS",
                "Просмотр всех курсов",
                "Назначение ролей"
            ],
            LmsCourseRole.Teacher =>
            [
                "Редактирование материалов",
                "Заполнение посещаемости",
                "Выставление оценок",
                "Мониторинг группы"
            ],
            LmsCourseRole.Assistant =>
            [
                "Просмотр списка группы",
                "Помощь с журналом",
                "Проверка лабораторных"
            ],
            LmsCourseRole.Curator =>
            [
                "Просмотр группы",
                "Мониторинг риска",
                "Коммуникация со студентами"
            ],
            _ =>
            [
                "Просмотр материалов",
                "Просмотр собственной посещаемости",
                "Просмотр собственных оценок"
            ]
        };
    }

    private int UpsertUsers(IEnumerable<UserTransferModel> imported)
    {
        var changed = 0;

        foreach (var item in imported)
        {
            var username = item.Username.Trim();
            var fullName = item.FullName.Trim();
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(fullName))
            {
                continue;
            }

            var existing = users.FirstOrDefault(user =>
                string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase));

            if (existing is null)
            {
                users.Add(new LmsUserModel
                {
                    Username = username,
                    FullName = fullName,
                    RecordBookNumber = item.RecordBookNumber.Trim(),
                    IsAdmin = item.IsAdmin
                });
            }
            else
            {
                existing.FullName = fullName;
                existing.RecordBookNumber = item.RecordBookNumber.Trim();
                existing.IsAdmin = item.IsAdmin;
            }

            changed++;
        }

        if (users.All(user => !user.IsAdmin))
        {
            throw new InvalidOperationException("В каталоге должен оставаться хотя бы один администратор.");
        }

        Changed?.Invoke();
        return changed;
    }

    private static UserTransferModel ToTransferModel(LmsUserModel user)
    {
        return new UserTransferModel
        {
            Username = user.Username,
            FullName = user.FullName,
            RecordBookNumber = user.RecordBookNumber,
            IsAdmin = user.IsAdmin
        };
    }

    private static string EscapeCsv(string? value)
    {
        var text = value ?? string.Empty;
        if (text.Contains('"') || text.Contains(',') || text.Contains('\n') || text.Contains('\r'))
        {
            return $"\"{text.Replace("\"", "\"\"")}\"";
        }

        return text;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var cells = new List<string>();
        var builder = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var symbol = line[i];
            if (symbol == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    builder.Append('"');
                    i++;
                    continue;
                }

                inQuotes = !inQuotes;
                continue;
            }

            if (symbol == ',' && !inQuotes)
            {
                cells.Add(builder.ToString());
                builder.Clear();
                continue;
            }

            builder.Append(symbol);
        }

        cells.Add(builder.ToString());
        return cells;
    }

    private static bool ParseBoolean(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            "true" or "1" or "yes" or "да" => true,
            _ => false
        };
    }

    private sealed class UserTransferModel
    {
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string RecordBookNumber { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
    }
}
