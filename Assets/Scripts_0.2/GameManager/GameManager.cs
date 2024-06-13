using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 预设常量
    // 物理
    public const float physics_gravityMultiply = 1.2f;

    // 摄像机
    // 摄像机视角切换速度
    public const float cameraShiftTime_ToOrtho = 0.5f;
    public const float cameraShiftTime_ToPersp = 1.0f;
    // 摄像机最远距离
    public const float camera_maxDistance = 5;
    // 平视时相机远离的距离
    public const float camera_farDistance = 30;

    // 玩家
    // 玩家移动速度
    public const float player_moveSpeed = 3.0f;
    // 玩家跳跃高度
    public const float player_jumpHeight = 2;
    // 玩家跳跃时间
    public const float player_jumpTime = .5f;
    
    // 玩家动画
    // 玩家旋转速度
    public const float player_rotateSpeed = 10.0f;

    // 视角转换系统
    // 玩家踩踏检测距离增量
    public const float player_stepOffset = 0.1f;
    // 视角转换时，与方块发生重合时，判断重合的权重比（1为完全判断（会过度判断），0为完全不判断）
    public const float player_OverlapPass = 0.5f;

    // 变量
    public static bool isCameraPerspective = true;
    public static int cameraFacingDirection = 4;


    private void Awake()
    {
        // 设定重力
        Physics.gravity = new Vector3(0, Physics.gravity.y * physics_gravityMultiply, 0);

        // 变量数据初始化
        isCameraPerspective = true;
    }

}