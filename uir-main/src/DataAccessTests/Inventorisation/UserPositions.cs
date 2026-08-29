using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;


namespace Mephi.Cybernetics.Race.DataAccessTests.Inventorisation
{
    [Table("UserPosition", Schema = "UserInfo")]
    public class UserPositions
    {
        [Required]
        public string UserId { get; set; }
        public virtual User User { get; set; }

        [Required]
        public Guid PositionId { get; set; }
        public virtual Position Position { get; set; }
    }
}
