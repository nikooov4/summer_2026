using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Mephi.Cybernetics.Race.DataAccessTests.Inventorisation
{
    [Table("Users", Schema = "UserInfo")]
    public class User
    {
        public User()
        {
            UserDepartments = new List<UserDepartment>();
            UserPositions = new List<UserPositions>();
        }

        [Required]
        public string Login { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public string Surname { get; set; }
        public string MiddleName { get; set; }

        public bool IsAdmin { get; set; }

        public virtual ICollection<UserDepartment> UserDepartments { get; set; }
        public virtual ICollection<UserPositions> UserPositions { get; set; }

        [NotMapped]
        public string Fio
        {
            get
            {
                return $"{Surname} {Name[0]}.{MiddleName[0]}.";
            }
        }
    }
}
