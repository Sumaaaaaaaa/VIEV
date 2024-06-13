/*
    -控制接口
        -相机视角切换
            CameraRotationByController(InputAction.CallbackContext context)
    
    - Unity界面接口
        - Input_Actions：InputActions的Asset
        - Camera_Controller：Camera_Controller组件

*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(PlayerInput))]
public class Controller : MonoBehaviour
{
    // -----------------------------------------接口声明-----------------------------------------
    // 输入系统
    [SerializeField] InputActionAsset inputActions;


    // 相机控制部分
    [SerializeField] Camera_Controller camera_Controller;

    // -----------------------------------------控制接入-----------------------------------------
    public void CameraRotationByController(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            TransformCamera(); // 摄像机视角转换
            ViewTransferTarget.ViewTransfer();
        }
    }

    // 摄像机视角转换
    private void TransformCamera()
    {
        if (GameManager.isCameraPerspective)
        {
            // 转换为正交
            camera_Controller.RotateCameraTo(GameManager.cameraShiftTime_ToOrtho);
            camera_Controller.ToOrtho(GameManager.cameraShiftTime_ToOrtho);
            // 切断控制/时停 + 延时后恢复
            StartCoroutine(_IE_PerspectiveShiftEnable(false));
        }
        else
        {
            // 转换为透视
            camera_Controller.ToPersp(GameManager.cameraShiftTime_ToPersp);
            // 切断控制/时停 + 延时后恢复
            StartCoroutine(_IE_PerspectiveShiftEnable(true));
        }
    }

    // 切断控制/时停 + 延时后恢复
    private IEnumerator _IE_PerspectiveShiftEnable(bool isToPerspective)
    {
        // 时停
        Time.timeScale = 0.0f;
        // 禁用视角变换按键一段时间
        PerspectiveShiftDisable();
        // 改变GameManager中的记录
        GameManager.isCameraPerspective = isToPerspective;
        // 禁用视角控制
        if(!isToPerspective) inputActions.FindAction("View", true).Disable();

        // 延时
        if(isToPerspective)
        {
            yield return _IE_ToPersp();
        }
        else
        {
            yield return _IE_ToOrtho();
        }

        //启用视角控制键
        inputActions.FindAction("PerspectiveShift", true).Enable();
        // 恢复时间流动
        Time.timeScale = 1.0f;
        // 启用视角控制
        if (isToPerspective) inputActions.FindAction("View", true).Enable();
        yield break;
    }
    private IEnumerator _IE_ToPersp()
    {
        yield return new WaitForSecondsRealtime(GameManager.cameraShiftTime_ToPersp);
        yield break;
    }
    private IEnumerator _IE_ToOrtho()
    {
        yield return new WaitForSecondsRealtime(GameManager.cameraShiftTime_ToOrtho);
        yield break;
    }
    private void PerspectiveShiftDisable()
    {
        inputActions.FindAction("PerspectiveShift", true).Disable();
    }
}

