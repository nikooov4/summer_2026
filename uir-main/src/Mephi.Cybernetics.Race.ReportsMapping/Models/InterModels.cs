using Mephi.Cybernetics.Race.AppraisalListsMapping.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mephi.Cybernetics.Race.AppraisalListsMapping.Models
{
    public enum Operations
    {
        SUM = 0,
        MIN = 1,
        EnumRange = 2
    }

    public class InterAppraisalList
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Year { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateClosed { get; set; }
        public string Comment { get; set; }

        public List<InterAppraisalListColumn> Columns { get; set; }
    }

    public class InterAppraisalListColumn
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid AppraisalListID { get; set; }
        public int OrderIndex { get; set; }
        public Guid? CourseInstanceModuleID { get; set; }
        public int MaxScore { get; set; }
        public int MinAcceptScore { get; set; }
        public string Comment { get; set; }

        public InterAppraisalScale Scale { get; set; }
        //public AppraisalLists _list { get; set; }

        public List<InterAppraisalListColumn>? DependsFrom { get; set; }
        //public List<AppraisalListColumnsDependencies> _dependedBy { get; set; }
        public InterAppraisalListColumnsCompute? Computation { get; set; }
        public List<InterAppraisalListMark> Marks { get; set; }
    }

    [DebuggerDisplay("{Score,nq}")]
    public class InterAppraisalListMark
    {
        public Guid AppraisalListColumnID { get; set; }
        //public Guid StudentPersonID { get; set; }
        public Guid? TeacherPersonID { get; set; }
        public int Score { get; set; }
        public string ScoreSourceName { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? OldScore { get; set; }
        public Guid? OldTeacherPersonID { get; set; }
        public string OldScoreSourceName { get; set; }
    }

    public class InterAppraisalScale
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public InterAppraisalScaleLinear LinearScale { get; set; }
        public List<InterAppraisalScalesEnumValue> EnumValues { get; set; }
    }

    public class InterAppraisalScaleLinear
    {
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public string ValueFormat { get; set; }
    }

    public class InterAppraisalScalesEnumValue
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Code { get; set; }
    }

    public class InterAppraisalListColumnsCompute
    {
        public Operations OperationID { get; set; }

        //public AppraisalListColumns _column { get; set; }
        public List<InterAppraisalScalesEnumRange>? Ranges { get; set; }

    }


    [DebuggerDisplay("{ToString(),nq}")]
    public class InterAppraisalScalesEnumRange
    {
        public int RangeStart { get; set; }
        public int RangeEnd { get; set; }

        public InterAppraisalScalesEnumValue EnumValue;

        public override string ToString()
        {
            return $"{RangeStart}:{RangeEnd}={EnumValue.Name}";
        }
    }

  





}
