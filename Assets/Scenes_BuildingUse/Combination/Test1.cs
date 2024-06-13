using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test1 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float v = 5.3f;
        float t = v*Time.deltaTime*transform.localScale.x;
        transform.Translate(0,0,t);
        // v -> t
        // t -> v
        print( t / (Time.deltaTime*transform.localScale.x));

    }
}
