using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YooAsset;

//Developer: SangonomiyaSakunovi

public class HotFixService : MonoBehaviour
{

    //补充元数据dll的列表，Yooasset中不需要带后缀
    public static List<string> AOTMetaAssemblyNames { get; } = new List<string>()
    {
        "Assets/AssetBundles/Scripts/Dlls/mscorlib.dll",
        "Assets/AssetBundles/Scripts/Dlls/System.dll",
        "Assets/AssetBundles/Scripts/Dlls/System.Core.dll",
        "Assets/AssetBundles/Scripts/Dlls/System.Xml.dll",
        "Assets/AssetBundles/Scripts/Dlls/UnityEngine.CoreModule.dll",
        "Assets/AssetBundles/Scripts/Dlls/Unity.InputSystem.dll",
        "Assets/AssetBundles/Scripts/Dlls/Cinemachine.dll",
        "Assets/AssetBundles/Scripts/Dlls/PESocket.dll",
        "Assets/AssetBundles/Scripts/Dlls/PEProtocol.dll",
        "Assets/AssetBundles/Scripts/Dlls/PETimer.dll",
        "Assets/AssetBundles/Scripts/Dlls/UniTask.dll",
        "Assets/AssetBundles/Scripts/Dlls/YooAsset.dll",
    };


    [SerializeField] private string HotDllName = "Assets/AssetBundles/Scripts/Dlls/GameMain.dll";
    [SerializeField] private string GameRootObject = "Assets/AssetBundles/Prefabs/RootPrefabs/HotFixRoot.prefab";

    //获取资源二进制
    private static Dictionary<string, byte[]> _dllAssetDataDict = new Dictionary<string, byte[]>();

    public static HotFixService Instance;

    public Transform _hotFixRootParent;

    private ResourceDownloaderOperation _downloaderOperation;
    private ResourcePackage _yooAssetResourcePackage;

    public HotFixWindow _hotFixWindow;
    private HotFixConfig _hotFixConfig;

    public void InitService()
    {
        Instance = this;
        _hotFixConfig = GetComponent<HotFixConfig>();
        _hotFixWindow.SetTips("正在检查更新");
        StartCoroutine(PrepareAssets());
    }

    public void RunHotFix()
    {
        StartCoroutine(RunDownloader(_downloaderOperation));
    }

    public void EnterSangoGameRoot()
    {
        LoadDll();
        LoadMetadataForAOTAssemblies();

#if !UNITY_EDITOR
        System.Reflection.Assembly.Load(GetAssetData(HotDllName));
#endif

        StopAllCoroutines();
        LoadGameRootObject();
    }

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

