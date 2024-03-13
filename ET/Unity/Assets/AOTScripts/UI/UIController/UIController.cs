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

    public bool isMainCitySceneLoad = false;
    public bool isPause = false;
    public string mainCitySceneName = "SceneMainCity";
    public bool _isPressingEsc = false;
    public bool _isPressingAlt = false;


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
        isMainCitySceneLoad = IsSceneLoaded(mainCitySceneName);

        bool esc = Convert.ToBoolean(_esc.ReadValue<float>());
        bool alt = Convert.ToBoolean(_alt.ReadValue<float>());

        if (isMainCitySceneLoad)
        {
            if (esc && !_isPressingEsc)
            {
                isPause = !isPause;
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
        }
        else
        {
            _isPressingEsc = false;
            _isPressingAlt = true;
        }


        //Time.timeScale = _isPause ? 0.0f : 1.0f;
        AudioListener.pause = isPause;

        if (isPause || _isPressingAlt)
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

    bool IsSceneLoaded(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return scene.isLoaded;
    }

}
#endif