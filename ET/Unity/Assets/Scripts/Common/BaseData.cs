//功能：配置数据类
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCfg : BaseData<MapCfg>
{
    public string mapName; //地图名称
    public string sceneName; //场景名称
    public Vector3 mainCamPos; //相机位置
    public Vector3 mainCamRote; //相机旋转
    public Vector3 playerBornPos; //玩家出生位置
    public Vector3 playerBornRote;
}


public class BaseData<T>
{
    public int ID;
}

