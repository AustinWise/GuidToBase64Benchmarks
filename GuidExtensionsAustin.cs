﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Based on code from:
// https://github.com/dotnet/coreclr/blob/9773db1e7b1acb3ec75c9cc0e36bd62dcbacd6d5/src/System.Private.CoreLib/shared/System/Convert.cs

using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace EfficientGuids
{
    public static class GuidExtensionsAustin
    {
        public static string EncodeBase64String(this Guid guid) => string.Create(22, guid, sDelegate);

        static readonly SpanAction<char, Guid> sDelegate = DoBase64;

        //This method is based on ConvertToBase64Array in the System.Convert class.
        //The main changes are:
        //  * The base64Table switches from using '+' to '_', and from '/' to '-'.
        //  * Since the length is always fixed, the last couple characters are written without testing the length.
        //  * The last two padding bytes are ommited.
        unsafe static void DoBase64(Span<char> outChars, Guid state)
        {
            ReadOnlySpan<byte> inData = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref state, 1));
            int lengthmod3 = inData.Length % 3;
            int calcLength = inData.Length - lengthmod3;
            int j = 0;
            //Convert three bytes at a time to base64 notation.  This will consume 4 chars.
            int i;

            // get a pointer to the base64Table to avoid unnecessary range checking
            fixed (char* base64 = &base64Table[0])
            {
                for (i = 0; i < calcLength; i += 3)
                {
                    outChars[j] = base64[(inData[i] & 0xfc) >> 2];
                    outChars[j + 1] = base64[((inData[i] & 0x03) << 4) | ((inData[i + 1] & 0xf0) >> 4)];
                    outChars[j + 2] = base64[((inData[i + 1] & 0x0f) << 2) | ((inData[i + 2] & 0xc0) >> 6)];
                    outChars[j + 3] = base64[(inData[i + 2] & 0x3f)];
                    j += 4;
                }

                //Where we left off before
                i = calcLength;

                outChars[j] = base64[(inData[i] & 0xfc) >> 2];
                outChars[j + 1] = base64[(inData[i] & 0x03) << 4];
                //Don't write the two padding bytes Base64 encoding would normally produce.
            }
        }

        static readonly char[] base64Table = {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O',
                                                       'P','Q','R','S','T','U','V','W','X','Y','Z','a','b','c','d',
                                                       'e','f','g','h','i','j','k','l','m','n','o','p','q','r','s',
                                                       't','u','v','w','x','y','z','0','1','2','3','4','5','6','7',
                                                       '8','9','_','-','=' };
    }
}