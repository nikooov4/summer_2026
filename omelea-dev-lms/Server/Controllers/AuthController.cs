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
using Microsoft.Extensions.Configuration;

namespace Mephi.Cyber.AuthService.WebApi.Controllers    
{
    public class AuthController : ControllerBase
    {
        private readonly FileAuditService _auditService;
        private readonly Auth _auth;
        private readonly AuthUser _authUser;
        private readonly IConfiguration _configuration;
        
        readonly ApplicationContext db;
        public AuthController(ApplicationContext context, IConfiguration configuration)
        {
            db = context;
            _auth = new Auth(db);
            _authUser = new AuthUser(db);
            _auditService = new FileAuditService();
            _configuration = configuration;

        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(string username, string password, string projectName)
        {
            if (!db.Projects.Any(p => p.Name == projectName))
            {
                return BadRequest("Project with such name does not exist");
            }
            if (!db.Users.Any(u => u.Name == username))
            {
                return Unauthorized("User with such name does not exist");
            }
            
            if (_auth.AllowedToLogin(username,password))
            {
                var secret = db.Projects.Single(p => p.Name == projectName).SharedSecret;
                var user = db.Users.Single(u => u.Name == username);
                var token = Token.GenerateToken(username, secret);
                
                // FIXME
                await _auditService.LogChangeAsync($"User {username} logged in");
                
                return Ok(token);
                
            }
            else
            {
                return Unauthorized("Incorrect password"!);
            }
        }
       
        [Route("user_permissions")]
        [HttpGet]
        public IActionResult GetPermission(string username, string projectName)
        {
            
            if (!db.Projects.Any(p => p.Name == projectName))
            {
                return BadRequest("Project with such name does not exist");
            }
            
            if (!db.Users.Any(u => u.Name == username))
            {
                return BadRequest("User with such name does not exist");
            }
            
            
            var authToken = HttpContext.Request.Headers["Authorization"];
            var realToken = db.Projects.Single(p => p.Name == projectName).AuthSecret;
            if (authToken != realToken)
            {
                return Unauthorized("Unauthorized");
            }
            var permissions = _auth.GetPermissions(username, projectName);
            return Ok(permissions);
        }

        [Route("mephi_login")]
        [HttpGet]
        public async Task<ActionResult> MephiSignIn(string ticket, string redirectUri, string projectName)
        {   
            if (!db.Projects.Any(p => p.Name == projectName))
            {
                return BadRequest("Project with such name does not exist");
            }
            
            var encodedRedirectUri = WebUtility.UrlEncode(redirectUri);
            var encodedProjectName = WebUtility.UrlEncode(projectName);
            var url = $"{Request.Scheme}://{Request.Host}{Request.Path}?redirectUri={encodedRedirectUri}&projectName={encodedProjectName}";
            
            var client = new HttpClient();
            
            // FIXME should be configuration parameter
            var uri = new Uri($"https://auth.mephi.ru/serviceValidate?ticket={ticket}&service={WebUtility.UrlEncode(url)}");
            var response = await client.GetAsync(uri);
            var textResult = await response.Content.ReadAsStreamAsync();

            XmlSerializer serializer = new XmlSerializer(typeof(serviceResponse));

            serviceResponse? serviceResponse = (serviceResponse?)serializer.Deserialize(textResult);

            var success = !string.IsNullOrEmpty(serviceResponse?.authenticationSuccess?.user);
            
            if (success && serviceResponse?.authenticationSuccess != null)
            {
                string username = serviceResponse.authenticationSuccess.user;
                if (!db.Users.Any(r => r.Name == username))
                {
                     // FIXME
                     var  password = RandomPasswordGenerator.GeneratePassword(passwordSize: 15);
                    _authUser.Create(username, password, new string[] {});
                }
                var user = db.Users.Single(u => u.Name == username);
                var secret = db.Projects.Single(p => p.Name == projectName).SharedSecret;
                var token = Token.GenerateToken(user.Name, secret);
                
                // FIXME
                await _auditService.LogChangeAsync($"User {username} logged in");
                
                return RedirectPermanent($"{redirectUri}/?token={token}");
            }

            return Unauthorized("Unauthorized");
        }

        [Route("oidc_login")]
        [HttpGet]
        public async Task<ActionResult>  Oidc(string id_token, string redirectUri, string projectName)
        {
            var key = "";
            var success = Token.ValidateRsaCurrentToken(id_token, key);
            if (success)
            {
               var  username = Token.GetClaim(id_token, "preferred_username");

                if (!db.Users.Any(r => r.Name == username))
                {   
                    // FIXME
                    var  password = RandomPasswordGenerator.GeneratePassword(passwordSize: 15);
                    _authUser.Create(username, password, new string[] {});
                }
                var user = db.Users.Single(u => u.Name == username);
                var secret = db.Projects.Single(p => p.Name == projectName).SharedSecret;
                var token = Token.GenerateToken(username, secret);
                return RedirectPermanent($"https://{redirectUri}/?token={token}");
            }



            return Unauthorized("Unauthorized");
        }

        [Route("check_user_permission")]
        [HttpGet]
        public IActionResult CheckPermission(string username, string projectName, string entityName, string actionName)
        {
            var res = false;
            
            if (!db.Projects.Any(p => p.Name == projectName))
            {
                return BadRequest("Project with such name does not exist");
            }
            
            if (!db.Users.Any(u => u.Name == username))
            {
                return BadRequest("User with such name does not exist");
            }
            
            if (!db.Entities.Any(e => e.Name == entityName))
            {
                return BadRequest("Object with such name does not exist");
            }
            
            var authToken = HttpContext.Request.Headers["Authorization"];
            var realToken = db.Projects.Single(p => p.Name == projectName).AuthSecret;
            if (authToken != realToken)
            {
                return Unauthorized("Unauthorized");
            }

            res = _auth.CheckPermissions(username, projectName, entityName, actionName);

            return Ok(res);
        }
        
         
        [HttpPost]
        [Route("adm_login")]
        public async Task<IActionResult> AdmLogin(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest("Username and password are required");
            }
            

            // Check if admin exists
            if (!db.Admins.Any(a => a.Name == username))
            {
                return Unauthorized("Admin with such name does not exist");
            }

            var admin = db.Admins.Single(a => a.Name == username);
            var saltFromDb = admin.Salt;
            var storedSaltedHash = admin.SaltedHash ?? string.Empty;
            Password pwd = new(password, saltFromDb);

            // Verify password
            if (pwd.ComputeSaltedHash() != storedSaltedHash)
            {
                return Unauthorized("Incorrect password");
            }

            // Get JWT secret from configuration
            var jwtSecret = _configuration["JwtSettings:AdminSecret"] 
                ?? throw new InvalidOperationException("JwtSettings:AdminSecret is not configured");
            
            // Get token expiration from configuration (default to 30 days for persistent sessions)
            var expirationMinutes = _configuration.GetValue<int>("JwtSettings:AccessTokenExpirationMinutes", 43200); // 30 days default
            var expireSec = expirationMinutes * 60;

            var token = Token.GenerateToken(username, jwtSecret, expireSec: expireSec);
            
            await _auditService.LogChangeAsync($"Admin {username} logged in");
            
            return Ok(token);
        }
        
