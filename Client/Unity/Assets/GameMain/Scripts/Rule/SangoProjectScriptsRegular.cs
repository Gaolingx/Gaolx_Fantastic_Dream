namespace Assets.Scripts.Regular
{
    public class SangoProjectScriptsRegular
    {
        //这是一个特殊的脚本，无法运行，仅用于说明书写规范
        #region 变量定义规则
        //变量命名时，具备get/set权限的public变量均要首字母大写，注意以下两种写法等价，但都不推荐
        public string AccountTest1;     //改写法会导致无法确定工程中正在访问该变量的脚本，会导致重构困难
        public string AccountTest2 { get; set; }    //改写法会导致不易分清get还是set，增加代码出错风险
        //建议使用的写法
        public string AccountTest3 { get; private set; }
        public void SetAccountTest3(string accoutTest3)
        {
            AccountTest3 = accoutTest3;
        }
        //private权限下的私有变量，首字母小写，如上述SetAccount3函数中参数accoutTest3即为该函数私有权限
        //该class私有变量举例
        private string accountTest4;
        //私有变量需要在外界命名，此时修改函数传入参数的命名即可
        private string accountTest5;
        public void SetAccountTest5(string accout)
        {
            accountTest5 = accout;
        }
        //该工程使用命名即注释方案，因此务必注意驼峰命名规范，需要使用尽可能长的单词
        public void SetLocalPlayerTransformCache() { }
        public void SetOnlinePlayerAvater() { }
        //对于缩写问题，只有最后一个单词可以缩写
        public void SetLocalPlayerTrans() { }
        public void SetEnemyTransCache() { }    //该写法会造成理解困难
        //如对上述规则不理解，可以参考函数传参默认值，也是只有最后一个参数可以有默认值
        private string account;
        private bool isOnline;
        public void SetOnlineAccount(string acc, bool bo = true)
        {
            account = acc;
            isOnline = bo;
        }
        //此外有人习惯下述写法，但实际上只要注意命名，这个this指针是可以省略的，注意看this的颜色暗了
        public string AccountTest6 { get; private set; }
        public void SetAccountTest6(string accoutTest4)
        {
            this.AccountTest6 = accoutTest4;
        }
        //不建议使用下述命名写法，这些写法都会造成理解困难
        public string _accountTest8 { get; private set; }
        public string m_accountTest7 { get; private set; }
        private string _accountTest9;
        private string m_accountTest10;
        #endregion

        #region class/struct定义
        //除非特殊情况，不要在class中嵌套class，嵌套的类只能是private权限，注意我们这个SangoProjectScriptsRegular类是public
        private class TestClass1 { }     //该写法正确，表明这个类是私有类，一般用于对象池等特殊情形
        public class TestClass2 { }     //该写法错误，这造成了类嵌套，套娃会导致潜在耦合度立即上升
        //可以使用嵌套的情况为专有定义类，如假设这个类专门定义常用的struct，可以使用下述写法
        public struct TestStruct
        {
            //这里X和Y没有get/set，默认为readonly，等价于 readonly int X
            int X;
            int Y;
            public TestStruct(int x, int y)
            {
                //在构造函数中为该结构体进行赋值
                X = x;
                Y = y;
            }
            //那么问题来了，前面的private set能不能也改成这种写法？答案是能，但是不要这么写，这会造成理解困难
        }
        //定义位置在服务器solution/SangoCommon工程中
        //常量如确定不需要客户端服务器共同访问，定义在Constant文件夹下
        //常量外部存储原则，需要在外部定义为enum或const
        #endregion

        #region 设计模式，调用链需要评估
        //无论客户端还是服务器，少用或不用继承，如果方法定义在父类，需要慎重考虑，尽量引用
        //多用单例模式，理由一：方便引用，理由二：客户端Start会存在调用顺序问题，防止出现奇怪错误无法debug，单例模式如下
        public static SangoProjectScriptsRegular Instance = null;
        public void Init()
        {
            Instance = this;
        }
        //调用链思路
        //客户端发起时：UIWindow => System => Request => 服务器 Handler => System => Cache => System => Handler => 客户端 Request => System => UIWindow
        //服务器发起时：System => Event => 客户端 Event => System => UIWindow
        #endregion
    }
}
