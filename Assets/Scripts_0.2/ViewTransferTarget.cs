/*
    - 视角转换时碰撞转换对象
        对所有的需要在视角转换时出现转换的对象绑定该脚本

    - 静态公开方法
        - 视角转换触发：更具情况改变所有转换对象
            ViewTransfer()

*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class ViewTransferTarget : MonoBehaviour
{
    // 实例化对象数据
    private ViewTransferTarget father = null;

    // 唯一公开方法
    public static void ViewTransfer()
    {
        // 通过判断知道转换是变为透视还是平视
        if (GameManager.isCameraPerspective/*转换后的结果，True为转为透视*/)
        {

            // 获取所有踩踏的对象
            RaycastHit[] raycastHits = _FindAllGroundedTargetByPlayer();
            // 如果有对象
            if (raycastHits.Length > 0)
            {
                // 比较得到离摄像机最近的平台，并转换玩家到该平台上。
                Vector3 moveValue = _TransferPLayerTo(_GetMostClosedTarget(raycastHits));
                // 将玩家相关的漂浮物也进行传送
                _TransferFloatingObject(moveValue);
            }
            // 销毁所有复制的碰撞框
            _DestoryAllCopiedStage();
            // 打开所有原始对象的碰撞框
            _OpenCollider(_GetOriginalStage());
        }
        else
        {
            // 复制所有碰撞框到主角对象位置
            _CopyStageCollider();
            // 忽略与玩家发生重合的平台对象的碰撞，直到玩家离开重合范围
            _IgnoreOverlap();
            // 关闭所有原始对象的碰撞框
            _CloseCollider(_GetOriginalStage());
        }
    }
    // 将所有的平台对象复制一个平齐于玩家的视角的轴向的碰撞框在其父组下
    static void _CopyStageCollider()
    {

        // 获取所有该脚本对象
        ViewTransferTarget[] scripts = FindObjectsOfType<ViewTransferTarget>();

        // 将所有的平台对象复制一个平齐于玩家的视角的轴向的碰撞框在其父组下
        foreach (ViewTransferTarget script in scripts)
        {
            Vector3 PlayerPosition = GameObject.Find("Player").transform.position;
            // 将玩家的Z坐标和原始XY坐标结合成新坐标，用于复制
            Vector3 originalPosition = script.transform.position;
            // 根据摄像机目标角度选择怎么创建新的位置
            Vector3 newPosition = Vector3.zero;
            if (GameManager.cameraFacingDirection == 0 | GameManager.cameraFacingDirection == 2)
            {
                newPosition = new Vector3(originalPosition.x,
                                        originalPosition.y,
                                        PlayerPosition.z);
            }
            else if (GameManager.cameraFacingDirection == 1 | GameManager.cameraFacingDirection == 3)
            {
                newPosition = new Vector3(PlayerPosition.x,
                                        originalPosition.y,
                                        originalPosition.z);
            }
            // 获取平台对象的旋转信息
            Quaternion rotation = script.transform.rotation;
            // 复制碰撞框到玩家目标坐标位置
            ViewTransferTarget copyTarget = GameObject.Instantiate(script,
                                                                    newPosition,
                                                                    rotation);
            // 修改tag / 关闭显示 / 关联父对象
            copyTarget.tag = "TransferTarget_copy";
            copyTarget.father = script;
            copyTarget.GetComponent<MeshRenderer>().enabled = false;
        }
    }
    // 忽略与玩家发生重合的平台对象的碰撞，直到玩家离开重合范围
    static void _IgnoreOverlap()
    {
        // 重合忽略功能
        GameObject player = GameObject.Find("Player");
        CharacterController playerCharacterController = player.GetComponent<CharacterController>();
        Vector3 center = playerCharacterController.center;
        float height = playerCharacterController.height;
        float radius = playerCharacterController.radius;
        Vector3 point1 = center + player.transform.position + new Vector3(0,height/2,0) * GameManager.player_OverlapPass;
        Vector3 point2 = center + player.transform.position - new Vector3(0,height/2,0) * GameManager.player_OverlapPass;
        Collider[] results = Physics.OverlapCapsule(point1, point2, radius);
        foreach (Collider r in results){
            if(r.CompareTag("TransferTarget_copy"))
            {
                r.GetComponent<Collider>().isTrigger = true;
                r.gameObject.AddComponent<ViewTransferTarget_Overlap>();
            }
        }
    }

    // 获取所有原始平台对象
    static List<ViewTransferTarget> _GetOriginalStage()
    {
        // 创建一个ViewTransferTarget的列表
        List<ViewTransferTarget> originalStage = new List<ViewTransferTarget>();
        // 获取所有该脚本对象
        ViewTransferTarget[] scripts = FindObjectsOfType<ViewTransferTarget>();
        foreach (ViewTransferTarget script in scripts)
        {
            // 若script的tag不为"TransferTarget_copy"，则将他们加入列表
            if (script.tag != "TransferTarget_copy")
            {
                originalStage.Add(script);
            }
        }
        return originalStage;
    }

    // 关闭所有原始平台的碰撞
    static void _CloseCollider(List<ViewTransferTarget> viewTransferTargets)
    {
        foreach (ViewTransferTarget originalStage in viewTransferTargets)
        {
            // 关闭他们的collider
            originalStage.GetComponent<Collider>().enabled = false;
        }
    }


    // 打开所有原始平台的碰撞
    static void _OpenCollider(List<ViewTransferTarget> viewTransferTargets)
    {
        foreach (ViewTransferTarget originalStage in viewTransferTargets)
        {
            // 打开他们的collider
            originalStage.GetComponent<Collider>().enabled = true;
        }
    }
    // 转移玩家到目标平台
    static Vector3 _TransferPLayerTo(ViewTransferTarget viewTransferTarget)
    {
        // 将玩家位置
        GameObject player = GameObject.Find("Player");
        Vector3 playerPosition = player.transform.position + player.GetComponent<CharacterController>().center;
        // 计算出目标位置
        Vector3 targetPosition = viewTransferTarget.transform.position;
        // 计算出玩家和目标之间的向量
        Vector3 moveValue = Vector3.zero;
        if (GameManager.cameraFacingDirection == 0 | GameManager.cameraFacingDirection == 2)
        {
            moveValue = new Vector3(0,
                                        0,
                                        targetPosition.z - playerPosition.z);
            Debug.DrawLine(playerPosition, playerPosition + moveValue, Color.red, 1.0f);
        }
        else if (GameManager.cameraFacingDirection == 1 | GameManager.cameraFacingDirection == 3)
        {
            moveValue = new Vector3(targetPosition.x - playerPosition.x,
                                    0,
                                    0);
            Debug.DrawLine(playerPosition, playerPosition + moveValue, Color.red, 1.0f);

        }
        // 移动玩家
        player.GetComponent<CharacterController>().Move(moveValue);
        return moveValue;
    }

    static void _TransferFloatingObject(Vector3 moveValue)
    {
        // 获取所有的 "Follow Effect"脚本
        var components = FindObjectsOfType<FollowEffect>();
        // 获取到他们的GameObject并变换位置
        foreach (var component in components)
        {
            component.gameObject.transform.position += moveValue;
        }
    }
    // 删除所有复制出的平台
    static void _DestoryAllCopiedStage()
    {
        // 删除所有被复制出的Target
        GameObject[] targets = GameObject.FindGameObjectsWithTag("TransferTarget_copy");
        foreach (GameObject target in targets)
        {
            Destroy(target);
        }
    }

    // 检测玩家所踩到的所有方块
    static RaycastHit[] _FindAllGroundedTargetByPlayer()
    {
        GameObject player = GameObject.Find("Player");
        Vector3 playerPosition = player.transform.position + player.GetComponent<CharacterController>().center;
        Vector3 endPosition = playerPosition - new Vector3(0, player.GetComponent<CharacterController>().height / 2 + GameManager.player_stepOffset, 0);
        float maxDistance = player.GetComponent<CharacterController>().height / 2 + GameManager.player_stepOffset;
        Debug.DrawLine(playerPosition, endPosition, Color.red, 0.0f, false);/*debug*/
        Ray ray = new Ray(player.transform.position, Vector3.down);
        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance);
        return hits;
    }
    // 获取父对象
    static ViewTransferTarget _GetFather(ViewTransferTarget viewTransferTarget)
    {
        print($"_GetFather...{viewTransferTarget.father}");
        return viewTransferTarget.father;
    }
    // 获取最接近玩家的目标平台
    static ViewTransferTarget _GetMostClosedTarget(RaycastHit[] raycastHits)
    {
        float farthestDistance = float.MinValue;
         ViewTransferTarget farthestHit = null;
        foreach (RaycastHit hit in raycastHits)
        {
            // 从 hit 中取出 ViewTransferTarget
            ViewTransferTarget viewTransferTarget = hit.collider.gameObject.GetComponent<ViewTransferTarget>();
            // 如果 ViewTransferTarget 对象中有father，则将对象转换为father
            viewTransferTarget = viewTransferTarget.father;
            Vector3 vectorOfEach = viewTransferTarget.transform.position - Camera.main.transform.position;
            float distance = vectorOfEach.x * Camera.main.transform.forward.x + vectorOfEach.z * Camera.main.transform.forward.z;

            if (distance > farthestDistance)
            {
                farthestHit = viewTransferTarget;
                farthestDistance = distance ;
            }
        }
        return farthestHit;
    }
}
