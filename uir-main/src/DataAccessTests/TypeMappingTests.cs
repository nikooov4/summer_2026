using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Mephi.Cybernetics.Race.Core;
using Mephi.Cybernetics.Race.Core.DomainModelMapping;
using Mephi.Cybernetics.Race.DataAccessTests.DataStructures;
using NUnit.Framework;
using Mephi.Cybernetics.Race.Core.TypeSystem;
using System.Collections.Generic;
using Mephi.Cybernetics.Race.DataAccessTests.Inventorisation;
using System.Xml.Linq;
using Mephi.Cybernetics.Race.AppraisalListsMapping.Models;

namespace Mephi.Cybernetics.Race.DataAccessTests
{
    [TestFixture]
    public class TypeMappingTests
    {
        [Test]
        public void TestNonPersistentMapping()
        {
            var transformation = MappingPoco<Ellipse, Ellipse2>.New().UsingCustomInstantiation(o => new Ellipse2());
            transformation.Property("Location").UsingMap<Ellipse, Point>(source => new Point(source.X, source.Y));
            transformation.Property("Semiaxises").UsingMap<Ellipse, Size>(source => new Size(source.LongSemiAxis, source.ShortSemiAxis));

            var ellipse = new Ellipse
            {
                Label = "111",
                LongSemiAxis = 4,
                ShortSemiAxis = 3,
                X = 5,
                Y = -6
            };
            var convertedEllipse = transformation.Map(ellipse);
           
            Assert.AreEqual(ellipse.Label, convertedEllipse.Label);
            Assert.AreEqual(ellipse.X, convertedEllipse.Location.X);
            Assert.AreEqual(ellipse.Y, convertedEllipse.Location.Y);
            Assert.AreEqual(ellipse.LongSemiAxis, convertedEllipse.Semiaxises.Width);
            Assert.AreEqual(ellipse.ShortSemiAxis, convertedEllipse.Semiaxises.Height);

            var ell2 = new Ellipse2
            {
                Label = "444",
                Location = new Point(11, 12),
                Semiaxises = new Size(34, 45)
            };
            transformation.MapOnto(ellipse, ref ell2);
            Assert.AreEqual(ellipse.Label, ell2.Label);
            Assert.AreEqual(ellipse.X, ell2.Location.X);
            Assert.AreEqual(ellipse.Y, ell2.Location.Y);
            Assert.AreEqual(ellipse.LongSemiAxis, ell2.Semiaxises.Width);
            Assert.AreEqual(ellipse.ShortSemiAxis, ell2.Semiaxises.Height);

            // Проверяем, что можно вычислять значения свойств по отдельности
            Assert.AreEqual(convertedEllipse.Label, transformation.Property("Label").Map(ellipse));
            Assert.AreEqual(convertedEllipse.Location, transformation.Property("Location").Map(ellipse));
        }

        [Test]
        public void TestInv()
        {
            var depMapping = MappingPoco<UserDepartment, UserDepartmentDto>.New().UsingCustomInstantiation(d => new());
            depMapping.Property("Id").UsingMap<UserDepartment, string>(ud => ud.DepartmentId.ToString());
            depMapping.Property("Label").UsingMap<UserDepartment, string>(ud => ud.Department?.Label);

            var userMapping = MappingPoco<User, UserInfoDto>.New().UsingCustomInstantiation(u => new());
            userMapping.Property("Departments").UsingMap<User, ICollection<UserDepartmentDto>>(u => depMapping.Map(u.UserDepartments));

            var user = new User()
            {
                IsAdmin = true,
                Login = "login",
                MiddleName = "Ivanovich",
                Name = "Ivan",
                Surname = "Ivanov",
                UserDepartments = new[] { new UserDepartment() {
                    DepartmentId = Guid.NewGuid(),
                    IsWorkspace = true,
                    UserId = Guid.NewGuid().ToString()
                }, new UserDepartment() {
                    DepartmentId = Guid.NewGuid(),
                    IsWorkspace = false,
                    UserId = Guid.NewGuid().ToString()
                } }
            };

            var userDto = userMapping.Map(user);
            Assert.AreEqual(user.Login, userDto.Login);
            Assert.AreEqual(user.Name, userDto.Name);
            Assert.AreEqual(user.MiddleName, userDto.MiddleName);
            Assert.AreEqual(user.Surname, userDto.Surname);
            Assert.AreEqual(user.IsAdmin, userDto.IsAdmin);
            for (int i = 0; i < user.UserDepartments.Count; i++)
            {
                var ud = user.UserDepartments.ElementAt(i);
                var uDto = userDto.Departments.ElementAt(i);
                Assert.AreEqual(ud.DepartmentId.ToString(), uDto.Id);
                Assert.AreEqual(ud.Department?.Label, uDto.Label);
            }
        }

