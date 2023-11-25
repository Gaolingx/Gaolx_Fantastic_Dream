using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Il2Cpp;
using UnityEditor.UnityLinker;
using UnityEngine.SceneManagement;

namespace GameMain.Editor.BuildPipeline.BuildPipelineCore
{
    public class BuildPipelineCallbackBase
    {
        public virtual void OnPreprocessBuild(BuildReport report) { }

        public virtual void OnProcessScene(Scene scene, BuildReport report) { }

        public virtual string[] OnFilterAssemblies(BuildOptions buildOptions, string[] assemblies)
        {
            return assemblies;
        }

        public virtual void OnPostBuildPlayerScriptDLLs(BuildReport report) { }

        public virtual string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
        {
            return string.Empty;
        }

        public virtual void OnBeforeRun(BuildReport report, UnityLinkerBuildPipelineData data) { }

        public virtual void OnAfterRun(BuildReport report, UnityLinkerBuildPipelineData data) { }

        public virtual void OnPostGenerateGradleAndroidProject(string path) { }

        public virtual void OnPostprocessBuild(BuildReport report) { }
    }
}
