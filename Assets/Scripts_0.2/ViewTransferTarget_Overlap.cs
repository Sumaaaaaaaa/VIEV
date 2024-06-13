using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ViewTransferTarget_Overlap : MonoBehaviour
{
    private void OnTriggerExit(Collider other) {
        if (other.name == "Player")
        {
            GetComponent<Collider>().isTrigger = false;
            Destroy(this);
        }
    }
}