        [Test]
        public void TestTransformationsWithInterfaces()
        {
            var intPoints = new[]
            {
                new IntPoint(1,2,"1"),
                new IntPoint(2,2,"2"),
                new IntPoint(0,0,"3"),
                new IntPoint(2,1,"4"),
                new IntPoint(3,3,"5"),
            }
                .Cast<IPoint<int>>().ToArray();
            //var typeIPointInt = ObjectDescription.Get<IPoint<int>>();
            //var typeIPointFloat = ObjectDescription.Get<IPoint<float>>();
            var trans = MappingPoco<IPoint<int>, IPoint<float>>.New();
            trans.CustomMapping<IPoint<int>, IPoint<float>>(p => p is IntPoint ip ? new RealPoint(ip.X, ip.Y, ip.Label) : throw new ArgumentException(""));

            var realPoints = trans.Map(intPoints);

            Assert.AreEqual(intPoints.Length, realPoints.Count);
            for (var i = 0; i < intPoints.Length; i++)
            {
                Assert.AreEqual(intPoints[i].X, (int)realPoints[i].X);
                Assert.AreEqual(intPoints[i].Y, (int)realPoints[i].Y);
                Assert.AreEqual(((IntPoint)intPoints[i]).Label, ((RealPoint)realPoints[i]).Label);
            }
        }

        [Test]
        public void TestMappingWithSubtyping()
        {
            var trans = MappingPoco<Figure, Shape>.New().UsingCustomInstantiation(f =>
            {
                switch (f.Type)
                {
                    case FigureType.Circle:
                        return new Circle();
                    case FigureType.Ellipse:
                        return new Ellipse2();
                    default:
                        throw new ArgumentException();
                }
            });

            trans.Property("Label").FromProperty<string[], string>("FullName", ss =>
            {
                return string.Join(".", ss);
            });

            trans.ForType<Circle>().Property("X").FromProperty<Point, int>("Location", p => p.X);
            trans.ForType<Circle>().Property("Y").FromProperty<Point, int>("Location", p => p.Y);
            trans.ForType<Circle>().Property("Radius").FromProperty<Size, int>("Size", s => s.Width);

            trans.ForType<Ellipse2>().Property("Location").FromProperty("Location");
            trans.ForType<Ellipse2>().Property("Semiaxises").FromProperty("Size");

            var figure1 = new Figure
            {
                FullName = new string[] { "ns1", "ns2", "name1" },
                Size = new Size(3, 4),
                Location = new Point(1, 2),
                Type = FigureType.Circle
            };

            var figure2 = new Figure
            {
                FullName = new string[] { "n3", "n4", "name2" },
                Size = new Size(3, 4),
                Location = new Point(2, 1),
                Type = FigureType.Ellipse
            };

            var circle = trans.Map(figure1);
            Assert.AreEqual(typeof(Circle), circle.GetType());
            Assert.AreEqual("ns1.ns2.name1", (circle as Circle).Label);
            Assert.AreEqual(figure1.Size.Width, (circle as Circle).Radius);
            Assert.AreEqual(figure1.Location.X, (circle as Circle).X);
            Assert.AreEqual(figure1.Location.Y, (circle as Circle).Y);

            var ellipse = trans.Map(figure2);
            Assert.AreEqual(typeof(Ellipse2), ellipse.GetType());
            Assert.AreEqual("n3.n4.name2", (ellipse as Ellipse2).Label);
            Assert.AreEqual(figure2.Size, (ellipse as Ellipse2).Semiaxises);
            Assert.AreEqual(figure2.Location, (ellipse as Ellipse2).Location);
        }

