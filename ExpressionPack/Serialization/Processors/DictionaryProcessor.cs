using ExpressionPack.Delegates;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using ExpressionPack.Serialization.Buffers;

namespace ExpressionPack.Serialization.Processors
{
    public class DictionaryProcessor<TKey, TValue> : TypeProcessor<Dictionary<TKey, TValue>>
    {
        public static DictionaryProcessor<TKey, TValue> Instance { get; } = new DictionaryProcessor<TKey, TValue>();

        private static readonly BinarySerializer<Dictionary<TKey, TValue>> serializer = BuildSerializer();
        private static readonly BinaryDeserializer<Dictionary<TKey, TValue>> deserializer = BuildDeserializer();

        public override void ExpressionSerializer(Dictionary<TKey, TValue> obj, ref BinaryWriter writer, Queue<int> queue)
            => serializer(obj, ref writer, queue);

        public override Dictionary<TKey, TValue> ExpressionDeserializer(ref BinaryReader reader, Queue<int> queue)
            => deserializer(ref reader, queue);

        private static BinarySerializer<Dictionary<TKey, TValue>> BuildSerializer()
        {
            var dict = Expression.Parameter(typeof(Dictionary<TKey, TValue>), "dict");
            var writer = Expression.Parameter(typeof(BinaryWriter).MakeByRefType(), "writer");
            var queue = Expression.Parameter(typeof(Queue<int>), "queue");

            // 获取处理器
            var keyProc = Processor.GetInstance(typeof(TKey));
            var valueProc = Processor.GetInstance(typeof(TValue));

            var keySerializer = keyProc.GetType().GetMethod("ExpressionSerializer");
            var valueSerializer = valueProc.GetType().GetMethod("ExpressionSerializer");

            // dict.Count 入队列
            var countExpr = Expression.Property(dict, "Count");
            var enqueueCall = Expression.Call(queue, typeof(Queue<int>).GetMethod("Enqueue"), countExpr);


            // 获取 Dictionary<TKey, TValue>.Enumerator 泛型定义
            var dictType = typeof(Dictionary<TKey, TValue>);
            var enumeratorTypeDef = dictType.GetNestedType("Enumerator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            // 生成封闭类型
            var enumeratorType = enumeratorTypeDef.MakeGenericType(typeof(TKey), typeof(TValue));
            var enumeratorVar = Expression.Variable(enumeratorType, "enumerator");

            var kvpVar = Expression.Variable(typeof(KeyValuePair<TKey, TValue>), "kvp");
            var loopVar = Expression.Label("LoopBreak");


            

    

            // dict.GetEnumerator() 赋值给 enumeratorVar
            var getEnumerator = Expression.Assign(
                enumeratorVar,
                Expression.Call(dict, typeof(Dictionary<TKey, TValue>).GetMethod("GetEnumerator"))
            );




            //var moveNext = Expression.Call(enumeratorVar, typeof(IEnumerator<KeyValuePair<TKey, TValue>>).GetMethod("MoveNext"));
            //var assignKvp = Expression.Assign(kvpVar, Expression.Property(enumeratorVar, "Current"));

            // enumerator.MoveNext()
            var moveNext = Expression.Call(enumeratorVar, enumeratorType.GetMethod("MoveNext"));

            // kvp = enumerator.Current
            var assignKvp = Expression.Assign(kvpVar, Expression.Property(enumeratorVar, "Current"));
            // 序列化 Key
            var serializeKey = Expression.Call(
                Expression.Convert(Expression.Constant(keyProc), keyProc.GetType()),
                keySerializer,
                Expression.Property(kvpVar, "Key"),
                writer,
                queue
            );
            // 序列化 Value
            var serializeValue = Expression.Call(
                Expression.Convert(Expression.Constant(valueProc), valueProc.GetType()),
                valueSerializer,
                Expression.Property(kvpVar, "Value"),
                writer,
                queue
            );

            var loop = Expression.Loop(
                Expression.IfThenElse(
                    moveNext,
                    Expression.Block(
                        assignKvp,
                        serializeKey,
                        serializeValue
                    ),
                    Expression.Break(loopVar)
                ),
                loopVar
            );

            var body = Expression.Block(
                new[] { enumeratorVar, kvpVar },
                enqueueCall,
                getEnumerator,
                loop
            );

            return Expression.Lambda<BinarySerializer<Dictionary<TKey, TValue>>>(body, dict, writer, queue).Compile();
        }

        private static BinaryDeserializer<Dictionary<TKey, TValue>> BuildDeserializer()
        {
            var reader = Expression.Parameter(typeof(BinaryReader).MakeByRefType(), "reader");
            var queue = Expression.Parameter(typeof(Queue<int>), "queue");

            var countVar = Expression.Variable(typeof(int), "count");
            var dictVar = Expression.Variable(typeof(Dictionary<TKey, TValue>), "dict");
            var iVar = Expression.Variable(typeof(int), "i");

            // 获取处理器
            var keyProc = Processor.GetInstance(typeof(TKey));
            var valueProc = Processor.GetInstance(typeof(TValue));
            var keyDeserializer = keyProc.GetType().GetMethod("ExpressionDeserializer");
            var valueDeserializer = valueProc.GetType().GetMethod("ExpressionDeserializer");

            var assignCount = Expression.Assign(countVar, Expression.Call(queue, typeof(Queue<int>).GetMethod("Dequeue")));
            var assignDict = Expression.Assign(dictVar, Expression.New(typeof(Dictionary<TKey, TValue>).GetConstructor(Type.EmptyTypes)));
            var assignI = Expression.Assign(iVar, Expression.Constant(0));

            // 反序列化 Key
            var deserializeKey = Expression.Call(
                Expression.Convert(Expression.Constant(keyProc), keyProc.GetType()),
                keyDeserializer,
                reader,
                queue
            );
            // 反序列化 Value
            var deserializeValue = Expression.Call(
                Expression.Convert(Expression.Constant(valueProc), valueProc.GetType()),
                valueDeserializer,
                reader,
                queue
            );

            var addCall = Expression.Call(
                dictVar,
                typeof(Dictionary<TKey, TValue>).GetMethod("Add"),
                deserializeKey,
                deserializeValue
            );

            var breakLabel = Expression.Label("LoopBreak");
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

            var body = Expression.Block(
                new[] { countVar, dictVar, iVar },
                assignCount,
                assignDict,
                assignI,
                loop,
                dictVar
            );

            return Expression.Lambda<BinaryDeserializer<Dictionary<TKey, TValue>>>(body, reader, queue).Compile();
        }
    }
}
