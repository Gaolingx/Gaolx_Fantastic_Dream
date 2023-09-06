#if UNITY_EDITOR

using MonKey.Editor.Internal;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class MonKeyInitialization : MonoBehaviour
{
    public static readonly string MonKeyVersion = "2023.0.1";

    /// <summary>
    /// used in case you change the version of Unity, 
    /// so that you can access all the functionalities without problems
    /// </summary>
    public static void InitMonKey()
    {
        MonKeySettings.InitSettings();
        CommandManager.Instance.RetrieveAllCommands();
    }

    [DidReloadScripts]
    public static void InitAndShowStartupPanel()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        if (Application.isBatchMode)
            return;
        
        InitMonKey();
        GettingStartedPanel.OpenPanelFirstTime();
    }

    [InitializeOnLoadMethod]
    public static void OpenPanel()
    {
        InitAndShowStartupPanel();
    }
}
#endif