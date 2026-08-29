using System;
using System.Collections.Generic;
using System.Linq;
using Mephi.Cyber.AuthService.Core.Models;
using Mephi.Cyber.AuthService.Core.Utils;

namespace Mephi.Cyber.AuthService.Core
{
    public class AuthAdmin : AuthObject
    {
        private readonly FileAuditService _auditService;
        
        public AuthAdmin(ApplicationContext context) : base(context)
        {
            _auditService = new FileAuditService();
        }

        public List<Admin> List()
        {
            return db.Admins.ToList();
        }

        public void Create(string username, string password, string email)
        {
            if (db.Admins.Any(a => a.Name == username))
            {
                throw new InvalidOperationException($"Admin with name '{username}' already exists");
            }

            Admin newAdmin = new()
            {
                Name = username,
                Email = email ?? string.Empty
            };
            
            int salt = Password.CreateRandomSalt();
            Password pwd = new Password(password, salt);
            string strHashedPassword = pwd.ComputeSaltedHash();
            newAdmin.Salt = salt;
            newAdmin.SaltedHash = strHashedPassword;
            
            db.Admins.Add(newAdmin);
            db.SaveChanges();
            
            _auditService.LogChangeAsync($"Admin {username} was created");
        }

        public void Delete(string name)
        {
            var admin = db.Admins.SingleOrDefault(a => a.Name == name);
            if (admin == null)
            {
                throw new InvalidOperationException($"Admin with name '{name}' does not exist");
            }
            
            db.Admins.Remove(admin);
            db.SaveChanges();
            
            _auditService.LogChangeAsync($"Admin {name} was deleted");
        }

        public void Rename(string name, string newName)
        {
            var admin = db.Admins.SingleOrDefault(a => a.Name == name);
            if (admin == null)
            {
                throw new InvalidOperationException($"Admin with name '{name}' does not exist");
            }
            
            if (db.Admins.Any(a => a.Name == newName))
            {
                throw new InvalidOperationException($"Admin with name '{newName}' already exists");
            }
            
            admin.Name = newName;
            db.SaveChanges();
            
            _auditService.LogChangeAsync($"Admin {name} was renamed to {newName}");
        }

        public void UpdatePassword(string name, string newPassword)
        {
            var admin = db.Admins.SingleOrDefault(a => a.Name == name);
            if (admin == null)
            {
                throw new InvalidOperationException($"Admin with name '{name}' does not exist");
            }
            
            int salt = Password.CreateRandomSalt();
            Password pwd = new Password(newPassword, salt);
            string strHashedPassword = pwd.ComputeSaltedHash();
            admin.Salt = salt;
            admin.SaltedHash = strHashedPassword;
            
            db.SaveChanges();
            
            _auditService.LogChangeAsync($"Password for admin {name} was updated");
        }
    }
}

