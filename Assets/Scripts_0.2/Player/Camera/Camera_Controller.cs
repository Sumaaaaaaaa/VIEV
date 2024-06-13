/*
 *  摄像机控制器
 *
 *      - 方法
 *          - 在time时间内旋转摄像机到最近平面方向
 *              RotateCameraTo(float time)

 *
 *          - 在time时间内转换摄像机的透视到 透视
 *              public void ToPersp(float time)
 *          - 在time时间内转换摄像机的透视到 正交
 *              public void ToOrtho(float time)

            - 在time时间内移动相机到一个远的不会切割平台对象的距离
                public void ToFar(float time)
            - 在time时间内移动相机到基本距离，当出现碰撞时候，移动到更近的距离
                public void ToClose(float time)

 *  
 *      - 控制器接口
 *          - 视角控制
 *              View(InputAction.CallbackContext context)
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Camera_Controller : MonoBehaviour
{
    // 子相机
    private GameObject _subCamera;


    private void Awake()
    {
        _Cameramera_PerspectiveSwitcher_SetUp();
        _MoveCameraByColliders_Setup();
    }
    private void Update()
    {
        _ControllerFrameUpdate(); /*控制器部分*/
    }

    #region 控制器部分
    private bool _isControlling = false;
    private Vector2 _controlGetValue;
    // 每帧一次更新，以保证手柄的输入控制能够正常运行
    private void _ControllerFrameUpdate()
    {
        if (_isControlling)
        {
            // 根据输入旋转相机
            PerspectiveRotate(_controlGetValue);
            // 根据碰撞体移动相机，以便不会卡视角
            MoveCameraByColliders();
        }
    }
    // 视角控制的输入接口
    public void View(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _isControlling = true;
            _controlGetValue = context.ReadValue<Vector2>();
        }

        if (context.canceled)
        {
            _isControlling = false;
        }
    }

    // 通过对这个GameObject的旋转控制，完成摄像机的绕角色旋转
    private void PerspectiveRotate(Vector2 inputValue)
    {
        // 将输入的控制器Vector2加到当前的旋转上，得到一个结果
        Vector3 result = transform.rotation.eulerAngles
                    + new Vector3(-inputValue.y, inputValue.x, 0.0f);
        // 当这个结果超过预设值时，会发生视角错乱，所以限制住它的 x 部分
        if (result.x < 270 && result.x > 90)
        {
            result.x = transform.rotation.eulerAngles.x;
        }
        // 将最终的结果实现在摄像机旋转上
        transform.rotation = Quaternion.Euler(result);
    }
    // 碰撞前移相机_设定
    private void _MoveCameraByColliders_Setup()
    {
        _subCamera = transform.GetChild(0).gameObject;
        //transform.GetChild(0).transform.position.z;
    }
    // 碰撞前移相机
    private void MoveCameraByColliders()
    {
        Ray ray = new Ray(transform.position, -transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, GameManager.camera_maxDistance))
        {
            _subCamera.transform.position = hit.point;
        }
        else
        {
            _subCamera.transform.transform.localPosition = Vector3.back * GameManager.camera_maxDistance;
        }
    }
    #endregion

    #region 事件部分（在时间内改变视角）
    private static readonly Vector3[] _targetDirection = { new Vector3(0, 0, 0),
                                                                new Vector3(0, 90, 0),
                                                                new Vector3(0, 180, 0),
                                                                new Vector3(0, -90, 0)
                                                            };
    private IEnumerator enumerator = null;

    // 公开方式，在time时间内旋转摄像机到最近平面方向
    public void RotateCameraTo(float time)
    {
        if (enumerator is null)
        {
            enumerator = _Rotate(_GetFacingDirection(), time);
            // 修改GameManager中的视角状态描述，用于ViewTransferTarget生成碰撞。
            GameManager.cameraFacingDirection = _GetFacingDirection();
            StartCoroutine(enumerator);
        }
    }
    // 协程
    private IEnumerator _Rotate(int cameraDirection, float time)
    {
        Quaternion preCameraRotation = Quaternion.Euler(transform.eulerAngles);
        Quaternion targetRotation = Quaternion.Euler(_targetDirection[cameraDirection]);
        float timeNow = 0.0f;

        //  每帧唤醒一次，进行对位置的
        while (timeNow < time)
        {
            timeNow += Time.unscaledDeltaTime;
            transform.rotation = Quaternion.Slerp(preCameraRotation, targetRotation, timeNow / time);
            yield return null;
        }
        // 当时间超过设定的过渡时间
        // 消除旋转上可能出现的误差
        transform.rotation = targetRotation;
        // 关闭这个协程
        StopCoroutine(enumerator);
        // 将对象清空为Null，以让判断可以重新启动协程
        enumerator = null;
        yield break;
    }
    // 判断相机最近的可转换平面是哪个
    private int _GetFacingDirection()
    {
        float angle = transform.rotation.eulerAngles.y;
        if (angle <= 45 | angle > 315)
        {
            return 0;
        }
        else if (angle > 45 & angle <= 135)
        {
            return 1;
        }
        else if (angle > 135 & angle <= 225)
        {
            return 2;
        }
        else
        {
            return 3;
        }
    }
    #endregion

    #region 接入Camera中的透视变换
    private Camera_PerspectiveSwitcher _camera_PerspectiveSwitcher;
    private void _Cameramera_PerspectiveSwitcher_SetUp()
    {
        _camera_PerspectiveSwitcher = transform.GetChild(0).GetComponent<Camera_PerspectiveSwitcher>();
        /*DEBUG*/
        if (_camera_PerspectiveSwitcher is null)
        {
            Debug.LogError("_camera_PerspectiveSwitcher didn't get !!!!");
        }
    }
    public void ToPersp(float time)
    {
        _camera_PerspectiveSwitcher.ToPersp(time);
        _ToClose(time);
    }
    public void ToOrtho(float time)
    {
        _camera_PerspectiveSwitcher.ToOrtho(time);
        _ToFar(time);
    }
    # endregion

    # region 相机的近远变换
    private void _ToFar(float time)
    {
        StartCoroutine(_IE_ToFar(time));
    }
    private IEnumerator _IE_ToFar(float time)
    {
        yield return new WaitForSecondsRealtime(GameManager.cameraShiftTime_ToOrtho);
        _subCamera.transform.localPosition = Vector3.back * GameManager.camera_farDistance;
        yield break;
    }
    private void _ToClose(float time)
    {
        _subCamera.transform.localPosition = Vector3.back * GameManager.camera_maxDistance;
    }
    #endregion
}
