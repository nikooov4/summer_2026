namespace LMSDashboardBlazor.Models.Attendance;

public sealed class OcrJob
{
    public string JobId { get; set; } = "";
    public string Status { get; set; } = "";
    public string? Error { get; set; }
    public OcrResult? Result { get; set; }
    public OcrReview? Review { get; set; }
}

public sealed class OcrResult
{
    public List<OcrRecognizedStudent> Students { get; set; } = [];
}

public sealed class OcrRecognizedStudent
{
    public string? StudentId { get; set; }
    public string SourceText { get; set; } = "";
    public double Confidence { get; set; }
    public List<string> RegionIds { get; set; } = [];
}

public sealed class OcrReview
{
    public List<OcrDecision> Decisions { get; set; } = [];
}

public sealed class OcrDecision
{
    public string DecisionId { get; set; } = "";
    public List<string> RegionIds { get; set; } = [];
}

public sealed class OcrImportRow
{
    public bool Include { get; set; } = true;
    public string SourceText { get; set; } = "";
    public string StudentId { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Username { get; set; } = "";
    public string RecordBookNumber { get; set; } = "";
    public string DecisionId { get; set; } = "";
    public double Confidence { get; set; }
}
