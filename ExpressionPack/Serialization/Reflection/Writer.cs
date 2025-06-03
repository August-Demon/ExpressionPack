using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ExpressionPack.Serialization.Reflection
{
    public static class Writer
    {
        private static readonly MethodInfo _WriteT = (
               from method in typeof(Buffers.BinaryWriter).GetMethods()
               where method.Name.Equals(nameof(Buffers.BinaryWriter.Write))
               let parameters = method.GetParameters()
               let generics = method.GetGenericArguments()
               where parameters.Length == 1 &&
                     generics.Length == 1 &&
                     parameters[0].ParameterType == generics[0]
               select method).First();


        //private static readonly MethodInfo _WriteSpanT = (
        //       from method in typeof(Buffers.BinaryWriter).GetMethods()
        //       where method.Name.Equals(nameof(Buffers.BinaryWriter.Write))
        //       let parameters = method.GetParameters()
        //       let generics = method.GetGenericArguments()
        //       where parameters.Length == 1 &&
        //             generics.Length == 1 &&
        //             parameters[0].ParameterType == typeof(Span<>).MakeGenericType(generics[0])
        //       select method).First();

        public static MethodInfo WriteT(Type type) => _WriteT.MakeGenericMethod(type);

        //public static MethodInfo WriteSpanT(Type type) => _WriteSpanT.MakeGenericMethod(type);
    }
}
