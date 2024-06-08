using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;


#if ENABLE_INPUT_SYSTEM
public class UIController : MonoBehaviour
{

    private InputActionAsset _inputActionAsset;
    private InputActionMap _player;
    private InputAction _point;
    private InputAction _click;
    private InputAction _esc;
    private InputAction _alt;

    public bool isPause = false;
    public bool _isPressingEsc = false;
    public bool _isPressingAlt = false;
    public bool _isActiveInput = false;


    private void Start()
    {
        _inputActionAsset = GetComponent<InputSystemUIInputModule>().actionsAsset;
        _player = _inputActionAsset.FindActionMap("Player");
        _esc = _inputActionAsset.FindActionMap("UI").FindAction("Menu");
        _alt = _inputActionAsset.FindActionMap("UI").FindAction("Alt");

        _esc.Enable();
        _alt.Enable();


    }

    private void Update()
    {
        bool alt = Convert.ToBoolean(_alt.ReadValue<float>());

        if (alt && !_isPressingAlt)
        {
            _isPressingAlt = true;
        }
        else if (!alt)
        {
            _isPressingAlt = false;
        }
        else
        {
            _isPressingEsc = false;
            _isPressingAlt = true;
        }


        //Time.timeScale = _isPause ? 0.0f : 1.0f;
        AudioListener.pause = isPause;

        if (isPause || _isPressingAlt || _isActiveInput)
        {
            _player.Disable();
#if !UNITY_ANDROID
            Cursor.lockState = CursorLockMode.None;
#endif
        }
        else
        {
            _player.Enable();
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