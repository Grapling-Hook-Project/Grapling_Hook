using System.Collections;
using UnityEngine;
public class Player_Controller : MonoBehaviour
{
    //PlayerPrefabの指定
    [SerializeField] GameObject player;
    [SerializeField] float moveSpeed = 1;　 //移動速度
    [SerializeField] float limitSpeed = 5f; //制限速度
    [SerializeField] float dowSpeed = 0.9f; //減速
    Rigidbody rigidbody;
    void Start()
    {
        rigidbody = gameObject.GetComponent<Rigidbody>();
    }
    void Update()
    {
        Player_Move();
    }
    void Player_Move()
    {
        //左右のキーの入力を取得
        float x = Input.GetAxis("Horizontal");

        // 上下のキーの入力を取得
        float z = Input.GetAxis("Vertical");

        // カメラの方向から、X-Z平面の単位ベクトルを取得
        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;

        // 方向キーの入力値とカメラの向きから、移動方向を決定
        Vector3 moveForward = cameraForward * z + Camera.main.transform.right * x;

        // 移動方向にスピードを掛ける。ジャンプや落下がある場合は、別途Y軸方向の速度ベクトルを足す。
        rigidbody.velocity = moveForward * moveSpeed + new Vector3(0, rigidbody.velocity.y, 0);

        // キャラクターの向きを進行方向に
        if (moveForward != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveForward);
        }
    }
}