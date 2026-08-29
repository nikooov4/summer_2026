using System;
using System.Collections.Generic;
using System.Text;

namespace Mephi.Cybernetics.Race.DataAccessTests.Inventorisation
{
    public class UserInfoDto
    {
        public string Login { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string MiddleName { get; set; }
        public bool IsAdmin { get; set; }

        public ICollection<UserDepartmentDto> Departments { get; set; }
    }

    public class UserDepartmentDto
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public bool IsWorkspace { get; set; }
    }
}
