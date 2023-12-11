using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentDontDestroy : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