        // weak types
        [Test]
        public void TestWeakNonPersistentMapping()
        {
            var typeEllipse = WeakType
                .Define()
                .Name("Ellipse")
                .Namespace("Mephi.Cybernetics.Race.Core.Tests")
                .Source(Assembly.GetExecutingAssembly().FullName)
                .Attributes()
                .Add("X", ObjectDescription.Get<int>())
                .Add("Y", ObjectDescription.Get<int>())
                .Add("LongSemiAxis", ObjectDescription.Get<int>())
                .Add("ShortSemiAxis", ObjectDescription.Get<int>())
                .Add("Label", ObjectDescription.Get<string>())
                .End()
                .End();

            var typeEllipse2 = WeakType
                .Define()
                .Name("Ellipse2")
                .Namespace("Mephi.Cybernetics.Race.Core.Tests")
                .Source(Assembly.GetExecutingAssembly().FullName)
                .Attributes()
                .Add("Location", ObjectDescription.Get<Point>())
                .Add("Semiaxises", ObjectDescription.Get<Size>())
                .Add("Label", ObjectDescription.Get<string>())
                .End()
                .End();
            var transformation = WeakTypeMapping.New(typeEllipse, typeEllipse2).UsingCustomInstantiation(o => AsWeakInstance(new Ellipse2()));
            transformation.Property("Location").UsingMap<WeakInstance, WeakInstance>(source => AsWeakInstance(new Point((int)source["X"], (int)source["Y"])));
            transformation.Property("Semiaxises").UsingMap<WeakInstance, WeakInstance>(source => AsWeakInstance(new Size((int)source["LongSemiAxis"], (int)source["ShortSemiAxis"])));

            var ellipse =  new WeakInstance(new Dictionary<string, object>
            {
                {"Label" , "111" },
                {"LongSemiAxis" , 4},
                {"ShortSemiAxis" , 3 },
                {"X" , 5 }, 
                {"Y" , -6}
            });

            var convertedEllipse = transformation.Map(ellipse);

            Assert.AreEqual(ellipse["Label"], convertedEllipse["Label"]);
            Assert.AreEqual(ellipse["X"], ((WeakInstance)convertedEllipse["Location"])["X"]);
            Assert.AreEqual((int)ellipse["Y"], ((WeakInstance)convertedEllipse["Location"])["Y"]);
            Assert.AreEqual(ellipse["LongSemiAxis"], ((WeakInstance)convertedEllipse["Semiaxises"])["Width"]);
            Assert.AreEqual(ellipse["ShortSemiAxis"], ((WeakInstance)convertedEllipse["Semiaxises"])["Height"]);

            var ell2 = AsWeakInstance(new Ellipse2
            {
                Label = "444",
                Location = new Point(11, 12),
                Semiaxises = new Size(34, 45)
            });
            transformation.MapOnto(ellipse, ref ell2);
            Assert.AreEqual(ellipse["Label"], ell2["Label"]);
            Assert.AreEqual(ellipse["X"], ((WeakInstance)ell2["Location"])["X"]);
            Assert.AreEqual((int)ellipse["Y"], ((WeakInstance)ell2["Location"])["Y"]);
            Assert.AreEqual(ellipse["LongSemiAxis"], ((WeakInstance)ell2["Semiaxises"])["Width"]);
            Assert.AreEqual(ellipse["ShortSemiAxis"], ((WeakInstance)ell2["Semiaxises"])["Height"]);

            // Проверяем, что можно вычислять значения свойств по отдельности
            Assert.AreEqual(convertedEllipse["Label"], transformation.Property("Label").Map(ellipse));
            Assert.IsTrue((convertedEllipse["Location"] as WeakInstance).Equals((WeakInstance)(transformation.Property("Location").Map(ellipse))));
        }

