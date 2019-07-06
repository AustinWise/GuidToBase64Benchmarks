//Originally from: https://twitter.com/markrendle/status/1141695153574486019

using System;
using System.Buffers.Text;
using System.Runtime.InteropServices;

namespace EfficientGuids
{
    public static class GuidExtensionsImproved
    {
        private const byte ForwardSlashByte = (byte)'/';
        private const byte PlusByte = (byte)'+';
        private const char Underscore = '_';
        private const char Dash = '-';

        public static string EncodeBase64String(Guid guid)
        {
            Span<byte> guidBytes = stackalloc byte[16];
            Span<byte> encodedBytes = stackalloc byte[24];

            MemoryMarshal.TryWrite(guidBytes, ref guid); // write bytes from the Guid
            Base64.EncodeToUtf8(guidBytes, encodedBytes, out _, out _);

            Span<char> chars = stackalloc char[22];

            // replace any characters which are not URL safe
            // skip the final two bytes as these will be '==' padding we don't need
            for (var i = 0; i < 22; i++)
            {
                switch (encodedBytes[i])
                {
                    case ForwardSlashByte:
                        chars[i] = Dash;
                        break;
                    case PlusByte:
                        chars[i] = Underscore;
                        break;
                    default:
                        chars[i] = (char)encodedBytes[i];
                        break;
                }
            }

            var final = new string(chars);

            return final;
        }
    }
}