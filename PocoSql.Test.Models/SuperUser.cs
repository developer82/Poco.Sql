using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocoSql.Test.Models
{
    public class SuperUser : User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public int Permissions { get; set; }
    }
}
