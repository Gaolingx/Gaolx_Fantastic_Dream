using System.Collections.Generic;
using System.IO;
using GameMain.Utils;
using UnityEditor;

namespace GameMain.Editor.BuildPipeline
{
    public partial class BuildPipeline
    {
        [MenuItem("HCLRExtTools/Tools/GlobalMetadata/合并代码到单独文件", false, 1002)]
        private static void CombineLzma()
        {
            var pathName = EditorUtility.OpenFolderPanel("选择路径", "请选择需要处理的目录", "");
            if (string.IsNullOrEmpty(pathName))
            {
                return;
            }

            pathName = Path.GetFullPath(pathName).Replace('\\', '/');
            if (!pathName.EndsWith("/"))
            {
                pathName += "/";
            }

            using var ms = new MemoryStream(10240);
            using var bw = new BinaryWriter(ms);
            var files = Directory.GetFiles(pathName, "*.*", SearchOption.AllDirectories);
            bw.Write(files.Length);
            foreach (var file in files)
            {
                var bytes = File.ReadAllBytes(file);
                var fileName = Path.GetFullPath(file).Replace('\\', '/').Replace(pathName, "");
                bw.Write(fileName);
                bw.Write(bytes.Length);
                bw.Write(bytes);
            }

            ms.Flush();
            bw.Flush();
            bw.Seek(0, SeekOrigin.Begin);

            using var msOut = new MemoryStream(1024);

            Utility.Compress(ms, msOut);

            var saveName = EditorUtility.SaveFilePanel("保存为...",
                "选择要保存的文件目录", "HCLRExtToolsCppModule", "bytes");
            if (string.IsNullOrEmpty(saveName))
            {
                SimpleLog.LogError("你没有选择保存的位置..., 保存失败");

                return;
            }

            File.WriteAllBytes(saveName, msOut.ToArray());

            SimpleLog.Log($"合并目录{pathName} 到 {saveName}成功!!!");
        }

        private static bool ExtractApiCodeToPath(string fileName, string outPath)
        {
            if (!Directory.Exists(outPath))
            {
                SimpleLog.LogError($"目录{outPath}不存在!!!");
                return false;
            }

            if (!File.Exists(fileName))
            {
                SimpleLog.LogError($"文件{fileName}不存在!!!");
                return false;
            }

            using var ms = new MemoryStream(File.ReadAllBytes(fileName));
            using var msOut = new MemoryStream(10240);
            Utility.Decompress(ms, msOut);
            msOut.Seek(0, SeekOrigin.Begin);

            using var br = new BinaryReader(msOut);
            var cnt = br.ReadInt32();
            for (var i = 0; i < cnt; ++i)
            {
                var fName = br.ReadString();
                var bytesLen = br.ReadInt32();
                var bytes = br.ReadBytes(bytesLen);

                var trueFileName = Path.Combine(outPath, fName);
                PathUtility.EnsureExistFileDirectory(trueFileName);

                File.WriteAllBytes(trueFileName, bytes);
            }

            return true;
        }

        [MenuItem("HCLRExtTools/Tools/GlobalMetadata/从文件里拆解代码到指定目录", false, 1003)]
        private static void ExtractLzma()
        {
            var fileName = EditorUtility.OpenFilePanel("加载...", "选择要加载的文件目录", "bytes");

            var savePath = "";
            Select2:
            savePath = EditorUtility.SaveFolderPanel("保存到的目录", "请选择需要保存的目录", "");
            if (string.IsNullOrEmpty(savePath))
            {
                if (EditorUtility.DisplayDialog("警告", "确定不保存了?", "确定", "重新选择"))
                {
                    return;
                }
                else
                {
                    goto Select2;
                }
            }

            if (Directory.Exists(savePath))
            {
                var files = Directory.GetFiles(savePath, "*.*", SearchOption.AllDirectories);
                var dirs = Directory.GetDirectories(savePath, "*", SearchOption.AllDirectories);

                if (files.Length + dirs.Length > 0)
                {
                    if (!EditorUtility.DisplayDialog("警告", "目录非空，是否继续?", "继续", "放弃"))
                    {
                        return;
                    }
                }
            }

            ExtractApiCodeToPath(fileName, savePath);
        }

        private class Helper
        {
            public byte[] Password;

            public Helper()
            {
                Password = new byte[32];
                var rand = new System.Random();
                for (var i = 0; i < Password.Length; ++i)
                {
                    Password[i] = (byte) rand.Next(byte.MinValue + 5, byte.MaxValue - 5);
                }
            }

            public void Xor(IList<byte> src)
            {
                for (var i = 0; i < Password.Length; ++i)
                {
                    src[i] ^= Password[i];
                }
            }
        }
    }
}
