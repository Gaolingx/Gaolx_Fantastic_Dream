using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BhvApplication : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
