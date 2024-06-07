using UnityEngine;
using YooAsset;
using static System.Net.WebRequestMethods;

//Developer: SangonomiyaSakunovi

public class HotFixConfig : MonoBehaviour
{
    // 资源系统运行模式
    public EPlayMode _ePlayMode = EPlayMode.EditorSimulateMode;

    //CDN地址
    public string hostServerIP = "http://127.0.0.1";
    public string appVersion = "v1.0";

    public bool appendTimeTicks = true;

    #region CDNServerConfig
    /// <summary>
    /// 获取资源服务器地址
    /// </summary>
    public string GetHostServerURL()
    {

#if UNITY_EDITOR
        if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
            return $"{hostServerIP}/CDN/Android/{appVersion}";
        else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
            return $"{hostServerIP}/CDN/IPhone/{appVersion}";
        else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
            return $"{hostServerIP}/CDN/WebGL/{appVersion}";
        else
            return $"{hostServerIP}/CDN/PC/{appVersion}";
#else
		if (Application.platform == RuntimePlatform.Android)
			return $"{hostServerIP}/CDN/Android/{appVersion}";
		else if (Application.platform == RuntimePlatform.IPhonePlayer)
			return $"{hostServerIP}/CDN/IPhone/{appVersion}";
		else if (Application.platform == RuntimePlatform.WebGLPlayer)
			return $"{hostServerIP}/CDN/WebGL/{appVersion}";
		else
			return $"{hostServerIP}/CDN/PC/{appVersion}";
#endif
    }

    public EPlayMode GetEPlayMode()
    {
        return _ePlayMode;
    }

    #endregion
}
