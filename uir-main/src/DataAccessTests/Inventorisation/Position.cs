using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;


namespace Mephi.Cybernetics.Race.DataAccessTests.Inventorisation
{
    [Table("Positions", Schema = "ReferenceInfo")]
    public class Position
    {
        public const string GUID_FORMAT = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";

        public Position()
        {
            UserPositions = new List<UserPositions>();
        }
        public Guid Id { get; set; }
        [Required]
        public string Label { get; set; }
        public string FullName { get; set; }

        public virtual ICollection<UserPositions> UserPositions { get; set; }
    }
}
