﻿using Poco.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocoSql.Test.Models.Mappings
{
    public class UserMap : PocoSqlMapping<User>
    {
        public UserMap()
        {
            // Primary Key
            this.HasKey(t => t.UserId).AutoGenerated();

            // Properties
            this.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(20);

            // Table & Column Mappings
            this.ToTable("Users");
            this.Property(t => t.Name).HasColumnName("Name");
            this.Property(t => t.Birthday).HasColumnName("DateOfBirth");
            this.Property(t => t.Age).Ignore();

            /*
            this.MapToStoredProcedures(s =>
                {
                    s.Update().IncludeExecution().IncludeParameters(); //TODO: BUG - why does the result includes where caluse??? ---> exec stp_User_UpdateAge = @Age, Name = @Name, Birthday = @Birthday where UserId= 1;
                });
            */
        }
    }
}