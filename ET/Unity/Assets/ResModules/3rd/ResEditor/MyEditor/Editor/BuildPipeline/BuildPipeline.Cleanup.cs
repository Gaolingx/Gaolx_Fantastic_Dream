using System;
using System.Collections.Generic;
using System.IO;
using GameMain.Utils;
using UnityEngine;

namespace GameMain.Editor.BuildPipeline
{
    public partial class BuildPipeline
    {
        private static readonly List<string> LstLibCachePath = new List<string>
        {
            "il2cpp_android_arm64-v8a",
            "il2cpp_android_armeabi-v7a",
            "Il2cppBuildCache",
            "il2cpp_cache"
        };

        private static void Cleanup()
        {
            var libraryPath = Path.Combine(Application.dataPath, "..", "Library");
            if (!Directory.Exists(libraryPath))
            {
                throw new Exception("[BuildPipeline::Cleanup] Library is not Exists!!!");
            }

            foreach (var cache in LstLibCachePath)
            {
                var tmp = Path.GetFullPath(Path.Combine(libraryPath, cache)).Replace('\\', '/');
                if (!Directory.Exists(tmp))
                {
                    continue;
                }

                Directory.Delete(tmp, true);
                SimpleLog.Log($"[BuildPipeline:Cleanup] delete {tmp}");
            }
        }
    }
}
