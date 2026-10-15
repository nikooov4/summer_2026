ALTER TABLE "Attendance"."Lessons"
 ADD CONSTRAINT "Lessons_OcrCourse" UNIQUE ("ID", "CourseID");

CREATE TABLE "Lms"."OcrJobs" (
 "ID" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
 "UpstreamJobID" varchar(128) NOT NULL UNIQUE CHECK ("UpstreamJobID" ~ '^[A-Za-z0-9_-]+$'),
 "OwnerPersonID" uuid NOT NULL REFERENCES "Lms"."UserAccounts" ("PersonID"),
 "CourseID" uuid NOT NULL REFERENCES "Lms"."CourseSettings" ("CourseID"),
 "LessonID" uuid,
 "Status" text NOT NULL CHECK ("Status" IN ('created','queued','processing','needs_review','completed','failed','confirmed')),
 "CreatedAt" timestamp NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
 "UpdatedAt" timestamp NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
 FOREIGN KEY ("LessonID", "CourseID") REFERENCES "Attendance"."Lessons" ("ID", "CourseID")
);

CREATE INDEX "OcrJobs_ByOwner" ON "Lms"."OcrJobs" ("OwnerPersonID", "CreatedAt");
