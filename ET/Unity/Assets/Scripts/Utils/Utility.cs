using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using GameMain.HCLRExtTools;

namespace GameMain.Utils
{
    public class Utility
    {
        public static bool Compress(MemoryStream msInput, MemoryStream msOutput)
        {
            if (msInput == null || msInput.Length == 0)
            {
                return false;
            }

            msInput.Seek(0, SeekOrigin.Begin);

            var coder = new SevenZip.Compression.LZMA.Encoder();
            coder.WriteCoderProperties(msOutput);
            msOutput.Write(BitConverter.GetBytes(msInput.Length), 0, 8);
            coder.Code(msInput, msOutput, msInput.Length, -1, null);
            msOutput.Flush();

            return true;
        }

        public static bool Decompress(MemoryStream msInput, MemoryStream msOutput)
        {
#if !UNITY_EDITOR 
            var ret = RuntimeApi.Decompress(msInput.GetBuffer(), (int) msInput.Length);
            msOutput.Write(ret, 0, ret.Length);
            return ret.Length > 0;
#else
            return DecompressCS(msInput, msOutput);
#endif
        }

        private static bool DecompressCS(MemoryStream msInput, MemoryStream msOutput)
        {
            if (msInput == null || msInput.Length == 0)
            {
                return false;
            }

            msInput.Seek(0, SeekOrigin.Begin);

            var coder = new SevenZip.Compression.LZMA.Decoder();
            var properties = new byte[5];
            msInput.Read(properties, 0, 5);

            var fileLengthBytes = new byte[8];
            msInput.Read(fileLengthBytes, 0, 8);
            var fileLength = BitConverter.ToInt64(fileLengthBytes, 0);

            msOutput.Capacity += (int)fileLength;
            coder.SetDecoderProperties(properties);
            coder.Code(msInput, msOutput, msInput.Length, fileLength, null);
            msOutput.Flush();

            return true;
        }

        public static string GetFileMd5(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return "";
            }

            if (!File.Exists(fileName))
            {
                return "";
            }

            var md5 = new MD5CryptoServiceProvider();
            var mdStr = new StringBuilder();

            var retVal = md5.ComputeHash(File.ReadAllBytes(fileName));
            for (int i = 0; i < retVal.Length; i++)
            {
                mdStr.Append(retVal[i].ToString("x2"));
            }
            return mdStr.ToString();
        }
    }
}
