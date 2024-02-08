using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.UnityLinker;
using UnityEngine.SceneManagement;
using UnityEngine;

#if UNITY_ANDROID
using UnityEditor.Android;
#endif

namespace GameMain.Editor.BuildPipeline.BuildPipelineCore
{
    public class BuildPipelineCore : IPreprocessBuildWithReport, IProcessSceneWithReport, IFilterBuildAssemblies,
        IPostBuildPlayerScriptDLLs, 
#if UNITY_ANDROID
        IPostGenerateGradleAndroidProject,
#endif
        IPostprocessBuildWithReport
    {
        private static MethodInfo _sBuildReportAddMessage = null;

        int IOrderedCallback.callbackOrder => 0;

        private readonly List<BuildPipelineCallbackBase> _buildPipelineCallback =
            new List<BuildPipelineCallbackBase>(16);

        private BuildReport _reporter = null;

        private void CheckPipelineCallback(BuildReport report)
        {
            if (_sBuildReportAddMessage == null)
            {
                var flag = BindingFlags.Instance | BindingFlags.NonPublic;
                _sBuildReportAddMessage = typeof(BuildReport).GetMethod("AddMessage", flag);
            }

            if (report != null)
            {
                _reporter = report;
            }

            if (_buildPipelineCallback.Count > 0)
            {
                return;
            }

            BuildPipelineException.Init((successful, willCancel) =>
            {
                if (Application.isPlaying)
                {
                    return;
                }

                if (!willCancel)
                {
                    return;
                }

                if (successful)
                {
                    return;
                }

                EditorUtility.DisplayDialog("错误", "有异常发生，请根据控制台提示修正对应错误!", "确定");
                _sBuildReportAddMessage.Invoke(_reporter,
                    new object[] {LogType.Exception, "用户取消", "BuildFailedException"});
            });

            var baseType = typeof(BuildPipelineCallbackBase);
            foreach (var callbackType in AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()))
            {
                if (!baseType.IsAssignableFrom(callbackType))
                {
                    continue;
                }

                var typeInstance = (BuildPipelineCallbackBase) Activator.CreateInstance(callbackType);
                if (typeInstance != null)
                {
                    _buildPipelineCallback.Add(typeInstance);
                }
            }
        }

        private void ForeachCall(Action<BuildPipelineCallbackBase> callback)
        {
            foreach (var node in _buildPipelineCallback)
            {
                callback?.Invoke(node);
            }
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            CheckPipelineCallback(report);

            ForeachCall(node => { node.OnPreprocessBuild(report); });
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            CheckPipelineCallback(report);

            ForeachCall(node => { node.OnProcessScene(scene, report); });
        }

        public string[] OnFilterAssemblies(BuildOptions buildOptions, string[] assemblies)
        {
            CheckPipelineCallback(null);

            ForeachCall(node => { assemblies = node.OnFilterAssemblies(buildOptions, assemblies); });

            return assemblies;
        }

        public void OnPostBuildPlayerScriptDLLs(BuildReport report)
        {
            CheckPipelineCallback(report);

            ForeachCall(node => { node.OnPostBuildPlayerScriptDLLs(report); });
        }

        public string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
        {
            CheckPipelineCallback(report);

            ForeachCall(node => { node.GenerateAdditionalLinkXmlFile(report, data); });

            return string.Empty;
        }

#if UNITY_ANDROID
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            CheckPipelineCallback(null);

            ForeachCall(node => { node.OnPostGenerateGradleAndroidProject(path); });
        }
#endif

        public void OnPostprocessBuild(BuildReport report)
        {
            CheckPipelineCallback(report);

            ForeachCall(node => { node.OnPostprocessBuild(report); });

            BuildPipelineException.Destroy();
        }
    }
}
