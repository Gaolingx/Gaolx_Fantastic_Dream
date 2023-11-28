using UnityEngine;
using YooAsset;
using static System.Net.WebRequestMethods;

//Developer: SangonomiyaSakunovi

public class HotFixConfig : MonoBehaviour
{
    // 资源系统运行模式
    public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;

    //CDN地址
    public string FOS_MMO_HostServer = "http://127.0.0.1/CNDServer_MMO";
    public string FOS_AR_HostServer = "http://127.0.0.1/CDNServer_AR";
    public string FOS_Moba_HostServer = "http://127.0.0.1/CNDServer_Moba";

    private EPlayMode _ePlayMode = EPlayMode.HostPlayMode;
    private CDNServerModeCode _cndServerMode = CDNServerModeCode.Local;
    public SangoApplicationCode _sangoApplication = SangoApplicationCode.FOS_Moba;

    #region CDNServerConfig
    public string GetCNDServerAddress()
    {
        string cndAddress = "";
        switch (_cndServerMode)
        {
            case CDNServerModeCode.Local:
                switch (_sangoApplication)
                {
                    case SangoApplicationCode.FOS_MMO:
                        cndAddress = FOS_MMO_HostServer;
                        break;
                    case SangoApplicationCode.FOS_AR:
                        cndAddress = FOS_AR_HostServer;
                        break;
                    case SangoApplicationCode.FOS_Moba:
                        cndAddress = FOS_Moba_HostServer;
                        break;
                }
                break;
            case CDNServerModeCode.Remote:
                //TODO
                break;
        }
        return cndAddress;
    }

    public EPlayMode GetEPlayMode()
    {
        return _ePlayMode;
    }

    public enum CDNServerModeCode
    {
        Local,
        Remote
    }

    public enum SangoApplicationCode
    {
        FOS_MMO,
        FOS_AR,
        FOS_Moba
    }
    #endregion
}
