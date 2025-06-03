using ExpressionPack.Serialization.Buffers;
using ExpressionPack.Serialization.Meta;
using ExpressionPack.Serialization.Processors;

namespace ExpressionPack
{
    public static class BinaryConverter
    {

        public static MetaInfo Serialize<T>(T obj)
        {
            var meta = new MetaInfo();
            BinaryWriter writer = new BinaryWriter(BinaryWriter.DefaultSize);
            try
            {
                var processor = (TypeProcessor<T>)Processor.GetInstance(typeof(T));
                processor.ExpressionSerializer(obj, ref writer, meta.Lengths);
                meta.Data = writer.Span.ToArray();
                return meta;

            }
            finally
            {
                writer.Dispose();
            }
        }
        public static T Deserialize<T>(MetaInfo meta)
        {
            BinaryReader reader = new BinaryReader(meta.Data);
            var processor = (TypeProcessor<T>)Processor.GetInstance(typeof(T));
            return processor.ExpressionDeserializer(ref reader, meta.Lengths);
        }
    }
}