        [Test]
        public void TestWeakTransformationsWithInterfaces()
        {
            var intPoints = new[]
            {
                new IntPoint(1,2,"1"),
                new IntPoint(2,2,"2"),
                new IntPoint(0,0,"3"),
                new IntPoint(2, 1, "4"),
                new IntPoint(3, 3, "5"),
            }
                .Cast<IPoint<int>>().Select(p => AsWeakInstance(p)).ToArray();
            var intPointType = WeakType.From<IPoint<int>>();
            var floatPointType = WeakType.From<IPoint<float>>();
            var trans = WeakTypeMapping.New(intPointType, floatPointType);
            trans.CustomMapping(p => AsWeakInstance(new RealPoint((float)(int)p["X"], (float)(int)p["Y"], (string)p["Label"])));
            var realPoints = trans.Map(intPoints);
            Assert.AreEqual(intPoints.Length, realPoints.Count);
            for (var i = 0; i < intPoints.Length; i++)
            {
                Assert.AreEqual(intPoints[i]["X"], (int)(float)realPoints[i]["X"]);
                Assert.AreEqual(intPoints[i]["Y"], (int)(float)realPoints[i]["Y"]);
                Assert.AreEqual((intPoints[i])["Label"], (realPoints[i])["Label"]);
            }
        }

        [Test]
        public void TestWeakMappingWithSubtyping()
        {

            var figureType = WeakType.From<Figure>();
            var shapeType = WeakType.From<Shape>();
            var trans = WeakTypeMapping.New(figureType, shapeType).UsingCustomInstantiation(f =>
            {
                switch (f["Type"])
                {
                    case FigureType.Circle:
                        return AsWeakInstance(new Circle());
                    case FigureType.Ellipse:
                        return AsWeakInstance(new Ellipse2());
                    default:
                        throw new ArgumentException();
                }
            });

            trans.Property("Label").FromProperty<string[], string>("FullName", ss =>
            {
                return string.Join(".", ss);
            });

            var circleType = WeakType.From<Circle>();
            var ellipse2Type = WeakType.From<Ellipse2>();

            trans.ForType(circleType).Property("X").FromProperty<WeakInstance, object>("Location", p => p["X"]);
            trans.ForType(circleType).Property("Y").FromProperty<WeakInstance, object>("Location", p => p["Y"]);
            trans.ForType(circleType).Property("Radius").FromProperty<WeakInstance, object>("Size", s => s["Width"]);

            trans.ForType(ellipse2Type).Property("Location").FromProperty("Location");
            trans.ForType(ellipse2Type).Property("Semiaxises").FromProperty("Size");

            var figure1 = AsWeakInstance(new Figure
            {
                FullName = new string[] { "ns1", "ns2", "name1" },
                Size = new Size(3, 4),
                Location = new Point(1, 2),
                Type = FigureType.Circle
            });

            var figure2 = AsWeakInstance(new Figure
            {
                FullName = new string[] { "n3", "n4", "name2" },
                Size = new Size(3, 4),
                Location = new Point(2, 1),
                Type = FigureType.Ellipse
            });

            var ts = new WeakTypeSystem();
            var circle = trans.Map(figure1);
            Assert.IsTrue(ts.IsInstanceOf(circle, circleType));
            Assert.AreEqual("ns1.ns2.name1", circle["Label"]);
            Assert.AreEqual((figure1["Size"] as WeakInstance)["Width"], circle["Radius"]);
            Assert.AreEqual((figure1["Location"] as WeakInstance)["X"], circle["X"]);
            Assert.AreEqual((figure1["Location"] as WeakInstance)["Y"], circle["Y"]);

            var ellipse = trans.Map(figure2);
            Assert.IsTrue(ts.IsInstanceOf(ellipse, ellipse2Type));
            Assert.AreEqual("n3.n4.name2", ellipse["Label"]);
            Assert.AreEqual(figure2["Size"], ellipse["Semiaxises"]);
            Assert.AreEqual(figure2["Location"], ellipse["Location"]);
        }

