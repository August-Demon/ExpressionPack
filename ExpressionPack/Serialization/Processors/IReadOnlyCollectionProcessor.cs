using ExpressionPack.Delegates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Text;
using ExpressionPack.Serialization.Buffers;

namespace ExpressionPack.Serialization.Processors
{
    public class IReadOnlyCollectionProcessor<T> : TypeProcessor<IReadOnlyCollection<T>>
    {
        public static IReadOnlyCollectionProcessor<T> Instance { get; } = new IReadOnlyCollectionProcessor<T>();

        private static readonly BinarySerializer<IReadOnlyCollection<T>> serializer = BuildSerializer();
        private static readonly BinaryDeserializer<IReadOnlyCollection<T>> deserializer = BuildDeserializer();

        public override void ExpressionSerializer(IReadOnlyCollection<T> obj, ref BinaryWriter writer, Queue<int> queue)
            => serializer(obj, ref writer, queue);

        public override IReadOnlyCollection<T> ExpressionDeserializer(ref BinaryReader reader, Queue<int> queue)
            => deserializer(ref reader, queue);

        private static BinarySerializer<IReadOnlyCollection<T>> BuildSerializer()
        {
            var obj = Expression.Parameter(typeof(IReadOnlyCollection<T>), "obj");
            var writer = Expression.Parameter(typeof(BinaryWriter).MakeByRefType(), "writer");
            var queue = Expression.Parameter(typeof(Queue<int>), "queue");

            var countExpr = Expression.Property(obj, "Count");
            var enqueueCall = Expression.Call(queue, typeof(Queue<int>).GetMethod("Enqueue"), countExpr);

            var enumeratorVar = Expression.Variable(typeof(IEnumerator<T>), "enumerator");
            var moveNext = Expression.Call(enumeratorVar, typeof(System.Collections.IEnumerator).GetMethod("MoveNext"));
            var current = Expression.Property(enumeratorVar, "Current");
            var breakLabel = Expression.Label("LoopBreak");

            var proc = Processor.GetInstance(typeof(T));
            var serializerMethod = proc.GetType().GetMethod("ExpressionSerializer");

            var getEnumerator = Expression.Assign(
                enumeratorVar,
                Expression.Call(obj, typeof(IEnumerable<T>).GetMethod("GetEnumerator"))
            );

            var loop = Expression.Loop(
                Expression.IfThenElse(
                    moveNext,
                    Expression.Call(
                        Expression.Convert(Expression.Constant(proc), proc.GetType()),
                        serializerMethod,
                        current,
                        writer,
                        queue
                    ),
                    Expression.Break(breakLabel)
                ),
                breakLabel
            );

            var body = Expression.Block(
                new[] { enumeratorVar },
                enqueueCall,
                getEnumerator,
                loop
            );

            return Expression.Lambda<BinarySerializer<IReadOnlyCollection<T>>>(body, obj, writer, queue).Compile();
        }

        private static BinaryDeserializer<IReadOnlyCollection<T>> BuildDeserializer()
        {
            var reader = Expression.Parameter(typeof(BinaryReader).MakeByRefType(), "reader");
            var queue = Expression.Parameter(typeof(Queue<int>), "queue");

            var countVar = Expression.Variable(typeof(int), "count");
            var arrVar = Expression.Variable(typeof(T[]), "arr");
            var iVar = Expression.Variable(typeof(int), "i");
            var breakLabel = Expression.Label("LoopBreak");

            var proc = Processor.GetInstance(typeof(T));
            var deserializerMethod = proc.GetType().GetMethod("ExpressionDeserializer");

            var assignCount = Expression.Assign(countVar, Expression.Call(queue, typeof(Queue<int>).GetMethod("Dequeue")));
            var assignArr = Expression.Assign(arrVar, Expression.NewArrayBounds(typeof(T), countVar));
            var assignI = Expression.Assign(iVar, Expression.Constant(0));

            var loop = Expression.Loop(
                Expression.IfThenElse(
                    Expression.LessThan(iVar, countVar),
                    Expression.Block(
                        Expression.Assign(
                            Expression.ArrayAccess(arrVar, iVar),
                            Expression.Call(
                                Expression.Convert(Expression.Constant(proc), proc.GetType()),
                                deserializerMethod,
                                reader,
                                queue
                            )
                        ),
                        Expression.PostIncrementAssign(iVar)
                    ),
                    Expression.Break(breakLabel)
                ),
                breakLabel
            );

            var readOnly = Expression.New(
                typeof(ReadOnlyCollection<T>).GetConstructor(new[] { typeof(IList<T>) }),
                arrVar
            );

            var body = Expression.Block(
                new[] { countVar, arrVar, iVar },
                assignCount,
                assignArr,
                assignI,
                loop,
                readOnly
            );

            return Expression.Lambda<BinaryDeserializer<IReadOnlyCollection<T>>>(body, reader, queue).Compile();
        }
    }
}
