using System;
using System.Diagnostics;
using System.IO;

namespace TemplateGeneratorAPI.Services.Latex
{
    public class Latex : IDisposable
    {
        public const string LatexBuildFileName = "main";

        private string _tempDirectory;
        private bool _disposed;

        public Latex() => _CreateTempDirectory();
        public string GetTexFileName() => LatexBuildFileName + ".tex";

        public string GetPdfFileName() => LatexBuildFileName + ".pdf";

        public byte[] CompilePDF(byte[] binaryTexFile)
        {
            _CreateTempTexFile(binaryTexFile);
            _RunLatexmk();
            var b = _ReadPdf();

            return b;
        }

        private void _CreateTempDirectory()
        {
            do
            {
                _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            } while (Directory.Exists(_tempDirectory));

            Directory.CreateDirectory(_tempDirectory);
        }

        private void _CreateTempTexFile(byte[] binaryTexFile)
        {
            File.WriteAllBytes(Path.Combine(_tempDirectory, GetTexFileName()), binaryTexFile);
        }

        private void _RunLatexmk()
        {
            var psi = new ProcessStartInfo
            {
                FileName = "latexmk",
                Arguments = "-xelatex -silent " + $"{GetTexFileName()}",
                RedirectStandardInput = true,
                WorkingDirectory = _tempDirectory,
            };

            var process = Process.Start(psi);
            process.WaitForExit();
        }

        private byte[] _ReadPdf()
        {
            return File.ReadAllBytes(Path.Combine(_tempDirectory, GetPdfFileName()));
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
                Directory.Delete(_tempDirectory, true);
            }

            // Note disposing has been done.
            _disposed = true;
        }

        ~Latex()
        {
            Dispose(disposing: false);
        }
    }
}
