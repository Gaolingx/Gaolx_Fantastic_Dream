
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Luban;
using SimpleJSON;


namespace cfg
{
public partial class Tbskill
{
    private readonly System.Collections.Generic.Dictionary<int, skill> _dataMap;
    private readonly System.Collections.Generic.List<skill> _dataList;
    
    public Tbskill(JSONNode _buf)
    {
        _dataMap = new System.Collections.Generic.Dictionary<int, skill>();
        _dataList = new System.Collections.Generic.List<skill>();
        
        foreach(JSONNode _ele in _buf.Children)
        {
            skill _v;
            { if(!_ele.IsObject) { throw new SerializationException(); }  _v = skill.Deserializeskill(_ele);  }
            _dataList.Add(_v);
            _dataMap.Add(_v.ID, _v);
        }
    }

    public System.Collections.Generic.Dictionary<int, skill> DataMap => _dataMap;
    public System.Collections.Generic.List<skill> DataList => _dataList;

    public skill GetOrDefault(int key) => _dataMap.TryGetValue(key, out var v) ? v : null;
    public skill Get(int key) => _dataMap[key];
    public skill this[int key] => _dataMap[key];

    public void ResolveRef(Tables tables)
    {
        foreach(var _v in _dataList)
        {
            _v.ResolveRef(tables);
        }
    }

}

}

