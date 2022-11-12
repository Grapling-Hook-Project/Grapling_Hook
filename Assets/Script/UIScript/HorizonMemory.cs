using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HorizonMemory : MonoBehaviour
{
    public Image horizonMemory;
    void Update()
    {
        float speed = 540 + (transform.rotation.x * 690
            - horizonMemory.transform.position.y);
        horizonMemory.transform.Translate(Vector3.up * speed);
    }


}