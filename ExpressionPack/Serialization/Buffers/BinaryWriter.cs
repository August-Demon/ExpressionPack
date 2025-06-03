using ExpressionPack.Extensions;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ExpressionPack.Serialization.Buffers
{
    public struct BinaryWriter
    {
        public const int DefaultSize = 1024;

        private byte[] _buffer;

        private int _position;

        public BinaryWriter(int initialSize)
        {
            _buffer = ArrayPool<byte>.Shared.Rent(initialSize);
            _position = 0;
        }

        public ReadOnlySpan<byte> Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ReadOnlySpan<byte>(_buffer, 0, _position);
        }

        public void Write<T>(T value) where T : unmanaged
        {
            int size = Unsafe.SizeOf<T>();

            EnsureCapacity(size);

            Unsafe.As<byte, T>(ref _buffer[_position]) = value;
            _position += size;
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public void Write<T>(Span<T> span) where T : unmanaged
        //{
        //    int
        //        elementSize = Unsafe.SizeOf<T>(),
        //        totalSize = elementSize * span.Length;

        //    EnsureCapacity(totalSize);

        //    ref T r0 = ref span.GetPinnableReference();
        //    ref byte r1 = ref Unsafe.As<T, byte>(ref r0);

        //    // Fix for CS0453: Ensure T is unmanaged and use MemoryMarshal.Cast safely
        //    MemoryMarshal.Cast<T, byte>(span).CopyTo(_buffer.AsSpan(_position, totalSize));
        //    _position += totalSize;
        //}

        public void Write(Span<byte> span)
        {
            EnsureCapacity(span.Length);
            span.CopyTo(_buffer.AsSpan(_position));
            _position += span.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureCapacity(int count)
        {
            int currentLength = _buffer.Length, requiredLength = _position + count;

            if (requiredLength <= currentLength) return;

            if (currentLength == 0x7FFFFFC7) throw new InvalidOperationException("Maximum size for a byte[] array exceeded (0x7FFFFFC7), see: https://msdn.microsoft.com/en-us/library/system.array");

            // Calculate the new size of the target array
            int targetLength = requiredLength.UpperBoundLog2();
            if (targetLength < 0) targetLength = 0x7FFFFFC7;

            // Rent the new array and copy the content of the current array
            byte[] rent = ArrayPool<byte>.Shared.Rent(targetLength);
            Unsafe.CopyBlock(ref rent[0], ref _buffer[0], (uint)_position);

            // Return the old buffer and swap it
            ArrayPool<byte>.Shared.Return(_buffer);
            _buffer = rent;
        }

        public void Dispose() => ArrayPool<byte>.Shared.Return(_buffer);
    }
}
