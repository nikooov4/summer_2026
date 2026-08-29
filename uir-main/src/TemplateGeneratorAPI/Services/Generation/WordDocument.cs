using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using OpenXmlPowerTools;

namespace TemplateGeneratorAPI.Services.Generation
{
    public class WordDocument : IDocument
    {
        private WordprocessingDocument _document;

        public void Open(string path)
        {
            _document = WordprocessingDocument.Open(path, false);
        }

        public void Open(byte[] binaryFile)
        {
            var documentStream = new MemoryStream();

            documentStream.Write(binaryFile, 0, binaryFile.Length);

            _document = WordprocessingDocument.Open(documentStream, false);
        }

        public IDocument Copy(string destPath)
        {
            return new WordDocument { _document = (WordprocessingDocument)_document.Clone(destPath, true) };
        }

        public IDocument Copy(Stream destStream)
        {
            return new WordDocument { _document = (WordprocessingDocument)_document.Clone(destStream, true) };
        }

        public void Replace(string oldString, string newString)
        {
            if (_document.MainDocumentPart == null) return;

            var document = _document.MainDocumentPart.GetXDocument().Root;

            if (document == null) return;

            var content = document.Descendants(W.p).ToList();

            OpenXmlRegex.Replace(content, new Regex(oldString), newString, null);
        }

        public IEnumerable<string> FindAllRegex(Regex regex)
        {
            if (_document.MainDocumentPart == null) return new HashSet<string>();

            var document = _document.MainDocumentPart.GetXDocument().Root;

            if (document == null) return new HashSet<string>();

            var content = document.Descendants(W.p).ToList();

            var matches = new HashSet<string>();

            OpenXmlRegex.Match(content, regex, (_, match) => matches.Add(match.Value));

            return matches;

        }

        public void Save()
        {
            _document.MainDocumentPart.PutXDocument();
        }

        public void Close()
        {
            _document.Dispose();
        }
    }
}