using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressionPack.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoredMemberAttribute : Attribute { }
}
