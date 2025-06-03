using ExpressionPack.Delegates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using ExpressionPack.Serialization.Buffers;

namespace ExpressionPack.Serialization.Processors
{
    public class IEnumerableProcessor<T> : TypeProcessor<IEnumerable<T>>
    {
        public static IEnumerableProcessor<T> Instance { get; } = new IEnumerableProcessor<T>();

        private static readonly BinarySerializer<IEnumerable<T>> serializer = BuildSerializer();
        private static readonly BinaryDeserializer<IEnumerable<T>> deserializer = BuildDeserializer();

        public override void ExpressionSerializer(IEnumerable<T> obj, ref BinaryWriter writer, Queue<int> queue)
            => serializer(obj, ref writer, queue);

        public override IEnumerable<T> ExpressionDeserializer(ref BinaryReader reader, Queue<int> queue)
            => deserializer(ref reader, queue);

        private static BinarySerializer<IEnumerable<T>> BuildSerializer()
        {
            // 转成 List<T> 统一处理
            var listProc = ListProcessor<T>.Instance;
            var method = listProc.GetType().GetMethod("ExpressionSerializer");
            var obj = Expression.Parameter(typeof(IEnumerable<T>), "obj");
            var writer = Expression.Parameter(typeof(BinaryWriter).MakeByRefType(), "writer");
            var queue = Expression.Parameter(typeof(Queue<int>), "queue");
            var call = Expression.Call(
                Expression.Convert(Expression.Constant(listProc), listProc.GetType()),
                method,
                Expression.Call(typeof(Enumerable), "ToList", new[] { typeof(T) }, obj),
                writer,
                queue
            );
            return Expression.Lambda<BinarySerializer<IEnumerable<T>>>(call, obj, writer, queue).Compile();
        }

        private static BinaryDeserializer<IEnumerable<T>> BuildDeserializer()
        {
            // 反序列化为 List<T>，返回 IEnumerable<T>
            var listProc = ListProcessor<T>.Instance;
            var method = listProc.GetType().GetMethod("ExpressionDeserializer");
            var reader = Expression.Parameter(typeof(BinaryReader).MakeByRefType(), "reader");
            var queue = Expression.Parameter(typeof(Queue<int>), "queue");
            var call = Expression.Call(
                Expression.Convert(Expression.Constant(listProc), listProc.GetType()),
                method,
                reader,
                queue
            );
            return Expression.Lambda<BinaryDeserializer<IEnumerable<T>>>(call, reader, queue).Compile();
        }
    }
}