        [Test]
        public void TestTypeRepresentation()
        {
            var appraisalID = Guid.NewGuid();
            var name = "Attestation";
            AppraisalLists appraisal = new()
            {
                Comment = $"Комментарий к ведомости '{name}'",
                CourseProgramID = null,
                DateClosed = DateTime.Today,
                DateCreated = DateTime.Today - TimeSpan.FromDays(10),
                Description = $"Описание ведомости '{name}'",
                ID = appraisalID,
                Name = name,
                Year = 2024,
            };

            var cols = GetColumns();
            appraisal._columns = cols;
            foreach (var col in cols)
            {
                col.AppraisalListID = appraisal.ID;
                col._list = appraisal;
            }


            var tr =  TypeRepresentation.Define(appraisal)
                                        .Name(a => a.Name)
                                        .Namespace("Mephi.Cybernetics.Race.Core.Tests")
                                        .Source(Assembly.GetExecutingAssembly().FullName)
                                        .Attributes()
                                        .AddFromCollection(a => a._columns, c => (c.Name, ObjectDescription.Get(c)))
                                        .End()
                                        .End();


            var attList = new Attestation();
            var map = InterMapping<ObjectDescription, TypeRepresentation>.New(ObjectDescription.Get<Attestation>(), tr);
            map.Property("Раздел 1").FromProperty(nameof(attList.Part1));
            map.Property("Раздел 2").FromProperty(nameof(attList.Part2));
            map.Property("Раздел 3").FromProperty(nameof(attList.Part3));
            map.Property("Сумма").FromProperty(nameof(attList.Sum));
            map.Property("Аттестация").FromProperty(nameof(attList.AttestationCol));
            map.Property("Оценка").FromProperty(nameof(attList.Mark));
            map.Property("ECTS").FromProperty(nameof(attList.ECTS));

            var x = map.Map<Attestation, WeakInstance>(attList);
        }

        public static List<AppraisalListColumns> GetColumns()
        {
            var scale1Id = Guid.NewGuid();
            var scaleLinear = new AppraisalScales
            {
                Description = $"Описание шкалы {scale1Id}",
                IsEnum = false,
                IsLinear = true,
                Name = "Линейная шкала",
                ID = scale1Id,
                _linearScales = new List<AppraisalScalesLinear>() { new AppraisalScalesLinear() { ID = scale1Id, MaxValue = 100, MinValue = 0, ValueFormat = "?" } },
                _enumValues = null
            };
            scaleLinear._linearScales[0]._scale = scaleLinear;

            var scaleEnumAttestation = MakeEnumScale([(0, "Не аттестован", "н/a"), (1, "Аттестован", "а")]);
            var scaleEnumRusMark = MakeEnumScale([(0, "Неудовлетварительно", "неуд."), (1, "Удовлетварительно", "удовл."), (2, "Хорошо", "хор."), (3, "Отлично", "отл.")]);
            var scaleEnumEnMark = MakeEnumScale([(0, "F", "F"), (1, "E", "E"), (2, "D", "D"), (3, "C", "C"), (4, "B", "B"), (5, "A", "A")]);

            var column1 = CreateColumn(1, scaleLinear, "Раздел 1", null, (35, 20), null, null);
            var column2 = CreateColumn(2, scaleLinear, "Раздел 2", null, (35, 20), null, null);
            var columnIn = CreateColumn(3, scaleLinear, "Раздел 3", null, (30, 20), null, null);
            var column3 = CreateColumn(4, scaleLinear, "Сумма", Operations.SUM, (100, 60), [column1, column2, columnIn], null);
            var column4 = CreateColumn(5, scaleEnumAttestation, "Аттестация", Operations.EnumRange, (scaleEnumAttestation._enumValues.Last().Code, scaleEnumAttestation._enumValues.Last().Code), [column3], [(0, 59), (60, 100)]);
            var column5 = CreateColumn(6, scaleEnumRusMark, "Оценка", Operations.EnumRange, (scaleEnumRusMark._enumValues.Last().Code, 1), [column3], [(0, 59), (60, 69), (70, 89), (90, 100)]);
            var column6 = CreateColumn(7, scaleEnumEnMark, "ECTS", Operations.EnumRange, (scaleEnumAttestation._enumValues.Last().Code, 1), [column3], [(0, 59), (60, 64), (65, 69), (70, 79), (80, 89), (90, 100)]);

            return new List<AppraisalListColumns> { column1, column2, columnIn, column3, column4, column5, column6 };
        }

