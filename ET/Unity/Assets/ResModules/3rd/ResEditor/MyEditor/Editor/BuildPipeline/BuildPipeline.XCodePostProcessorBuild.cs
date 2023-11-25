#if UNITY_IPHONE

using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using GameMain.Utils;

namespace GameMain.Editor.BuildPipeline
{
    public partial class BuildPipeline
    {
        [PostProcessBuild(88)]
        private static void OnPostProcessBuild(BuildTarget target, string targetPath)
        {
            if (target != BuildTarget.iOS)
            {
                SimpleLog.LogWarning("[BuildPipeline::OnPostProcessBuild] Target is not iPhone. XCodePostProcess will not run");
                return;
            }

            OnEncryptMetadataProcess(Path.Combine(targetPath, "Data", "Managed", "Metadata", "global-metadata.dat"));
        }
    }
}

#endif
