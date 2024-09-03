using UnityEngine;
using UnityEditor;

public class PrefabInstantiator : MonoBehaviour
{
    [MenuItem("GameObject/CopyTools/CopyInstantiatePrefab %d", false, 10)]
    static void DuplicatePrefab()
    {
        if (Selection.activeGameObject != null)
        {
            Object prefabRoot =
            PrefabUtility.GetCorrespondingObjectFromSource(Selection.activeGameObject);
            if (prefabRoot != null)
            {
                GameObject cloned = (GameObject)PrefabUtility.InstantiatePrefab(prefabRoot);
                cloned.transform.SetParent(Selection.activeGameObject.transform.parent, false);
                cloned.transform.SetSiblingIndex(Selection.activeGameObject.transform.GetSiblingIndex());
                Selection.activeGameObject = cloned;
            }
            else
            {
                Instantiate(Selection.activeGameObject, Selection.activeGameObject.transform.parent);
            }
        }
    }

    [MenuItem("GameObject/CopyTools/CopyInstantiatePrefab %d", true)]
    static bool ValidateDuplicatePrefab()
    {
        // 验证当前选中的是否为GameObject
        return Selection.activeGameObject != null;
    }
}