        public static AppraisalScales MakeEnumScale(List<(int code, string desc, string name)> data)
        {
            var scale2Id = Guid.NewGuid();
            var scaleEnum = new AppraisalScales
            {
                Description = $"Описание шкалы {scale2Id}",
                IsEnum = true,
                IsLinear = false,
                Name = $"Enum шкала {scale2Id}",
                ID = scale2Id,
                _linearScales = null,
                _enumValues = data.Select(d => new AppraisalScalesEnumValues() { Code = d.code, Description = d.desc, Name = d.name, ScaleID = scale2Id }).ToList()
            };
            for (int i = 0; i < scaleEnum._enumValues.Count; i++)
                scaleEnum._enumValues[i]._scale = scaleEnum;
            return scaleEnum;
        }

        public static AppraisalListColumns CreateColumn(int order, AppraisalScales scale, string name, Operations? operation, (int maxScore, int minAcceptScore) score, List<AppraisalListColumns> dependencies, List<(int min, int max)> intranges, bool fillMarks = true)
        {
            var OpertationsDict = new Dictionary<Operations, ColumnOperations>()
            {
                { Operations.SUM, new ColumnOperations{ ID="SUM", Description="Суммирование" } },
                { Operations.MIN, new ColumnOperations{ ID="MIN", Description="Минимальное значение" } },
                { Operations.EnumRange, new ColumnOperations{ ID=Operations.EnumRange.ToString(), Description="Enum для диапаона" } },
            };

            var columnId = Guid.NewGuid();
            var column = new AppraisalListColumns
            {
                Name = name,
                Comment = $"Комментарий к '{name}'",
                CourseInstanceModuleID = null,
                Description = $"Описание к '{name}'",
                IsComputed = operation != null,
                MaxScore = score.maxScore,
                MinAcceptScore = score.minAcceptScore,
                OrderIndex = order,
                ScaleID = scale.ID,
                ID = columnId,
                _computeColumns = null,
                _scale = scale,
                _dependsFrom = null,
            };
            if (scale._columns == null)
                scale._columns = new List<AppraisalListColumns>();
            scale._columns.Add(column);

            if (column.IsComputed)
            {
                column._computeColumns = new List<AppraisalListColumnsCompute>() { new AppraisalListColumnsCompute() { ColumnID = column.ID, OperationID = OpertationsDict[operation.Value].ID, _column = column, _operation = OpertationsDict[operation.Value] } };
                column._dependsFrom = dependencies.Select(d => new AppraisalListColumnsDependencies() { ColumnID = column.ID, _column = column, _dependsFrom = d, ColumnDepID = d.ID }).ToList();
                foreach (var dep in column._dependsFrom)
                {
                    if (dep._dependsFrom._dependedBy == null)
                        dep._dependsFrom._dependedBy = new List<AppraisalListColumnsDependencies>();
                    dep._dependsFrom._dependedBy.Add(dep);
                }
                if (operation.Value == Operations.EnumRange)
                {
                    InitColumnWithRange(scale, column._computeColumns[0], intranges);
                }
            }
            //if (column._scale.IsLinear && !column.IsComputed && fillMarks)
            //    column._marks = GenerateRandomMarks(column);

            return column;
        }

