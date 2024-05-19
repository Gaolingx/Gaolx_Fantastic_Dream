using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    public class Test1 : MonoBehaviour
    {
        public Transform monsterTrans; //���ﱾ��λ��
        public Transform enemyIndicator; //��ͷλ��
        public Transform player; //���λ��

        private Camera mainCamera;
        private Vector3 monsterScreenPos;
        private Vector3 playerScreenPos;
        private Vector3 target; //����

        private Canvas canvas;
        public RectTransform canvasRectTransform;
        private RectTransform indicatorRectTransform;
        private static float edgeOffset = 50f; //��Եƫ����

        private void Start()
        {
            mainCamera = Camera.main;
            indicatorRectTransform = enemyIndicator.GetComponent<RectTransform>();
            enemyIndicator.gameObject.SetActive(false); //��ʼʱ����ָʾ��
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

                //��ý���
                UIItemUtils.OnLinearAlgebra(enemyIndicator, target, playerScreenPos, monsterScreenPos, edgeOffset);

                //����Ļ����ת��ΪCanvas����
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, target, mainCamera, out Vector2 localPoint);
                indicatorRectTransform.localPosition = localPoint;

                UIItemUtils.UILookAt(indicatorRectTransform, monsterScreenPos - playerScreenPos, Vector3.up);
            }
        }

    }
}
