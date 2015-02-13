using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocoSql.Test.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public User User { get; set; }
    }
}
