using ExpressionPack.Delegates;
using ExpressionPack.Extensions;
using System.Collections.Generic;
using ExpressionPack.Serialization.Buffers;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressionPack.Serialization.Processors
{
    public class ObjectProcessor<T> : TypeProcessor<T> where T : new()
    {
        public static ObjectProcessor<T> Instance { get; } = new ObjectProcessor<T>();

        private static readonly IReadOnlyCollection<PropertyInfo> propertyInfos = typeof(T).GetSerializableProperties();

        private static readonly BinarySerializer<T> serializer = BuildSerializer();

        private static readonly BinaryDeserializer<T> deserializer = BuildDeserializer();

        


        public override void ExpressionSerializer(T obj, ref BinaryWriter writer, Queue<int> queue) => serializer(obj, ref writer, queue);

        public override T ExpressionDeserializer(ref BinaryReader reader, Queue<int> queue) => deserializer(ref reader, queue);


        private static BinarySerializer<T> BuildSerializer()
        {
            var obj = Expression.Parameter(typeof(T), "obj");
            var writer = Expression.Parameter(typeof(BinaryWriter).MakeByRefType(), "writer");
            var queue = Expression.Parameter(typeof(Queue<int>), "queue");

            var blockExprs = new List<Expression>();

            foreach (var prop in propertyInfos)
            {
                var valueExpr = Expression.Property(obj, prop);
                var proc = Processor.GetInstance(prop.PropertyType);
                var method = proc.GetType().GetMethod("ExpressionSerializer");
                blockExprs.Add(
                    Expression.Call(
                        Expression.Convert(Expression.Constant(proc), proc.GetType()),
                        method,
                        valueExpr,
                        writer,
                        queue
                    )
                );
            }
              
            var body = Expression.Block(blockExprs);
            return Expression.Lambda<BinarySerializer<T>>(body, obj, writer, queue).Compile();



        }

        private static BinaryDeserializer<T> BuildDeserializer()
        {
            var reader = Expression.Parameter(typeof(BinaryReader).MakeByRefType(), "reader");
            var queue = Expression.Parameter(typeof(Queue<int>), "queue");
            var objVar = Expression.Variable(typeof(T), "obj");
            var assignObj = Expression.Assign(objVar, Expression.New(typeof(T)));
            var blockExprs = new List<Expression> { assignObj };


            foreach (var prop in propertyInfos)
            {
                var proc = Processor.GetInstance(prop.PropertyType);
                var method = proc.GetType().GetMethod("ExpressionDeserializer");
                var call = Expression.Call(
                    Expression.Convert(Expression.Constant(proc), proc.GetType()),
                    method,
                    reader,
                    queue
                );
                var setProp = Expression.Call(objVar, prop.GetSetMethod(), call);
                blockExprs.Add(setProp);
            }
            blockExprs.Add(objVar);
            var body = Expression.Block(new[] { objVar }, blockExprs);
            return Expression.Lambda<BinaryDeserializer<T>>(body, reader, queue).Compile();
        }


       
    }
}
