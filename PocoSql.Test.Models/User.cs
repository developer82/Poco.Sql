using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocoSql.Test.Models
{
    public class User
    {
        public User()
        {
            Orders = new List<Order>();
        }

        public int UserId { get; set; }
        public int Age { get; set; }
        public string Name { get; set; }
        public DateTime Birthday { get; set; }
        public IList<Order> Orders { get; set; }
    }
}
