using GameMain.Utils;
using HybridCLR.Editor;
using HybridCLR.Editor.Installer;
using System;
using System.IO;
using UnityEditor;

namespace GameMain.Editor.BuildPipeline
{
    public partial class BuildPipeline
    {
        [MenuItem("HCLRExtTools/CheckHybridCLRInstall", false, 0)]
        public static void CheckHybridCLRInstall()
        {
            SimpleLog.Log($"[BuildPipeline::InstallDefaultHybridCLR] 开始检查插件状态");
            var localHclrVersion = EditorPrefs.GetString("[CustomBuildPipeline]installed_hclr_version_new");
            var HclrInstaller = new InstallerController();
            if (!string.IsNullOrEmpty(localHclrVersion) && localHclrVersion == HclrInstaller.PackageVersion.Trim() && HclrInstaller.HasInstalledHybridCLR())
            {
                SimpleLog.Log($"[BuildPipeline::InstallDefaultHybridCLR] 无需安装插件");
            }
            else
            {
                SimpleLog.Log($"[BuildPipeline::InstallDefaultHybridCLR] 开始安装插件");
                EditorUtility.DisplayProgressBar("正在安装插件...", "", 0);
                HclrInstaller.InstallDefaultHybridCLR();
                EditorPrefs.SetString("[CustomBuildPipeline]installed_hclr_version_new", HclrInstaller.PackageVersion.Trim());

                PatchCppCode();

                SimpleLog.Log($"[BuildPipeline::InstallDefaultHybridCLR] 安装插件完成");
            }
            EditorUtility.ClearProgressBar();
        }

        private static void PatchCppCode()
        {
            SimpleLog.Log($"[BuildPipeline::PatchCppCode] patch开始");

            var assets = AssetDatabase.FindAssets("LibIl2cppModule-2021");
            if (assets.Length == 0)
            {
                throw new Exception($"[BuildPipeline::PatchCppCode] LibIl2cppModule-2021!!!");
            }

            var filePath = AssetDatabase.GUIDToAssetPath(assets[0]);
            var dst = Path.Combine(SettingsUtil.LocalIl2CppDir, "libil2cpp");
            var dirInfo = new DirectoryInfo(dst);
            if (dirInfo.Exists)
            {
                dirInfo.Delete(true);
            }
            dirInfo.Create();

            if (!ExtractApiCodeToPath(filePath, dst))
            {
                throw new Exception($"[BuildPipeline::PatchCppCode] 释放LibIl2cppModule-2021出错");
            }

            SimpleLog.Log($"[BuildPipeline::PatchCppCode] patch结束");
        }
    }
}
