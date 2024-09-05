
using UnityEngine;
using HuHu;
using System.Collections.Generic;

public class GameBlackboard<K> : Singleton<K> where K : Singleton<K>
{
    protected override void Awake()
    {
        base.Awake();

        GameDataDic.Clear();
    }

    //<T>表示声明一个泛型，但是字段和属性、委托字段都是无法声明泛型T的，只能在类名上面声明，而方法是可以在方法名后面声明<T>,供参数和返回类型使用
    //目前在使用共享角色的数据
    private Dictionary<string, object> GameDataDic = new Dictionary<string, object>();

    protected virtual void SetGameData<T>(string DataName, T value) where T : class
    {
        if (GameDataDic.ContainsKey(DataName))
        {
            GameDataDic[DataName] = value;
        }
        else
        {
            GameDataDic.Add(DataName, value);
        }

    }

    protected virtual T GetGameData<T>(string DataName) where T : class
    {
        if (GameDataDic.TryGetValue(DataName, out var e))
        {
            return e as T;
        }
        return default(T);
        //泛型T的返回类型比object更具有类型的安全性，因为在调用设置方法时需要说明指定的类型，从而直接转换为该类型
        //而用object你需要显式转换：(类型)object，如果这个e根本就不是这个类型就会发生报错
        //这里的T是一个引用类型，那么存入的值也必须是引用类型
        //如果想让T既可以为值、也可以为引用，那么需要if（e is T A）{return A} 来进行转化，as只能支持引用类型的转化
    }
}
