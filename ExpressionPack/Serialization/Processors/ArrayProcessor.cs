using ExpressionPack.Delegates;
using ExpressionPack.Serialization.Buffers;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressionPack.Serialization.Processors
{
    public class ArrayProcessor<T> : TypeProcessor<T[]>
    {
        public static ArrayProcessor<T> Instance { get; } = new ArrayProcessor<T>();

        private static readonly BinarySerializer<T[]> serializer = BuildSerializer();

        private static readonly BinaryDeserializer<T[]> deserializer = BuildDeserializer();

        public override void ExpressionSerializer(T[] obj, ref BinaryWriter writer, Queue<int> queue) => serializer(obj, ref writer, queue);
        public override T[] ExpressionDeserializer(ref BinaryReader reader, Queue<int> queue) => deserializer(ref reader, queue);


        private static BinarySerializer<T[]> BuildSerializer()
        {
            var arr = Expression.Parameter(typeof(T[]), "arr");
            var writer = Expression.Parameter(typeof(BinaryWriter).MakeByRefType(), "writer");
            var queue = Expression.Parameter(typeof(Queue<int>), "queue");

            var proc = Processor.GetInstance(typeof(T));
            var buildSerializerMethod = proc.GetType().GetMethod("ExpressionSerializer");
            // 获取数组长度并入队列
            var lenVar = Expression.Property(arr, "Length");
            var enqueueCall = Expression.Call(queue, typeof(Queue<int>).GetMethod("Enqueue"), lenVar);
            // 循环变量 i
            var iVar = Expression.Variable(typeof(int), "i");
            var breakLabel = Expression.Label("LoopBreak");

            // 循环体：调用处理器的序列化方法并递增 i
            var loop = Expression.Loop(
               Expression.IfThenElse(
                   Expression.LessThan(iVar, lenVar),
                   Expression.Block(
                       Expression.Call(
                           Expression.Convert(Expression.Constant(proc), proc.GetType()),
                           buildSerializerMethod,
                           Expression.ArrayIndex(arr, iVar),
                           writer,
                           queue
                       ),
                       Expression.PostIncrementAssign(iVar)
                   ),
                   Expression.Break(breakLabel)
               ),
               breakLabel
           );
            // 最终的表达式块
            var body = Expression.Block(
                new[] { iVar },
                enqueueCall,
                Expression.Assign(iVar, Expression.Constant(0)),
                loop
            );
            return Expression.Lambda<BinarySerializer<T[]>>(body, arr, writer, queue).Compile();


        }

        private static BinaryDeserializer<T[]> BuildDeserializer()
        {
            // 定义 BinaryReader 和 Queue<int> 的参数
            var reader = Expression.Parameter(typeof(BinaryReader).MakeByRefType(), "reader");
            var queue = Expression.Parameter(typeof(Queue<int>), "queue");

            // 读取队列中的长度值
            var lenVar = Expression.Variable(typeof(int), "length");
            var dequeueCall = Expression.Call(queue, typeof(Queue<int>).GetMethod("Dequeue"));
            var assignLen = Expression.Assign(lenVar, dequeueCall);

            // 创建一个新的数组
            var arrVar = Expression.Variable(typeof(T[]), "arr");
            var assignArr = Expression.Assign(arrVar, Expression.NewArrayBounds(typeof(T), lenVar));

            // 数组i赋值
            var iVar = Expression.Variable(typeof(int), "i");
            var assignI = Expression.Assign(iVar, Expression.Constant(0));
            // 获取数组类型的实例
            var proc = Processor.GetInstance(typeof(T));
            var buildDeserializerMethod = proc.GetType().GetMethod("ExpressionDeserializer");

            // 调用对应类型的反序列化方法
            var callDeserializer = Expression.Call(
                Expression.Convert(Expression.Constant(proc), proc.GetType()),
                buildDeserializerMethod,
                reader,
                queue
            );

            // 循环写入数组
            var breakLabel = Expression.Label("LoopBreak");
            var loop = Expression.Loop(
                Expression.IfThenElse(
                    Expression.LessThan(iVar, lenVar),
                    Expression.Block(
                        Expression.Assign(Expression.ArrayAccess(arrVar, iVar), callDeserializer),
                        Expression.PostIncrementAssign(iVar)
                    ),
                    Expression.Break(breakLabel)
                ),
                breakLabel
            );
            // 最终的表达式块
            var body = Expression.Block(
                new[] { lenVar, arrVar, iVar },
                assignLen,
                assignArr,
                assignI,
                loop,
                arrVar // 返回数组
            );
            return Expression.Lambda<BinaryDeserializer<T[]>>(body, reader, queue).Compile();

        }

    }
}
