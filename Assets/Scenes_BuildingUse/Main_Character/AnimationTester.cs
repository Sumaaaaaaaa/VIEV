using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class AnimationTester : MonoBehaviour
{
    [Range(-12f,12f)][SerializeField]private float front = 0.0f;
    [Range(-12f,12f)][SerializeField]private float right = 0.0f;
    void Update() {
        GetComponent<AnimationController>().Move(new Vector3(right,0,front));
    }
}
