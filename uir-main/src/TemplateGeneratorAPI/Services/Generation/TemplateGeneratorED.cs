using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Mephi.Cybernetics.Race.DataAccess.MetaObjects;
using TemplateGeneratorAPI.SubjectDomain;

namespace TemplateGeneratorAPI.Services.Generation
{
    public class TemplateGeneratorED : IDisposable
    {
        private delegate object AttributeGetter(object subjectDomain);
        private IDocument _templateDocument;
        private IDictionary<string, AttributeGetter> _placeholdersAttributesGetters;
        private bool _disposed;

        public TemplateGeneratorED(string pathToTemplate, Type subjectDomainType)
        {
            _GetFileType(pathToTemplate);

            _templateDocument.Open(pathToTemplate);

            _CreatePlaceholdersAttributesGetters(subjectDomainType);
        }

        public TemplateGeneratorED(string fileName, byte[] binaryFile, Type subjectDomainType)
        {
            _GetFileType(fileName);

            _templateDocument.Open(binaryFile);

            _CreatePlaceholdersAttributesGetters(subjectDomainType);
        }

        private void _CreatePlaceholdersAttributesGetters(Type subjectDomainType)
        {
            _placeholdersAttributesGetters = new Dictionary<string, AttributeGetter>();

            var searchPattern = new Regex(@"\<\<[a-zA-Z\.\(\)]*\>\>");
            var placeholders = _templateDocument.FindAllRegex(searchPattern);

            var subjectDomainED = EntityDescription.GetEntityDescription(subjectDomainType);

            foreach (var placeholder in placeholders)
            {
                var subjectSubdomainED = subjectDomainED;
                var placeholderParts = placeholder.TrimStart('<').TrimEnd('>').Split('.').Skip(1);

                _placeholdersAttributesGetters[placeholder] = (Object subjectDomain) =>
                {
                    var subjectSubdomain = new List<object>() { subjectDomain };

                    foreach (var placeholderPart in placeholderParts)
                    {
                        var newSubjectSubdomain = new List<object>();

                        var attr = subjectSubdomainED.Attributes[placeholderPart];

                        foreach (var item in subjectSubdomain)
                        {
                            var value = attr.GetValue(item);

                            switch (value)
                            {
                                case IEnumerable<object> enumVal:
                                    newSubjectSubdomain.AddRange(enumVal);
                                    break;
                                default:
                                    newSubjectSubdomain.Add(value);
                                    break;
                            }
                        }

                        subjectSubdomain = newSubjectSubdomain;

                        if (attr.IsRelation)
                        {
                            subjectSubdomainED = attr.RelatedEntity;
                        }
                    }

                    return subjectSubdomain;
                };
            }
        }

        private void _GetFileType(string fileName)
        {
            var fileInfo = new FileInfo(fileName);

            switch (fileInfo.Extension)
            {
                case ".doc":
                case ".docx":
                    _templateDocument = new WordDocument();
                    break;
                case ".tex":
                    _templateDocument = new LatexDocument();
                    break;
                default:
                    return;
            }
        }

        public void GenerateBySubjectDomain(Object subjectDomain, string destPath)
        {
            var destDocument = _templateDocument.Copy(destPath);

            foreach (var placeholderAttributeGetter in _placeholdersAttributesGetters)
            {
                destDocument.Replace(placeholderAttributeGetter.Key, _CastToString(placeholderAttributeGetter.Value(subjectDomain)));
            }

            destDocument.Save();
            destDocument.Close();
        }

        public void GenerateBySubjectDomain(Object subjectDomain, Stream destStream)
        {
            var destDocument = _templateDocument.Copy(destStream);

            foreach (var placeholderAttributeGetter in _placeholdersAttributesGetters)
            {
                var value = _CastToString(placeholderAttributeGetter.Value(subjectDomain));
                destDocument.Replace(placeholderAttributeGetter.Key, value);
            }

            destDocument.Save();
            destDocument.Close();
        }

        private static string? _CastToString(object obj)
        {
            switch (obj)
            {
                case string str:
                    return str;
                case IEnumerable enumVar:
                    var enumerator = enumVar.GetEnumerator();

                    if (!enumerator.MoveNext())
                    {
                        return string.Empty;
                    }

                    var builder = new StringBuilder().Append(enumerator.Current);

                    while (enumerator.MoveNext())
                    {
                        builder.Append(", ").Append(enumerator.Current);
                    }

                    return builder.ToString();
                default:
                    return obj.ToString();
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (_disposed) return;

            // If disposing equals true, dispose all managed
            // and unmanaged resources.
            if (disposing)
            {
                // Dispose managed resources.
                _templateDocument.Close();
            }

            // Note disposing has been done.
            _disposed = true;
        }

        ~TemplateGeneratorED()
        {
            Dispose(disposing: false);
        }
    }
}