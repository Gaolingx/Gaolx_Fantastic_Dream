using UnityEngine;

//Developer: SangonomiyaSakunovi

public class AOTRoot : MonoBehaviour
{
    public static AOTRoot Instance;

    private void Start()
    {
        Debug.Log("Æô¶¯³É¹¦");
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
