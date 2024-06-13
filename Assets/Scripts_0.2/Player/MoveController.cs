/*
    - 控制接口
        - 移动
            OnMove(InputAction.CallbackContext callbackContext)
        - 跳跃
            OnJump(InputAction.CallbackContext callbackContext)

    - 编辑器变量
        - 跳跃曲线动画(0~1)
            _jumpCurve
        - 角色模型对象(GameObject)
            _mainCharacter
*/

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class MoveController : MonoBehaviour
{
    private CharacterController characterController;
    private bool _isJumping;
    private float _jumpValue = 0.0f;

    private void Awake()
    {
        _CharacterControllerSetUp();
        _AnimationSetUp();
    }


    private void Update()
    {
        Vector3 moveVector = new Vector3();

        moveVector += _Move();
        if (!_isJumping)
        {
            moveVector += _GravityFall();
        }
        else
        {
            moveVector += GetJumpValue();
        }
        // CharacterController组件的角色移动
        characterController.Move(moveVector);

        // 动画
        _Animation(moveVector);
    }

    // 重力下落 :: _GravityFall();
    #region 重力下落
    private float _gravityVelocity = 0;
    private float _gravity = Physics.gravity.y * GameManager.physics_gravityMultiply;
    // 重力下落移动
    private Vector3 _GravityFall()
    {
        if (characterController.isGrounded)
        {
            _gravityVelocity = 0;
        }
        _gravityVelocity += _gravity * Time.deltaTime;
        return Vector3.up * _gravityVelocity * Time.deltaTime;
    }
    #endregion

    // 组件接入 :: CharacterControllerSetUp();
    #region 组件接入
    private void _CharacterControllerSetUp()
    {
        characterController = GetComponent<CharacterController>();
    }
    #endregion

    // 控制移动 :: _Move();
    #region 控制移动
    // 控制器的移动会转换为这个数据
    private Vector3 _moveValue;
    // 控制接口
    public void OnMove(InputAction.CallbackContext callbackContext)
    {
        if (GameManager.isCameraPerspective)
        {
            _moveValue = callbackContext.ReadValue<Vector2>();
        }
        else
        {
            _moveValue = new Vector2(callbackContext.ReadValue<Vector2>().x,0);
        }
    }
    private Vector3 _Move()
    {
        return _TransInputByCamera(_moveValue) * Time.deltaTime * GameManager.player_moveSpeed;
    }
    // 将控制输入转换为面对相机的坐标，以完成更具相机角度移动角色
    // 该方法来自https://www.youtube.com/watch?v=7kGCrq1cJew
    private Vector3 _TransInputByCamera(Vector2 inputVector)
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        cameraForward.y = 0.0f;
        cameraRight.y = 0.0f;
        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

        var cameraRelativeMovement = inputVector.y * cameraForward + inputVector.x * cameraRight;
        return cameraRelativeMovement;
    }
    #endregion

    // 跳跃 :: GetJumpValue()
    #region 跳跃 
    // 跳跃动画曲线，控制跳跃动作
    [SerializeField] private AnimationCurve _jumpCurve;
    private Coroutine jumpCoroutine;
    // 控制接口
    public void OnJump(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.performed)
        {
            if (characterController.isGrounded)
            {
                _isJumping = true;
                jumpCoroutine = StartCoroutine(_JumpIEnumerator());
            }
        }
    }
    private Vector3 GetJumpValue()
    {
        return new Vector3(0, _jumpValue, 0);
    }
    // 跳跃协程
    private IEnumerator _JumpIEnumerator()
    {
        float processTime = 0.0f;
        float lastHeight = 0.0f;

        // 每帧执行一次处理
        while (processTime < GameManager.player_jumpTime)
        {
            processTime += Time.deltaTime;
            float ValueThisFrame = _jumpCurve.Evaluate(processTime / GameManager.player_jumpTime) * GameManager.player_jumpHeight;
            _jumpValue = ValueThisFrame - lastHeight;
            lastHeight = ValueThisFrame;
            // 当头部撞到顶部物件时，停止跳跃
            if ((characterController.collisionFlags & CollisionFlags.Above) != 0)
            {
                _isJumping = false;
                jumpCoroutine = null;
                yield break;
            }
            yield return null;
        }
        // 将在跳跃布尔转回false，结束协程。
        _jumpValue = 0;
        _isJumping = false;
        jumpCoroutine = null;
        yield break;
    }
    #endregion

    #region 角色动画接入
    [SerializeField]private GameObject _mainCharacter = null;
    private AnimationController _animationController;
    private RotationController _rotationController;
    private void _AnimationSetUp()
    {
        _animationController = _mainCharacter.GetComponent<AnimationController>();
        _rotationController = _mainCharacter.GetComponent<RotationController>();
    }
    private void _Animation(Vector3 moveValue)
    {
        _animationController.Move(moveValue);
        _rotationController.Move(moveValue);
    }
    # endregion
}
