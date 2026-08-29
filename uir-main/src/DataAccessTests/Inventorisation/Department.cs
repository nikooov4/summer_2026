using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mephi.Cybernetics.Race.DataAccessTests.Inventorisation
{
    [Table("Departments", Schema = "ReferenceInfo")]
    public class Department
    {
        public const string GUID_FORMAT1 = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";

        public Guid Id { get; set; }

        [Required]
        public string Label { get; set; }
        public string FullName { get; set; }

        public virtual ICollection<UserDepartment> UserDepartments { get; set; }
        public virtual ICollection<Room> Rooms { get; set; }
    }
}
