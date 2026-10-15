CREATE SCHEMA "Lms";
CREATE SCHEMA "Attendance";
CREATE TABLE "Lms"."UserAccounts" (
 "PersonID" uuid PRIMARY KEY REFERENCES "Person"."Persons", "Username" text NOT NULL,
 "DisplayName" text NOT NULL, "RecordBookNumber" text NOT NULL DEFAULT '', "IsAdmin" boolean NOT NULL DEFAULT false,
 "IsArchived" boolean NOT NULL DEFAULT false, CHECK (length(trim("Username")) BETWEEN 1 AND 100),
 CHECK (length(trim("DisplayName")) BETWEEN 1 AND 200)
);
CREATE UNIQUE INDEX "UserAccounts_Username" ON "Lms"."UserAccounts" (lower("Username"));
CREATE TABLE "Lms"."GroupNames" (
 "Name" text PRIMARY KEY, "GroupStartDate" timestamp NOT NULL,
 FOREIGN KEY ("Name", "GroupStartDate") REFERENCES "Education"."EducationalGroups" ("GroupName", "StartDate")
);
CREATE TABLE "Lms"."CourseSettings" (
 "CourseID" uuid PRIMARY KEY REFERENCES "Course"."Courses", "Code" text NOT NULL,
 "GroupName" text NOT NULL, "DefaultRoom" text NOT NULL, "ActiveYear" integer NOT NULL,
 "IsArchived" boolean NOT NULL DEFAULT false,
 FOREIGN KEY ("GroupName") REFERENCES "Lms"."GroupNames" ("Name")
);
CREATE UNIQUE INDEX "CourseSettings_Code" ON "Lms"."CourseSettings" (lower("Code")) WHERE NOT "IsArchived";
CREATE TABLE "Lms"."Enrollments" (
 "ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(), "PersonID" uuid NOT NULL REFERENCES "Lms"."UserAccounts",
 "CourseID" uuid NOT NULL, "Year" integer NOT NULL, "Role" text NOT NULL CHECK ("Role" IN ('Student','Teacher','Assistant','Curator')),
 "StartDate" timestamp NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'), "EndDate" timestamp,
 FOREIGN KEY ("CourseID", "Year") REFERENCES "Course"."CourseInstances", CHECK ("EndDate">"StartDate")
);
CREATE UNIQUE INDEX "Enrollments_Active" ON "Lms"."Enrollments" ("PersonID", "CourseID", "Year", "Role") WHERE "EndDate" IS NULL;
CREATE INDEX "Enrollments_ByCourse" ON "Lms"."Enrollments" ("CourseID", "Year") WHERE "EndDate" IS NULL;
CREATE TABLE "Attendance"."Lessons" (
 "ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(), "CourseID" uuid NOT NULL, "Year" integer NOT NULL,
 "Topic" text NOT NULL, "Room" text NOT NULL, "StartsAt" timestamp NOT NULL, "CompletedAt" timestamp,
 "Revision" bigint NOT NULL DEFAULT 0,
 FOREIGN KEY ("CourseID", "Year") REFERENCES "Course"."CourseInstances", UNIQUE ("CourseID", "Year", "StartsAt")
);
ALTER TABLE "CourseLabs"."StudentVisits"
 ADD COLUMN "LessonID" uuid REFERENCES "Attendance"."Lessons",
 ADD COLUMN "Status" text NOT NULL DEFAULT 'Present' CHECK ("Status" IN ('Unmarked','Present','Late','Excused','Absent')),
 ADD COLUMN "LateMinutes" integer,
 ADD COLUMN "UpdatedAt" timestamp,
 ADD CONSTRAINT "StudentVisits_Late" CHECK (("Status"='Late' AND "LateMinutes">0 AND "LateMinutes" IS NOT NULL) OR ("Status"<>'Late' AND "LateMinutes" IS NULL)),
 ADD CONSTRAINT "StudentVisits_Excused" CHECK ("Status"<>'Excused' OR length(trim(COALESCE("Comment",'')))>0),
 ADD CONSTRAINT "StudentVisits_LessonStudent" UNIQUE ("LessonID", "StudentPersonID");
CREATE TABLE "Attendance"."VisitHistory" (
 "ID" bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
 "VisitID" uuid NOT NULL REFERENCES "CourseLabs"."StudentVisits",
 "ActorPersonID" uuid NOT NULL REFERENCES "Person"."Persons", "ChangedAt" timestamp NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
 "OldStatus" text, "NewStatus" text NOT NULL, "OldLateMinutes" integer, "NewLateMinutes" integer,
 "OldComment" text, "NewComment" text, "DataSourceName" text
);
CREATE TABLE "Lms"."StatementDrafts" (
 "PersonID" uuid PRIMARY KEY REFERENCES "Person"."Persons", "Payload" jsonb NOT NULL,
 "Revision" bigint NOT NULL DEFAULT 1, "UpdatedAt" timestamp NOT NULL DEFAULT (now() AT TIME ZONE 'UTC')
);
INSERT INTO "Education"."CourseTaskTypes" VALUES (1,'Контрольная работа'),(2,'Лабораторная работа'),(3,'Коллоквиум'),(4,'Курсовая работа'),(5,'Курсовой проект');
INSERT INTO "Organization"."OrgUnits" ("ID","Name","UnitCode","IsDepartmentGraduate")
 VALUES ('00000000-0000-0000-0000-000000000022','Кафедра №22 Кибернетика','22',true);
INSERT INTO "Education"."Specialties" ("SpecialtyCode","SpecialtyCaption","EducationLevel","StartDate")
 VALUES ('09.03.01','Информатика и вычислительная техника',1,'2023-09-01');
INSERT INTO "Education"."EducationalPrograms" ("ID","ProgramName","SpecialtyCode","OrgUnitID","StartDate")
 VALUES ('00000000-0000-0000-0000-000000000023','Тестовая образовательная программа','09.03.01','00000000-0000-0000-0000-000000000022','2023-09-01');
