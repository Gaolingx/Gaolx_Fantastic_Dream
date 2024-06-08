using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using YooAsset;

//Developer: SangonomiyaSakunovi,Gaolingx

public class HotFixService : MonoBehaviour
{
    public static HotFixService Instance;

    public Transform _hotFixRootParent;
    public HotFixWindow _hotFixWindow;
    public HotFixConfig _hotFixConfig;

    [SerializeField] private string HotDllName = "Assets/AssetBundles/Scripts/Dlls/GameMain.dll";
    [SerializeField] private string GameRootObject = "Assets/AssetBundles/Prefabs/RootPrefabs/HotFixRoot.prefab";

    //补充元数据dll的列表，Yooasset中不需要带后缀
    private static List<string> AOTMetaAssemblyNames { get; } = new List<string>()
    {
        "Assets/AssetBundles/Scripts/Dlls/mscorlib.dll",
        "Assets/AssetBundles/Scripts/Dlls/System.dll",
        "Assets/AssetBundles/Scripts/Dlls/System.Core.dll",
        "Assets/AssetBundles/Scripts/Dlls/System.Xml.dll",
        "Assets/AssetBundles/Scripts/Dlls/UnityEngine.CoreModule.dll",
        "Assets/AssetBundles/Scripts/Dlls/Unity.InputSystem.dll",
        "Assets/AssetBundles/Scripts/Dlls/DOTween.dll",
        "Assets/AssetBundles/Scripts/Dlls/Cinemachine.dll",
        "Assets/AssetBundles/Scripts/Dlls/PESocket.dll",
        "Assets/AssetBundles/Scripts/Dlls/PEProtocol.dll",
        "Assets/AssetBundles/Scripts/Dlls/PETimer.dll",
        "Assets/AssetBundles/Scripts/Dlls/UniTask.dll",
        "Assets/AssetBundles/Scripts/Dlls/YooAsset.dll",
    };

    //获取资源二进制
    private static Dictionary<string, byte[]> _dllAssetDataDict = new Dictionary<string, byte[]>();
    private static byte[] GetAssetData(string dllName)
    {
        return _dllAssetDataDict[dllName];
    }

    /// <summary>
	/// 运行模式
	/// </summary>
	public EPlayMode PlayMode { private set; get; }

    /// <summary>
    /// 包裹的版本信息
    /// </summary>
    public string PackageVersion { set; get; }

    /// <summary>
    /// 下载器
    /// </summary>
    public ResourceDownloaderOperation Downloader { set; get; }

    private ResourcePackage _yooAssetResourcePackage;

    public void InitService()
    {
        Instance = this;

        _hotFixConfig = GetComponent<HotFixConfig>();
        _hotFixWindow.SetTips("正在检查更新");
        StartCoroutine(DownLoadAssetsByYooAssets());
    }

    private IEnumerator DownLoadAssetsByYooAssets()
    {
        // 1.初始化资源系统
        yield return InitPackage();

        // 2.获取资源版本
        yield return RequestPackageVersion();

        // 3.更新补丁清单
        yield return RequestPackageManifest();

        // 4.下载补丁包
        yield return PrepareDownloader();

    }

    private IEnumerator InitPackage()
    {
        _hotFixWindow.SetTips("初始化资源包！");
        yield return new WaitForSeconds(1f);

        YooAssets.Initialize();
        string packageName = "DefaultPackage";
        _yooAssetResourcePackage = YooAssets.TryGetPackage(packageName);

        if (_yooAssetResourcePackage == null)
        {
            // 创建默认的资源包
            _yooAssetResourcePackage = YooAssets.CreatePackage(packageName);
            // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
            YooAssets.SetDefaultPackage(_yooAssetResourcePackage);
        }

        InitializationOperation initializationOperation = null;
        PlayMode = _hotFixConfig.GetEPlayMode(); // 资源系统运行模式

        // 编辑器下的模拟模式
        if (PlayMode == EPlayMode.EditorSimulateMode)
        {
            var createParameters = new EditorSimulateModeParameters();
            createParameters.SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(packageName);
            initializationOperation = _yooAssetResourcePackage.InitializeAsync(createParameters);
        }
        // 联机运行模式
        else if (PlayMode == EPlayMode.HostPlayMode)
        {
            string defaultHostServer = _hotFixConfig.GetHostServerURL();
            string fallbackHostServer = _hotFixConfig.GetHostServerURL();
            var createParameters = new HostPlayModeParameters();
            createParameters.DecryptionServices = new GameDecryptionServices();
            createParameters.BuildinQueryServices = new GameQueryServices();
            createParameters.DeliveryQueryServices = new DefaultDeliveryQueryServices();
            createParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            initializationOperation = _yooAssetResourcePackage.InitializeAsync(createParameters);
        }
        // 单机运行模式
        else if (PlayMode == EPlayMode.OfflinePlayMode)
        {
            var createParameters = new OfflinePlayModeParameters();
            createParameters.DecryptionServices = new GameDecryptionServices();
            initializationOperation = _yooAssetResourcePackage.InitializeAsync(createParameters);
        }
        // WebGL运行模式
        else if (PlayMode == EPlayMode.WebPlayMode)
        {
            string defaultHostServer = _hotFixConfig.GetHostServerURL();
            string fallbackHostServer = _hotFixConfig.GetHostServerURL();
            var createParameters = new WebPlayModeParameters();
            createParameters.DecryptionServices = new GameDecryptionServices();
            createParameters.BuildinQueryServices = new GameQueryServices();
            createParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
            initializationOperation = _yooAssetResourcePackage.InitializeAsync(createParameters);
        }

        yield return initializationOperation;
        if (initializationOperation.Status != EOperationStatus.Succeed)
        {
            _hotFixWindow.SetTips("初始化资源包失败！");
            Debug.LogWarning($"{initializationOperation.Error}");
            yield break;
        }
    }

