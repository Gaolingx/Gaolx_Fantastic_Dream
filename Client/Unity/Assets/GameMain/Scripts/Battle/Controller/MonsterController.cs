/****************************************************
	文件：MonsterController.cs
	作者：Plane
	邮箱: 1785275942@qq.com
	日期：2019/03/26 9:20   	
	功能：怪物表现实体角色控制器类
*****************************************************/

using UnityEngine;

namespace DarkGod.Main
{
    public class MonsterController : Controller
    {
        public float MonsterMoveSpeed;
        public bool EnableDownSpeed = false;

        private void Update()
        {
            //AI逻辑表现
            if (isMove)
            {
                SetDir();

                SetMove(EnableDownSpeed);
            }
        }

        private void SetDir()
        {
            float angle = Vector2.SignedAngle(Dir, new Vector2(0, 1));
            Vector3 eulerAngles = new Vector3(0, angle, 0);
            transform.localEulerAngles = eulerAngles;
        }

        private void SetMove(bool downSpeedIfNeed)
        {
            ctrl.Move(transform.forward * Time.deltaTime * MonsterMoveSpeed);
            //给一个向下的速度，便于在没有apply root时怪物可以落地。Fix Res Error
            if (downSpeedIfNeed)
            {
                ctrl.Move(Vector3.down * Time.deltaTime * MonsterMoveSpeed);
            }
        }
    }
}
