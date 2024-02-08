/****************************************************
    文件：PlayerController.cs
	作者：SIKI学院——Plane
    邮箱: 1785275942@qq.com
    日期：2018/12/14 6:1:47
	功能：角色控制器
*****************************************************/

using UnityEngine;

public class PlayerController : MonoBehaviour {
    private Transform camTrans;
    private Vector3 camOffset;
    public Animator animator;
    public CharacterController characterCtrl;

    private bool isMove = false;
    private Vector2 characterDir = Vector2.zero;
    public Vector2 Dir {
        get {
            return characterDir;
        }

        set {
            if (value == Vector2.zero) {
                isMove = false;
            }
            else {
                isMove = true;
            }
            characterDir = value;
        }
    }

    private float targetBlend;
    private float currentBlend;

    public void Init() {
        camTrans = Camera.main.transform;
        camOffset = transform.position - camTrans.position;
    }

    private void Update() {
        #region InputDebug
        /*
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector2 _dir = new Vector2(h, v).normalized;
        if (_dir != Vector2.zero) {
            Dir = _dir;
            SetBlend(Constants.BlendWalk);
        }
        else {
            Dir = Vector2.zero;
            SetBlend(Constants.BlendIdle);
        }
        */
        #endregion

        //只有当运动状态发生变化时（targetBlend值变化）才调用UpdateMixBlend()平滑动画过渡
        if (currentBlend != targetBlend) {
            UpdateMixBlend();
        }

        if (isMove) {
            //设置方向
            SetDir();
            //产生移动
            SetMove();
            //相机跟随
            SetCam();
        }
    }

    //计算角色移动的方向：获取当前输入的向量，计算它与原来在z轴0度时候之间的夹角，将该夹角产生的偏移量赋值给角色即为角色移动的方向
    private void SetDir() {
        //需要考虑到当前摄像机相对于角色的偏移
        float angle = Vector2.SignedAngle(Dir, new Vector2(0, 1)) + camTrans.eulerAngles.y;
        Vector3 eulerAngles = new Vector3(0, angle, 0);
        transform.localEulerAngles = eulerAngles;
    }

    private void SetMove() {
        characterCtrl.Move(transform.forward * Time.deltaTime * Constants.PlayerMoveSpeed);
    }

    public void SetCam() {
        //并非所有Character（如Monster）都需要摄像机跟随
        if (camTrans != null) {
            camTrans.position = transform.position - camOffset;
        }
    }

    //设定目标的blend值（targetBlend）
    public void SetBlend(float blend) {
        targetBlend = blend;
    }

    //混合blendtree，使动画平滑过渡
    private void UpdateMixBlend() {
        //判断当前currentBlend值与targetBlend值差异
        if (Mathf.Abs(currentBlend - targetBlend) < Constants.AccelerSpeed * Time.deltaTime) {
            currentBlend = targetBlend;
        }
        //如果当前blend值大于目标blend值，说明从运动转向idle状态，让currentBlend逐渐向targetBlend减少
        else if (currentBlend > targetBlend) {
            currentBlend -= Constants.AccelerSpeed * Time.deltaTime; //随着时间（每一帧），让当前的blend值（currentBlend）逐渐向目标的blend值（targetBlend）平滑过渡，直至相等
        }
        else {
            currentBlend += Constants.AccelerSpeed * Time.deltaTime;
        }
        animator.SetFloat("Blend", currentBlend);
    }
}