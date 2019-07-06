using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EfficientGuids
{
    public static class GuidExtensions5
    {
        static readonly SpanAction<char, Guid> sDelegate = EncodeGuid;
        public unsafe static string EncodeBase64String(Guid guid)
        {
            return string.Create<Guid>(22, guid, sDelegate);
        }

        static void EncodeGuid(Span<char> chars, Guid guid)
        {
            EncodeToUtf8(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref guid, 1)), chars);
        }

        public static void EncodeToUtf8(ReadOnlySpan<byte> bytes, Span<char> outString)
        {
            Debug.Assert(bytes.Length == 16);
            Debug.Assert(outString.Length == 22);

            ref byte srcBytes = ref MemoryMarshal.GetReference(bytes);
            ref byte destBytes = ref MemoryMarshal.GetReference(MemoryMarshal.AsBytes(outString));

            int srcLength = bytes.Length;
            int destLength = outString.Length;

            int maxSrcLength = 0;
            if (srcLength <= MaximumEncodeLength && destLength >= GetMaxEncodedToUtf8Length(srcLength))
            {
                maxSrcLength = srcLength - 2;
            }
            else
            {
                maxSrcLength = (destLength >> 2) * 3 - 2;
            }

            int sourceIndex = 0;
            int destIndex = 0;
            long result = 0;

            ref byte encodingMap = ref s_encodingMap[0];

            while (sourceIndex < maxSrcLength)
            {
                result = Encode(ref Unsafe.Add(ref srcBytes, sourceIndex), ref encodingMap);
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref destBytes, destIndex), result);
                destIndex += 8;
                sourceIndex += 3;
            }

            {
                int i = (Unsafe.Add(ref srcBytes, sourceIndex) << 8);
                int i0 = Unsafe.Add(ref encodingMap, i >> 10);
                int i1 = Unsafe.Add(ref encodingMap, (i >> 4) & 0x3F);
                Unsafe.WriteUnaligned<int>(ref Unsafe.Add(ref destBytes, destIndex), i0 | (i1 << 16));
            }
        }

        static void ThrowArgumentOutOfRangeException_length()
        {
            throw new ArgumentOutOfRangeException("length");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMaxEncodedToUtf8Length(int length)
        {
            if ((uint)length > MaximumEncodeLength)
                ThrowArgumentOutOfRangeException_length();

            return (((length + 2) / 3) * 4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long Encode(ref byte threeBytes, ref byte encodingMap)
        {
            int i = (threeBytes << 16) | (Unsafe.Add(ref threeBytes, 1) << 8) | Unsafe.Add(ref threeBytes, 2);

            long i0 = Unsafe.Add(ref encodingMap, i >> 18);
            long i1 = Unsafe.Add(ref encodingMap, (i >> 12) & 0x3F);
            long i2 = Unsafe.Add(ref encodingMap, (i >> 6) & 0x3F);
            long i3 = Unsafe.Add(ref encodingMap, i & 0x3F);

            return i0 | (i1 << 16) | (i2 << 32) | (i3 << 48);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long EncodeAndPadOne(ref byte twoBytes, ref byte encodingMap)
        {
            int i = (twoBytes << 16) | (Unsafe.Add(ref twoBytes, 1) << 8);

            long i0 = Unsafe.Add(ref encodingMap, i >> 18);
            long i1 = Unsafe.Add(ref encodingMap, (i >> 12) & 0x3F);
            long i2 = Unsafe.Add(ref encodingMap, (i >> 6) & 0x3F);

            return i0 | (i1 << 16) | (i2 << 32) | (EncodingPad << 48);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long EncodeAndPadTwo(ref byte oneByte, ref byte encodingMap)
        {
            int i = (oneByte << 8);

            long i0 = Unsafe.Add(ref encodingMap, i >> 10);
            long i1 = Unsafe.Add(ref encodingMap, (i >> 4) & 0x3F);

            return i0 | (i1 << 16) | (EncodingPad << 32) | (EncodingPad << 48);
        }

        // Pre-computing this table using a custom string(s_characters) and GenerateEncodingMapAndVerify (found in tests)
        private static readonly byte[] s_encodingMap = {
            65, 66, 67, 68, 69, 70, 71, 72,         //A..H
            73, 74, 75, 76, 77, 78, 79, 80,         //I..P
            81, 82, 83, 84, 85, 86, 87, 88,         //Q..X
            89, 90, 97, 98, 99, 100, 101, 102,      //Y..Z, a..f
            103, 104, 105, 106, 107, 108, 109, 110, //g..n
            111, 112, 113, 114, 115, 116, 117, 118, //o..v
            119, 120, 121, 122, 48, 49, 50, 51,     //w..z, 0..3
            52, 53, 54, 55, 56, 57, 95, 45          //4..9, _, -
        };

        private const long EncodingPad = (byte)'='; // '=', for padding

        private const int MaximumEncodeLength = (int.MaxValue / 4) * 3; // 1610612733
    }
}