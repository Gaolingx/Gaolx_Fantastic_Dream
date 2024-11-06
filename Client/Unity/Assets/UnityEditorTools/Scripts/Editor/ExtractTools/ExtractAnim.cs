using System.IO;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class ExtractAnim : MonoBehaviour
{
    [MenuItem("Assets/Create/Extract/AnimationClips", false, 1)]
    static void CreateAnimFromFBX()
    {
        UnityEngine.Object[] gameObjects = Selection.objects;
        string[] strs = Selection.assetGUIDs;

        if (gameObjects.Length > 0)
        {
            int gameNum = gameObjects.Length;
            for(int i = 0; i < gameNum; i++)
            {
                string fbxName = gameObjects[i].name;
                string assetPath = AssetDatabase.GUIDToAssetPath(strs[i]);
                //Debug.Log(assetPath); //具体到fbx的路径
                string animFolder = Path.GetDirectoryName(assetPath) + "/Anim";
                // 如果不存在该文件夹则创建一个新的
                if (!AssetDatabase.IsValidFolder(animFolder))
                {
                    AssetDatabase.CreateFolder(Path.GetDirectoryName(assetPath), "Anim");
                }
                // 获取assetPath下所有资源
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                bool isCreate = false;
                List<Object> animation_clip_list = new List<Object>();
                foreach (Object item in assets)
                {
                    if (typeof(AnimationClip) == item?.GetType())//找到fbx里面的动画
                    {
                        Debug.Log("找到动画片段：" + item);
                        if(!item.name.StartsWith("__preview")){
                            animation_clip_list.Add(item);
                        }
                    }
                }
                foreach(AnimationClip animation_clip in animation_clip_list){
                    Object new_animation_clip = new AnimationClip();
                    EditorUtility.CopySerialized(animation_clip, new_animation_clip);
                    new_animation_clip.name = Path.GetFileNameWithoutExtension(assetPath);
                    string animation_path = Path.Combine(animFolder, new_animation_clip.name + ".anim");
                    Debug.Log(animation_path);
                    AssetDatabase.CreateAsset(new_animation_clip, animation_path);
                    
                    isCreate = true;
                }
                //AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.Refresh();
                if (isCreate)
                    Debug.Log("自动创建动画片段成功：" + animFolder);
                else
                    Debug.Log("未自动创建动画片段。");

            }
        }
        else
        {
            Debug.LogError("请选中需要一键提取动画片段的模型");
        }
    }
}