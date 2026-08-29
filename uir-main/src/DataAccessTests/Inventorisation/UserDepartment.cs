using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;


namespace Mephi.Cybernetics.Race.DataAccessTests.Inventorisation
{
    [Table("UserDepartment", Schema = "UserInfo")]
    public class UserDepartment
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public virtual User User { get; set; }

        [Required]
        public Guid DepartmentId { get; set; }
        [Required]
        public virtual Department Department { get; set; }

        public bool IsWorkspace { get; set; }
    }
}
