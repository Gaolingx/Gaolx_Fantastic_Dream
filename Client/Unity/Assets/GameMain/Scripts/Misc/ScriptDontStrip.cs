using SRUniversal.Main;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class ScriptDontStrip : MonoBehaviour
{
    //防裁剪引用
    [HideInInspector] public NavMeshSurface navMeshSurface;
    [HideInInspector] public SRCharacterRenderingController srCharacterRenderingController;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
