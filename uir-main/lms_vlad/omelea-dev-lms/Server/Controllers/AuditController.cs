using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mephi.Cyber.AuthService.Core.Models;
using System.Security.Claims;
using Mephi.Cyber.AuthService.Core.Utils;
using Mephi.Cyber.AuthService.Core;
using System.Net;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Authorization;

namespace Mephi.Cyber.AuthService.WebApi.Controllers
{
    [Authorize]
    public class AuditController : ControllerBase
    {
        private readonly FileAuditService _auditService;

        public AuditController()
        {
            _auditService = new FileAuditService();
        }

        [HttpGet]
        [Route("audit_logs")]
        public async Task<IActionResult> GetAuditLogs()
        {
            var logs = await _auditService.GetLogsAsync();
            return Ok(logs);
        }
    }
}