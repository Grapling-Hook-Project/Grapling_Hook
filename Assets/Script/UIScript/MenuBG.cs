using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuBG : MonoBehaviour
{
    public RawImage BackGround;
    private float speed = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        speed -= Time.deltaTime / 30;
        this.BackGround.uvRect = new Rect(speed, 0f, 1f, 1f);
    }
}
