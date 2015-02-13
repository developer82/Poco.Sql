using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using PocoSql.Test.Models;
using Poco.Sql.Extensions;

namespace PocoSql.UnitTests
{
    [TestClass]
    public class PocoSqlTests
    {
        [TestMethod]
        public void test_the_most_basic_select_query_on_object_with_default_settings_and_no_mappings()
        {
            // Arrange
            var user = new User()
            {
                UserId = 1,
                Name = "Ophir Oren",
                Age = 32,
                Birthday = new DateTime(1982, 5, 6)
            };

            // Act
            var sql = user.PocoSql().Select().ToString();

            // Assert
            sql.Should().Be("select UserId, Age, Name, Birthday from User;");
        }
    }
}
