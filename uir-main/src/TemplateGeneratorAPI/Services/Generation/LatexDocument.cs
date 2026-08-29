using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System;

namespace TemplateGeneratorAPI.Services.Generation
{
    public class LatexDocument : IDocument
    {
        private string _texContent;
        private Stream _texStream;

        public void Open(string path)
        {
            _texContent = File.ReadAllText(path);
            _texStream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }

        public void Open(byte[] binaryFile)
        {
            _texContent = System.Text.Encoding.Default.GetString(binaryFile);
            _texStream = new MemoryStream(binaryFile);
        }

        public IDocument Copy(string destPath)
        {
            var uniEncoding = new UnicodeEncoding();

            using (var fs = File.OpenWrite(destPath))
            {
                _texStream.CopyTo(fs);
                _texStream.Position = 0;
            }

            var doc = new LatexDocument();
            doc.Open(destPath);

            return doc;
        }

        public IDocument Copy(Stream destStream)
        {
            _texStream.CopyTo(destStream);
            _texStream.Position = 0;
            destStream.Position = 0;

            var doc = new LatexDocument()
            {
                _texContent = _texContent,
                _texStream = destStream,
            };

            return doc;
        }

        public void Replace(string oldString, string newString)
        {
            _texContent = _texContent.Replace(oldString, newString);
        }

        public IEnumerable<string> FindAllRegex(Regex regex)
        {
            var matches = new HashSet<string>();

            var lineMatches = regex.Matches(_texContent);

            foreach (var lineMatch in lineMatches)
            {
                if (lineMatch != null) matches.Add(lineMatch.ToString());
            }

            return matches;
        }

        public void Save()
        {
            _texStream.Write(System.Text.Encoding.Default.GetBytes(_texContent));
            _texStream.Position = 0;
        }

        public void Close()
        {
            _texStream.Dispose();
        }
    }
}