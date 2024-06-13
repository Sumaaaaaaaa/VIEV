using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class PlayerCharacter_FloatEffect : MonoBehaviour
{
    // 旋转
    [SerializeField] Vector3 rotation_DefaultValue;
    [SerializeField] float rotation_sin_speed;
    [SerializeField][Range(0.0f, 1.0f)] float rotation_sin_effectPercent;
    [SerializeField][Range(0.0f,2*Mathf.PI)]float rotation_sin_delay;
    private float _rotation_timeValue = 0.0f;

    //上下浮动
    [SerializeField] float upDown_height;
    [SerializeField] float upDown_speed;
    [SerializeField][Range(0, 2 * Mathf.PI)] float upDown_delay;
    private Vector3 _upDown_preValue;
    private float upDown_timeValue = 0.0f;

    private void Awake()
    {
        _upDown_preValue = transform.localPosition;
        _rotation_timeValue +=rotation_sin_delay;
        upDown_timeValue += upDown_delay;
    }
    private void Update()
    {
        _Rotation();
        _UpDown();
    }
    private void _Rotation()
    {
        // 对sin数值进行计算
        _rotation_timeValue += Time.deltaTime * rotation_sin_speed;
        if (_rotation_timeValue >= 2 * Mathf.PI)
        {
            _rotation_timeValue = 0.0f;
        }
        // 计算被sin值上下调整过的新旋转Vector3
        float sinedVectorPersent = 1 - rotation_sin_effectPercent / 2 + rotation_sin_effectPercent / 2 * Mathf.Sin(_rotation_timeValue);
        Vector3 sinedVector = new Vector3(rotation_DefaultValue.x, rotation_DefaultValue.y, rotation_DefaultValue.z) * sinedVectorPersent;

        // 将数值应用在旋转上
        transform.Rotate(sinedVector * Time.deltaTime, Space.World);

    }
    private void _UpDown()
    {
        // 对sin数值进行计算
        upDown_timeValue += Time.deltaTime * upDown_speed;
        if (upDown_timeValue >= 2 * Mathf.PI)
        {
            upDown_timeValue = 0.0f;
        }

        transform.localPosition = _upDown_preValue +
                                    Mathf.Sin(upDown_timeValue) * Vector3.up * upDown_height;

    }
}
