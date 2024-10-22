using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; set; }

    [SerializeField]
    private CursorLockMode m_cursorLocked = CursorLockMode.None;

    [SerializeField]
    private int m_FrameRate = 60;

    [SerializeField]
    private float m_GameSpeed = 1f;

    [SerializeField]
    private bool m_RunInBackground = true;

    [SerializeField]
    private bool m_NeverSleep = true;

    [SerializeField]
    private bool m_FullScreenMode = true;

    [SerializeField]
    private Vector2 m_ScreenResolution = new Vector2(1280f, 720f);

    private float m_GameSpeedBeforePause = 1f;

    private void Init()
    {
        Cursor.lockState = m_cursorLocked;
        Application.targetFrameRate = m_FrameRate;
        Time.timeScale = m_GameSpeed;
        Application.runInBackground = m_RunInBackground;
        Screen.sleepTimeout = m_NeverSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
        Screen.SetResolution((int)m_ScreenResolution.x, (int)m_ScreenResolution.y, m_FullScreenMode);

        Debug.Log("UIController Init Done.");
    }

    private void Awake()
    {
        Instance = this;

        Init();
    }

    /// <summary>
    /// 获取或设置光标状态。
    /// </summary>
    public CursorLockMode CursorLock
    {
        get => m_cursorLocked;
        set => Cursor.lockState = m_cursorLocked = value;
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
    public bool FullScreen
    {
        get => m_FullScreenMode;
        set
        {
            m_FullScreenMode = value;
            Screen.SetResolution((int)m_ScreenResolution.x, (int)m_ScreenResolution.y, value);
        }
    }

    /// <summary>
    /// 获取或设置屏幕分辨率。
    /// </summary>
    public Vector2 ScreenResolution
    {
        get => m_ScreenResolution;
        set
        {
            m_ScreenResolution = value;
            Screen.SetResolution((int)value.x, (int)value.y, m_FullScreenMode);
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
        Debug.Log("Exit");
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
