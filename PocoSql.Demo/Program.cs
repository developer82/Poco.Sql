using Poco.Sql;
using Poco.Sql.Exceptions;
using Poco.Sql.Extensions;
using PocoSql.Test.Models;
using PocoSql.Test.Models.Mappings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocoSql.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: Bug: Values are injected in WHERE statement when they should be parameterized

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(@" ____   __    ___  __     ____   __   __   ");
            Console.WriteLine(@"(  _ \ /  \  / __)/  \   / ___) /  \ (  )  ");
            Console.WriteLine(@" ) __/(  O )( (__(  O )_ \___ \(  O )/ (_/\");
            Console.WriteLine(@"(__)   \__/  \___)\__/(_)(____/ \__\)\____/");
            Console.WriteLine("");
            Console.WriteLine("Poco.Sql");
            Console.WriteLine("Written by Ophir Oren. All rights reserved © Ophir Oren 2015.");
            Console.WriteLine("Released under MIT license.");
            Console.WriteLine("http://www.webe.co.il");
            Console.WriteLine("");
            Console.ResetColor();

            QueryBuilder.Configuration.PluralizeTableNames = true; // for example: for object User the table name will be Users
            QueryBuilder.Configuration.StoredProceduresPrefix = "stp_";
            //QueryBuilder.Configuration.Comment = true;
            //QueryBuilder.Configuration.InjectValuesToQueies = true;
            //QueryBuilder.Configuration.SelectFullGraphAsDefault = true; // TODO: not completed yet

            // Add mappings
            QueryBuilder.AddStaticMapping(new UserMap());
            QueryBuilder.AddStaticMapping(new OrderMap());
            QueryBuilder.AddStaticMapping(new VUserMap());

            string sql;

            var user = new User()
            {
                UserId = 1,
                Age = 32,
                Name = "Ophir",
                Birthday = new DateTime(1982, 5, 6)
            };

            var vuser = new VUser()
            {
                UserId = 1,
                Age = 32,
                Name = "Ophir",
                Birthday = new DateTime(1982, 5, 6)
            };

            var orders = new List<Order>()
            {
                new Order() { UserId = 1, User = user, ItemName = "Item 1", OrderId = 1, Quantity = 5 },
                new Order() { UserId = 1, User = user, ItemName = "Item 2", OrderId = 1, Quantity = 4 },
                new Order() { UserId = 1, User = user, ItemName = "Item 3", OrderId = 1, Quantity = 3 },
                new Order() { UserId = 1, User = user, ItemName = "Item 4", OrderId = 1, Quantity = 2 },
                new Order() { UserId = 1, User = user, ItemName = "Item 5", OrderId = 1, Quantity = 1 }
            };

            sql = user.PocoSql().Select().ToString();
            Console.WriteLine("user.PocoSql().Select()");
            Console.WriteLine(sql + Environment.NewLine);

            sql = user.PocoSql().Insert().ToString();
            Console.WriteLine("user.PocoSql().Insert()");
            Console.WriteLine(sql + Environment.NewLine);

            sql = user.PocoSql().Update().ToString();
            Console.WriteLine("user.PocoSql().Update()");
            Console.WriteLine(sql + Environment.NewLine);

            sql = user.PocoSql().Delete().ToString();
            Console.WriteLine("user.PocoSql().Delete()");
            Console.WriteLine(sql + Environment.NewLine);

            sql = vuser.PocoSql().Select().ToString();
            Console.WriteLine("vuser.PocoSql().Select()");
            Console.WriteLine(sql + Environment.NewLine);

            sql = vuser.PocoSql().Select().Where<VUser>(u => u.UserId == 1).ToString();
            Console.WriteLine("vuser.PocoSql().Select()");
            Console.WriteLine(sql + Environment.NewLine);

            sql = vuser.PocoSql().Select().Where<VUser>(u => u.Birthday > DateTime.Now.AddYears(-50) && u.Birthday < DateTime.Now).ToString(); // TODO: this query is not performing correctly
            Console.WriteLine("vuser.PocoSql().Select()");
            Console.WriteLine(sql + Environment.NewLine);

            try
            {
                sql = vuser.PocoSql().Insert().ToString();
            }
            catch(CantUpdateVirtualException ex)
            {
                Console.WriteLine("The following exception is expected");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }

            Console.ReadLine();
        }
    }
}
