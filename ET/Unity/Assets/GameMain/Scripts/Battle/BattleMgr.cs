//功能：战场管理器


using UnityEngine;

public class BattleMgr : MonoBehaviour
{
    private ResSvc resSvc;
    private StateMgr stateMgr;
    private SkillMgr skillMgr;
    private MapMgr mapMgr;

    public void Init(int mapid)
    {
        //初始化服务模块
        resSvc = ResSvc.Instance;

        //初始化各管理器
        stateMgr = gameObject.AddComponent<StateMgr>();
        stateMgr.Init();
        skillMgr = gameObject.AddComponent<SkillMgr>();
        skillMgr.Init();

        //加载战场地图
        MapCfg mapData = resSvc.GetMapCfg(mapid);
        resSvc.AsyncLoadScene(mapData.sceneName, () =>
        {
            //初始化地图数据

        });
    }
}
