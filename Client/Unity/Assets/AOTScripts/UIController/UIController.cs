using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

#if ENABLE_INPUT_SYSTEM
public class UIController : MonoBehaviour
{
    public static UIController Instance { get; set; }

    [SerializeField]
    private CursorLockMode m_cursorLocked = CursorLockMode.None;

    [SerializeField]
    private Canvas touchZone;

    [SerializeField]
    private int m_FrameRate = 60;

    [SerializeField]
    private float m_GameSpeed = 1f;

    [SerializeField]
    private bool m_RunInBackground = true;

    [SerializeField]
    private bool m_NeverSleep = true;

    private float m_GameSpeedBeforePause = 1f;


    private void Awake()
    {
        Instance = this;

        Cursor.lockState = m_cursorLocked;
        Application.targetFrameRate = m_FrameRate;
        Time.timeScale = m_GameSpeed;
        Application.runInBackground = m_RunInBackground;
        Screen.sleepTimeout = m_NeverSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
    }

    private void Start()
    {
        PECommon.Log("UIController Init Done.");


#if UNITY_ANDROID
        if (touchZone != null)
        {
            touchZone.gameObject.SetActive(true);
        }

#else
        if (touchZone != null)
        {
            touchZone.gameObject.SetActive(false);
        }
#endif
    }

    /// <summary>
    /// ��ȡ�����ù��״̬��
    /// </summary>
    public CursorLockMode CursorLock
    {
        get => m_cursorLocked;
        set => Cursor.lockState = m_cursorLocked = value;
    }

    /// <summary>
    /// ��ȡ��������Ϸ֡�ʡ�
    /// </summary>
    public int FrameRate
    {
        get => m_FrameRate;
        set => Application.targetFrameRate = m_FrameRate = value;
    }

    /// <summary>
    /// ��ȡ��������Ϸ�ٶȡ�
    /// </summary>
    public float GameSpeed
    {
        get => m_GameSpeed;
        set => Time.timeScale = m_GameSpeed = value >= 0f ? value : 0f;
    }

    /// <summary>
    /// ��ȡ��Ϸ�Ƿ���ͣ��
    /// </summary>
    public bool IsGamePaused => m_GameSpeed <= 0f;

    /// <summary>
    /// ��ȡ�Ƿ�������Ϸ�ٶȡ�
    /// </summary>
    public bool IsNormalGameSpeed => Math.Abs(m_GameSpeed - 1f) < 0.01f;

    /// <summary>
    /// ��ȡ�������Ƿ������̨���С�
    /// </summary>
    public bool RunInBackground
    {
        get => m_RunInBackground;
        set => Application.runInBackground = m_RunInBackground = value;
    }

    /// <summary>
    /// ��ȡ�������Ƿ��ֹ���ߡ�
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
    /// ��ͣ��Ϸ��
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
    /// �ָ���Ϸ��
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
    /// ����Ϊ������Ϸ�ٶȡ�
    /// </summary>
    public void ResetNormalGameSpeed()
    {
        if (IsNormalGameSpeed)
        {
            return;
        }

        GameSpeed = 1f;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
#if !UNITY_ANDROID
        Cursor.lockState = CursorLockMode.Locked;
#endif
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
#endif
