using System;
using System.Collections.Generic;

namespace ExpressionPack.Serialization.Processors
{
    public static class Processor
    {

        public static object GetInstance(Type type)
        {
            if (type == typeof(string))
                return StringProcessor.Instance;
            if (type.IsArray)
                return typeof(ArrayProcessor<>).MakeGenericType(type.GetElementType())
                   .GetProperty("Instance").GetValue(null);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return typeof(ListProcessor<>).MakeGenericType(type.GetGenericArguments()[0])
                    .GetProperty("Instance").GetValue(null);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return typeof(IEnumerableProcessor<>).MakeGenericType(type.GetGenericArguments()[0])
                    .GetProperty("Instance").GetValue(null);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>))
                return typeof(IReadOnlyCollectionProcessor<>).MakeGenericType(type.GetGenericArguments()[0])
                    .GetProperty("Instance").GetValue(null);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>))
                return typeof(ICollectionProcessor<>).MakeGenericType(type.GetGenericArguments()[0])
                    .GetProperty("Instance").GetValue(null);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                return typeof(DictionaryProcessor<,>)
                     .MakeGenericType(type.GetGenericArguments())
                     .GetProperty("Instance").GetValue(null);
            if (type.IsValueType && (type.IsPrimitive || type == typeof(decimal) || type == typeof(DateTime)))
                return typeof(PrimitiveProcessor<>).MakeGenericType(type)
                   .GetProperty("Instance").GetValue(null);
            return typeof(ObjectProcessor<>).MakeGenericType(type)
                .GetProperty("Instance").GetValue(null);
        }
    }
}
