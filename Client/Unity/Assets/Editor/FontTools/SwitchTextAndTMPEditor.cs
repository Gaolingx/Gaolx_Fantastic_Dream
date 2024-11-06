using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 编辑器工具，在text和tmp组件之间切换
/// </summary>
public class SwitchTextAndTMPEditor : Editor
{
    private const string DefaultTMPFontPath = "Assets/Fonts/AlibabaPuHuiTi/AlibabaPuHuiTi-M SDF.asset";
    private const string DefaultTextFontPath = "Assets/Fonts/AlibabaPuHuiTi/AlibabaPuHuiTi-M.otf";

    [MenuItem("Tools/UI/选中的预制体中TMP转Text")]
    [MenuItem("GameObject/TMP组件工具/Tmp转Text", priority = 201)]
    public static void TMP2Text()
    {
        GameObject[] selectGos = Selection.gameObjects;
        foreach (var gameObj in selectGos)
        {
            TextMeshProUGUI[] allTmp = gameObj.GetComponentsInChildren<TextMeshProUGUI>(true);
            if (allTmp.Length == 0)
            {
                EditorUtility.DisplayDialog("提示", gameObj.name + "预制件下没有Tmp组件", "确定");
                return;
            }
            foreach (var tmp in allTmp)
            {
                GameObject tmpObj = tmp.gameObject;
                string tmpObjName = tmpObj.name;
                if (tmpObjName.Contains("tmp"))
                {
                    string replaceName = tmpObjName.Replace("tmp", "text");
                    tmpObj.name = replaceName;
                }

                TMP_FontAsset tmpFontAsset = tmp.font;
                string tmpText = tmp.text;
                float tmpFontSize = tmp.fontSize;

                Color tmpColor = tmp.color;
                TextAlignmentOptions tmpAlignment = tmp.alignment;

                TextAnchor textAnchor = TextAnchor.MiddleCenter;
                switch (tmpAlignment)
                {
                    case TextAlignmentOptions.Bottom:
                        textAnchor = TextAnchor.MiddleRight;
                        break;
                    case TextAlignmentOptions.BottomLeft:
                        textAnchor = TextAnchor.LowerLeft;
                        break;
                    case TextAlignmentOptions.BottomRight:
                        textAnchor = TextAnchor.LowerRight;
                        break;
                    case TextAlignmentOptions.Top:
                        textAnchor = TextAnchor.MiddleLeft;
                        break;
                    case TextAlignmentOptions.TopLeft:
                        textAnchor = TextAnchor.UpperLeft;
                        break;
                    case TextAlignmentOptions.TopRight:
                        textAnchor = TextAnchor.UpperRight;
                        break;
                }

                DestroyImmediate(tmp, true);

                Text text = tmpObj.AddComponent<Text>();

                Font DefaultFont = AssetDatabase.LoadAssetAtPath<Font>(DefaultTextFontPath);
                text.font = DefaultFont;
                text.text = tmpText;
                text.fontSize = Convert.ToInt32(tmpFontSize);
                text.color = tmpColor;
                text.alignment = textAnchor;
                text.raycastTarget = false;
            }
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("提示", "转换成功", "确定");
        }
    }

    [MenuItem("Tools/UI/选中的预制体将Text转TMP")]
    [MenuItem("GameObject/TMP组件工具/Text转Tmp", priority = 200)]
    public static void AorTextToAorTMP()
    {
        GameObject[] selectGos = Selection.gameObjects;
        foreach (var gameObj in selectGos)
        {
            ChangeTMPName(gameObj.transform);
            Text[] allText = gameObj.GetComponentsInChildren<Text>(true);
            if (allText.Length != 0)
            {
                foreach (var text in allText)
                {
                    GameObject textObj = text.gameObject;
                    string textObjName = textObj.name;
                    if (textObjName.Contains("text"))
                    {
                        string replaceName = textObjName.Replace("text", "tmp");
                        textObj.name = replaceName;
                    }

                    string textText = text.text;
                    int tmpFontSize = text.fontSize;
                    Color tmpColor = text.color;
                    var textAlignment = text.alignment;
                    TextAlignmentOptions tmpAnchor;
                    switch (textAlignment)
                    {
                        case TextAnchor.UpperLeft:
                            tmpAnchor = TextAlignmentOptions.TopLeft;
                            break;
                        case TextAnchor.UpperCenter:
                            tmpAnchor = TextAlignmentOptions.Top;
                            break;
                        case TextAnchor.UpperRight:
                            tmpAnchor = TextAlignmentOptions.TopRight;
                            break;
                        case TextAnchor.MiddleLeft:
                            tmpAnchor = TextAlignmentOptions.Left;
                            break;
                        case TextAnchor.MiddleCenter:
                            tmpAnchor = TextAlignmentOptions.Center;
                            break;
                        case TextAnchor.MiddleRight:
                            tmpAnchor = TextAlignmentOptions.Right;
                            break;
                        case TextAnchor.LowerLeft:
                            tmpAnchor = TextAlignmentOptions.BottomLeft;
                            break;
                        case TextAnchor.LowerCenter:
                            tmpAnchor = TextAlignmentOptions.Bottom;
                            break;
                        case TextAnchor.LowerRight:
                            tmpAnchor = TextAlignmentOptions.BottomRight;
                            break;
                        default:
                            tmpAnchor = TextAlignmentOptions.Center;
                            break;
                    }
                    //如果有Outline那么要先移除掉
                    if (textObj.TryGetComponent<Outline>(out var outline))
                    {
                        DestroyImmediate(outline, true);
                    }

                    DestroyImmediate(text, true);

                    if (!textObj.TryGetComponent<TextMeshProUGUI>(out var tmp))
                    {
                        tmp = textObj.AddComponent<TextMeshProUGUI>();
                    }

                    tmp.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DefaultTMPFontPath); ;

                    tmp.text = textText;
                    tmp.fontSize = tmpFontSize;
                    tmp.color = tmpColor;
                    tmp.alignment = tmpAnchor;
                    tmp.raycastTarget = false;

                    EditorUtility.SetDirty(gameObj);
                }
            }
        }
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("提示", "转换成功", "确定");
    }

    private static void ChangeTMPName(Transform trans)
    {
        TextMeshProUGUI[] all = trans.gameObject.GetComponentsInChildren<TextMeshProUGUI>(true);
        if (all != null && all.Length > 0)
        {
            foreach (var obj in all)
            {
                string a = obj.name.Replace("text", "tmp");
                obj.name = a;
            }
        }
    }
}
