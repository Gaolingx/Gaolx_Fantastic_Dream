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

    //����Ԫ����dll���б�Yooasset�в���Ҫ����׺
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

    //��ȡ��Դ������
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
        _hotFixWindow.SetTips("���ڼ�����");
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

        // 1.��ʼ����Դϵͳ
        YooAssets.Initialize();
        string packageName = "DefaultPackage";
        _yooAssetResourcePackage = YooAssets.TryGetPackage(packageName);

        if (_yooAssetResourcePackage == null)
        {
            // ����Ĭ�ϵ���Դ��
            _yooAssetResourcePackage = YooAssets.CreatePackage(packageName);
            // ���ø���Դ��ΪĬ�ϵ���Դ��������ʹ��YooAssets��ؼ��ؽӿڼ��ظ���Դ�����ݡ�
            YooAssets.SetDefaultPackage(_yooAssetResourcePackage);
        }
        EPlayMode PlayMode = _hotFixConfig.GetEPlayMode(); // ��Դϵͳ����ģʽ
        switch (PlayMode)
        {
            // �༭���µ�ģ��ģʽ
            case EPlayMode.EditorSimulateMode:
                {
                    var initParameters = new EditorSimulateModeParameters();
                    var simulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(packageName);
                    initParameters.SimulateManifestFilePath = simulateManifestFilePath;
                    var initOperation = _yooAssetResourcePackage.InitializeAsync(initParameters);
                    yield return initOperation;

                    if (initOperation.Status == EOperationStatus.Succeed)
                    {
                        Debug.Log("��Դ����ʼ���ɹ���");
                        EnterSangoGameRoot();
                        yield break;
                    }
                    else
                    {
                        Debug.LogError($"��Դ����ʼ��ʧ�ܣ�{initOperation.Error}");
                        yield break;
                    }
                }
            // ��������ģʽ
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
                        Debug.Log("��Դ����ʼ���ɹ���");
                    }
                    else
                    {
                        Debug.LogError($"��Դ����ʼ��ʧ�ܣ�{initOperation.Error}");
                        yield break;
                    }
                }
                break;
            // ��������ģʽ
            case EPlayMode.OfflinePlayMode:
                {
                    var initParameters = new OfflinePlayModeParameters();
                    var initOperation = _yooAssetResourcePackage.InitializeAsync(initParameters);
                    yield return initOperation;

                    if (initOperation.Status == EOperationStatus.Succeed)
                    {
                        Debug.Log("��Դ����ʼ���ɹ���");
                        EnterSangoGameRoot();
                        yield break;
                    }
                    else
                    {
                        Debug.LogError($"��Դ����ʼ��ʧ�ܣ�{initOperation.Error}");
                        yield break;
                    }
                }
        }

        //2.��ȡ��Դ�汾
        var updatePackageVersionOperation = _yooAssetResourcePackage.UpdatePackageVersionAsync();
        yield return updatePackageVersionOperation;

        if (updatePackageVersionOperation.Status != EOperationStatus.Succeed)
        {
            Debug.LogError(updatePackageVersionOperation.Error);
            yield break;
        }
        string packageVersion = updatePackageVersionOperation.PackageVersion;

        //3.���²����嵥
        bool savePackageVersion = true;
        var updatePackageManifestOperation = _yooAssetResourcePackage.UpdatePackageManifestAsync(packageVersion, savePackageVersion);
        yield return updatePackageManifestOperation;

        if (updatePackageManifestOperation.Status != EOperationStatus.Succeed)
        {
            Debug.LogError(updatePackageManifestOperation.Error);
            yield break;
        }

        //4.���ز�����
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
            Debug.Log("û���κ�������Ҫ����Ŷ~");
            return;
        }

        //��Ҫ���ص��ļ��������ܴ�С
        int totalDownloadCount = downloader.TotalDownloadCount;
        long totalDownloadBytes = downloader.TotalDownloadBytes;
        Debug.Log($"�ļ�����:{totalDownloadCount}:::�ܴ�С:{totalDownloadBytes}");
        //ע��ص�����
        downloader.OnDownloadErrorCallback = OnDownloadErrorFunction;
        downloader.OnDownloadProgressCallback = OnDownloadProgressUpdateFunction;
        downloader.OnDownloadOverCallback = OnDownloadOverFunction;
        downloader.OnStartDownloadFileCallback = OnStartDownloadFileFunction;

        _downloaderOperation = downloader;

        _hotFixWindow.OpenHotFixPanel();
        _hotFixWindow.SetHotFixInfoText(totalDownloadBytes);

        Debug.Log("�����Ѿ�׼������������Ŷ~");
    }

    private IEnumerator RunDownloader(ResourceDownloaderOperation downloader)
    {
        //��������
        downloader.BeginDownload();
        _hotFixWindow.SetTips("�������ظ�����");
        yield return downloader;

        //������ؽ��
        if (downloader.Status == EOperationStatus.Succeed)
        {
            Debug.Log("���سɹ�");
            EnterSangoGameRoot();
        }
        else
        {
            Debug.Log("����ʧ��");
        }
    }

    /// <summary>
    /// ��ʼ����
    /// </summary>
    /// <param name="totalDownloadCount"></param>
    /// <param name="currentDownloadCount"></param>
    /// <param name="totalDownloadBytes"></param>
    /// <param name="currentDownloadBytes"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnDownloadProgressUpdateFunction(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
    {
        Debug.Log($"�ļ�����: {totalDownloadCount}, �������ļ����� {currentDownloadCount}, �ܴ�С: {totalDownloadBytes}, �����ش�С: {currentDownloadBytes}");
        float progress = (float)currentDownloadBytes / totalDownloadBytes;
        _hotFixWindow.SetLoadingProgress(progress);
    }

    /// <summary>
    /// ��ʼ����
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="sizeBytes"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnStartDownloadFileFunction(string fileName, long sizeBytes)
    {
        Debug.Log(string.Format("��ʼ���أ��ļ�����{0}, �ļ���С��{1}", fileName, sizeBytes));
    }

    /// <summary>
    /// �������
    /// </summary>
    /// <param name="isSucceed"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnDownloadOverFunction(bool isSucceed)
    {
        Debug.Log("����" + (isSucceed ? "�ɹ�" : "ʧ��"));
    }

    /// <summary>
    /// ���س���
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="error"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnDownloadErrorFunction(string fileName, string error)
    {
        Debug.LogError(string.Format("���س����ļ�����{0}, ������Ϣ��{1}", fileName, error));
    }

    private static byte[] GetAssetData(string dllName)
    {
        return _dllAssetDataDict[dllName];
    }

    /// <summary>
    /// Ϊaot assembly����ԭʼmetadata�� ��������aot�����ȸ��¶��С�
    /// һ�����غ����AOT���ͺ�����Ӧnativeʵ�ֲ����ڣ����Զ��滻Ϊ����ģʽִ��
    /// </summary>
    private static void LoadMetadataForAOTAssemblies()
    {
        // ���Լ�������aot assembly�Ķ�Ӧ��dll����Ҫ��dll������unity build���������ɵĲü����dllһ�£�������ֱ��ʹ��ԭʼdll��
        // ������BuildProcessors������˴�����룬��Щ�ü����dll�ڴ��ʱ�Զ������Ƶ� {��ĿĿ¼}/HybridCLRData/AssembliesPostIl2CppStrip/{Target} Ŀ¼��

        /// ע�⣬����Ԫ�����Ǹ�AOT dll����Ԫ���ݣ������Ǹ��ȸ���dll����Ԫ���ݡ�
        /// �ȸ���dll��ȱԪ���ݣ�����Ҫ���䣬�������LoadMetadataForAOTAssembly�᷵�ش���
        /// 
        HomologousImageMode mode = HomologousImageMode.SuperSet;
        foreach (var aotDllName in AOTMetaAssemblyNames)
        {
            byte[] dllBytes = GetAssetData(aotDllName);
            // ����assembly��Ӧ��dll�����Զ�Ϊ��hook��һ��aot���ͺ�����native���������ڣ��ý������汾����
            LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, mode);
            Debug.Log($"LoadMetadataForAOTAssembly:{aotDllName}. mode:{mode} return:{err}");
        }
    }

    /// <summary>
    /// Զ����Դ��ַ��ѯ������
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
    /// Ĭ�ϵķַ���Դ��ѯ������
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
