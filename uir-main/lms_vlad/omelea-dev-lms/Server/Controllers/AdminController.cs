using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mephi.Cyber.AuthService.Core.Models;
using Mephi.Cyber.AuthService.Core;
using Microsoft.AspNetCore.Authorization;

namespace Mephi.Cyber.AuthService.WebApi.Controllers
{
    [Authorize]
    [Route("admin/admin")]
    public class AdminController : ControllerBase
    {
        private readonly AuthAdmin _authAdmin;
        readonly ApplicationContext db;
        
        public AdminController(ApplicationContext context)
        {
            db = context;
            _authAdmin = new AuthAdmin(db);
        }
        
        [Route("list")]
        [HttpGet]
        public IActionResult List()
        {
            return Ok(_authAdmin.List());
        }
        
        [Route("create")]
        [HttpPost]
        public IActionResult Create(string username, string password, string email)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest("Username and password are required");
            }

            if (db.Admins.Any(a => a.Name == username))
            {
                return BadRequest("Admin with such name already exists");
            }
            
            try
            {
                _authAdmin.Create(username, password, email ?? string.Empty);
                return Ok(new { message = "Admin created successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [Route("delete")]
        [HttpPost]
        public IActionResult Delete(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Admin name is required");
            }

            if (!db.Admins.Any(a => a.Name == name))
            {
                return BadRequest("Admin with such name does not exist");
            }
            
            try
            {
                _authAdmin.Delete(name);
                return Ok(new { message = "Admin deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("rename")]
        [HttpPost]
        public IActionResult Rename(string name, string newName)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(newName))
            {
                return BadRequest("Both current name and new name are required");
            }

            if (!db.Admins.Any(a => a.Name == name))
            {
                return BadRequest("Admin with such name does not exist");
            }

            if (db.Admins.Any(a => a.Name == newName))
            {
                return BadRequest("Admin with the new name already exists");
            }
            
            try
            {
                _authAdmin.Rename(name, newName);
                return Ok(new { message = "Admin renamed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("update-password")]
        [HttpPost]
        public IActionResult UpdatePassword(string name, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(newPassword))
            {
                return BadRequest("Admin name and new password are required");
            }

            if (!db.Admins.Any(a => a.Name == name))
            {
                return BadRequest("Admin with such name does not exist");
            }
            
            try
            {
                _authAdmin.UpdatePassword(name, newPassword);
                return Ok(new { message = "Password updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

