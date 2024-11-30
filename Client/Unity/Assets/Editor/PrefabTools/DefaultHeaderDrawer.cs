using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[InitializeOnLoad]
public static class DefaultHeaderDrawer
{
    [MenuItem("Tools/Prefab/组件复制/开启快捷复制组件", true)]
    static bool CheckEnableCopy()
    {
        return !EditorPrefs.GetBool("EnableCopyComponent", false);
    }


    [MenuItem("Tools/Prefab/组件复制/开启快捷复制组件", false)]
    static void EnableCopy()
    {
        EditorPrefs.SetBool("EnableCopyComponent", true);
    }


    [MenuItem("Tools/Prefab/组件复制/关闭快捷复制组件", true)]
    static bool CheckDisableCopy()
    {
        return EditorPrefs.GetBool("EnableCopyComponent", false);
    }


    [MenuItem("Tools/Prefab/组件复制/关闭快捷复制组件", false)]
    static void DisableCopy()
    {
        EditorPrefs.SetBool("EnableCopyComponent", false);
    }

    static DefaultHeaderDrawer()
    {
        Editor.finishedDefaultHeaderGUI += OnDefaultHeaderGUI;
    }

    private static void OnDefaultHeaderGUI(Editor editor)
    {
        if (EditorPrefs.GetBool("EnableCopyComponent", false))
        {
            GameObject obj = editor.target as GameObject;
            if (obj)
            {
                EditorGUILayout.BeginHorizontal();
                if (EditorGUILayout.DropdownButton(new GUIContent("复制组件"), FocusType.Passive, "DropDownButton"))
                {
                    ShowComponentList(obj);
                }
                if (GUILayout.Button("粘贴组件"))
                {
                    ComponentUtility.PasteComponentAsNew(obj);
                }
                EditorGUILayout.EndHorizontal();
                return;
            }

            //animation缺少设置Legacy
            AnimationClip clip = editor.target as AnimationClip;
            if (clip)
            {
                GUILayout.Box(GUIContent.none, GUILayout.Height(1f));
                EditorGUI.BeginChangeCheck();
                clip.legacy = EditorGUILayout.Toggle(new GUIContent("Legacy"), clip.legacy);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(clip);
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }

    private static void ShowComponentList(GameObject obj)
    {
        Component[] components = obj.GetComponents(typeof(Component));
        GenericMenu menu = new GenericMenu();
        foreach (var com in components)
        {
            menu.AddItem(new GUIContent(com.GetType().Name), false, () =>
            {
                ComponentUtility.CopyComponent(com);
            });
        }
        menu.ShowAsContext();
    }
}
