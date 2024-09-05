
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

    //<T>��ʾ����һ�����ͣ������ֶκ����ԡ�ί���ֶζ����޷���������T�ģ�ֻ�������������������������ǿ����ڷ�������������<T>,�������ͷ�������ʹ��
    //Ŀǰ��ʹ�ù����ɫ������
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
        //����T�ķ������ͱ�object���������͵İ�ȫ�ԣ���Ϊ�ڵ������÷���ʱ��Ҫ˵��ָ�������ͣ��Ӷ�ֱ��ת��Ϊ������
        //����object����Ҫ��ʽת����(����)object��������e�����Ͳ���������;ͻᷢ������
        //�����T��һ���������ͣ���ô�����ֵҲ��������������
        //�������T�ȿ���Ϊֵ��Ҳ����Ϊ���ã���ô��Ҫif��e is T A��{return A} ������ת����asֻ��֧���������͵�ת��
    }
}
