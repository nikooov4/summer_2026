using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;


namespace Mephi.Cybernetics.Race.DataAccessTests.Inventorisation
{
    [Table("Rooms", Schema = "ReferenceInfo")]
    public class Room
    {
        public Guid Id { get; set; }

        [Required]
        public int RoomNumber { get; set; }
        [Required]
        public string Label { get; set; }
        [Required]
        public string FullName { get; set; }

        [Required]
        public Guid DepartmentId { get; set; }
        [Required]
        public virtual Department Department { get; set; }
    }
}
