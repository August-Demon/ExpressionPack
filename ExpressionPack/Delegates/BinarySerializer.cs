using ExpressionPack.Serialization.Buffers;
using System.Collections.Generic;

namespace ExpressionPack.Delegates
{
    public delegate void BinarySerializer<T>(T obj, ref BinaryWriter writer, Queue<int> queue);
}
