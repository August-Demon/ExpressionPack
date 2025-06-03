using ExpressionPack.Serialization.Buffers;
using System.Collections.Generic;

namespace ExpressionPack.Serialization.Processors
{
    public abstract class TypeProcessor<T>
    {

        public abstract void ExpressionSerializer(T obj, ref BinaryWriter writer, Queue<int> queue);

        public abstract T ExpressionDeserializer(ref BinaryReader reader, Queue<int> queue);
    }
}
