using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameMain.Scripts.Utils
{
    public static class RuntimeApi
    {
#if UNITY_STANDALONE_WIN
            private const string dllName = "GameAssembly";
#elif UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_WEBGL
            private const string dllName = "__Internal";
#else
        private const string dllName = "il2cpp";
#endif

        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr HCLRExtTools_Decompress(byte[] src, int len, ref int outLen);

        public static byte[] Decompress(byte[] src, int len)
        {
#if !UNITY_EDITOR
            var outLen = 0;
            var retIntPtr = HCLRExtTools_Decompress(src, len, ref outLen);
            if (retIntPtr == IntPtr.Zero)
            {
                return null;
            }

            var ret = new byte[outLen];
            Marshal.Copy(retIntPtr, ret, 0, (int) outLen);
            return ret;
#endif
            return null;
        }

        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr HCLRExtTools_XXTeaEncrypt(byte[] s, int l, byte[] k, int kl, ref int ol);

        public static byte[] XXTeaEncrypt(byte[] src, byte[] key)
        {
#if !UNITY_EDITOR
            var outLen = 0;
            var retIntPtr = HCLRExtTools_XXTeaEncrypt(src, src.Length, key, key.Length, ref outLen);
            if (retIntPtr == IntPtr.Zero)
            {
                return null;
            }

            var ret = new byte[outLen];
            Marshal.Copy(retIntPtr, ret, 0, (int) outLen);
            return ret;
#endif
            return null;
        }

        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr HCLRExtTools_XXTeaDecrypt(byte[] s, int l, byte[] k, int kl, ref int ol);

        public static byte[] XXTeaDecrypt(byte[] src, byte[] key)
        {
#if !UNITY_EDITOR
            var outLen = 0;
            var retIntPtr = HCLRExtTools_XXTeaDecrypt(src, src.Length, key, key.Length, ref outLen);
            if (retIntPtr == IntPtr.Zero)
            {
                return null;
            }

            var ret = new byte[outLen];
            Marshal.Copy(retIntPtr, ret, 0, (int) outLen);
            return ret;
#endif
            return null;
        }
    }
}
