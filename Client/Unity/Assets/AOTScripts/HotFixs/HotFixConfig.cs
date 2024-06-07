using UnityEngine;
using YooAsset;
using static System.Net.WebRequestMethods;

//Developer: SangonomiyaSakunovi

public class HotFixConfig : MonoBehaviour
{
    // ��Դϵͳ����ģʽ
    public EPlayMode _ePlayMode = EPlayMode.EditorSimulateMode;

    //CDN��ַ
    public string hostServerIP = "http://127.0.0.1";
    public string appVersion = "v1.0";

    public bool appendTimeTicks = true;

    #region CDNServerConfig
    /// <summary>
    /// ��ȡ��Դ��������ַ
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
