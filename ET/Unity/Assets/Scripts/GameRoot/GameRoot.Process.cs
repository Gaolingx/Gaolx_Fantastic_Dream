using System;
using System.Collections.Generic;
using System.Linq;
using GameMain.Utils;
using System.Reflection;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace GameMain.Scripts
{
    public partial class GameRoot
    {
        private static Assembly _gameDll = null;
        public static Assembly GameAssembly => _gameDll;

        private async UniTask LoadGameDll()
        {
            SimpleLog.Log("[GameRoot::LoadGameDll] Start");
            if (_gameDll != null)
            {
                return;
            }

            var metadata = default(Dictionary<string, byte[]>);
#if !UNITY_EDITOR
            metadata = await LoadDataAsync("Base.bytes", "base");
            if (metadata == null || metadata.Count == 0)
            {
                SimpleLog.Log("[GameRoot::LoadGameDll] metadata is null!");
                return;
            }
#endif
            await LoadMetadataForAotAssembly(metadata);
            if (!Application.isEditor)
            {
                var dict = await LoadDataAsync("Cfg.bytes", "hotfix");
                if (dict == null || dict.Count == 0)
                {
                    SimpleLog.Log("[GameRoot::LoadGameDll] dict is null!");
                    return;
                }

                if (!dict.TryGetValue("Assembly-CSharp.dll", out var hotfixData))
                {
                    SimpleLog.Log("[GameRoot::LoadGameDll] can't find Assembly-CSharp.dll!");
                    return;
                }

                if (hotfixData == null || hotfixData.Length == 0)
                {
                    SimpleLog.Log("[GameRoot::LoadGameDll] can't find Assembly-CSharp.dll is empty");
                    return;
                }

                SimpleLog.Log("[GameRoot::LoadGameDll] load Assembly-CSharp.dll ok!!!");

                _gameDll = System.Reflection.Assembly.Load(hotfixData);
                if (_gameDll == null)
                {
                    SimpleLog.Log("[GameRoot::LoadGameDll] load Assembly-CSharp.dll failed");
                    return;
                }

                SimpleLog.Log("[GameRoot::LoadGameDll] load Assembly-CSharp.dll success");
            }
            else
            {
                _gameDll = AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Assembly-CSharp");
            }
        }

        private static async UniTask LoadMetadataForAotAssembly(Dictionary<string, byte[]> dllBytes)
        {
            await UniTask.Yield();
#if !UNITY_EDITOR
            dllBytes ??= new Dictionary<string, byte[]>();

            var loadType = global::HybridCLR.HomologousImageMode.SuperSet;
            SimpleLog.Log($"[App::LoadMetadataForAotAssembly] Count:{dllBytes.Count}");
            foreach (var dll in dllBytes)
            {
                SimpleLog.Log($"[App::LoadMetadataForAotAssembly] load {dll.Key}");
                try
                {
                    var err = global::HybridCLR.RuntimeApi.LoadMetadataForAOTAssembly(dll.Value, loadType);
                    if (err != global::HybridCLR.LoadImageErrorCode.OK)
                    {
                        SimpleLog.Log($"[App::LoadMetadataForAotAssembly] load {dll.Key} ret:{err}");
                    }
                }
                catch (Exception ex)
                {
                    SimpleLog.Log($"[App::LoadMetadataForAotAssembly] load {dll.Key} exception");
                    SimpleLog.LogException(ex);
                }
            }

            SimpleLog.Log($"[App::LoadMetadataForAotAssembly] finished!");
#endif
        }

        private async UniTask RunDll()
        {
            SimpleLog.Log("[GameRoot::RunDll] Begin");

            if (_gameDll == null)
            {
                SimpleLog.Log($"[GameRoot::RunDll] Dll is null");
                await UniTask.Yield();
            }

            var appType = _gameDll.GetType("HotFix.LaunchMain");
            if (appType == null)
            {
                SimpleLog.Log($"[GameRoot::RunDll] appType is null");
                await UniTask.Yield();
            }

            var mainMethod = appType.GetMethod("Main");
            if (mainMethod == null)
            {
                SimpleLog.Log($"[GameRoot::RunDll] Main is null");
                await UniTask.Yield();
            }

            mainMethod.Invoke(null, null);
        }
    }
}