    private IEnumerator PrepareAssets()
    {
        yield return new WaitForSeconds(1f);

        // 1.初始化资源系统
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
        EPlayMode PlayMode = _hotFixConfig.GetEPlayMode(); // 资源系统运行模式
        switch (PlayMode)
        {
            // 编辑器下的模拟模式
            case EPlayMode.EditorSimulateMode:
                {
                    var initParameters = new EditorSimulateModeParameters();
                    var simulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(packageName);
                    initParameters.SimulateManifestFilePath = simulateManifestFilePath;
                    var initOperation = _yooAssetResourcePackage.InitializeAsync(initParameters);
                    yield return initOperation;

                    if (initOperation.Status == EOperationStatus.Succeed)
                    {
                        Debug.Log("资源包初始化成功！");
                        EnterSangoGameRoot();
                        yield break;
                    }
                    else
                    {
                        Debug.LogError($"资源包初始化失败：{initOperation.Error}");
                        yield break;
                    }
                }
            // 联机运行模式
            case EPlayMode.HostPlayMode:
                {
                    string defaultHostServer = _hotFixConfig.GetHostServerURL();
                    string fallbackHostServer = _hotFixConfig.GetHostServerURL();
                    var initParameters = new HostPlayModeParameters();
                    initParameters.BuildinQueryServices = new GameQueryServices();
                    initParameters.DeliveryQueryServices = new DefaultDeliveryQueryServices();
                    initParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                    var initOperation = _yooAssetResourcePackage.InitializeAsync(initParameters);
                    yield return initOperation;

                    if (initOperation.Status == EOperationStatus.Succeed)
                    {
                        Debug.Log("资源包初始化成功！");
                    }
                    else
                    {
                        Debug.LogError($"资源包初始化失败：{initOperation.Error}");
                        yield break;
                    }
                }
                break;
            // 单机运行模式
            case EPlayMode.OfflinePlayMode:
                {
                    var initParameters = new OfflinePlayModeParameters();
                    var initOperation = _yooAssetResourcePackage.InitializeAsync(initParameters);
                    yield return initOperation;

                    if (initOperation.Status == EOperationStatus.Succeed)
                    {
                        Debug.Log("资源包初始化成功！");
                        EnterSangoGameRoot();
                        yield break;
                    }
                    else
                    {
                        Debug.LogError($"资源包初始化失败：{initOperation.Error}");
                        yield break;
                    }
                }
        }

        //2.获取资源版本
        var updatePackageVersionOperation = _yooAssetResourcePackage.UpdatePackageVersionAsync();
        yield return updatePackageVersionOperation;

        if (updatePackageVersionOperation.Status != EOperationStatus.Succeed)
        {
            Debug.LogError(updatePackageVersionOperation.Error);
            yield break;
        }
        string packageVersion = updatePackageVersionOperation.PackageVersion;

        //3.更新补丁清单
        bool savePackageVersion = true;
        var updatePackageManifestOperation = _yooAssetResourcePackage.UpdatePackageManifestAsync(packageVersion, savePackageVersion);
        yield return updatePackageManifestOperation;

        if (updatePackageManifestOperation.Status != EOperationStatus.Succeed)
        {
            Debug.LogError(updatePackageManifestOperation.Error);
            yield break;
        }

        //4.下载补丁包
        PrepareDownloader();
        yield break;
    }

    private void PrepareDownloader()
    {
        int downloadingMaxNum = 10;
        int failedTryAgain = 3;
        int timeout = 60;
        var downloader = _yooAssetResourcePackage.CreateResourceDownloader(downloadingMaxNum, failedTryAgain, timeout);

        if (downloader.TotalDownloadCount == 0)
        {
            EnterSangoGameRoot();
            Debug.Log("没有任何数据需要下载哦~");
            return;
        }

        //需要下载的文件总数和总大小
        int totalDownloadCount = downloader.TotalDownloadCount;
        long totalDownloadBytes = downloader.TotalDownloadBytes;
        Debug.Log($"文件总数:{totalDownloadCount}:::总大小:{totalDownloadBytes}");
        //注册回调方法
        downloader.OnDownloadErrorCallback = OnDownloadErrorFunction;
        downloader.OnDownloadProgressCallback = OnDownloadProgressUpdateFunction;
        downloader.OnDownloadOverCallback = OnDownloadOverFunction;
        downloader.OnStartDownloadFileCallback = OnStartDownloadFileFunction;

        _downloaderOperation = downloader;

        _hotFixWindow.OpenHotFixPanel();
        _hotFixWindow.SetHotFixInfoText(totalDownloadBytes);

        Debug.Log("现在已经准备好下载器了哦~");
    }

    private IEnumerator RunDownloader(ResourceDownloaderOperation downloader)
    {
        //开启下载
        downloader.BeginDownload();
        _hotFixWindow.SetTips("正在下载更新中");
        yield return downloader;

        //检测下载结果
        if (downloader.Status == EOperationStatus.Succeed)
        {
            Debug.Log("下载成功");
            EnterSangoGameRoot();
        }
        else
        {
            Debug.Log("下载失败");
        }
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

    private static byte[] GetAssetData(string dllName)
    {
        return _dllAssetDataDict[dllName];
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
