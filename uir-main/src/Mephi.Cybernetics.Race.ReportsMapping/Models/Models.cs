using System;
using System.Diagnostics;

namespace Mephi.Cybernetics.Race.AppraisalListsMapping.Models
{

    public class AppraisalLists
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid CourseID { get; set; }
        public int Year { get; set; }
        public Guid? CourseProgramID { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateClosed { get; set; }
        public string Comment { get; set; }


        public List<AppraisalListColumns> _columns { get; set; }

    }

    public class AppraisalListColumns
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid AppraisalListID { get; set; }
        public int OrderIndex { get; set; }
        public Guid? CourseInstanceModuleID { get; set; }
        public Guid? ScaleID { get; set; }
        public int MaxScore { get; set; }
        public int MinAcceptScore { get; set; }
        public string Comment { get; set; }
        public bool IsComputed { get; set; }


        public AppraisalScales _scale { get; set; }
        public AppraisalLists _list{ get; set; }

        public List<AppraisalListColumnsDependencies> _dependsFrom { get; set; }
        public List<AppraisalListColumnsDependencies> _dependedBy { get; set; }
        public List<AppraisalListColumnsCompute> _computeColumns { get; set; }
        public List<AppraisalListMarks> _marks{ get; set; }
    }

    public class AppraisalScales
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsEnum { get; set; }
        public bool IsLinear { get; set; }  

        public List<AppraisalListColumns> _columns { get; set; }
        public List<AppraisalScalesLinear> _linearScales { get; set; }
        public List<AppraisalScalesEnumValues> _enumValues { get; set; }
    }

    public class AppraisalScalesLinear
    {
        public Guid ID { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public string ValueFormat { get; set; }

        public AppraisalScales _scale { get; set; }
    }

    public class AppraisalScalesEnumValues
    {
        public Guid ScaleID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Code { get; set; }

        public AppraisalScales _scale { get; set; }
        public List<AppraisalScalesEnumRanges>? _ranges { get; set; }
    }

    public class AppraisalListColumnsCompute
    {
        public Guid ColumnID { get; set; }
        public string OperationID { get; set; }

        public AppraisalListColumns _column { get; set; }
        public ColumnOperations _operation { get; set; }
        public List<AppraisalScalesEnumRanges>? _ranges { get; set; }

    }

    public class AppraisalListColumnsDependencies
    {
        public Guid ColumnID { get; set; }
        public Guid ColumnDepID { get; set; }

        public AppraisalListColumns _column { get; set; }
        public AppraisalListColumns _dependsFrom { get; set; }

    }

    [DebuggerDisplay("{ID,nq}")]
    public class ColumnOperations
    {
        public string ID { get; set; }
        public string Description { get; set; }

        public List<AppraisalListColumnsCompute> _computeColumns { get; set; }
    }

    [DebuggerDisplay("{Score,nq}")]
    public class AppraisalListMarks
    {
        public Guid AppraisalListColumnID { get; set; }
        public Guid StudentPersonID { get; set; }
        public Guid? TeacherPersonID { get; set; }
        public int Score { get; set; }
        public string ScoreSourceName { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? OldScore { get; set; }
        public Guid? OldTeacherPersonID { get; set; }
        public string OldScoreSourceName { get; set; }

        public AppraisalListColumns _column { get; set; }
    }


    [DebuggerDisplay("{ToString(),nq}")]
    public class AppraisalScalesEnumRanges
    {
        public Guid EnumValueID { get; set;}
        public Guid ComputeColumnID { get; set;}
        public int RangeStart { get; set;}
        public int RangeEnd { get; set;}

        public AppraisalScalesEnumValues _enumValue;
        public AppraisalListColumnsCompute _column { get; set; }

        public override string ToString()
        {
            return $"{RangeStart}:{RangeEnd}={_enumValue.Name}";
        }
    }

    //public class AppraisalListMarksHistory
    //{
    //    public Guid AppraisalListColumnID { get; set; }
    //    public Guid StudentPersonID { get; set; }
    //    public Guid? TeacherPersonID { get; set; }
    //    public int Score { get; set; }
    //    public string ScoreSourceName { get; set; }
    //    public DateTime StartDate { get; set; }
    //    public DateTime? EndDate { get; set; }

    //    public AppraisalListColumns _column { get; set; }

    //}
}
