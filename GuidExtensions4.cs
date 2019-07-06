using System;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EfficientGuids
{
    public static class GuidExtensions4
    {
        public unsafe static string EncodeBase64String(Guid guid)
        {
            return string.Create(22, guid, (outChars, state) =>
            {
                ref byte inData = ref Unsafe.As<Guid, byte>(ref state);
                int lengthmod3 = Unsafe.SizeOf<Guid>() % 3;
                int calcLength = Unsafe.SizeOf<Guid>() - lengthmod3;
                int j = 0;
                //Convert three bytes at a time to base64 notation.  This will consume 4 chars.
                int i;

                // get a pointer to the base64Table to avoid unnecessary range checking
                fixed (char* base64 = &base64Table[0])
                {
                    for (i = 0; i < calcLength; i += 3)
                    {
                        outChars[j] = base64[(Unsafe.Add(ref inData, i) & 0xfc) >> 2];
                        outChars[j + 1] = base64[((Unsafe.Add(ref inData, i) & 0x03) << 4) | ((Unsafe.Add(ref inData, i + 1) & 0xf0) >> 4)];
                        outChars[j + 2] = base64[((Unsafe.Add(ref inData, i + 1) & 0x0f) << 2) | ((Unsafe.Add(ref inData, i + 2) & 0xc0) >> 6)];
                        outChars[j + 3] = base64[(Unsafe.Add(ref inData, i + 2) & 0x3f)];
                        j += 4;
                    }

                    //Where we left off before
                    i = calcLength;

                    outChars[j] = base64[(Unsafe.Add(ref inData, i) & 0xfc) >> 2];
                    outChars[j + 1] = base64[(Unsafe.Add(ref inData, i) & 0x03) << 4];
                }
            });
        }

        internal static readonly char[] base64Table = {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O',
                                                       'P','Q','R','S','T','U','V','W','X','Y','Z','a','b','c','d',
                                                       'e','f','g','h','i','j','k','l','m','n','o','p','q','r','s',
                                                       't','u','v','w','x','y','z','0','1','2','3','4','5','6','7',
                                                       '8','9','_','-','=' };
    }
}