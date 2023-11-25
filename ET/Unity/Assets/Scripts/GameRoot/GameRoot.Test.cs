using UnityEngine;

namespace GameMain.Scripts
{
    public partial class GameRoot
    {
        public static string Test1 = "GameRoot::Test1::Static";
        public string Test2 = "GameRoot::Test2::NoStatic";
        public static string Test3 => "GameRoot::Test3::Static func";
        public string Test4 => "GameRoot::Test4::NoStatic func";
    }
}
