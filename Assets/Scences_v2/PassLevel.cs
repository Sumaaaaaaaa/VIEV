using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public class PassLevel : MonoBehaviour
{
    public string nextLevelName;
    public GameObject theUI;

    private void OnTriggerEnter(Collider other) {
        if (other.name!="Player")
        {
            return;
        }
        Time.timeScale = 0;
        Instantiate(theUI,GameObject.Find("Canvas").transform);
        if (nextLevelName == "")
        {
            return;
        }
        StartCoroutine(_ToNextLevel());
        
    }
    private IEnumerator _ToNextLevel()
    {
        yield return new WaitForSecondsRealtime(1);
        SceneManager.LoadScene(nextLevelName);
        Time.timeScale = 1;
    } 
}