        [HttpPost]
        [Route("adm_refresh")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult AdmRefresh()
        {
            // Get username from JWT token claims
            var usernameClaim = User.Claims.FirstOrDefault(c => c.Type == "username");
            if (usernameClaim == null || string.IsNullOrEmpty(usernameClaim.Value))
            {
                return Unauthorized("Invalid token");
            }

            var username = usernameClaim.Value;

            // Verify admin still exists
            if (!db.Admins.Any(a => a.Name == username))
            {
                return Unauthorized("Admin no longer exists");
            }

            // Get JWT secret from configuration
            var jwtSecret = _configuration["JwtSettings:AdminSecret"] 
                ?? throw new InvalidOperationException("JwtSettings:AdminSecret is not configured");
            
            // Get token expiration from configuration
            var expirationMinutes = _configuration.GetValue<int>("JwtSettings:AccessTokenExpirationMinutes", 43200);
            var expireSec = expirationMinutes * 60;

            var token = Token.GenerateToken(username, jwtSecret, expireSec: expireSec);
            
            return Ok(token);
        }
        
        

        //[Route("valid")]
        //[HttpGet]
        //public IActionResult Valid(string token)
        //{
        //    //string s = "";
        //    var secret = db.Projects.Single(p => p.Name == "test2").SharedSecret;
        //    if (!Token.ValidateCurrentToken(token, secret))
        //    {
        //        return Content("Failed");
        //    }
        //    var user = Token.GetClaim(token, "userId");

        //    return Content(user);
        //}

    }
}

