CREATE SCHEMA IF NOT EXISTS "Person";
CREATE SCHEMA IF NOT EXISTS "Organization";
CREATE SCHEMA IF NOT EXISTS "Education";
CREATE SCHEMA IF NOT EXISTS "Course";
CREATE SCHEMA IF NOT EXISTS "CourseReport";
CREATE SCHEMA IF NOT EXISTS "CourseLabs";
CREATE SCHEMA IF NOT EXISTS "Content";
CREATE SCHEMA IF NOT EXISTS "Achievements";

CREATE TABLE "Person"."Persons" (
 "ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(), "FirstName" text NOT NULL,
 "MiddleName" text, "LastName" text NOT NULL, "RegisterDate" timestamp,
 "BirthYear" integer CHECK ("BirthYear" BETWEEN 1 AND 9999),
 "BirthMonth" smallint CHECK ("BirthMonth" BETWEEN 1 AND 12),
 "BirthDay" smallint CHECK ("BirthDay" BETWEEN 1 AND 31),
 "BirthDate" date GENERATED ALWAYS AS (CASE WHEN "BirthYear" IS NOT NULL AND "BirthMonth" IS NOT NULL AND "BirthDay" IS NOT NULL
 THEN make_date("BirthYear", "BirthMonth", "BirthDay") END) STORED
);
CREATE TABLE "Person"."PersonEmails" (
 "Email" text NOT NULL, "PersonID" uuid NOT NULL REFERENCES "Person"."Persons",
 "IsActive" boolean NOT NULL DEFAULT true, "IsPreferred" boolean NOT NULL DEFAULT true,
 "StartDate" timestamp NOT NULL, "EndDate" timestamp,
 PRIMARY KEY ("Email", "StartDate"), CHECK ("EndDate" > "StartDate")
);
CREATE TABLE "Person"."PersonPhones" (
 "Phone" text NOT NULL, "AdditionalCode" text NOT NULL DEFAULT '',
 "PersonID" uuid NOT NULL REFERENCES "Person"."Persons", "StartDate" timestamp NOT NULL, "EndDate" timestamp,
 PRIMARY KEY ("Phone", "AdditionalCode", "StartDate"), CHECK ("EndDate" > "StartDate")
);
CREATE TABLE "Organization"."Positions" (
 "ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(), "Name" text NOT NULL UNIQUE,
 "ShortName" text, "ShortestName" text, "IsTeacher" boolean NOT NULL DEFAULT false,
 "IsAdministrative" boolean NOT NULL DEFAULT false, "IsTeachingSuppl" boolean NOT NULL DEFAULT false,
 "IsTechnical" boolean NOT NULL DEFAULT false, "IsHousehold" boolean NOT NULL DEFAULT false, "IsHead" boolean NOT NULL DEFAULT false
);
CREATE TABLE "Organization"."OrgUnits" (
 "ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(), "Name" text NOT NULL UNIQUE, "UnitCode" text NOT NULL UNIQUE,
 "ShortName" text, "ShortestName" text, "IsDepartmentGeneral" boolean NOT NULL DEFAULT false,
 "IsDepartmentGraduate" boolean NOT NULL DEFAULT false,
 "IsDepartment" boolean GENERATED ALWAYS AS ("IsDepartmentGeneral" OR "IsDepartmentGraduate") STORED,
 "IsFaculty" boolean NOT NULL DEFAULT false, "IsBranch" boolean NOT NULL DEFAULT false,
 "IsTechnical" boolean NOT NULL DEFAULT false, "IsHousehold" boolean NOT NULL DEFAULT false,
 "IsNotEducational" boolean GENERATED ALWAYS AS
 (NOT ("IsDepartmentGeneral" OR "IsDepartmentGraduate" OR "IsFaculty" OR "IsBranch")) STORED
);
CREATE TABLE "Person"."PersonEmployments" (
 "PersonID" uuid NOT NULL REFERENCES "Person"."Persons", "PositionID" uuid NOT NULL REFERENCES "Organization"."Positions",
 "OrgUnitID" uuid NOT NULL REFERENCES "Organization"."OrgUnits", "IsMain" boolean NOT NULL DEFAULT true,
 "StartDate" timestamp NOT NULL, "EndDate" timestamp,
 PRIMARY KEY ("PersonID", "PositionID", "OrgUnitID", "StartDate"), CHECK ("EndDate" > "StartDate")
);
CREATE TABLE "Person"."PersonEmploymentsInformal" (
 "PersonID" uuid NOT NULL REFERENCES "Person"."Persons", "OrgUnitID" uuid NOT NULL REFERENCES "Organization"."OrgUnits",
 "StartDate" timestamp NOT NULL, "EndDate" timestamp,
 PRIMARY KEY ("PersonID", "OrgUnitID", "StartDate"), CHECK ("EndDate" > "StartDate")
);
CREATE VIEW "Organization"."InstructorsView" AS
 SELECT e."PersonID", e."PositionID", e."OrgUnitID", false AS "IsInformal", e."StartDate", e."EndDate"
 FROM "Person"."PersonEmployments" e JOIN "Organization"."Positions" p ON p."ID"=e."PositionID" WHERE p."IsTeacher"
 UNION ALL SELECT "PersonID", NULL::uuid, "OrgUnitID", true, "StartDate", "EndDate" FROM "Person"."PersonEmploymentsInformal";
