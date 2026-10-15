using LMSDashboardBlazor.Models;
using LMSDashboardBlazor.Models.Attendance;
using LMSDashboardBlazor.Services.Admin;
using LMSDashboardBlazor.Services.Attendance;

var catalog = new LmsCatalogService();
var attendance = new AttendanceService(catalog);
var lesson = attendance.Data.Lessons.First(item => item.CourseId == "c-stis");
var entry = lesson.Entries.First(item => item.StudentId == "u-student");
entry.Status = AttendanceStatus.Unmarked;
ExpectFailure(() => attendance.ApplyRecognizedPresent("student", lesson.Id, [entry.StudentId], false));
ExpectFailure(() => attendance.ApplyRecognizedPresent("assistant", lesson.Id, [entry.StudentId], false));
ExpectFailure(() => attendance.ApplyRecognizedPresent("teacher", lesson.Id, ["u-student6"], false));
ExpectFailure(() => attendance.ApplyRecognizedPresent("teacher", lesson.Id, [entry.StudentId, entry.StudentId], false));
Check(entry.Status == AttendanceStatus.Unmarked, "Invalid requests leave marks untouched");
Check(attendance.ApplyRecognizedPresent("teacher", lesson.Id, [entry.StudentId], false) == 1, "Teacher marks enrolled student");
entry.Status = AttendanceStatus.Late; entry.LateMinutes = 7; entry.Comment = "Existing comment";
Check(attendance.ApplyRecognizedPresent("teacher", lesson.Id, [entry.StudentId], false) == 0, "Preserve existing late mark");
Check(entry.LateMinutes == 7, "Preserve late minutes");
Check(attendance.ApplyRecognizedPresent("teacher", lesson.Id, [entry.StudentId], true) == 1, "Explicit overwrite");
Check(entry.LateMinutes is null && entry.Comment == "Existing comment", "Clear late minutes, keep comment");
lesson.CompletedAt = DateTime.Now;
ExpectFailure(() => attendance.ApplyRecognizedPresent("admin", lesson.Id, [entry.StudentId], true));
lesson.CompletedAt = null;
attendance.SelectLesson(lesson.Id);
attendance.SaveSelectedLesson();
entry.Status = AttendanceStatus.Unmarked;
attendance.ApplyRecognizedPresent("teacher", lesson.Id, [entry.StudentId], false);
attendance.RevertSelectedLesson();
Check(lesson.Entries.First(item => item.StudentId == entry.StudentId).Status == AttendanceStatus.Present, "Revert restores saved journal");

var count = catalog.Users.Count;
var fresh = new OcrImportRow { StudentId = "__new", FullName = "Test Student", Username = "ocr_test", RecordBookNumber = "T-01" };
ExpectFailure(() => catalog.ImportOcrStudents("teacher", "c-stis", [fresh]));
ExpectFailure(() => catalog.ImportOcrStudents("admin", "missing", [fresh]));
ExpectFailure(() => catalog.ImportOcrStudents("admin", "c-stis", [fresh, new() { StudentId = "__new", FullName = "Second Student", Username = "admin" }]));
Check(catalog.Users.Count == count, "Batch validation is atomic");
var result = catalog.ImportOcrStudents("admin", "c-stis", [fresh]);
var created = catalog.GetUserByUsername("ocr_test")!;
Check(result == (1, 1) && !created.IsAdmin, "Create non-admin student and enroll");
Check(lesson.Entries.Any(item => item.StudentId == created.Id), "Journal synchronizes after import");
Check(catalog.ImportOcrStudents("admin", "c-stis", [new() { StudentId = created.Id }]) == (0, 0), "Repeat enrollment is idempotent");
var ignored = new OcrImportRow { Include = false, StudentId = "__new", FullName = "Ignored Person", Username = "ignored" };
catalog.ImportOcrStudents("admin", "c-stis", [new() { StudentId = created.Id }, ignored]);
Check(catalog.GetUserByUsername("ignored") is null, "Excluded rows are not imported");
Console.WriteLine("All LMS OCR integration checks passed.");

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    Console.WriteLine($"PASS: {message}");
}
static void ExpectFailure(Action action)
{
    try { action(); }
    catch (InvalidOperationException) { return; }
    throw new Exception("Expected validation failure");
}
