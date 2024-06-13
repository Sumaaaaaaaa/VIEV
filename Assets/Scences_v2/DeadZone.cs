using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public class DeadZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        if (other.name!="Player")
        {
            return;
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