    private IEnumerator RequestPackageVersion()
    {
        _hotFixWindow.SetTips("获取最新的资源版本！");
        yield return new WaitForSecondsRealtime(0.5f);

        var updatePackageVersionOperation = _yooAssetResourcePackage.UpdatePackageVersionAsync(_hotFixConfig.appendTimeTicks);
        yield return updatePackageVersionOperation;

        if (updatePackageVersionOperation.Status == EOperationStatus.Succeed)
		{
			HotFixService.Instance.PackageVersion = updatePackageVersionOperation.PackageVersion;
			Debug.Log($"远端最新版本为: {updatePackageVersionOperation.PackageVersion}");
			yield return updatePackageVersionOperation;
		}
		else
		{
            _hotFixWindow.SetTips("请检查本地网络，获取资源版本失败！");
            Debug.LogWarning(updatePackageVersionOperation.Error);
            yield break;
        }
    }

    private IEnumerator RequestPackageManifest()
    {
        _hotFixWindow.SetTips("更新资源清单！");
        yield return new WaitForSecondsRealtime(0.5f);

        bool autoSaveVersion = true;
        var updatePackageManifestOperation = _yooAssetResourcePackage.UpdatePackageManifestAsync(PackageVersion, autoSaveVersion);
        yield return updatePackageManifestOperation;

        if (updatePackageManifestOperation.Status != EOperationStatus.Succeed)
        {
            _hotFixWindow.SetTips("请检查本地网络，资源清单更新失败！");
            Debug.LogWarning(updatePackageManifestOperation.Error);
            yield break;
        }
    }

    private IEnumerator PrepareDownloader()
    {
        _hotFixWindow.SetTips("创建补丁下载器！");
        yield return new WaitForSecondsRealtime(0.5f);

        int downloadingMaxNum = 10;
        int failedTryAgain = 3;
        int timeout = 60;
        var downloader = _yooAssetResourcePackage.CreateResourceDownloader(downloadingMaxNum, failedTryAgain, timeout);
        Downloader = downloader;

        if (downloader.TotalDownloadCount == 0)
        {
            Debug.Log("Not found any download files !");
            ClearUnusedCacheFiles(_yooAssetResourcePackage);
        }
        else
        {
            //A total of 10 files were found that need to be downloaded
            Debug.Log($"Found total {downloader.TotalDownloadCount} files that need download ！");

            // 发现新更新文件后，挂起流程系统
            // 注意：开发者需要在下载前检测磁盘空间不足
            int totalDownloadCount = downloader.TotalDownloadCount;
            long totalDownloadBytes = downloader.TotalDownloadBytes;

            // 注册下载回调
            RegisterDownloadCallback();

            _hotFixWindow.OpenHotFixPanel();
            _hotFixWindow.SetHotFixInfoText(totalDownloadCount, totalDownloadBytes);

        }
    }

    #region RunHotFix
    //这个步骤由用户手动执行
    public void RunHotFix()
    {
        StartCoroutine(DownloaderCoroutine());
    }

    private IEnumerator DownloaderCoroutine()
    {
        yield return RunDownloader(Downloader);
        ClearUnusedCacheFiles(_yooAssetResourcePackage);
    }

    private IEnumerator RunDownloader(ResourceDownloaderOperation downloader)
    {
        _hotFixWindow.SetTips("开始下载补丁文件！");
        //开启下载
        downloader.BeginDownload();
        yield return downloader;

        // 检测下载结果
        if (downloader.Status != EOperationStatus.Succeed)
            yield break;
    }

    private void ClearUnusedCacheFiles(ResourcePackage package)
    {
        _hotFixWindow.SetTips("清理未使用的缓存文件！");
        var operation = package.ClearUnusedCacheFilesAsync();
        operation.Completed += Operation_Completed;
    }
    private void Operation_Completed(YooAsset.AsyncOperationBase obj)
    {
        EnterSangoGameRoot();
    }
    #endregion

    #region DownloadCallBack
    private void RegisterDownloadCallback()
    {
        var downloader = HotFixService.Instance.Downloader;

        downloader.OnDownloadErrorCallback = OnDownloadErrorFunction;
        downloader.OnDownloadProgressCallback = OnDownloadProgressUpdateFunction;
        downloader.OnDownloadOverCallback = OnDownloadOverFunction;
        downloader.OnStartDownloadFileCallback = OnStartDownloadFileFunction;
    }

