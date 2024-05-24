using HybridCLR;
using System;
using System.Collections;
using System.Collections.Generic;
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

    //����Ԫ����dll���б�Yooasset�в���Ҫ����׺
    private static List<string> AOTMetaAssemblyNames { get; } = new List<string>()
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

    //��ȡ��Դ������
    private static Dictionary<string, byte[]> _dllAssetDataDict = new Dictionary<string, byte[]>();

    /// <summary>
	/// ����ģʽ
	/// </summary>
	public EPlayMode PlayMode { private set; get; }

    /// <summary>
    /// �����İ汾��Ϣ
    /// </summary>
    public string PackageVersion { set; get; }

    /// <summary>
    /// ������
    /// </summary>
    public ResourceDownloaderOperation Downloader { set; get; }

    private ResourcePackage _yooAssetResourcePackage;

    public void InitService()
    {
        Instance = this;

        _hotFixConfig = GetComponent<HotFixConfig>();
        _hotFixWindow.SetTips("���ڼ�����");
        PrepareAssets();
    }

    private void PrepareAssets()
    {
        // 1.��ʼ����Դϵͳ
        StartCoroutine(InitYooAsset());

        // 2.��ȡ��Դ�汾
        RequestPackageVersion();

        // 3.���²����嵥
        RequestPackageManifest();

        // 4.���ز�����
        PrepareDownloader();

    }

    private IEnumerator InitYooAsset()
    {
        yield return new WaitForSeconds(1f);

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

        InitializationOperation initOperation = null;
        PlayMode = _hotFixConfig.GetEPlayMode(); // ��Դϵͳ����ģʽ
        switch (PlayMode)
        {
            // �༭���µ�ģ��ģʽ
            case EPlayMode.EditorSimulateMode:
                {
                    var initParameters = new EditorSimulateModeParameters();
                    var simulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(packageName);
                    initParameters.SimulateManifestFilePath = simulateManifestFilePath;
                    initOperation = _yooAssetResourcePackage.InitializeAsync(initParameters);
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
                    initOperation = _yooAssetResourcePackage.InitializeAsync(initParameters);
                    yield return initOperation;

                    if (initOperation.Status == EOperationStatus.Succeed)
                    {
                        Debug.Log("��Դ����ʼ���ɹ���");
                        yield break;
                    }
                    else
                    {
                        Debug.LogError($"��Դ����ʼ��ʧ�ܣ�{initOperation.Error}");
                        yield break;
                    }
                }
            // ��������ģʽ
            case EPlayMode.OfflinePlayMode:
                {
                    var initParameters = new OfflinePlayModeParameters();
                    initOperation = _yooAssetResourcePackage.InitializeAsync(initParameters);
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
            // WebGL����ģʽ
            case EPlayMode.WebPlayMode:
                {
                    string defaultHostServer = _hotFixConfig.GetHostServerURL();
                    string fallbackHostServer = _hotFixConfig.GetHostServerURL();
                    var createParameters = new WebPlayModeParameters();
                    createParameters.BuildinQueryServices = new GameQueryServices();
                    createParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                    initOperation = _yooAssetResourcePackage.InitializeAsync(createParameters);
                    yield return initOperation;

                    if (initOperation.Status == EOperationStatus.Succeed)
                    {
                        Debug.Log("��Դ����ʼ���ɹ���");
                        yield break;
                    }
                    else
                    {
                        Debug.LogError($"��Դ����ʼ��ʧ�ܣ�{initOperation.Error}");
                        yield break;
                    }
                }
            default:
                Debug.LogWarning($"{initOperation.Error}");
                yield break;

        }
    }

    private IEnumerator RequestPackageVersion()
    {
        var updatePackageVersionOperation = _yooAssetResourcePackage.UpdatePackageVersionAsync();
        yield return updatePackageVersionOperation;

        if (updatePackageVersionOperation.Status == EOperationStatus.Succeed)
		{
			HotFixService.Instance.PackageVersion = updatePackageVersionOperation.PackageVersion;
			Debug.Log($"Զ�����°汾Ϊ: {updatePackageVersionOperation.PackageVersion}");
			yield return updatePackageVersionOperation;
		}
		else
		{
			Debug.LogWarning(updatePackageVersionOperation.Error);
		}
    }

    private IEnumerator RequestPackageManifest()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        bool savePackageVersion = true;
        var updatePackageManifestOperation = _yooAssetResourcePackage.UpdatePackageManifestAsync(PackageVersion, savePackageVersion);
        yield return updatePackageManifestOperation;

        if (updatePackageManifestOperation.Status != EOperationStatus.Succeed)
        {
            Debug.LogWarning(updatePackageManifestOperation.Error);
        }
    }

    private IEnumerator PrepareDownloader()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        int downloadingMaxNum = 10;
        int failedTryAgain = 3;
        int timeout = 60;
        var downloader = _yooAssetResourcePackage.CreateResourceDownloader(downloadingMaxNum, failedTryAgain, timeout);
        Downloader = downloader;

        if (downloader.TotalDownloadCount == 0)
        {
            EnterSangoGameRoot();
            Debug.Log("û���κ�������Ҫ����Ŷ~");
        }
        else
        {
            //A total of 10 files were found that need to be downloaded
            Debug.Log($"��Ҫ���ص��ļ�����:{downloader.TotalDownloadCount}:::�ܴ�С:{downloader.TotalDownloadBytes}");

            // �����¸����ļ��󣬹�������ϵͳ
            // ע�⣺��������Ҫ������ǰ�����̿ռ䲻��
            int totalDownloadCount = downloader.TotalDownloadCount;
            long totalDownloadBytes = downloader.TotalDownloadBytes;

            // ע�����ػص�
            RegisterDownloadCallback();

            _hotFixWindow.OpenHotFixPanel();
            _hotFixWindow.SetHotFixInfoText(totalDownloadCount, totalDownloadBytes);
            Debug.Log("�����Ѿ�׼������������Ŷ~");
        }
    }

    #region RunHotFix
    //����������û��ֶ�ִ��
    public void RunHotFix()
    {
        StartCoroutine(RunDownloader(Downloader));
    }

    private IEnumerator RunDownloader(ResourceDownloaderOperation downloader)
    {
        //��������
        downloader.BeginDownload();
        _hotFixWindow.SetTips("�������ظ�����");
        yield return downloader;

        // ������ؽ��
        if (downloader.Status != EOperationStatus.Succeed)
            yield break;
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
    #endregion

    #region EnterGameRoot
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
    #endregion

}
