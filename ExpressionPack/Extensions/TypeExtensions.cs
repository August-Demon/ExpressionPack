using ExpressionPack.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ExpressionPack.Extensions
{
    public static class TypeExtensions
    {
        public static IReadOnlyCollection<MemberInfo> GetSerializableMembers(this Type type)
        {
            IReadOnlyCollection<MemberInfo> members = (
                from member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                where member.MemberType == MemberTypes.Property &&
                      !member.IsDefined(typeof(IgnoredMemberAttribute)) &&
                      member is PropertyInfo propertyInfo && propertyInfo.CanRead && propertyInfo.CanWrite &&
                      propertyInfo.GetIndexParameters().Length == 0
                orderby member.Name
                select member
            ).ToArray();
            return members;

        }

        // 获取 PropertyInfo

        public static IReadOnlyCollection<PropertyInfo> GetSerializableProperties(this Type type)
        {
            IReadOnlyCollection<PropertyInfo> properties =
            (from info in type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
             where info.CanRead && info.CanWrite &&
                   info.GetIndexParameters().Length == 0 &&
                   !info.IsDefined(typeof(IgnoredMemberAttribute), true)
             orderby info.Name
             select info).ToArray();

            return properties;
        }
    }
}