        public static void InitColumnWithRange(AppraisalScales scale, AppraisalListColumnsCompute column, List<(int min, int max)> intranges)
        {
            var ranges = new List<AppraisalScalesEnumRanges>();

            int i = 0;
            foreach (var e in scale._enumValues)
            {
                var range = new AppraisalScalesEnumRanges() { ComputeColumnID = column.ColumnID, EnumValueID = e.ScaleID, _column = column, _enumValue = e, RangeStart = intranges[i].min, RangeEnd = intranges[i].max };
                e._ranges = [range];
                ranges.Add(range);
                i++;
            }
            column._ranges = ranges;
        }

        public WeakInstance AsWeakInstance<T>(T o)
        {
            var kvPairs = new Dictionary<string, object>();
            var mainOd = ObjectDescription.Get(o);
            foreach (var p in mainOd.Properties)
            {
                var pValue = p.GetValue(o);
                if (pValue == null || ObjectDescription.Get(pValue).IsAssumedPrimitive || pValue is ICollection<object>)
                {
                    kvPairs.Add(p.Name, pValue);
                    continue;
                }
                var weakValue = AsWeakInstance(pValue);
                kvPairs.Add(p.Name, weakValue);
            }
            var res = new WeakInstance(kvPairs);
            return res;
        }
    }

    public class Attestation
    {
        //var tr = TypeRepresentation.Define(appraisal)
        //                               .Name(a => a.Name)
        //                               .Namespace("Mephi.Cybernetics.Race.Core.Tests")
        //                               .Source(Assembly.GetExecutingAssembly().FullName)
        //                               .Attributes()
        //                               .AddFromCollection(a => a._columns, c => (c.Name, ObjectDescription.Get(c)))
        //                               .End()
        //                               .End();

        //var column1 = CreateColumn(1, scaleLinear, "Раздел 1", null, (35, 20), null, null);
        //var column2 = CreateColumn(2, scaleLinear, "Раздел 2", null, (35, 20), null, null);
        //var columnIn = CreateColumn(3, scaleLinear, "Раздел 3", null, (30, 20), null, null);
        //var column3 = CreateColumn(4, scaleLinear, "Сумма", Operations.SUM, (100, 60), [column1, column2, columnIn], null);
        //var column4 = CreateColumn(5, scaleEnumAttestation, "Аттестация", Operations.EnumRange, (scaleEnumAttestation._enumValues.Last().Code, scaleEnumAttestation._enumValues.Last().Code), [column3], [(0, 59), (60, 100)]);
        //var column5 = CreateColumn(6, scaleEnumRusMark, "Оценка", Operations.EnumRange, (scaleEnumRusMark._enumValues.Last().Code, 1), [column3], [(0, 59), (60, 69), (70, 89), (90, 100)]);
        //var column6 = CreateColumn(7, scaleEnumEnMark, "ECTS", Operations.EnumRange, (scaleEnumAttestation._enumValues.Last().Code, 1), [column3], [(0, 59), (60, 64), (65, 69), (70, 79), (80, 89), (90, 100)]);

        public AppraisalListColumns Part1 { get; set; }
        public AppraisalListColumns Part2 { get; set; }
        public AppraisalListColumns Part3 { get; set; }
        public AppraisalListColumns Sum { get; set; }
        public AppraisalListColumns AttestationCol { get; set; }
        public AppraisalListColumns Mark { get; set; }
        public AppraisalListColumns ECTS { get; set; }

        public Attestation()
        {
            var cols = TypeMappingTests.GetColumns();
            Part1 = cols[0];
            Part2 = cols[1];
            Part3 = cols[2];
            Sum = cols[3];
            AttestationCol = cols[4];
            Mark = cols[5];
            ECTS = cols[6];
        }

    }
}
