using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HorizonMemory : MonoBehaviour
{
    //public Image horizonMemory;
    public RawImage CompassL;
    public RawImage CompassR;
    public Transform PlayerCamera;
    public GameObject up;
    public GameObject down;

    void Update()
    {
        //float speed = 540 + (transform.rotation.x * 690
        //    - horizonMemory.transform.position.y);
        //horizonMemory.transform.Translate(Vector3.up * speed);

        float angle = PlayerCamera.rotation.eulerAngles.x;
        this.CompassL.uvRect = new Rect(0f, (angle / 360f) * -1 - 1f, 1f, 1f);
        this.CompassR.uvRect = new Rect(0f, (angle / 360f) * -1 - 1f, 1f, 1f);

        if (angle >= 180)
        {
            up.SetActive(true);
            down.SetActive(false);
        }
        if (angle < 180)
        {
            up.SetActive(false);
            down.SetActive(true);
        }
    }


}