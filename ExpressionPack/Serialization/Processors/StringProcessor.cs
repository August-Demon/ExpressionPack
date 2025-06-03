using ExpressionPack.Delegates;
using ExpressionPack.Serialization.Buffers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ExpressionPack.Serialization.Processors
{
    public class StringProcessor : TypeProcessor<string>
    {
        public static StringProcessor Instance { get; } = new StringProcessor();

        private static readonly BinarySerializer<string> serializer = BuildSerializer();

        private static readonly BinaryDeserializer<string> deserializer = BuildDeserializer();



        public override void ExpressionSerializer(string obj, ref BinaryWriter writer, Queue<int> queue) => serializer(obj, ref writer, queue);

        public override string ExpressionDeserializer(ref BinaryReader reader, Queue<int> queue) => deserializer(ref reader, queue);

        private static BinarySerializer<string> BuildSerializer()
        {
            var obj = Expression.Parameter(typeof(string), "obj");
            var writer = Expression.Parameter(typeof(BinaryWriter).MakeByRefType(), "writer");
            var queue = Expression.Parameter(typeof(Queue<int>), "queue");
            var bytesVar = Expression.Variable(typeof(byte[]), "bytes");


            // bytes = obj == null ? null : Encoding.UTF8.GetBytes(obj)
            var getBytes = Expression.Assign(
                bytesVar,
                Expression.Condition(
                    Expression.Equal(obj, Expression.Constant(null, typeof(string))),
                    Expression.Constant(null, typeof(byte[])),
                    Expression.Call(
                        Expression.Property(null, typeof(Encoding).GetProperty(nameof(Encoding.UTF8))),
                        typeof(Encoding).GetMethod(nameof(Encoding.GetBytes), new[] { typeof(string) }),
                        obj
                    )
                )
            );

            // int len = bytes?.Length ?? -1
            var lenExpr = Expression.Condition(
                Expression.Equal(bytesVar, Expression.Constant(null, typeof(byte[]))),
                Expression.Constant(-1),
                Expression.Property(bytesVar, "Length")
            );

            // queue.Enqueue(len)
            var enqueueLen = Expression.Call(queue, typeof(Queue<int>).GetMethod("Enqueue"), lenExpr);

            // if (bytes != null && bytes.Length > 0) writer.Write(bytes)

            var writeBytes = Expression.IfThen(
                Expression.AndAlso(
                    Expression.NotEqual(bytesVar, Expression.Constant(null, typeof(byte[]))),
                    Expression.GreaterThan(Expression.Property(bytesVar, "Length"), Expression.Constant(0))
                ),
                Expression.Call(
                    writer,
                      typeof(BinaryWriter).GetMethod("Write", new[] { typeof(Span<byte>) }),
                    Expression.Call(typeof(Span<byte>), "op_Implicit", null, bytesVar)
                )
            );

            var body = Expression.Block(
                new[] { bytesVar },
                getBytes,
                enqueueLen,
                writeBytes
            );


            return Expression.Lambda<BinarySerializer<string>>(body, obj, writer, queue).Compile();
        }

        private static BinaryDeserializer<string> BuildDeserializer()
        {
            var reader = Expression.Parameter(typeof(BinaryReader).MakeByRefType(), "reader");
            var queue = Expression.Parameter(typeof(Queue<int>), "queue");

            var lenVar = Expression.Variable(typeof(int), "len");
            var bytesVar = Expression.Variable(typeof(byte[]), "bytes");
            var labelReturn = Expression.Label(typeof(string));

            // int len = queue.Dequeue()
            var assignLen = Expression.Assign(
                lenVar,
                Expression.Call(queue, typeof(Queue<int>).GetMethod("Dequeue"))
            );

            // if (len == -1) return null;
            var ifNull = Expression.IfThen(
                Expression.Equal(lenVar, Expression.Constant(-1)),
                Expression.Return(labelReturn, Expression.Constant(null, typeof(string)))
            );

            // if (len == 0) return string.Empty;
            var ifEmpty = Expression.IfThen(
                Expression.Equal(lenVar, Expression.Constant(0)),
                Expression.Return(labelReturn, Expression.Constant(string.Empty, typeof(string)))
            );

            // bytes = new byte[len]
            var assignBytes = Expression.Assign(bytesVar, Expression.NewArrayBounds(typeof(byte), lenVar));


            // var readMethod = Reader.ReadT(typeof(T));

            // reader.Read(bytes)
            var readBytes = Expression.Call(
                reader,
                typeof(BinaryReader).GetMethod("Read", new[] { typeof(Span<byte>) }),
                Expression.Call(typeof(Span<byte>), "op_Implicit", null, bytesVar)
            );

            // return Encoding.UTF8.GetString(bytes)
            var getString = Expression.Call(
                Expression.Property(null, typeof(Encoding).GetProperty(nameof(Encoding.UTF8))),
                typeof(Encoding).GetMethod(nameof(Encoding.GetString), new[] { typeof(byte[]) }),
                bytesVar
            );

            var body = Expression.Block(
                new[] { lenVar, bytesVar },
                assignLen,
                ifNull,
                ifEmpty,
                assignBytes,
                readBytes,
                Expression.Return(labelReturn, getString),
                Expression.Label(labelReturn, Expression.Constant(null, typeof(string)))
            );



            return Expression.Lambda<BinaryDeserializer<string>>(body, reader, queue).Compile();
        }
    }
}
