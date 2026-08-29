using System;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace Mephi.Cybernetics.Race.DataAccess.DataSources.Xml
{
    internal static class XmlUtils
    {
        public static XmlReader NavigateToNode(string nodePath, XmlReader reader)
        {
            string[] path = GetPathFromString(nodePath);
            return NavigateToNode(path, reader);
        }

        private static string[] GetPathFromString(string nodePath)
        {
            return nodePath.Replace("\\", ".").Replace("/", ".").Replace("->", ".").Split('.');
        }

        public static XmlReader NavigateToNode(string[] nodePath, XmlReader reader)
        {
            if (NavigateToNodeInternal(reader, nodePath))
                return reader.ReadSubtree();
            else
                return null;
        }

        private static bool NavigateToNodeInternal(XmlReader reader, string[] nodePath)
        {
            if (reader.EOF)
                return false;
            foreach (string s in nodePath)
                if (reader.Name != s && !reader.ReadToDescendant(s))
                    return false;
            return true;
        }

        public static bool NavigateThisToNode(XmlReader reader, string nodePath)
        {
            return NavigateToNodeInternal(reader, GetPathFromString(nodePath));
                //throw new Exception(string.Format("Cannot navigate to node '{0}': the specidied path is not found", nodePath));
        }

        public static XPathNavigator NavigateToNode(string nodePath, XPathNavigator navigator)
        {
            string[] path = GetPathFromString(nodePath);
            return NavigateToNode(path, navigator);
        }

        public static XPathNavigator NavigateToNode(string[] nodePath, XPathNavigator navigator)
        {
            var chidNavigator = navigator.CreateNavigator();
            foreach (string s in nodePath)
                if (chidNavigator.Name != s && !chidNavigator.MoveToChild(s,""))
                    return null;
            return chidNavigator;
        }

        public static XPathNavigator NavigateToNodeOrCreate(string nodePath, XPathNavigator navigator)
        {
            return NavigateToNodeOrCreate(GetPathFromString(nodePath), navigator);
        }

        public static XPathNavigator NavigateToNodeOrCreate(string[] nodePath, XPathNavigator navigator)
        {
            var chidNavigator = navigator.CreateNavigator();
            foreach (string s in nodePath)
                if (chidNavigator.Name != s && !chidNavigator.MoveToChild(s, ""))
                {
                    var builder = new StringBuilder();
                    builder.Append("<");
                    builder.Append(s);
                    builder.Append(">");
                    builder.Append("</");
                    builder.Append(s);
                    builder.Append(">");
                    chidNavigator.AppendChild(builder.ToString());
                    chidNavigator.MoveToChild(s, "");
                }
            return chidNavigator;
        }
    }
}
