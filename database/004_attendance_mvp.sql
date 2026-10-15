ALTER TABLE "Attendance"."Lessons"
 ADD COLUMN "IsCancelled" boolean NOT NULL DEFAULT false;

-- Creation-time roster, not an effective-dated timetable. Catalog changes never update these rows.
CREATE TABLE "Attendance"."LessonRosters" (
 "LessonID" uuid NOT NULL REFERENCES "Attendance"."Lessons",
 "StudentPersonID" uuid NOT NULL REFERENCES "Person"."Persons",
 "FullName" text NOT NULL,
 "RecordBookNumber" text NOT NULL DEFAULT '',
 "FrozenAt" timestamp NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
 PRIMARY KEY ("LessonID", "StudentPersonID")
);

-- Visits are the authoritative legacy roster, including archived and excluded students.
INSERT INTO "Attendance"."LessonRosters" ("LessonID", "StudentPersonID", "FullName", "RecordBookNumber")
SELECT v."LessonID", v."StudentPersonID",
 COALESCE(u."DisplayName", NULLIF(concat_ws(' ', p."FirstName", p."LastName", p."MiddleName"), ''), v."StudentPersonID"::text),
 COALESCE(u."RecordBookNumber", '')
FROM "CourseLabs"."StudentVisits" v
JOIN "Person"."Persons" p ON p."ID"=v."StudentPersonID"
LEFT JOIN "Lms"."UserAccounts" u ON u."PersonID"=v."StudentPersonID"
WHERE v."LessonID" IS NOT NULL;

ALTER TABLE "CourseLabs"."StudentVisits"
 ADD CONSTRAINT "StudentVisits_FrozenRoster" FOREIGN KEY ("LessonID", "StudentPersonID")
 REFERENCES "Attendance"."LessonRosters" ("LessonID", "StudentPersonID");

-- Keep the legacy visit identity consistent when a lesson is rescheduled.
UPDATE "CourseLabs"."StudentVisits" v
SET "CourseID"=l."CourseID", "Year"=l."Year", "VisitDate"=l."StartsAt"
FROM "Attendance"."Lessons" l
WHERE v."LessonID"=l."ID" AND (v."CourseID", v."Year", v."VisitDate") IS DISTINCT FROM (l."CourseID", l."Year", l."StartsAt");
ALTER TABLE "Attendance"."Lessons"
 ADD CONSTRAINT "Lessons_VisitIdentity" UNIQUE ("ID", "CourseID", "Year", "StartsAt");
ALTER TABLE "CourseLabs"."StudentVisits"
 ADD CONSTRAINT "StudentVisits_LessonIdentity" FOREIGN KEY ("LessonID", "CourseID", "Year", "VisitDate")
 REFERENCES "Attendance"."Lessons" ("ID", "CourseID", "Year", "StartsAt") ON UPDATE CASCADE;

CREATE INDEX "VisitHistory_ByVisit" ON "Attendance"."VisitHistory" ("VisitID", "ChangedAt", "ID");
