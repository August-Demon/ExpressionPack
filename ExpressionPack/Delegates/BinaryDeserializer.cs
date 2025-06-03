using ExpressionPack.Serialization.Buffers;
using System.Collections.Generic;

namespace ExpressionPack.Delegates
{
    public delegate T BinaryDeserializer<T>(ref BinaryReader reader, Queue<int> queue);
}
