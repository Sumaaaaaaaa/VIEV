/*
    - 唯一方法
        - 移动
            Move(Vector3 moveValue)

    - 编辑器变量
        -【重要※】numOfIdleAction: Idle随机动作的数量，在Animator中的前缀为'IdleAction_'
        - _t_IdleAction: Idle随机动作的时间区间。
    
    - Animator
        - 【重要※】当需要添加IdleAction时，必须在动画的结尾处加上event，事件为IdleActionFinish
*/
/*
    走路
        基本速度：5.3
        时间速度区间：0.5~1.5
        算数结果： 2.65~7.95
    跑步
        基本速度：10
        时间速度区间：0.8~1.2
        算数结果： 8~12
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
    // 组件接入
    private Animator animator;

    // 根据Animator需要修改的数据
    [SerializeField]private int numOfIdleAction = 1;

    // 预设时间量
    [SerializeField]private float[] _t_IdleAction = {20.0f, 30.0f};  // Idle状态下随机动作触发时间区间(s)

    // 内部变量
    private bool _isMoving = false;  // 移动状态记录

    // Start is called before the first frame update
    void Start()
    {
        // 绑定组件
        animator = GetComponent<Animator>();

        // 触发 Idle动作 倒计时
        _IdleActionProcess_CountDown();
    }

    
    // 外接移动方法
    public void Move(Vector3 moveValue)
    {
        if (_isMoving ==false && moveValue != Vector3.zero) // 开始移动
        {
            _isMoving = true;
            // 如果移动时正在倒计时，终止倒计时
            _IdleActionProcess_CancelCountDown();
            // 如果移动时正在IdleAction，跳出IdleAction
        }
        else if (_isMoving == true && moveValue == Vector3.zero) // 终止移动
        {
            _isMoving = false;
            // 触发 Idle动作 倒计时
            _IdleActionProcess_CountDown();
        }
        _MoveActionProcess(moveValue);
    }
    # region 移动动画
    private void _MoveActionProcess(Vector3 moveValue)
    {
        /*
            走路
                基本速度：5.3
                时间速度区间：0.5~1.5
                算数结果： 2.65~7.95
            跑步
                基本速度：10
                时间速度区间：0.8~1.2
                算数结果： 8~12

            0 - 静止
            1 - 走路
            2 - 跑步
        */
        // 将移动Vector3量值转换为速度
        
        float speed = new Vector3(moveValue.x,0,moveValue.z).magnitude / Time.deltaTime;
        if (Time.deltaTime==0)
        {
            return;
        }
        // print($"AniationController.........(GET:{moveValue})(Vr:{speed})(Vl:{speed/transform.localScale.x})");
        speed /= transform.localScale.x;
        // 判断行动状态
        if (speed>0 && speed<=8f)
        {
            animator.SetInteger("Action_Index",1);
        }
        else if (speed>8f)
        {
            animator.SetInteger("Action_Index",2);
        }
        else 
        {
            animator.SetInteger("Action_Index",0);
        }
        animator.SetFloat("Walk_Multiplier", speed / 5.3f);
        animator.SetFloat("Run_Multiplier", speed / 10f);
        //animator.SetFloat("Speed",moveValue.magnitude);  /*这个动作会自动的打断正在进行的Idle随即动画*/
    }
    # endregion
    
    # region Idle状态下随机动画
    // 倒计时进入下个Idle随即动画
    private void _IdleActionProcess_CountDown()
    {
        Invoke(nameof(_IdleActionProcess_Trigge),Random.Range(_t_IdleAction[0],_t_IdleAction[1])); 
    }
    
    // 取消倒计时
    private void _IdleActionProcess_CancelCountDown()
    {
        CancelInvoke(nameof(_IdleActionProcess_Trigge));
    }
    
    // 触发Idle随机动画
    private void _IdleActionProcess_Trigge()
    {
            int index = Random.Range(0,numOfIdleAction);
            print($"_InvokeIdleAction.......{index}");
            // 设置动画序列号并触发动画
            animator.SetInteger("IdleAction_Index",index);
            animator.SetTrigger("IdleTrigger");
    }
    
    // [Important!] 外接到Animate Event的用于触发Idle随机动画终止信号
    public void IdleActionFinish()
    {
        Invoke("_IdleActionProcess_Trigge",Random.Range(_t_IdleAction[0],_t_IdleAction[1])); 
    }
    # endregion
}
