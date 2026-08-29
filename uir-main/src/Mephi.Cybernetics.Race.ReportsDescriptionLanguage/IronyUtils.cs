using Irony.Parsing;

namespace Mephi.Cybernetics.Race.ReportsDescriptionLanguage
{
    public static class IronyUtils
    {
        public static Dictionary<string, ParseTreeNode> GetChildrenDict(this ParseTreeNode node)
        {
            return new Dictionary<string, ParseTreeNode>(node.ChildNodes.Select((child) => new KeyValuePair<string, ParseTreeNode>(child.Term.Name, child)));
        }

        private static HashSet<string> skipSet = new HashSet<string>{"(", ")"};
        //ужасный костыль, я не виноват, это всё Irony
        public static List<ParseTreeNode> GetChildNodes(this ParseTreeNode node)
        {
            return node.ChildNodes.Where(node => !(node.Token != null && skipSet.Contains(node.Token.ValueString))).ToList();
        }
    }
}