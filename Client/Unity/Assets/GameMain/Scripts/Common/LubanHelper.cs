using Luban;
using SimpleJSON;
using System.IO;
using UnityEngine;

namespace DarkGod.Main
{
    public class LubanHelper
    {
        public static ByteBuf LoadByteBufBytes(string file)
        {
            //return new ByteBuf(File.ReadAllBytes($"{Application.dataPath}/../../GenerateDatas/bytes/{file}.bytes"));
            TextAsset text = ResSvc.MainInstance.LoadAssetSync<TextAsset>(Constants.ResourcePackgeName, $"{PathDefine.ConfigDataPath}/{file}.bytes", true);
            byte[] fileData = text.bytes;
            return new ByteBuf(fileData);
        }

        public static JSONNode LoadByteBufJson(string file)
        {
            //return JSON.Parse(File.ReadAllText(Application.dataPath + "/../../GenerateDatas/json/" + file + ".json", System.Text.Encoding.UTF8));
            TextAsset text = ResSvc.MainInstance.LoadAssetSync<TextAsset>(Constants.ResourcePackgeName, $"{PathDefine.ConfigDataPath}/{file}.json", true);
            return JSON.Parse(text.text);
        }
    }
}
