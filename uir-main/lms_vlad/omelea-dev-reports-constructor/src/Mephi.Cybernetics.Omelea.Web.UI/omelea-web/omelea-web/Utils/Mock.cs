using Bogus;
using Mephi.Cybernetics.Omelea.Core.Models.Mock;
using omelea_web.Pages;
using Person = Mephi.Cybernetics.Omelea.Core.Models.Mock.Person;

namespace omelea_web.Utils
{
    public static class Mock
    {

        private static List<AppraisalList> _fakeAppraisalListResults = new List<AppraisalList>();

        public static List<AppraisalList> FakeAppraisalListResults => _fakeAppraisalListResults;
        public static AppraisalList FakeAppraisalList()
        {

            var result = new Faker<AppraisalList>()
            .RuleFor(a => a.Id, f => f.Random.Guid())
            .RuleFor(a => a.Name, f =>
                $"{f.Random.ArrayElement(new[] { "Б", "С", "М" })}{f.Random.Number(19, 24)}-{f.Random.Number(510, 535)}"
            )
            .RuleFor(a => a.Description, f => f.Lorem.Text())
            .RuleFor(c => c.Year, f => f.Random.Int(2000, 2024))
            .RuleFor(c => c.DateCreated, f => f.Date.Past())
            .RuleFor(c => c.AppraisalListColumns, FakeAppraisalListColumns());

            return result;
        }

        public static CourseInstance FakeCourseInstance()
    {

            return new Faker<CourseInstance>()
                .RuleFor(c => c.CourseId, f => f.Random.Guid())
                .RuleFor(c => c.Year, f => f.Random.Int(2000, 2024))
                .RuleFor(c => c.CreatedAt, f => f.Date.Past())
                .RuleFor(c => c.AppraisalLists, f =>
                    {
                        int count = f.Random.Number(1, 2);
                        var lists = new List<AppraisalList>();

                        for (int i = 0; i < count; i++)
                        {
                            var fakeList = FakeAppraisalList();

                            _fakeAppraisalListResults.Add(fakeList);
                            lists.Add(fakeList);
                        }

                        return lists;
                    }
                 );
    }
        public static Course FakeCourse(string CourseName)
        {

            return new Faker<Course>()
                .RuleFor(c => c.Id, f => f.Random.Guid())
                .RuleFor(c => c.CourseName, f => CourseName)
                .RuleFor(c => c.Description, f => f.Lorem.Text())
                .RuleFor(c => c.CourseInstances, f =>
                {
                    int count = f.Random.Number(1, 2);
                    var lists = new List<CourseInstance>();

                    for (int i = 0; i < count; i++)
                    {
                        lists.Add(FakeCourseInstance());
                    }

                    return lists;
                }
                 );
        }

