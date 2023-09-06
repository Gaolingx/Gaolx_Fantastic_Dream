using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class FogUtil : MonoBehaviour
{
    public Color frogColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
    public float Density = 0.01f;
    public float fogStart = 0;
    public float fogEnd = 100;   
    public Color SkyColor = new Color(0.6f, 0.7f, 0.7f, 1.0f);
    public float SkyColor_int = 1;    
    public Color EquatorColor = new Color(0.8f, 0.85f, 1.0f, 1.0f);
    public float EquatorColor_int = 1;    
    public Color GroundColor = new Color(0.3f, 0.85f, 0.95f, 1.0f);
    public float GroundColor_int = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RenderSettings.fogColor = frogColor;
        RenderSettings.fogDensity = Density;
        if(RenderSettings.fogMode == FogMode.Linear)
        {
            RenderSettings.fogStartDistance = fogStart;
            RenderSettings.fogEndDistance = fogEnd;
        }
        RenderSettings.ambientEquatorColor = EquatorColor* EquatorColor_int;
        RenderSettings.ambientGroundColor = GroundColor* GroundColor_int;
        RenderSettings.ambientSkyColor = SkyColor* SkyColor_int;               
    }
}
