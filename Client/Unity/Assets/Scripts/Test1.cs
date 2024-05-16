using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public class Test1 : MonoBehaviour
    {
        public Transform monsterTrans; //怪物本身位置
        public Transform enemyIndicator; //箭头位置
        public Transform player; //玩家位置

        private Camera mainCamera;
        private Vector3 monsterScreenPos;
        private Vector3 playerScreenPos;
        private Vector3 target; //交点

        private Canvas canvas;
        public RectTransform canvasRectTransform;
        private RectTransform indicatorRectTransform;
        private static float edgeOffset = 50f; //边缘偏移量

        private void Start()
        {
            mainCamera = Camera.main;
            indicatorRectTransform = enemyIndicator.GetComponent<RectTransform>();
            enemyIndicator.gameObject.SetActive(false); //初始时隐藏指示器
        }

        private void Update()
        {
            monsterScreenPos = mainCamera.WorldToScreenPoint(monsterTrans.position);
            playerScreenPos = mainCamera.WorldToScreenPoint(player.position);
            if (UIItemUtils.IsMonsterOnScreen(monsterScreenPos))
            {
                enemyIndicator.gameObject.SetActive(false);
            }
            else
            {
                enemyIndicator.gameObject.SetActive(true);

                //获得焦点
                UIItemUtils.OnLinearAlgebra(enemyIndicator, target, playerScreenPos, monsterScreenPos, edgeOffset);

                //将屏幕坐标转换为Canvas坐标
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, target, mainCamera, out Vector2 localPoint);
                indicatorRectTransform.localPosition = localPoint;

                UIItemUtils.UILookAt(indicatorRectTransform, monsterScreenPos - playerScreenPos, Vector3.up);
            }
        }

    }
}