    /// <summary>
    /// 开始下载
    /// </summary>
    /// <param name="totalDownloadCount"></param>
    /// <param name="currentDownloadCount"></param>
    /// <param name="totalDownloadBytes"></param>
    /// <param name="currentDownloadBytes"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnDownloadProgressUpdateFunction(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
    {
        Debug.Log($"文件总数: {totalDownloadCount}, 已下载文件数： {currentDownloadCount}, 总大小: {totalDownloadBytes}, 已下载大小: {currentDownloadBytes}");
        float progress = (float)currentDownloadBytes / totalDownloadBytes;
        _hotFixWindow.SetLoadingProgress(progress);
    }

    /// <summary>
    /// 开始下载
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="sizeBytes"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnStartDownloadFileFunction(string fileName, long sizeBytes)
    {
        Debug.Log(string.Format("开始下载：文件名：{0}, 文件大小：{1}", fileName, sizeBytes));
    }

    /// <summary>
    /// 下载完成
    /// </summary>
    /// <param name="isSucceed"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnDownloadOverFunction(bool isSucceed)
    {
        Debug.Log("下载" + (isSucceed ? "成功" : "失败"));
    }

    /// <summary>
    /// 下载出错
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="error"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnDownloadErrorFunction(string fileName, string error)
    {
        Debug.LogError(string.Format("下载出错：文件名：{0}, 错误信息：{1}", fileName, error));
    }
    #endregion

    #region EnterGameRoot
    public void EnterSangoGameRoot()
    {
        _hotFixWindow.SetTips("开始游戏！");
        LoadDll();
        LoadMetadataForAOTAssemblies();

#if !UNITY_EDITOR
        System.Reflection.Assembly.Load(GetAssetData(HotDllName));
#endif

        StopAllCoroutines();
        LoadGameRootObject();
    }

    private void LoadGameRootObject()
    {
        var asset1 = _yooAssetResourcePackage.LoadAssetSync<GameObject>(GameRootObject);
        GameObject hotFixRoot = asset1.InstantiateSync();
        hotFixRoot.transform.SetParent(_hotFixRootParent);
        hotFixRoot.transform.position = Vector3.zero;
        hotFixRoot.transform.localScale = Vector3.one;
        RectTransform rect = hotFixRoot.GetComponent<RectTransform>();
        rect.offsetMax = new Vector2(0, 0);
        _hotFixWindow.gameObject.SetActive(false);
    }
    #endregion

    #region LoadAssemblies
    private void LoadDll()
    {
        var dllNameList = new List<string>()
        {
            HotDllName,
        }.Concat(AOTMetaAssemblyNames);
        foreach (var dllName in dllNameList)
        {
            var obj = _yooAssetResourcePackage.LoadRawFileSync(dllName);
            byte[] fileData = obj.GetRawFileData();
            _dllAssetDataDict.Add(dllName, fileData);
            Debug.Log($"dll:{dllName}  size:{fileData.Length}");
        }
    }

    /// <summary>
    /// 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
    /// 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行
    /// </summary>
    private static void LoadMetadataForAOTAssemblies()
    {
        // 可以加载任意aot assembly的对应的dll。但要求dll必须与unity build过程中生成的裁剪后的dll一致，而不能直接使用原始dll。
        // 我们在BuildProcessors里添加了处理代码，这些裁剪后的dll在打包时自动被复制到 {项目目录}/HybridCLRData/AssembliesPostIl2CppStrip/{Target} 目录。

        /// 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
        /// 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误
        /// 
        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var aotDllName in AOTMetaAssemblyNames)
        {
            byte[] dllBytes = GetAssetData(aotDllName);
            // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
            Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} return:{err}");
        }
    }
    #endregion

    /// <summary>
	/// 远端资源地址查询服务类
	/// </summary>
	private class RemoteServices : IRemoteServices
    {
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public RemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = defaultHostServer;
            _fallbackHostServer = fallbackHostServer;
        }
        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return $"{_defaultHostServer}/{fileName}";
        }
        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return $"{_fallbackHostServer}/{fileName}";
        }
    }

    /// <summary>
    /// 资源文件解密服务类
    /// </summary>
    private class GameDecryptionServices : IDecryptionServices
    {
        public ulong LoadFromFileOffset(DecryptFileInfo fileInfo)
        {
            return 32;
        }

        public byte[] LoadFromMemory(DecryptFileInfo fileInfo)
        {
            throw new NotImplementedException();
        }

        public Stream LoadFromStream(DecryptFileInfo fileInfo)
        {
            BundleStream bundleStream = new BundleStream(fileInfo.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return bundleStream;
        }

        public uint GetManagedReadBufferSize()
        {
            return 1024;
        }
    }

    /// <summary>
    /// 默认的分发资源查询服务类
    /// </summary>
    private class DefaultDeliveryQueryServices : IDeliveryQueryServices
    {
        public DeliveryFileInfo GetDeliveryFileInfo(string packageName, string fileName)
        {
            throw new NotImplementedException();
        }
        public bool QueryDeliveryFiles(string packageName, string fileName)
        {
            return false;
        }
    }

}