CREATE TABLE "Education"."Specialties" (
 "SpecialtyCode" text PRIMARY KEY, "SpecialtyCaption" text NOT NULL,
 "EducationLevel" integer NOT NULL CHECK ("EducationLevel" BETWEEN 1 AND 4),
 "StartDate" timestamp NOT NULL, "EndDate" timestamp, CHECK ("EndDate" > "StartDate")
);
CREATE TABLE "Education"."EducationalPrograms" (
 "ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(), "ProgramName" text NOT NULL,
 "SpecialtyCode" text NOT NULL REFERENCES "Education"."Specialties", "OrgUnitID" uuid NOT NULL REFERENCES "Organization"."OrgUnits",
 "CreatedAt" timestamp, "StartDate" timestamp NOT NULL, "EndDate" timestamp, CHECK ("EndDate" > "StartDate")
);
CREATE TABLE "Education"."EducationalGroups" (
 "GroupName" text NOT NULL, "GroupCaption" text NOT NULL,
 "EdProgramID" uuid NOT NULL REFERENCES "Education"."EducationalPrograms", "AdmissionYear" integer NOT NULL,
 "StartDate" timestamp NOT NULL, "EndDate" timestamp,
 PRIMARY KEY ("GroupName", "StartDate"), CHECK ("EndDate" > "StartDate")
);
CREATE UNIQUE INDEX "EducationalGroups_ActiveName" ON "Education"."EducationalGroups" ("GroupName") WHERE "EndDate" IS NULL;
CREATE TABLE "Organization"."GroupStudents" (
 "PersonID" uuid NOT NULL REFERENCES "Person"."Persons", "EdGroupID" text NOT NULL, "EdGroupStartDate" timestamp NOT NULL,
 "StartDate" timestamp NOT NULL, "EndDate" timestamp,
 PRIMARY KEY ("PersonID", "EdGroupID", "StartDate"), CHECK ("EndDate" > "StartDate"),
 FOREIGN KEY ("EdGroupID", "EdGroupStartDate") REFERENCES "Education"."EducationalGroups" ("GroupName", "StartDate")
);
CREATE TABLE "Education"."Courses" ("ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(), "Name" text NOT NULL);
CREATE TABLE "Education"."CoursePrograms" (
 "ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(), "EdCourseID" uuid NOT NULL REFERENCES "Education"."Courses", "Name" text NOT NULL,
 "EdProgramID" uuid NOT NULL REFERENCES "Education"."EducationalPrograms", "LectureHours" integer NOT NULL CHECK ("LectureHours">=0),
 "SeminarHours" integer NOT NULL CHECK ("SeminarHours">=0), "LabHours" integer NOT NULL CHECK ("LabHours">=0),
 "OtherPresenceHours" integer NOT NULL DEFAULT 0 CHECK ("OtherPresenceHours">=0), "NonAuditorialHours" integer CHECK ("NonAuditorialHours">=0),
 "IsElective" boolean NOT NULL DEFAULT false, "IsOptional" boolean NOT NULL DEFAULT false
);
CREATE TABLE "Education"."CourseAuthors" (
 "CourseProgramID" uuid NOT NULL REFERENCES "Education"."CoursePrograms", "PersonID" uuid NOT NULL REFERENCES "Person"."Persons",
 PRIMARY KEY ("CourseProgramID", "PersonID")
);
CREATE TABLE "Education"."CourseProgramModules" (
 "ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(), "CourseProgramID" uuid NOT NULL REFERENCES "Education"."CoursePrograms",
 "Name" text NOT NULL, "MaxScore" integer NOT NULL, "MinAcceptScore" integer NOT NULL, "IsAppraisal" boolean NOT NULL DEFAULT false,
 "OrderIndex" integer NOT NULL, CHECK ("MinAcceptScore">=0 AND "MaxScore">="MinAcceptScore"), UNIQUE ("CourseProgramID", "OrderIndex")
);
CREATE TABLE "Education"."CourseTaskTypes" ("ID" integer PRIMARY KEY, "Name" text NOT NULL UNIQUE);
CREATE TABLE "Education"."CourseTasks" (
 "ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(), "CourseModuleID" uuid NOT NULL REFERENCES "Education"."CourseProgramModules",
 "Name" text NOT NULL, "Description" text, "TaskTypeID" integer NOT NULL REFERENCES "Education"."CourseTaskTypes",
 "MaxScore" integer NOT NULL, "MinAcceptScore" integer NOT NULL, CHECK ("MinAcceptScore">=0 AND "MaxScore">="MinAcceptScore")
);
CREATE TABLE "Content"."ContentScopes" (
 "ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(), "ParentID" uuid REFERENCES "Content"."ContentScopes", CHECK ("ParentID" IS DISTINCT FROM "ID")
);
CREATE TABLE "Content"."CourseGroupSubScopes" (
 "CourseScopeID" uuid NOT NULL REFERENCES "Content"."ContentScopes", "GroupID" text NOT NULL, "GroupStartDate" timestamp NOT NULL,
 "SubscopeID" uuid NOT NULL REFERENCES "Content"."ContentScopes", PRIMARY KEY ("CourseScopeID", "GroupID", "GroupStartDate"),
 FOREIGN KEY ("GroupID", "GroupStartDate") REFERENCES "Education"."EducationalGroups" ("GroupName", "StartDate")
);
CREATE TABLE "Course"."Courses" (
 "ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(), "Name" text NOT NULL, "ContentScopeID" uuid NOT NULL UNIQUE REFERENCES "Content"."ContentScopes"
);
CREATE TABLE "Course"."CourseCurators" (
 "CourseID" uuid NOT NULL REFERENCES "Course"."Courses", "PersonID" uuid NOT NULL REFERENCES "Person"."Persons",
 "StartDate" timestamp NOT NULL, "EndDate" timestamp, "IsLead" boolean NOT NULL DEFAULT false,
 PRIMARY KEY ("CourseID", "PersonID", "StartDate"), CHECK ("EndDate">"StartDate")
);
CREATE UNIQUE INDEX "CourseCurators_Active" ON "Course"."CourseCurators" ("CourseID", "PersonID") WHERE "EndDate" IS NULL;
CREATE TABLE "Course"."CourseInstances" (
 "CourseID" uuid NOT NULL REFERENCES "Course"."Courses", "Year" integer NOT NULL,
 "ContentScopeID" uuid NOT NULL UNIQUE REFERENCES "Content"."ContentScopes", "CreatedAt" timestamp NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
 PRIMARY KEY ("CourseID", "Year")
);
CREATE TABLE "Course"."CourseInstanceEdPrograms" (
 "CourseID" uuid NOT NULL, "Year" integer NOT NULL, "CourseProgramID" uuid NOT NULL REFERENCES "Education"."CoursePrograms",
 "IsMain" boolean NOT NULL DEFAULT false, PRIMARY KEY ("CourseID", "Year", "CourseProgramID"),
 FOREIGN KEY ("CourseID", "Year") REFERENCES "Course"."CourseInstances"
);
CREATE UNIQUE INDEX "CourseInstanceEdPrograms_OneMain" ON "Course"."CourseInstanceEdPrograms" ("CourseID", "Year") WHERE "IsMain";
CREATE TABLE "Course"."CourseInstanceModules" (
 "ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(), "CourseID" uuid NOT NULL, "Year" integer NOT NULL,
 "CourseModuleID" uuid REFERENCES "Education"."CourseProgramModules", FOREIGN KEY ("CourseID", "Year") REFERENCES "Course"."CourseInstances"
);
CREATE TABLE "Course"."CourseInstanceModuleTasks" (
 "ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(), "Name" text NOT NULL,
 "CourseInstanceModuleID" uuid NOT NULL REFERENCES "Course"."CourseInstanceModules", "TaskTypeID" integer NOT NULL REFERENCES "Education"."CourseTaskTypes",
 "TaskID" uuid REFERENCES "Education"."CourseTasks", "MaxScore" integer NOT NULL, "MinAcceptScore" integer NOT NULL,
 "TaskDue" timestamp, "OrderIndex" integer NOT NULL, "StartDate" timestamp NOT NULL, "EndDate" timestamp,
 CHECK ("EndDate">"StartDate"), CHECK ("MinAcceptScore">=0 AND "MaxScore">="MinAcceptScore"), UNIQUE ("CourseInstanceModuleID", "OrderIndex")
);
CREATE TABLE "Course"."CourseInstanceTeachers" (
 "CourseID" uuid NOT NULL, "Year" integer NOT NULL, "PersonID" uuid NOT NULL REFERENCES "Person"."Persons",
 "StartDate" timestamp NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'), "EndDate" timestamp,
 PRIMARY KEY ("CourseID", "Year", "PersonID"), CHECK ("EndDate">"StartDate"),
 FOREIGN KEY ("CourseID", "Year") REFERENCES "Course"."CourseInstances"
);
CREATE TABLE "Course"."CourseInstanceStudents" (
 "CourseID" uuid NOT NULL, "Year" integer NOT NULL, "PersonID" uuid NOT NULL REFERENCES "Person"."Persons",
 "StartDate" timestamp NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'), "EndDate" timestamp,
 PRIMARY KEY ("CourseID", "Year", "PersonID"), CHECK ("EndDate">"StartDate"),
 FOREIGN KEY ("CourseID", "Year") REFERENCES "Course"."CourseInstances"
);
CREATE TABLE "Course"."CourseInstanceStudentTasks" (
 "CourseInstanceModuleTaskID" uuid NOT NULL REFERENCES "Course"."CourseInstanceModuleTasks", "PersonID" uuid NOT NULL REFERENCES "Person"."Persons",
 "Score" integer NOT NULL, "AcceptanceDate" timestamp NOT NULL, "Comment" text, "OrderIndex" integer NOT NULL,
 "StartDate" timestamp NOT NULL, "EndDate" timestamp, PRIMARY KEY ("CourseInstanceModuleTaskID", "PersonID", "OrderIndex"), CHECK ("EndDate">"StartDate")
);
CREATE TABLE "CourseReport"."ColumnOperations" ("ID" text PRIMARY KEY, "Description" text NOT NULL DEFAULT '');
INSERT INTO "CourseReport"."ColumnOperations" ("ID") VALUES ('SUM'),('AVG'),('MIN'),('MAX'),('COUNT');
CREATE TABLE "CourseReport"."ValueFormats" ("ID" text PRIMARY KEY);
INSERT INTO "CourseReport"."ValueFormats" VALUES ('NUMERIC'),('ALPHENG'),('ALPHRUS');
CREATE TABLE "CourseReport"."AppraisalScales" (
 "ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(), "Name" text NOT NULL, "Description" text NOT NULL DEFAULT '',
 "IsEnum" boolean NOT NULL, "IsLinear" boolean NOT NULL, CHECK ("IsEnum"<>"IsLinear")
);
CREATE TABLE "CourseReport"."AppraisalScalesLinear" (
 "ID" uuid PRIMARY KEY REFERENCES "CourseReport"."AppraisalScales", "MinValue" integer NOT NULL, "MaxValue" integer NOT NULL,
 "ValueFormat" text NOT NULL REFERENCES "CourseReport"."ValueFormats", CHECK ("MaxValue">="MinValue")
);
CREATE TABLE "CourseReport"."AppraisalScalesEnumValues" (
 "ScaleID" uuid NOT NULL REFERENCES "CourseReport"."AppraisalScales", "Name" text NOT NULL,
 "Description" text NOT NULL DEFAULT '', "Code" integer NOT NULL, PRIMARY KEY ("ScaleID", "Code"), UNIQUE ("ScaleID", "Name")
);
CREATE TABLE "CourseReport"."EnumValuesPartialOrdering" (
 "Scale1ID" uuid NOT NULL, "Code1" integer NOT NULL, "Scale2ID" uuid NOT NULL, "Code2" integer NOT NULL,
 PRIMARY KEY ("Scale1ID", "Code1", "Scale2ID", "Code2"),
 FOREIGN KEY ("Scale1ID", "Code1") REFERENCES "CourseReport"."AppraisalScalesEnumValues",
 FOREIGN KEY ("Scale2ID", "Code2") REFERENCES "CourseReport"."AppraisalScalesEnumValues",
 CHECK (("Scale1ID", "Code1")<>("Scale2ID", "Code2"))
);
CREATE TABLE "CourseReport"."AppraisalLists" (
 "ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(), "Name" text NOT NULL, "Description" text,
 "CourseID" uuid NOT NULL, "Year" integer NOT NULL, "CourseProgramID" uuid REFERENCES "Education"."CoursePrograms",
 "DateCreated" timestamp NOT NULL, "DateClosed" timestamp, "Comment" text NOT NULL DEFAULT '',
 FOREIGN KEY ("CourseID", "Year") REFERENCES "Course"."CourseInstances", CHECK ("DateClosed">="DateCreated")
);
CREATE TABLE "CourseReport"."AppraisalListColumns" (
 "ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(), "Name" text NOT NULL, "Description" text DEFAULT '',
 "AppraisalListID" uuid NOT NULL REFERENCES "CourseReport"."AppraisalLists", "OrderIndex" integer NOT NULL,
 "CourseInstanceModuleID" uuid REFERENCES "Course"."CourseInstanceModules", "ScaleID" uuid REFERENCES "CourseReport"."AppraisalScales",
 "MaxScore" integer NOT NULL, "MinAcceptScore" integer NOT NULL, "Comment" text NOT NULL DEFAULT '', "IsComputed" boolean NOT NULL DEFAULT false,
 CHECK ("MinAcceptScore">=0 AND "MaxScore">="MinAcceptScore"), UNIQUE ("AppraisalListID", "OrderIndex")
);
CREATE TABLE "CourseReport"."AppraisalListColumnsCompute" (
 "ColumnID" uuid PRIMARY KEY REFERENCES "CourseReport"."AppraisalListColumns", "OperationID" text NOT NULL REFERENCES "CourseReport"."ColumnOperations"
);
CREATE TABLE "CourseReport"."AppraisalListColumnsDependencies" (
 "ColumnID" uuid NOT NULL REFERENCES "CourseReport"."AppraisalListColumns", "ColumnDepID" uuid NOT NULL REFERENCES "CourseReport"."AppraisalListColumns",
 PRIMARY KEY ("ColumnID", "ColumnDepID"), CHECK ("ColumnID"<>"ColumnDepID")
);
CREATE TABLE "CourseReport"."AppraisalScalesEnumRanges" (
 "ComputeColumnID" uuid NOT NULL REFERENCES "CourseReport"."AppraisalListColumns", "ScaleID" uuid NOT NULL, "Code" integer NOT NULL,
 "RangeStart" integer NOT NULL, "RangeEnd" integer NOT NULL, PRIMARY KEY ("ComputeColumnID", "ScaleID", "Code"),
 FOREIGN KEY ("ScaleID", "Code") REFERENCES "CourseReport"."AppraisalScalesEnumValues", CHECK ("RangeEnd">="RangeStart")
);
CREATE TABLE "CourseReport"."AppraisalListMarks" (
 "AppraisalListColumnID" uuid NOT NULL REFERENCES "CourseReport"."AppraisalListColumns", "StudentPersonID" uuid NOT NULL REFERENCES "Person"."Persons",
 "TeacherPersonID" uuid NOT NULL REFERENCES "Person"."Persons", "Score" integer NOT NULL, "ScoreSourceName" text,
 "UpdatedAt" timestamp NOT NULL, "OldScore" integer, "OldTeacherPersonID" uuid REFERENCES "Person"."Persons", "OldScoreSourceName" text,
 PRIMARY KEY ("AppraisalListColumnID", "StudentPersonID")
);
CREATE TABLE "CourseReport"."AppraisalListMarksHistory" (
 "AppraisalListColumnID" uuid NOT NULL REFERENCES "CourseReport"."AppraisalListColumns", "StudentPersonID" uuid NOT NULL REFERENCES "Person"."Persons",
 "TeacherPersonID" uuid NOT NULL REFERENCES "Person"."Persons", "Score" integer NOT NULL, "ScoreSourceName" text,
 "StartDate" timestamp NOT NULL, "EndDate" timestamp, PRIMARY KEY ("AppraisalListColumnID", "StudentPersonID", "StartDate"), CHECK ("EndDate">"StartDate")
);
CREATE TABLE "CourseLabs"."StudentCourseTasks" (
 "ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(), "Name" text NOT NULL, "CourseID" uuid NOT NULL, "Year" integer NOT NULL,
 "StudentPersonID" uuid NOT NULL REFERENCES "Person"."Persons", "TaskTypeID" integer NOT NULL REFERENCES "Education"."CourseTaskTypes",
 "MaxScore" integer NOT NULL, "MinAcceptScore" integer NOT NULL, "OrderIndex" integer NOT NULL,
 "StartDate" timestamp NOT NULL, "IsCancelled" boolean NOT NULL DEFAULT false, "EndDate" timestamp,
 FOREIGN KEY ("CourseID", "Year") REFERENCES "Course"."CourseInstances", CHECK ("EndDate">"StartDate"),
 CHECK ("MinAcceptScore">=0 AND "MaxScore">="MinAcceptScore"), UNIQUE ("CourseID", "Year", "StudentPersonID", "OrderIndex")
);
CREATE TABLE "CourseLabs"."CourseTaskOptions" (
 "ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(), "Name" text NOT NULL, "Description" text NOT NULL DEFAULT '',
 "Type" integer NOT NULL DEFAULT 0 CHECK ("Type" BETWEEN 0 AND 3), "CourseInstanceModuleTaskID" uuid NOT NULL REFERENCES "Course"."CourseInstanceModuleTasks"
);
CREATE TABLE "CourseLabs"."CourseTaskOptionValues" (
 "ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(), "Name" text NOT NULL, "Description" text NOT NULL DEFAULT '', "Value" integer NOT NULL,
 "CourseInstanceModuleTaskID" uuid NOT NULL REFERENCES "Course"."CourseInstanceModuleTasks"
);
CREATE TABLE "CourseLabs"."CourseTaskOptionAssignments" (
 "TaskOptionID" uuid NOT NULL REFERENCES "CourseLabs"."CourseTaskOptions", "ValueID" uuid NOT NULL REFERENCES "CourseLabs"."CourseTaskOptionValues",
 "Value" integer NOT NULL, PRIMARY KEY ("TaskOptionID", "ValueID")
);
CREATE TABLE "CourseLabs"."StudentCourseTaskOptions" (
 "TaskOptionID" uuid NOT NULL REFERENCES "CourseLabs"."CourseTaskOptions", "StudentCourseTaskID" uuid NOT NULL REFERENCES "CourseLabs"."StudentCourseTasks",
 "IsStudentSelection" boolean NOT NULL DEFAULT true, PRIMARY KEY ("TaskOptionID", "StudentCourseTaskID")
);
CREATE TABLE "CourseLabs"."Relations" ("ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(), "Name" text NOT NULL UNIQUE, "Description" text);
CREATE TABLE "CourseLabs"."CourseTaskOptionRelations" (
 "TaskOption1ID" uuid NOT NULL REFERENCES "CourseLabs"."CourseTaskOptions", "TaskOption2ID" uuid NOT NULL REFERENCES "CourseLabs"."CourseTaskOptions",
 "RelationID" uuid NOT NULL REFERENCES "CourseLabs"."Relations", PRIMARY KEY ("TaskOption1ID", "TaskOption2ID", "RelationID")
);
CREATE TABLE "CourseLabs"."TaskCriteria" (
 "ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(), "Name" text NOT NULL, "Description" text NOT NULL DEFAULT '',
 "OrderIndex" integer NOT NULL, "CourseInstanceModuleTaskID" uuid NOT NULL REFERENCES "Course"."CourseInstanceModuleTasks",
 "MinAcceptScore" integer NOT NULL, "MaxScore" integer NOT NULL, "IsMoreThanMaxScoreAllowed" boolean NOT NULL DEFAULT false,
 CHECK ("MinAcceptScore">=0 AND "MaxScore">="MinAcceptScore"), UNIQUE ("CourseInstanceModuleTaskID", "OrderIndex")
);
CREATE TABLE "CourseLabs"."StudentCourseTaskCriteria" (
 "StudentCourseTaskID" uuid NOT NULL REFERENCES "CourseLabs"."StudentCourseTasks", "TaskCriteriaID" uuid NOT NULL REFERENCES "CourseLabs"."TaskCriteria",
 "IsMandatory" boolean NOT NULL DEFAULT true, PRIMARY KEY ("StudentCourseTaskID", "TaskCriteriaID")
);
CREATE TABLE "CourseLabs"."CourseInstanceTaskCriteria" (
 "CourseInstanceModuleTaskID" uuid NOT NULL REFERENCES "Course"."CourseInstanceModuleTasks", "TaskCriteriaID" uuid NOT NULL REFERENCES "CourseLabs"."TaskCriteria",
 "IsMandatory" boolean NOT NULL DEFAULT true, PRIMARY KEY ("CourseInstanceModuleTaskID", "TaskCriteriaID")
);
CREATE TABLE "CourseLabs"."StudentCourseTaskLinks" (
 "StudentCourseTaskID" uuid NOT NULL REFERENCES "CourseLabs"."StudentCourseTasks",
 "CourseInstanceModuleTaskID" uuid NOT NULL REFERENCES "Course"."CourseInstanceModuleTasks", "RelationID" uuid NOT NULL REFERENCES "CourseLabs"."Relations",
 PRIMARY KEY ("StudentCourseTaskID", "CourseInstanceModuleTaskID", "RelationID")
);
CREATE TABLE "CourseLabs"."StudentVisits" (
 "ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(), "CourseID" uuid NOT NULL, "Year" integer NOT NULL,
 "StudentPersonID" uuid NOT NULL REFERENCES "Person"."Persons", "VisitDate" timestamp NOT NULL,
 "RegisterDate" timestamp NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'), "DataSourceName" text,
 "TeacherPersonID" uuid NOT NULL REFERENCES "Person"."Persons", "Comment" text,
 FOREIGN KEY ("CourseID", "Year") REFERENCES "Course"."CourseInstances", UNIQUE ("CourseID", "Year", "StudentPersonID", "VisitDate")
);
CREATE TABLE "CourseLabs"."StudentVisitDetails" (
 "ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(), "StudentVisitID" uuid NOT NULL REFERENCES "CourseLabs"."StudentVisits", "OrderIndex" integer NOT NULL,
 "StudentCourseTaskID" uuid NOT NULL REFERENCES "CourseLabs"."StudentCourseTasks", "CriterionID" uuid REFERENCES "CourseLabs"."TaskCriteria", "Score" integer,
 UNIQUE NULLS NOT DISTINCT ("StudentVisitID", "StudentCourseTaskID", "CriterionID")
);
CREATE TABLE "CourseLabs"."StudentVisitDetailComments" (
 "ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(), "StudentVisitID" uuid NOT NULL REFERENCES "CourseLabs"."StudentVisits",
 "CourseInstanceModuleTaskID" uuid NOT NULL REFERENCES "Course"."CourseInstanceModuleTasks", "OrderIndex" integer NOT NULL, "Comment" text NOT NULL,
 UNIQUE ("StudentVisitID", "CourseInstanceModuleTaskID", "OrderIndex")
);
CREATE TABLE "Achievements"."Achievements" (
 "ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(), "CourseID" uuid NOT NULL, "Year" integer NOT NULL, "Name" text NOT NULL, "Description" text,
 FOREIGN KEY ("CourseID", "Year") REFERENCES "Course"."CourseInstances", UNIQUE ("CourseID", "Year", "Name")
);
CREATE TABLE "Achievements"."AchievementCriteria" (
 "AchievementID" uuid PRIMARY KEY REFERENCES "Achievements"."Achievements", "ExpressionSource" text NOT NULL,
 "ExpressionCompiled" bytea NOT NULL, "IsEnabled" boolean NOT NULL DEFAULT true
);
CREATE TABLE "Achievements"."PersonAchievements" (
 "AchievementID" uuid NOT NULL REFERENCES "Achievements"."Achievements", "PersonID" uuid NOT NULL REFERENCES "Person"."Persons",
 "AchievementGotDate" timestamp NOT NULL, "AchievementFoundDate" timestamp NOT NULL, "Source" text,
 "IsNotificationSeen" boolean NOT NULL DEFAULT false, "IsFirstAnimationShown" boolean NOT NULL DEFAULT false,
 PRIMARY KEY ("AchievementID", "PersonID")
);
