using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressionPack.Serialization.Meta
{
    public class MetaInfo
    {
        public byte[] Data { get; set; }

        public Queue<int> Lengths { get; set; } = new Queue<int>();
    }
}
