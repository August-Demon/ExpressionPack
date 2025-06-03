using ExpressionPack.Delegates;
using ExpressionPack.Serialization.Buffers;
using ExpressionPack.Serialization.Reflection;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressionPack.Serialization.Processors
{
    public class PrimitiveProcessor<T> : TypeProcessor<T> where T : struct
    {
        public static PrimitiveProcessor<T> Instance { get; } = new PrimitiveProcessor<T>();

        private static readonly BinarySerializer<T> serializer = BuildSerializer();

        private static readonly BinaryDeserializer<T> deserializer = BuildDeserializer();



        public override void ExpressionSerializer(T obj, ref BinaryWriter writer, Queue<int> queue) => serializer(obj, ref writer, queue);

        public override T ExpressionDeserializer(ref BinaryReader reader, Queue<int> queue) => deserializer(ref reader, queue);

        private static BinarySerializer<T> BuildSerializer()
        {
            var obj = Expression.Parameter(typeof(T), "obj");
            var writer = Expression.Parameter(typeof(BinaryWriter).MakeByRefType(), "writer");
            var queue = Expression.Parameter(typeof(Queue<int>), "queue");

            var writeMethod = Writer.WriteT(typeof(T));

            var call = Expression.Call(writer, writeMethod, obj);

            return Expression.Lambda<BinarySerializer<T>>(call, obj, writer, queue).Compile();
        }

        private static BinaryDeserializer<T> BuildDeserializer()
        {

            var reader = Expression.Parameter(typeof(BinaryReader).MakeByRefType(), "reader");
            var queue = Expression.Parameter(typeof(Queue<int>), "queue");

            var readMethod = Reader.ReadT(typeof(T));

            var call = Expression.Call(reader, readMethod);
            return Expression.Lambda<BinaryDeserializer<T>>(call, reader, queue).Compile();
        }

       
    }
}
