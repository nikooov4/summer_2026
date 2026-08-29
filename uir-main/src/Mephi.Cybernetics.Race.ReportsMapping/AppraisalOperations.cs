using Mephi.Cybernetics.Race.AppraisalListsMapping.Models;
using Mephi.Cybernetics.Race.MappingDescriptionLanguage;
using Mephi.Cybernetics.Race.AppraisalListsMapping.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mephi.Cybernetics.Race.AppraisalListsMapping
{

    public delegate int Aggregation(List<(int mark, AppraisalListColumnsDependencies column)> args);

    public delegate int InterAggregation((List<(int mark, InterAppraisalListColumn column)> list, InterAppraisalListColumn col) args);


    public static class AppraisalOperations
    {
        private static Dictionary<Operations, Aggregation> _aggregations;
        private static Dictionary<Operations, InterAggregation> _aggregationsInter;


        static AppraisalOperations()
        {
            _aggregations = new Dictionary<Operations, Aggregation>()
            {
                {Operations.SUM, (args) => args.Select(a => a.mark).Sum() },
                {Operations.MIN, (args) => args.Select(a => a.mark).Min() },
                {Operations.EnumRange, EnumRange}
                //{Operations.InRangeThen, (l) => l.ConvertAll(v => (int)v).Min() },

            };


            _aggregationsInter = new Dictionary<Operations, InterAggregation>()
            {
                {Operations.SUM, (args) => args.list.Select(a => a.mark).Sum() },
                {Operations.MIN, (args) => args.list.Select(a => a.mark).Min() },
                {Operations.EnumRange, EnumRangeInter}
                //{Operations.InRangeThen, (l) => l.ConvertAll(v => (int)v).Min() },

            };
        }

        private static AppraisalScalesEnumRanges FindRange(int mark, List<AppraisalScalesEnumRanges> ranges) => ranges.First(r => mark >= r.RangeStart && mark <= r.RangeEnd);
        private static InterAppraisalScalesEnumRange FindRangeInter(int mark, List<InterAppraisalScalesEnumRange> ranges) => ranges.First(r => mark >= r.RangeStart && mark <= r.RangeEnd);

        private static int EnumRange(List<(int mark, AppraisalListColumnsDependencies column)> args)
        {
            Debug.Assert(args.Count == 1);
            var (mark, column) = args.Single();
            var range = FindRange(mark, column._column._computeColumns.Single()._ranges);
            return range._enumValue.Code;
        }

        private static int EnumRangeInter((List<(int mark, InterAppraisalListColumn column)> list, InterAppraisalListColumn col) args)
        {
            Debug.Assert(args.list.Count == 1);
            var (mark, column) = args.list.Single();
            var range = FindRangeInter(mark, args.col.Computation.Ranges);
            return range.EnumValue.Code;
        }

        public static Operations FromId(string id)
        {
            return id switch
            {
                "MIN" => Operations.MIN,
                "SUM" => Operations.SUM,
                "EnumRange" => Operations.EnumRange,
                _ => throw new InvalidProgramException($"No operation with id: {id}"),
            };
        }

        public static Aggregation Get(Operations op) => _aggregations[op];
        public static Aggregation Get(string id) => _aggregations[FromId(id)];

        public static InterAggregation GetInter(Operations op) => _aggregationsInter[op];
        public static InterAggregation GetInter(string id) => _aggregationsInter[FromId(id)];

    }
}
