using UnityEngine;

//Developer: SangonomiyaSakunovi

public class AOTRoot : MonoBehaviour
{
    public static AOTRoot Instance;

    private void Start()
    {
        Debug.Log("�����ɹ�");
        Instance = this;
        DontDestroyOnLoad(this);
        InitRoot();
    }

    private void InitRoot()
    {
        HotFixService hotFixService = GetComponent<HotFixService>();
        hotFixService.InitService();
    }
}
