using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mephi.Cybernetics.Race.AppraisalListsMapping.Models
{
    public class AttestationAppraisalList
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Year { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateClosed { get; set; }
        public string Comment { get; set; }

        public List<string> Students { get; set; }
        public InterAppraisalListColumn Part1 { get; set; }
        public InterAppraisalListColumn Part2 { get; set; }
        public InterAppraisalListColumn Sum { get; set; }
        public InterAppraisalListColumn Attestation { get; set; }
    }

    public class MarksCountAppraisalList
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Year { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateClosed { get; set; }
        public string Comment { get; set; }

        public List<string> Students { get; set; }
        public InterAppraisalListColumn Twos { get; set; }
        public List<int> AllMarks { get; set; }
        public InterAppraisalListColumn Threes { get; set; }
        public InterAppraisalListColumn Fours { get; set; }
        public InterAppraisalListColumn Fives { get; set; }

    }
}
