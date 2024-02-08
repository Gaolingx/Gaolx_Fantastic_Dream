using GameMain.Utils;
using HybridCLR.Editor;
using HybridCLR.Editor.AOT;
using HybridCLR.Editor.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameMain.Editor.BuildPipeline
{
    public partial class BuildPipeline
    {
        private static void Generate(string scriptOutDir, string srcDir, string name, string password, List<string> fileList = null, bool tinyDLL = false)
        {
            var dstFile = Path.Combine(scriptOutDir, name).Replace('\\', '/');
            if (File.Exists(dstFile))
            {
                File.Delete(dstFile);
            }

            var srcFullDir = Path.GetFullPath(srcDir).Replace('\\', '/');
            if (!srcFullDir.EndsWith("/"))
            {
                srcFullDir += "/";
            }

            if (!Directory.Exists(srcFullDir))
            {
                SimpleLog.LogError($"[BuildPipeline::GenerateScripts] can't found {srcFullDir}");
                return;
            }

            if (fileList == null || fileList.Count == 0)
            {
                var files = Directory.GetFiles(srcFullDir, "*", SearchOption.AllDirectories);
                fileList = new List<string>(files.Length);
                foreach (var file in files)
                {
                    if (file.ToLower().EndsWith(".dll"))
                    {
                        var tmp = file.Replace('\\', '/').Replace(srcFullDir, "");
                        fileList.Add(tmp);
                    }
                }
            }

            if (fileList.Count == 0)
            {
                SimpleLog.Log($"[BuildPipeline::GenerateScripts] {srcDir} filelist is empty!!! {name}");
                return;
            }

            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);
            bw.Write(fileList.Count);
            foreach (var file in fileList)
            {
                var filePath = $"{srcDir}/{file}";
                if (!File.Exists(filePath))
                {
                    filePath += ".dll";
                }

                if (!File.Exists(filePath))
                {
                    SimpleLog.Log($"[BuildPipeline::GenerateScripts]文件{filePath}不存在!");
                    continue;
                }

                SimpleLog.Log($"[BuildPipeline::GenerateScripts] copy {filePath} to {dstFile}");

                bw.Write(file);

                var dstFilePath = filePath;
                if (tinyDLL)
                {
                    dstFilePath = dstFilePath.Replace(".dll", "_ext.dll");
                    AOTAssemblyMetadataStripper.Strip(filePath, dstFilePath);
                }
                var bytes = File.ReadAllBytes(dstFilePath);
                if (bytes != null && bytes.Length > 0)
                {
                    bw.Write(bytes.Length);
                    bw.Write(bytes);
                }
            }

            using var output = new MemoryStream((int)ms.Length);
            if (!Utility.Compress(ms, output))
            {
                SimpleLog.LogError($"[BuildPipeline::GenerateScripts] {name}失败？？？？");
                return;
            }

            var outBuffer = output.ToArray();
            outBuffer = XXTEA.Encrypt(outBuffer, password);
            PathUtility.EnsureExistFileDirectory(dstFile);
            File.WriteAllBytes(dstFile, outBuffer);
        }

        private static HashSet<string> PatchedAOTAssemblyList
        {
            get
            {
                var ret = new HashSet<string>();
                var localAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var ass in localAssemblies)
                {
                    var types = ass.GetTypes();
                    foreach (var type in types)
                    {
                        if (type.FullName.Contains("AOTGenericReferences"))
                        {
                            var val = (IReadOnlyList<string>)type?.GetField("PatchedAOTAssemblyList")?.GetValue(null);
                            if (val != null)
                            {
                                foreach (var v in val)
                                {
                                    ret.Add(v);
                                }
                            }
                        }
                    }

                    if (ret.Count > 0)
                    {
                        return ret;
                    }
                }

                return new();
            }
        }

        [MenuItem("HCLRExtTools/GenerateScripts/ActiveBuildTarget", false, 100)]
        public static void GenerateScripts()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            SimpleLog.Log($"[BuildPipeline::GenerateScripts] 重新编译脚本 {target}");
            CompileDllCommand.CompileDll(target);
            AOTReferenceGeneratorCommand.GenerateAOTGenericReference(target);
            SimpleLog.Log($"[BuildPipeline::GenerateScripts] 打包hotfix.dll");

            var scriptOutDir = Path.Combine(Application.dataPath, "..", "HotfixUpload", target.ToString());
            scriptOutDir = Path.GetFullPath(scriptOutDir).Replace('\\', '/');

            var dirInfo = new DirectoryInfo(scriptOutDir);
            if (dirInfo.Exists)
            {
                Directory.Delete(scriptOutDir, true);
            }
            dirInfo.Create();

            var hotDlls = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(target);
            Generate(scriptOutDir, hotDlls, "Cfg.bytes", "hotfix", SettingsUtil.HotUpdateAssemblyFilesIncludePreserved);

            SimpleLog.Log($"[BuildPipeline::GenerateScripts] 打包元数据.dll");
            string baseDll = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
            var patchedAOTAssemblyList = PatchedAOTAssemblyList;
            foreach (var other in SettingsUtil.AOTAssemblyNames)
            {
                if (!patchedAOTAssemblyList.Contains(other))
                {
                    patchedAOTAssemblyList.Add(other);
                }
            }
            Generate(scriptOutDir, baseDll, "Base.bytes", "base", new(patchedAOTAssemblyList), true);

            var scriptConfig = Path.GetFullPath(Path.Combine(scriptOutDir, "Ver.bytes")).Replace('\\', '/');
            if (File.Exists(scriptConfig))
            {
                File.Delete(scriptConfig);
            }

            SimpleLog.Log($"[BuildPipeline::GenerateScripts] 生成版本文件");
            var config = new HotfixConfig();
            foreach (var file in Directory.GetFiles(scriptOutDir, "*.bytes"))
            {
                config.Items.Add(new()
                {
                    name = Path.GetFileName(file),
                    size = (uint)(new FileInfo(file)?.Length ?? 0),
                    hash = Utility.GetFileMd5(file)
                });
            }
            File.WriteAllText(scriptConfig, JsonConvert.SerializeObject(config, Formatting.Indented));
            SimpleLog.Log($"[BuildPipeline::GenerateScripts] 打包脚本结束");
        }
    }
}
