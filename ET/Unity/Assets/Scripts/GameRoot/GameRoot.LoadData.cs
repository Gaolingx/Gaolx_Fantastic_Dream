using Cysharp.Threading.Tasks;
using GameMain.Utils;
using System.Collections.Generic;
using System.IO;

namespace GameMain.Scripts
{
    public partial class GameRoot
    {
        private async UniTask<Dictionary<string, byte[]>> LoadDataAsync(string fileName, string password)
        {
            var ret = new Dictionary<string, byte[]>();
            SimpleLog.Log($"[GameRoot::LoadData] {fileName}");
            byte[] bytes = null;
            var writablePath = Path.Combine(GameConfig.LocalPath, fileName).Replace('\\', '/');
            if (File.Exists(writablePath))
            {
                SimpleLog.Log($"[GameRoot::LoadData] {fileName} from {writablePath}");
                bytes = await File.ReadAllBytesAsync(writablePath);
            }

            if (bytes == null || bytes.Length == 0)
            {
                SimpleLog.Log($"[GameRoot::LoadData] bytes is empty!");
                return ret;
            }

            SimpleLog.Log($"[GameRoot::LoadData] {fileName} load ok!");
            bytes = XXTEA.Decrypt(bytes, password);

            SimpleLog.Log($"[GameRoot::LoadData] begin decompressfast....{fileName}");
            using var ms = new MemoryStream(bytes, 0, bytes.Length, false, true);
            using var output = new MemoryStream(bytes.Length);
            if (!Utility.Decompress(ms, output))
            {
                SimpleLog.Log($"[GameRoot::LoadData] DecompressFast {fileName} error!");
                return ret;
            }

            SimpleLog.Log($"[GameRoot::LoadData] end decompressfast....{fileName}");
            output.Seek(0, SeekOrigin.Begin);
            using var br = new BinaryReader(output);
            var count = br.ReadInt32();
            for (var i = 0; i < count; ++i)
            {
                var fn = br.ReadString();
                var cnt = br.ReadInt32();
                var buf = br.ReadBytes(cnt);

                ret.Add(fn, buf);
            }

            SimpleLog.Log($"[GameRoot::LoadData] {fileName} finished!");

            return ret;
        }
    }
}
