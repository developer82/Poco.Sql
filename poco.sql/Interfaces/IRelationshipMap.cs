using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Poco.Sql
{
    public interface IRelationshipMap
    {
        string GetForeignKey();
        Type GetRelatedObjectType();
    }
}
