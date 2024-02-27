using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class EnableAssetsPreProcess
{
    [MenuItem("Honkai Star Rail/�ʲ�Ԥ����/�����ʲ�Ԥ����", true)]
    public static bool CheckEnablePreProcess()
    {
        return !EditorPrefs.GetBool("EnableAssetsPreProcess", false);
    }

    [MenuItem("Honkai Star Rail/�ʲ�Ԥ����/�����ʲ�Ԥ����", false)]
    public static void EnablePreProcess()
    {
        EditorPrefs.SetBool("EnableAssetsPreProcess", true);
    }

    [MenuItem("Honkai Star Rail/�ʲ�Ԥ����/�ر��ʲ�Ԥ����", true)]
    public static bool CheckDisablePreProcess()
    {
        return EditorPrefs.GetBool("EnableAssetsPreProcess", false);
    }

    [MenuItem("Honkai Star Rail/�ʲ�Ԥ����/�ر��ʲ�Ԥ����", false)]
    public static void DisablePreProcess()
    {
        EditorPrefs.SetBool("EnableAssetsPreProcess", false);
    }
}
