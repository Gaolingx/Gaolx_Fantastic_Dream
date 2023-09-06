/*
 * 相机跟随玩家移动，并控制玩家转向，适合第三人称游戏
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMoveWithPlayer : MonoBehaviour
{
    [Header("相机距离")]
    public float freeDistance = 2;
    [Header("相机最近距离")]
    public float minDistance = 0.5f;
    [Header("相机最远距离")]
    public float maxDistance = 3;
    [Header("是否可控制相机距离(鼠标中键)")]
    public bool canControlDistance = true;
    [Header("更改相机距离的速度")]
    public float distanceSpeed = 1;

    [Header("视角灵敏度")]
    public float rotateSpeed = 1;
    [Header("物体转向插值(灵敏度,取值为0到1)")]
    public float TargetBodyRotateLerp = 0.3f;
    [Header("需要转向的物体")]
    public GameObject TargetBody;//此脚本能操作转向的物体
    [Header("相机焦点物体")]
    public GameObject CameraPivot;//相机焦点物体  

    [Header("是否可控制物体转向")]
    public bool CanControlDirection = true;
    //相机的距离比例，该向量防止相机撞墙
    private float proportion = 1f;

    private Vector3 CubePosition;
    private Vector3 m_rayDirection;

    private RaycastHit m_hit;
    Transform m_transform;
    private Transform target;
    private Ray m_ray;
    private Vector3 offset;//偏移
    private RaycastHit[] _hits = new RaycastHit[64];                       // Start is called before the first frame update

    private void Awake()
    {
        m_transform = transform;
        CubePosition = m_transform.position;
        target = TargetBody.transform;
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        //获取目标与相机之间的差值
        offset = transform.position - CameraPivot.transform.position;
    }

    void FreeCamera()
    {
        if (CanControlDirection)//控制角色方向开关
        {
            Quaternion TargetBodyCurrentRotation = TargetBody.transform.rotation;
            TargetBody.transform.rotation = Quaternion.Lerp(TargetBodyCurrentRotation, Quaternion.Euler(new Vector3(TargetBody.transform.localEulerAngles.x, transform.localEulerAngles.y, TargetBody.transform.localEulerAngles.z)), TargetBodyRotateLerp);

        }

        //控制相机随鼠标的旋转
        float inputY = Input.GetAxis("Mouse Y");
        float inputX = Input.GetAxis("Mouse X");
        transform.RotateAround(CameraPivot.transform.position, Vector3.up, rotateSpeed * inputX);
        transform.RotateAround(CameraPivot.transform.position, TargetBody.transform.right, -rotateSpeed * inputY);

        transform.LookAt(CameraPivot.transform.position);
        //旋转之后以上方向发生了变化,需要更新方向向量
        offset = transform.position - CameraPivot.transform.position;
        offset = offset.normalized * freeDistance * proportion;
        //更新相机的位置
        transform.position = CameraPivot.transform.position + offset;
    }


    private void PreventThroughWall()
    {
        //相机根据物体的位置发射一条反向的的射线
        m_rayDirection = m_transform.position - target.position;
        //将该向量规范化，即向量的模为1
        m_rayDirection.Normalize();
        //从相机的跟随目标向相机发射一条距离为相机默认距离的向量
        m_ray = new Ray(target.position, m_rayDirection * freeDistance);

        //如果可以检测到碰撞物体，并且碰撞物体的tag未"Cube"
        if (Physics.Raycast(m_ray, out m_hit) && m_hit.collider.tag.Equals("Cube"))
        {
            //获取射击点的坐标
            CubePosition = new Vector3(m_hit.point.x, m_hit.point.y, m_hit.point.z);
            //获取射击点与检测点的距离
            float distance = Vector3.Distance(CubePosition, target.position);
            //更新相机的距离比例
            proportion = Mathf.Min(1.0f, distance / freeDistance);
        }
        else
        {
            proportion = 1.0f;
            offset = offset.normalized * freeDistance;
            transform.position = Vector3.Lerp(transform.position, CameraPivot.transform.position + offset, 1f);//更新位置
            if (canControlDistance)
            {
                freeDistance -= Input.GetAxis("Mouse ScrollWheel") * TargetBodyRotateLerp * Time.deltaTime;
                freeDistance = Mathf.Clamp(freeDistance, minDistance, maxDistance);
            }
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        PreventThroughWall();
        FreeCamera();
    }

    // Update is called once per frame
    void Update()
    {
        //PreventThroughWall();
    }
}
