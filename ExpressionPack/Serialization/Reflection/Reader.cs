using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ExpressionPack.Serialization.Reflection
{
    public static class Reader
    {

        private static readonly MethodInfo _WriteT = (
                from method in typeof(Buffers.BinaryReader).GetMethods()
                where method.Name.Equals(nameof(Buffers.BinaryReader.Read))
                let parameters = method.GetParameters()
                let generics = method.GetGenericArguments()
                where parameters.Length == 0 &&
                      generics.Length == 1
                select method).First();

        public static MethodInfo ReadT(Type type) => _WriteT.MakeGenericMethod(type);

        //private static readonly MethodInfo _WriteSpanT = (
        //      from method in typeof(Buffers.BinaryReader).GetMethods()
        //      where method.Name.Equals(nameof(Buffers.BinaryReader.Read))
        //      let parameters = method.GetParameters()
        //      let generics = method.GetGenericArguments()
        //      where parameters.Length == 1 &&
        //            generics.Length == 1 &&
        //            parameters[0].ParameterType == typeof(Span<>).MakeGenericType(generics[0])
        //      select method).First();


        //public static MethodInfo ReadSpanT(Type type) => _WriteSpanT.MakeGenericMethod(type);
    }
}
