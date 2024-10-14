using HotFix.Config;
using SangoUtils.Patchs_YooAsset;
using UnityEngine;

namespace HotFix.Service
{
    [RequireComponent(typeof(HotFixConfig))]
    public class HotfixService : MonoBehaviour
    {
        private HotFixConfig hotFixConfig;

        private void Start()
        {
            hotFixConfig = this.GetComponent<HotFixConfig>();

            SangoPatchConfig config = new SangoPatchConfig();
            config.HostServerIP = hotFixConfig.HostServerIP;
            config.AppID = hotFixConfig.AppID;
            config.AppVersion = hotFixConfig.AppVersion;
            config.AppendTimeTicks = hotFixConfig.AppendTimeTicks;
            config.Timeout = hotFixConfig.Timeout;
            config.DownloadingMaxNum = hotFixConfig.DownloadingMaxNum;
            config.FailedTryAgain = hotFixConfig.FailedTryAgain;
            config.PackageName = hotFixConfig.PackageName;
            config.PlayMode = hotFixConfig.PlayMode;
            config.BuildPipeline = hotFixConfig.BuildPipeline;
            config.SangoPatchWnd = hotFixConfig.SangoPatchWnd;
            config.GameRootObjectName = hotFixConfig.GameRootObjectName;
            config.GameRootParentTransform = hotFixConfig.GameRootParentTransform;
            config.HotUpdateDllList = hotFixConfig.HotUpdateDllList;
            config.AOTMetaAssemblyNames = hotFixConfig.AOTMetaAssemblyNames;
            config.OnUpdaterDone = hotFixConfig.OnUpdaterDone;

            SangoPatchRoot patchRoot = this.gameObject.AddComponent<SangoPatchRoot>();
            patchRoot.Initialize(config);
        }
    }
}
