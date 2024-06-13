using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class RotationController : MonoBehaviour
{
    public void Move(Vector3 moveValue)
    {
        
        moveValue.y = 0;
        if (moveValue == Vector3.zero)
        {
            return;
        }
        Quaternion direction = Quaternion.LookRotation(moveValue);
        transform.rotation = Quaternion.Lerp(transform.rotation, direction, GameManager.player_rotateSpeed * Time.deltaTime);
    }
}
