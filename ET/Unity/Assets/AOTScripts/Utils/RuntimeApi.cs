using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GameMain.HCLRExtTools
{
    public static class RuntimeApi
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
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

        [MethodImpl(MethodImplOptions.InternalCall)]
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

        [MethodImpl(MethodImplOptions.InternalCall)]
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

        public delegate void LogDelegate(IntPtr pMsg, int iSize);

#if !UNITY_EDITOR
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void HCLRExtTools_SetLogHandler(LogDelegate logDelegate);
#else
        public static void HCLRExtTools_SetLogHandler(LogDelegate _) { }
#endif
    }
}
