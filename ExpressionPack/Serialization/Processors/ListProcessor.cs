using ExpressionPack.Delegates;
using System.Collections.Generic;
using ExpressionPack.Serialization.Buffers;
using System.Linq.Expressions;

namespace ExpressionPack.Serialization.Processors
{
    public class ListProcessor<T> : TypeProcessor<List<T>>
    {
        public static ListProcessor<T> Instance { get; } = new ListProcessor<T>();

        private static readonly BinarySerializer<List<T>> serializer = BuildSerializer();

        private static readonly BinaryDeserializer<List<T>> deserializer = BuildDeserializer();

       
        public override void ExpressionSerializer(List<T> obj, ref BinaryWriter writer, Queue<int> queue) => serializer(obj, ref writer, queue);

        public override List<T> ExpressionDeserializer(ref BinaryReader reader, Queue<int> queue) => deserializer(ref reader, queue);



        private static BinarySerializer<List<T>> BuildSerializer()
        {
            var list = Expression.Parameter(typeof(List<T>), "list");
            var writer = Expression.Parameter(typeof(BinaryWriter).MakeByRefType(), "writer");
            var queue = Expression.Parameter(typeof(Queue<int>), "queue");

            // 当前类型的处理实例
            var proc = Processor.GetInstance(typeof(T));
            var buildSerializerMethod = proc.GetType().GetMethod("ExpressionSerializer");

            // 获取 Count 属性
            var countExpr = Expression.Property(list, "Count");
            // 将 Count 入队列
            var enqueueCall = Expression.Call(queue, typeof(Queue<int>).GetMethod("Enqueue"), countExpr);
            // 循环变量 i
            var loopVar = Expression.Variable(typeof(int), "i");
            // 循环条件：i < count
            var breakLabel = Expression.Label("LoopBreak");
            // 循环变量 i 的初始值
            var getItem = Expression.Call(list, typeof(List<T>).GetProperty("Item").GetGetMethod(), loopVar);
            // 调用处理器的序列化方法
            var callSerializer = Expression.Call(
                Expression.Convert(Expression.Constant(proc), proc.GetType()),
                buildSerializerMethod,
                getItem,
                writer,
                queue
            );
            // 循环体：调用序列化方法并递增 i
            var loop = Expression.Loop(
                Expression.IfThenElse(
                    Expression.LessThan(loopVar, countExpr),
                    Expression.Block(
                        callSerializer,
                        Expression.PostIncrementAssign(loopVar)
                    ),
                    Expression.Break(breakLabel)
                ),
                breakLabel
            );
            // 循环体的初始值为 0
            var body = Expression.Block(
                new[] { loopVar },
                enqueueCall,
                Expression.Assign(loopVar, Expression.Constant(0)),
                loop
            );
            // 返回一个 Lambda 表达式，参数为 list, writer, queue
            return Expression.Lambda<BinarySerializer<List<T>>>(body, list, writer, queue).Compile();



        }

        private static BinaryDeserializer<List<T>> BuildDeserializer()
        {
            var reader = Expression.Parameter(typeof(BinaryReader).MakeByRefType(), "reader");
            var queue = Expression.Parameter(typeof(Queue<int>), "queue");

            var countVar = Expression.Variable(typeof(int), "count");
            var listVar = Expression.Variable(typeof(List<T>), "list");
            var iVar = Expression.Variable(typeof(int), "i");
            var breakLabel = Expression.Label("LoopBreak");

            // 获取当前类型的处理实例 

            var proc = Processor.GetInstance(typeof(T));
            var buildDeserializerMethod = proc.GetType().GetMethod("ExpressionDeserializer");
            //获取队列的 Dequeue 方法
            var dequeueCall = Expression.Call(queue, typeof(Queue<int>).GetMethod("Dequeue"));
            // 将 Dequeue 的结果赋值给 countVar
            var assignCount = Expression.Assign(countVar, dequeueCall);
            // 创建一个新的 List<T> 实例
            var assignList = Expression.Assign(listVar, Expression.New(typeof(List<T>).GetConstructor(new[] { typeof(int) }), countVar));
            // 初始化循环变量 i 为 0
            var assignI = Expression.Assign(iVar, Expression.Constant(0));

            // 调用处理器的序列化方法
            var callDeserializer = Expression.Call(
                Expression.Convert(Expression.Constant(proc), proc.GetType()),
                buildDeserializerMethod,
                reader,
                queue
            );
            // 将处理器的反序列化结果添加到 listVar 中
            var addCall = Expression.Call(listVar, typeof(List<T>).GetMethod("Add"), callDeserializer);
            // 循环体：如果 i < count，则调用处理器的反序列化方法并将结果添加到 listVar 中，然后递增 i；否则跳出循环
            var loop = Expression.Loop(
                Expression.IfThenElse(
                    Expression.LessThan(iVar, countVar),
                    Expression.Block(
                        addCall,
                        Expression.PostIncrementAssign(iVar)
                    ),
                    Expression.Break(breakLabel)
                ),
                breakLabel
            );
            // 创建一个块表达式，包含所有的变量声明和赋值
            var body = Expression.Block(
                new[] { countVar, listVar, iVar },
                assignCount,
                assignList,
                assignI,
                loop,
                listVar
            );
            // 返回一个 Lambda 表达式，参数为 reader, queue
            return Expression.Lambda<BinaryDeserializer<List<T>>>(body, reader, queue).Compile();
        }
    }
}
