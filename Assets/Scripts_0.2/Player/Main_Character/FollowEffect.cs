/*
跟随效果
    - 编辑器
        - 跟踪目标(GameObject)：指示性对象，物体将会跟随这个物体的位置移动
        - 比例（AniamtionCurve）:(x:距离,y：跟随速度)

*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowEffect : MonoBehaviour
{
    private GameObject _target;
    [SerializeField]private AnimationCurve _proportion;
    [SerializeField]private float _bounceStartDistance;
    private float _maxSpeed = GameManager.player_moveSpeed;
    // Start is called before the first frame update
    void Start()
    {
        _target = new GameObject($"{this.name}_target");
        if (transform.parent != null) // 如果被赋予跟随特效的 这个游戏对象 有 父物体
        {
            // 把创建的物体放入自己的父对象
            _target.transform.SetParent(transform.parent,false);
            _target.transform.position = transform.position;
            // 就把自己从这个父物体中扔出去
            transform.SetParent(null,true);
        }

        _proportion.preWrapMode = WrapMode.Clamp;
        _proportion.postWrapMode = WrapMode.Clamp;
        //_bounceStartDistance *= _bounceStartDistance; // sqrMagnitude
    }

    // Update is called once per frame
    void Update()
    {
        // 根据自身和_target的位置计算出移动方向
        Vector3 directionRaw = _target.transform.position - transform.position;
        Vector3 direction = directionRaw.normalized;
        //float Length = directionRaw.sqrMagnitude; // sqrMagnitude
        float Length = directionRaw.magnitude; // sqrMagnitude
        float proportion = Length/_bounceStartDistance;
        float speed = _proportion.Evaluate(proportion) * _maxSpeed;

        // 最后移动
        transform.Translate(speed*direction*Time.deltaTime);
    }
}