        public static List<Person> FakePeople()
        {
            return new List<Person>
            {
                new Person { Id = Guid.Parse("4dc67e32-bded-48d8-b521-e46c18e048b8"), LastName = "Иванов" },
                new Person { Id = Guid.Parse("a8acbd3b-61a4-4eb9-85c6-33c55ea2ee41"), LastName = "Смирнов" },
                new Person { Id = Guid.Parse("ea0bf550-2aac-4394-9cf5-69a6438ca90b"), LastName = "Петров" },
                new Person { Id = Guid.Parse("4cd8d62d-9080-4b0d-a82e-9e4288b7faac"), LastName = "Соколов" },
                new Person { Id = Guid.Parse("7a7b91f0-f440-4266-a6d0-961df387cad8"), LastName = "Попов" },
                new Person { Id = Guid.Parse("a7a22e9c-af2d-48e6-8df1-1bb4480e01c0"), LastName = "Лебедев" },
                new Person { Id = Guid.Parse("e31033a6-22de-4e9b-8b87-ce2f193ffdb3"), LastName = "Козлов" },
                new Person { Id = Guid.Parse("a91f332f-ae0e-4922-b7b6-d68078adb94d"), LastName = "Николаев" },
                new Person { Id = Guid.Parse("333bc99e-5488-41a9-90ab-31e52d88328e"), LastName = "Михайлов" },
                new Person { Id = Guid.Parse("3249d63f-b251-4dde-b47d-2a9b8e8d8864"), LastName = "Борисов" },
                new Person { Id = Guid.Parse("df5c31ed-8ca5-4cd5-9035-ea8e22fce0d0"), LastName = "Котов" },
                new Person { Id = Guid.Parse("f043f81a-76b6-4c9d-b4dd-79ddda426b55"), LastName = "Кузнецов" },
                new Person { Id = Guid.Parse("d9a6b32f-46c8-4265-8430-f6c1b85abbb8"), LastName = "Соловьев" },
                new Person { Id = Guid.Parse("1558ba31-e019-48f5-9dc0-c39a11f15f89"), LastName = "Кузнецова" },
                new Person { Id = Guid.Parse("c6f2115c-c7b8-439f-8d7d-95eba5476358"), LastName = "Григорьев" },
                new Person { Id = Guid.Parse("3eb6ede9-3bf3-45f2-bb6a-9c512ec79022"), LastName = "Федоров" },
                new Person { Id = Guid.Parse("c19dcff6-661d-40aa-a45a-371d80e1a780"), LastName = "Егоров" },
                new Person { Id = Guid.Parse("80b3af31-e5ac-439f-bf7b-8cbb2bb26aec"), LastName = "Куликов" },
                new Person { Id = Guid.Parse("1c2655b6-ac3e-4c7a-87ea-35ec7986fcc1"), LastName = "Степанов" },
                new Person { Id = Guid.Parse("b11b265a-2198-46cb-a192-48972b61ada1"), LastName = "Яковлев" },
                new Person { Id = Guid.Parse("0c114a55-ea4a-4f21-9775-d194ab8a9a04"), LastName = "Кулигин" },
                new Person { Id = Guid.Parse("cdbfc076-fa97-4abc-b5ac-14cbe0ae179f"), LastName = "Сорокин" },
                new Person { Id = Guid.Parse("be131dbb-1826-435a-a64f-2b5fee9e1743"), LastName = "Миронов" },
                new Person { Id = Guid.Parse("72afd3de-17c6-45a9-8dc8-8fa1209803d4"), LastName = "Абрамов" },
                new Person { Id = Guid.Parse("09b49c97-fbfb-4012-82fc-578ba8f72806"), LastName = "Воронов" },
                new Person { Id = Guid.Parse("83b86023-8e5d-4f76-99fb-bf0b402af27c"), LastName = "Мишин" },
                new Person { Id = Guid.Parse("9450829e-dbde-46c2-9c56-fcafa77041eb"), LastName = "Гаврилов" },
                new Person { Id = Guid.Parse("cdf8fbdc-4e13-4d9c-94cb-b601c3b83def"), LastName = "Кудрявцев" },
                new Person { Id = Guid.Parse("da305ef5-ee48-4d80-b6f2-4c75a840d1cd"), LastName = "Борисенков" },
                new Person { Id = Guid.Parse("e0156dee-1f82-4e3b-a36c-5ed36686b7a7"), LastName = "Логинов" },
                new Person { Id = Guid.Parse("b6343180-3aed-47a7-882e-14ba0dba6ac5"), LastName = "Сафронов" },
                new Person { Id = Guid.Parse("e04ec2dd-293f-43e4-9ef3-ca075f7b891d"), LastName = "Белов" },
                new Person { Id = Guid.Parse("183ed12f-64e7-4c02-9dee-f7e152e890cc"), LastName = "Куликов" },
                new Person { Id = Guid.Parse("25eb2eeb-6c22-4cde-870a-09063f6865f3"), LastName = "Макаров" },
                new Person { Id = Guid.Parse("d75ae0a0-2b45-4973-9072-b5d50e33b062"), LastName = "Кузнецов" },
            };
        }

        public static List<AppraisalListMark> FakeMarks(AppraisalListColumn column)
        {
            var students = FakePeople();

            var random = new Random();

            var marks = new List<AppraisalListMark>();

            for (int i = 0; i < students.Count; i++)
            {
                var student = students[i];

                var shouldHaveMark = random.Next(0, 2) == 1 ? true : false;

                if(!shouldHaveMark)
                {
                    continue;
                }

                var mark = new AppraisalListMark
                {
                    Id = Guid.NewGuid(),
                    AppraisalListColumnId = column.Id,
                    StudentPersonId = student.Id,
                    Score = new Random().Next(0, column.MaxScore),
                    StudentPerson = student
                };

                marks.Add(mark);
            }

            return marks;
        }

        public static List<AppraisalListColumn> FakeAppraisalListColumns()
        {
            var columns = new List<AppraisalListColumn>();
            var random = new Random();
            var count = random.Next(2, 11);

            for (int i = 0; i < count; i++)
            {
                int[] maxScores = { 30, 40, 50, 60, 70, 80, 100 };
                int randomIndex = new Random().Next(0, maxScores.Length);
                var MaxScore = maxScores[randomIndex];

                var column = new AppraisalListColumn
                {
                    Id = Guid.NewGuid(),
                    Name = $"Раздел {i + 1}",
                    OrderIndex = i,
                    MaxScore = MaxScore,
                    MinAcceptScore = (int)(0.6 * MaxScore),
                };

                column.AppraisalListMarks = FakeMarks(column);

                columns.Add(column);
            }

            return columns;
        }

    }
}
