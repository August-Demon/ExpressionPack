using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace ExpressionPack.Serialization.Buffers
{
    public ref struct BinaryReader
    {
        private readonly Span<byte> _buffer;
        private int _position;

        public BinaryReader(Span<byte> buffer)
        {
            _buffer = buffer;
            _position = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>() where T : unmanaged
        {
            ref byte r0 = ref _buffer[_position];
            T value = Unsafe.As<byte, T>(ref r0);
            _position += Unsafe.SizeOf<T>();

            return value;
        }

        //public void Read<T>(Span<T> span) 
        //{
        //    int size = Unsafe.SizeOf<T>() * span.Length;
        //    ref T r0 = ref span.GetPinnableReference();
        //    ref byte r1 = ref Unsafe.As<T, byte>(ref r0);
<<<<<<< HEAD
        //    Span<byte> destination = _buffer.Slice(_position, size); 
=======
        //    Span<byte> destination = _buffer.Slice(_position, size); // Fix: Replace MemoryMarshal.CreateSpan with Slice
>>>>>>> b727eba456d73f556206c882928e89af7bc6e723
        //    Span<byte> source = _buffer.Slice(_position, size);

        //    source.CopyTo(destination);
        //    _position += size;
        //}

        public void Read(Span<byte> span)
        {
            _buffer.Slice(_position, span.Length).CopyTo(span);
            _position += span.Length;
        }
    }
}
