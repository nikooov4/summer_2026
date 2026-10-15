CREATE TABLE "Lms"."AuthCredentials" (
 "PersonID" uuid PRIMARY KEY REFERENCES "Lms"."UserAccounts",
 "PasswordHash" text NOT NULL,
 "IsEnabled" boolean NOT NULL DEFAULT true,
 "MustChangePassword" boolean NOT NULL DEFAULT true,
 "FailedAttempts" integer NOT NULL DEFAULT 0 CHECK ("FailedAttempts">=0),
 "LockoutUntil" timestamptz,
 "UpdatedAt" timestamptz NOT NULL DEFAULT now()
);
CREATE TABLE "Lms"."AuthSessions" (
 "ID" uuid PRIMARY KEY, "PersonID" uuid NOT NULL REFERENCES "Lms"."AuthCredentials",
 "CreatedAt" timestamptz NOT NULL DEFAULT now(), "ExpiresAt" timestamptz NOT NULL,
 "RevokedAt" timestamptz, CHECK ("ExpiresAt">"CreatedAt")
);
CREATE INDEX "AuthSessions_Active" ON "Lms"."AuthSessions" ("PersonID","ExpiresAt") WHERE "RevokedAt" IS NULL;
