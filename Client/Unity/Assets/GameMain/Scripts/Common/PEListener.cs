/****************************************************
    文件：PEListener.cs
	作者：Plane
    邮箱: 1785275942@qq.com
    日期：2018/12/13 4:30:24
	功能：UI事件监听插件
*****************************************************/

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DarkGod.Main
{
    public class PEListener : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public Action<object> onClick;
        public Action<PointerEventData> onClickDown;
        public Action<PointerEventData> onClickUp;
        public Action<PointerEventData> onDrag;

        public object args;

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke(args);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            onClickDown?.Invoke(eventData);
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            onClickUp?.Invoke(eventData);
        }
        public void OnDrag(PointerEventData eventData)
        {
            onDrag?.Invoke(eventData);
        }

    }
}