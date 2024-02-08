using GameMain.Utils;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameMain.Editor.BuildPipeline
{
    public partial class BuildPipeline
    {
        private static void Prepare()
        {
            PrepareMetadata();
        }

        [MenuItem("HCLRExtTools/Export/ActiveBuildTarget", false, 102)]
        private static void Real_Build_ActiveBuild()
        {
            var output = EditorUtility.SaveFolderPanel("Export ActiveBuildTarget", null, null);
            if (string.IsNullOrEmpty(output))
            {
                throw new Exception($"[BuildPipeline::BuildPlayer] 选择的目录为空!!!");
            }

            ResetBuildPlayerOptions();

            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    Build_Android(output);
                    break;
                case BuildTarget.StandaloneWindows64:
                    Build_Win64(output);
                    break;
                case BuildTarget.StandaloneOSX:
                    Build_MacOSX(output);
                    break;
                case BuildTarget.iOS:
                    Build_iOS(output);
                    break;
                default:
                    throw new Exception($"[BuildPipeline::CheckPlatform] 不支持{EditorUserBuildSettings.activeBuildTarget}, 请先切到合适平台再打包");
            }
        }

        private static string[] GetActiveSceneList()
        {
            var buildScenes = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled )
                {
                    buildScenes.Add(scene.path);
                }
            }

            return buildScenes.ToArray();
        }

        private static BuildPlayerOptions _bpOption;
        private static void ResetBuildPlayerOptions()
        {
            _bpOption = new()
            {
                scenes = GetActiveSceneList(),
                options = BuildOptions.CompressWithLz4 | BuildOptions.AcceptExternalModificationsToPlayer,
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
            };
        }

        private static void Build_Internal(string funcName, string outputPath)
        {
            var report = UnityEditor.BuildPipeline.BuildPlayer(_bpOption);
            SimpleLog.Log($"[BuildPipeline::{funcName}] End");
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                SimpleLog.Log($"[BuildPipeline::{funcName}] BuildPlayer 打包失败:{report.summary.result}");
            }
            else
            {
                Application.OpenURL($"file:///{outputPath}");
            }
        }

        private static void Build_Win64(string outputPath)
        {
            SimpleLog.Log($"[BuildPipeline::Build_Win64] Begin");

            var location = $"{outputPath}/HCLRExtTools.exe";
            SimpleLog.Log($"[BuildPipeline::Build_Win64] BuildPlayer");

            _bpOption.locationPathName = location;
            _bpOption.target = BuildTarget.StandaloneWindows64;
            _bpOption.targetGroup = BuildTargetGroup.Standalone;
            _bpOption.options = BuildOptions.CompressWithLz4;
            UnityEditor.WindowsStandalone.UserBuildSettings.createSolution = true;

            Build_Internal("Build_Win64", location);
        }

        private static void Build_Android(string outputPath)
        {
            SimpleLog.Log($"[BuildPipeline::Build_Android] Begin");

            var location = outputPath;
            if (!EditorUserBuildSettings.exportAsGoogleAndroidProject)
            {
                location = outputPath + "/HCLRExtTools.apk";
            }

            SimpleLog.Log($"[BuildPipeline::Build_Android] BuildPlayer");

            _bpOption.locationPathName = location;
            _bpOption.target = BuildTarget.Android;
            _bpOption.targetGroup = BuildTargetGroup.Android;

            Build_Internal("Build_Android", outputPath);
        }

        private static void Build_MacOSX(string outputPath)
        {
            SimpleLog.Log($"[BuildPipeline::Build_MacOSX] Begin");

            var location = $"{outputPath}/HCLRExtTools.app";

            SimpleLog.Log($"[BuildPipeline::Build_MacOSX] BuildPlayer");

            _bpOption.locationPathName = location;
            _bpOption.target = BuildTarget.StandaloneOSX;
            _bpOption.targetGroup = BuildTargetGroup.Standalone;

            Build_Internal("Build_MacOSX", location);
        }

        private static void Build_iOS(string outputPath)
        {
            SimpleLog.Log($"[BuildPipeline::Build_iOS] Begin");

            SimpleLog.Log($"[BuildPipeline::Build_iOS] BuildPlayer");

            _bpOption.locationPathName = outputPath;
            _bpOption.target = BuildTarget.iOS;
            _bpOption.targetGroup = BuildTargetGroup.iOS;

            Build_Internal("Build_iOS", outputPath);
        }
    }
}
