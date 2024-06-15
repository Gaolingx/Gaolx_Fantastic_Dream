using DarkGod.Main;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

#if ENABLE_INPUT_SYSTEM
public class UIController : MonoBehaviour
{
    public Canvas menu;
    public Canvas touchZone;

    private EventSystem eventSystem;
    private InputActionAsset _inputActionAsset;
    private InputActionMap _player;
    private InputAction _point;
    private InputAction _click;
    private InputAction _esc;
    private InputAction _alt;

    public bool _isPause = false;
    public bool _isInputEnable = true;
    public bool _isPressingEsc = false;
    public bool _isPressingAlt = false;

    private void Start()
    {
        eventSystem = GameObject.Find(Constants.EventSystemGOName).GetComponent<EventSystem>();
        _inputActionAsset = eventSystem.GetComponent<InputSystemUIInputModule>().actionsAsset;
        _player = _inputActionAsset.FindActionMap("Player");
        _esc = _inputActionAsset.FindActionMap("UI").FindAction("Menu");
        _alt = _inputActionAsset.FindActionMap("UI").FindAction("Alt");

        _esc.Enable();
        _alt.Enable();


#if UNITY_ANDROID
        if (touchZone != null)
        {
            touchZone.gameObject.SetActive(true);
        }

        Application.targetFrameRate = 60;
#else
        if (touchZone != null)
        {
            touchZone.gameObject.SetActive(false);
        }
#endif
    }

    bool esc = false;
    bool alt = false;
    private void Update()
    {
        if (_esc != null)
        {
            esc = Convert.ToBoolean(_esc.ReadValue<float>());
        }
        if (_alt != null)
        {
            alt = Convert.ToBoolean(_alt.ReadValue<float>());
        }

        if (esc && !_isPressingEsc)
        {
            //_isPause = !_isPause;
            _isPressingEsc = true;
        }
        else if (!esc)
        {
            _isPressingEsc = false;
        }

        if (alt && !_isPressingAlt)
        {
            _isPressingAlt = true;
        }
        else if (!alt)
        {
            _isPressingAlt = false;
        }

        //Time.timeScale = _isPause ? 0.0f : 1.0f;
        AudioListener.pause = _isPause;
        if (menu != null)
        {
            menu.gameObject.SetActive(_isPause);
        }
#if UNITY_ANDROID
        if (touchZone != null)
        {
            touchZone.gameObject.SetActive(!_isPause);
        }
#endif

        if (_isPause || _isPressingAlt || GameRoot.Instance.GetGameState() == GameState.Login)
        {
            //_player.Disable();
#if !UNITY_ANDROID
            Cursor.lockState = CursorLockMode.None;
#endif
        }
        else
        {
            //_player.Enable();
#if !UNITY_ANDROID
            Cursor.lockState = CursorLockMode.Locked;
#endif
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
#if !UNITY_ANDROID
        Cursor.lockState = CursorLockMode.Locked;
#endif
    }

    public void OnValueChangedVSync(Int32 value)
    {
        QualitySettings.vSyncCount = value;
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


}
#endif
