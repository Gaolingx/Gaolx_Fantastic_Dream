using GameMain.Utils;
using HybridCLR.Editor.Installer;
using UnityEditor;

namespace GameMain.Editor.BuildPipeline
{
    public partial class BuildPipeline
    {
        [MenuItem("HCLRExtTools/CheckHybridCLRInstall", false, 0)]
        public static void CheckHybridCLRInstall()
        {
            SimpleLog.Log($"[BuildPipeline::InstallDefaultHybridCLR] 开始检查插件状态");
            var localHclrVersion = EditorPrefs.GetString("[CustomBuildPipeline]installed_hclr_version");
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
                EditorPrefs.SetString("[CustomBuildPipeline]installed_hclr_version", HclrInstaller.PackageVersion.Trim());
                SimpleLog.Log($"[BuildPipeline::InstallDefaultHybridCLR] 安装插件完成");
            }
            EditorUtility.ClearProgressBar();
        }
    }
}
