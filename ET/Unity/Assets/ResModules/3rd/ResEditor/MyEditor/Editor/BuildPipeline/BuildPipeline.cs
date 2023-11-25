using GameMain.Editor.BuildPipeline.BuildPipelineCore;
using HybridCLR.Editor.Commands;
using UnityEditor.Build.Reporting;
using HybridCLR.Editor;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace GameMain.Editor.BuildPipeline
{
    public partial class BuildPipeline : BuildPipelineCallbackBase
    {
        public override void OnPostBuildPlayerScriptDLLs(BuildReport report)
        {
            GenerateLinkfile(report.summary.platform);
        }

        private static void GenerateLinkfile(BuildTarget target)
        {
            LinkGeneratorCommand.GenerateLinkXml(target);

            var ls = SettingsUtil.HybridCLRSettings;
            var dir = Path.Combine(Application.dataPath, ls.outputLinkFile.Replace("link.xml", ""), "Custom");
            GenerateLinkfile(Path.Combine(dir, "link.xml"));
        }

        public override void OnPreprocessBuild(BuildReport report)
        {
            Cleanup();
            Prepare();
        }

        public override void OnPostGenerateGradleAndroidProject(string path)
        {
#if UNITY_ANDROID
            OnEncryptMetadataProcess(path + "/src/main/assets/bin/Data/Managed/Metadata/global-metadata.dat");
#endif
        }

        public override void OnPostprocessBuild(BuildReport report)
        {
#if !UNITY_ANDROID && !UNITY_IPHONE
            var path = report.summary.outputPath;
            path = path.Replace('\\', '/');
            path = path.Substring(0, path.LastIndexOf('/'));
            var file = Directory.GetFiles(path, "global-metadata.dat", SearchOption.AllDirectories);
            if (file.Length == 0)
            {
                throw new Exception($"[BuildPipeline::Encrypt] OnPostprocessBuild {path}’“≤ªµΩ!!!");
            }

            OnEncryptMetadataProcess(file[0]);
#endif
        }
    }
}
