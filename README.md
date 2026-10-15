# summer_2026

## Attendance MVP

The attendance API uses password login and revocable HttpOnly-cookie sessions,
not unsigned demo tokens. Local Development also offers Admin/Teacher/Student
quick-login buttons, using server-held initial test credentials. They are not
available in Production; disable locally with `Auth__EnableDemoLogin=false`.
Start at [ATTENDANCE_MVP.md](ATTENDANCE_MVP.md)
for capabilities, authentication, endpoint contracts and remaining limitations.
Development account passwords are generated once in ignored
`.local/initial-credentials.json`. Never publish this file.

## Run the LMS and OCR service

Requirements: .NET 10 SDK for the local database bridge, .NET 8 runtime for
the existing Blazor client, and PostgreSQL 15+ (16.15 is installed locally).
Database schema, document adaptations, backup instructions and limitations:
[database/README.md](database/README.md).

From the repository root, start PostgreSQL and the database bridge first:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/start-local-database.ps1
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project .\omelea-dev-lms\LmsApi\LmsApi.csproj --urls http://localhost:8085
```

On another computer, install PostgreSQL or supply `ConnectionStrings__Lms`
for a dedicated database. Portable binaries and data in `.local/` are not in Git.
Then, in another terminal, start the client:

```powershell
dotnet run --project .\omelea-dev-lms\LMSDashboardBlazor_context_roles_dynamic_generate\LMSDashboardBlazor.csproj --urls http://localhost:5293
```

```powershell
cd attendance-ocr-service
# Clear PYTHONHOME only if it is set globally; it can break virtual environments.
$env:PYTHONHOME = ''
py -3.13 -m venv .venv
.\.venv\Scripts\python.exe -m pip install -r requirements-api.txt
.\.venv\Scripts\python.exe -m uvicorn app.main:app --host 127.0.0.1 --port 8010
```

Open http://localhost:5293/login and sign in with a generated local account.
The API-only requirements run the service and demo adapters, without downloading
large machine-learning dependencies. Real adapters additionally require their
original pipeline source, model weights, and `requirements.txt` dependencies.
Python 3.13 is the interpreter used for the API tests; use a version supported by
the chosen ML pipeline when installing its full dependencies.

## OCR Workflows

- Teacher: `/attendance`, select an editable lesson, open the photo-list section,
  upload JPG/PNG (up to 12 MiB), choose a mode, and recognize. Review every row,
  select a student from this course or exclude the row, then apply attendance.
  Existing marks remain unchanged unless replacement is explicitly enabled.
  The resulting journal edits use the existing Save/Revert workflow.
- Admin: `/admin`, choose the target course, upload a photo, and review names.
  Link rows to existing users or create students with editable names, logins,
  and record-book numbers. Confirming creates users and student enrollments
  together after validating the entire batch. Existing profiles and admin flags
  are never overwritten. Admin roster review is saved to PostgreSQL by the
  local LMS database bridge; it does not
  call the OCR service's mock LMS export or attendance-confirmation endpoint.
- Students have no OCR controls or journal-editing access in the demo interface.

**Demo mode returns fixed packaged names, regardless of the uploaded photo.**
The `Nasledie` and `OCR v1` modes perform actual recognition only when their
pipeline sources and weights are installed. These assets are not included here;
missing models produce an explicit error, without falling back to demo data.
The supplied real adapters are roster-constrained: names absent from the supplied
roster may be returned only as empty unmatched regions. Importing entirely new
names therefore depends on the OCR engine actually returning their text; the
integration does not invent missing names.

## Configuration and Limits

Client URLs: `omelea-dev-lms/LMSDashboardBlazor_context_roles_dynamic_generate/wwwroot/appsettings.json`,
keys `LmsApi:BaseUrl` (8085) and `AttendanceOcr:BaseUrl` (8010).
This is public client configuration, not a place for secrets.

OCR service environment variables (set before starting Uvicorn):

- `OCR_ALLOWED_ORIGINS`: comma-separated LMS origins, default
  `http://localhost:5293,http://127.0.0.1:5293`.
- `OCR_NASLEDIE_ROOT`: pipeline folder containing `e2e`.
- `OCR_NASLEDIE_MODELS_DIR`: folder containing `rot_regr.pt`, `text_det.pt`, `signs_det.pt`.
- `OCR_LEGACY_ROOT`, `OCR_LEGACY_DETECTION_MODEL`, `OCR_LEGACY_RECOGNITION_MODEL`:
  legacy pipeline and model paths.
- `OCR_DEVICE`: Nasledie device, default `cpu`.

Each OCR job stores its own LMS roster snapshot in SQLite; parallel courses do
not share or overwrite rosters. Uploaded images and jobs are stored under
`attendance-ocr-service/data/` (ignored by Git). The LMS catalog, enrollments,
lessons and saved attendance persist in PostgreSQL, including across full
refreshes. Monitoring uses these saved records. Journal edits are drafts until
Save is clicked. Each author's current statement is saved as a JSONB draft;
normalized academic marks are not yet connected to the free-form constructor.
Other original mock LMS widgets have not been migrated.

The C# API filters data, checks course permissions, validates CSRF tokens and
uses server-verified cookie sessions. The old demo username header and unsigned
tokens no longer authenticate. Browser OCR requests go through this C# gateway;
the Python OCR API itself still has no user authentication and must remain on
loopback or a private network. This is an **MVP**, not a production security
certification: HTTPS, protected storage, backups, retention policy and deployment
checks remain necessary before handling real student data publicly. The original
statement constructor retains its layout and calculations; only its save path
was connected to the database. University CAS/SSO is not connected.

To stop the project database after stopping the API:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/start-local-database.ps1 -Stop
```

The `attendance-ocr-service` source and integration changes are included as
ordinary files, not a submodule. Upstream provenance is recorded in
`attendance-ocr-service/UPSTREAM.md`. A nested `.git` on the original development
computer is retained locally but is not needed by clones. Virtual environments,
runtime databases, uploaded images and model weights are not published.

## Checks

```powershell
dotnet build .\omelea-dev-lms\LMSDashboardBlazor_context_roles_dynamic_generate\LMSDashboardBlazor.csproj
dotnet build .\omelea-dev-lms\LmsApi\LmsApi.csproj
dotnet run --project .\tests\LmsOcrChecks\LmsOcrChecks.csproj
# Requires running PostgreSQL and the database bridge:
.\attendance-ocr-service\.venv\Scripts\python.exe -m pytest tests/test_lms_database.py -q -p no:cacheprovider
.\attendance-ocr-service\.venv\Scripts\python.exe -m pytest tests/test_attendance_mvp.py -q -p no:cacheprovider
cd attendance-ocr-service
$env:PYTHONHOME = ''
.\.venv\Scripts\python.exe -m pytest -q
```

Optional browser checks (requires the database/API/client/OCR and Microsoft Edge):

```powershell
# From the repository root:
.\attendance-ocr-service\.venv\Scripts\python.exe -m pip install -r .\attendance-ocr-service\requirements-ui-checks.txt
.\attendance-ocr-service\.venv\Scripts\python.exe .\tests\ocr_browser_check.py
.\attendance-ocr-service\.venv\Scripts\python.exe .\tests\database_browser_check.py
.\attendance-ocr-service\.venv\Scripts\python.exe .\tests\mvp_browser_check.py
```
