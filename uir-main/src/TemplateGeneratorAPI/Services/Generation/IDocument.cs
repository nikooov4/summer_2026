using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;

namespace TemplateGeneratorAPI.Services.Generation
{
    public delegate string? ReplaceCallback(string match);
    
    public interface IDocument
    {
        void Open(string path);
        void Open(byte[] binaryFile);
        IDocument Copy(string destPath);
        IDocument Copy(Stream destStream);
        void Replace(string oldString, string newString);
        IEnumerable<string> FindAllRegex(Regex regex);
        void Save();
        void Close();
    }
}