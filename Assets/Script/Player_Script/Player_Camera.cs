using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Camera : MonoBehaviour
{
    [SerializeField] Camera mainCamera;//視点
    [SerializeField] Vector2 rotationSpeed;//視点の移動速度
    [SerializeField] GameObject player;
    public bool reverse;//マウス移動方向とカメラ回転方向を反転する判定フラグ
    private Vector2 MousePosition;//マウスの座標
    private Vector2 cameraAngle = new Vector2(1, 1);//カメラの角度
    void Start()
    {
        //リジットボディの取得
        Rigidbody rigidbody = GetComponent<Rigidbody>();

    }
    void Update()
    {
        Camera_rotate();
        Camera_Follow();
    }
    public void Camera_rotate()
    {
        //左クリック時
        if (Input.GetMouseButtonDown(0))
        {
            //視点角度を取得しcameraAngleに格納(transform.localEulerAnglesは回転角の取得が可能)
            cameraAngle = mainCamera.transform.localEulerAngles;
            //マウス座標を取得しMousePositionに格納(Input.mousePositiomで現在のマウス座標の取得が可能)
            MousePosition = Input.mousePosition;
        }
        //左ドラッグしている間
        else 
        {
            if (reverse == false)
            {
                //視点角度に(クリック時の座標とマウス座標の現在値の差分値)*回転速度を代入
                cameraAngle.y -= (MousePosition.x - Input.mousePosition.x) * rotationSpeed.y;
                cameraAngle.x -= (Input.mousePosition.y - MousePosition.y) * rotationSpeed.x;

                //cameraAngleの角度を角度に格納
                mainCamera.transform.localEulerAngles = cameraAngle;

                //マウス座標を変数MousePositionに格納
                MousePosition = Input.mousePosition;
            }
            else if (reverse == true)
            {
                //視点角度に(クリック時の座標とマウス座標の現在値の差分値)*回転速度を代入
                cameraAngle.y -= (Input.mousePosition.x - MousePosition.x) * rotationSpeed.y;
                cameraAngle.x -= (MousePosition.y - Input.mousePosition.y) * rotationSpeed.x;
                //cameraAngleの角度を角度に格納
                mainCamera.transform.localEulerAngles = cameraAngle;
                //マウス座標を変数MousePositionに格納
                MousePosition = Input.mousePosition;
            }
        }
    }
    public void Camera_Follow()
    {
        //プレイヤーの回転
        player.transform.rotation = Quaternion.Euler(player.transform.rotation.x, mainCamera.transform.localEulerAngles.y, player.transform.rotation.z);
        //player.transform.localEulerAngles = new Vector3(player.transform.localEulerAngles.x, mainCamera.transform.localEulerAngles.y, player.transform.localEulerAngles.z); 
    }
    //ドラッグ方向と視点回転方向を反転する処理
    public void DirectionChange()
    {
        if (reverse == false)
        {
            reverse = true;
        }
        else
        {
            reverse = false;
        }
    }
}
