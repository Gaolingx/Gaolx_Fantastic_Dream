using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; set; }

    [SerializeField]
    private bool IsRunningOnSteamDeck = false;

    [SerializeField]
    private bool m_cursorLocked = false;

    [SerializeField]
    private int m_FrameRate = 60;

    [SerializeField]
    private float m_GameSpeed = 1f;

    [SerializeField]
    private bool m_RunInBackground = true;

    [SerializeField]
    private bool m_NeverSleep = true;

    [SerializeField]
    private FullScreenMode m_FullScreenMode = FullScreenMode.FullScreenWindow;

    private (int, int) m_ScreenResolution;

    private float m_GameSpeedBeforePause = 1f;

    private void Init()
    {
        SetCursorState(m_cursorLocked);
        Application.targetFrameRate = m_FrameRate;
        Time.timeScale = m_GameSpeed;
        Application.runInBackground = m_RunInBackground;
        Screen.sleepTimeout = m_NeverSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
        m_ScreenResolution = GetResolution();

        Debug.Log("UIController Info:Init Done.");
    }

    private void Awake()
    {
        Instance = this;

        Init();
    }

    private void OnApplicationFocus(bool focus)
    {
        SetCursorState(m_cursorLocked);
    }

    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }

    private class SettingsResolutionConfigure
    {
        public SettingsResolutionConfigure() { Id = 1; Width = 0; Height = 0; }

        public int Id { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
    private (int, int) GetResolution()
    {
        if (IsRunningOnSteamDeck)
        {
            return (1280, 800);
        }
        SettingsResolutionConfigure resolutionConfigData = new();
        if (resolutionConfigData.Width == 0 && resolutionConfigData.Height == 0 && resolutionConfigData.Id == 1)
        {
            int num = Display.main.systemWidth;
            int num2 = Display.main.systemHeight;
            float num3 = 1.7777778f;
            float num4 = (float)num * 1f / (float)num2;
            if (num4 > num3)
            {
                num = num2 * 16 / 9;
            }
            else if (num4 < num3)
            {
                num2 = num * 9 / 16;
            }
            return (num, num2);
        }
        return (resolutionConfigData.Width, resolutionConfigData.Height);
    }

    /// <summary>
    /// 获取或设置光标状态。
    /// </summary>
    public bool CursorLock
    {
        get => m_cursorLocked;
        set
        {
            SetCursorState(value);
            m_cursorLocked = value;
        }
    }

    /// <summary>
    /// 获取或设置游戏帧率。
    /// </summary>
    public int FrameRate
    {
        get => m_FrameRate;
        set => Application.targetFrameRate = m_FrameRate = value;
    }

    /// <summary>
    /// 获取或设置游戏速度。
    /// </summary>
    public float GameSpeed
    {
        get => m_GameSpeed;
        set => Time.timeScale = m_GameSpeed = value >= 0f ? value : 0f;
    }

    /// <summary>
    /// 获取游戏是否暂停。
    /// </summary>
    public bool IsGamePaused => m_GameSpeed <= 0f;

    /// <summary>
    /// 获取是否正常游戏速度。
    /// </summary>
    public bool IsNormalGameSpeed => Math.Abs(m_GameSpeed - 1f) < 0.01f;

    /// <summary>
    /// 获取或设置是否允许后台运行。
    /// </summary>
    public bool RunInBackground
    {
        get => m_RunInBackground;
        set => Application.runInBackground = m_RunInBackground = value;
    }

    /// <summary>
    /// 获取或设置是否禁止休眠。
    /// </summary>
    public bool NeverSleep
    {
        get => m_NeverSleep;
        set
        {
            m_NeverSleep = value;
            Screen.sleepTimeout = value ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
        }
    }

    /// <summary>
    /// 获取或设置是否全屏。
    /// </summary>
    public FullScreenMode FullScreen
    {
        get => m_FullScreenMode;
        set
        {
            m_FullScreenMode = value;
            Screen.SetResolution(m_ScreenResolution.Item1, m_ScreenResolution.Item2, value);
        }
    }

    /// <summary>
    /// 获取或设置屏幕分辨率。
    /// </summary>
    public (int, int) ScreenResolution
    {
        get => m_ScreenResolution;
        set
        {
            m_ScreenResolution = value;
            Screen.SetResolution(value.Item1, value.Item2, m_FullScreenMode);
        }
    }

    /// <summary>
    /// 暂停游戏。
    /// </summary>
    public void PauseGame()
    {
        if (IsGamePaused)
        {
            return;
        }

        m_GameSpeedBeforePause = GameSpeed;
        GameSpeed = 0f;
    }

    /// <summary>
    /// 恢复游戏。
    /// </summary>
    public void ResumeGame()
    {
        if (!IsGamePaused)
        {
            return;
        }

        GameSpeed = m_GameSpeedBeforePause;
    }

    /// <summary>
    /// 重置为正常游戏速度。
    /// </summary>
    public void ResetNormalGameSpeed()
    {
        if (IsNormalGameSpeed)
        {
            return;
        }

        GameSpeed = 1f;
    }

    public void OnClickExit()
    {
        Debug.Log("UIController Info:Game Exit");
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnApplicationQuit()
    {
        StopAllCoroutines();
    }

}
