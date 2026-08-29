using System;
using System.Collections.Generic;
using System.Linq;
using Mephi.Cyber.AuthService.Core.Models;
using Mephi.Cyber.AuthService.Core.Utils;

namespace Mephi.Cyber.AuthService.Core
{
   
    public class FileAuditService {
        private readonly string _filePath;

        public FileAuditService()
        {
            // FIXME
            _filePath = "audit.log";
        }

        public async Task LogChangeAsync(string logMessage)
        {
            var logEntry = $"{DateTime.UtcNow}: {logMessage}{Environment.NewLine}";
            await File.AppendAllTextAsync(_filePath, logEntry);
        }

        public async Task<List<AuditLog>> GetLogsAsync()
        {
            var logs = new List<AuditLog>();

            if (File.Exists(_filePath))
            {
                var logLines = await File.ReadAllLinesAsync(_filePath);
                foreach (var line in logLines)
                {
                    var parts = line.Split(": ", 2);
                    if (parts.Length == 2)
                    {
                        logs.Add(new AuditLog
                        {
                            Timestamp = DateTime.Parse(parts[0]),
                            LogMessage = parts[1]
                        });
                    }
                }
            }

            return logs;
        }
    }
}


