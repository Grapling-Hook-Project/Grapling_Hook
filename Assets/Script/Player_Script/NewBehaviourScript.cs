using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//[RequireComponent(typeof(Animator), typeof(Rigidbody), typeof(LineRenderer))]
public class NewBehaviourScript : MonoBehaviour
{
    [SerializeField] private float maxhook_shot_distance = 100.0f; //Hook_Shotを伸ばせる最大距離
    [SerializeField] private float wire_shorten = 1.0f;             //ワイヤーの縮めた際の自然長
    [SerializeField] private float spring = 50.0f;                  //ワイヤーの物理的挙動
    [SerializeField] private float damper = 20.0f;                  //ワイヤーの物理的挙動
    [SerializeField] private Vector3 launch_portCenter = new Vector3(0.0f, 0.5f, 0.0f);//Hook_Shotの]発射地点
    [SerializeField] private LayerMask interactiveLayers;           //ワイヤーをくっつけられるレイヤー   
    [SerializeField] private RawImage target_current;               //表示を照準マーク・禁止マークに切り替える
    [SerializeField] private Texture target_marker;                 //照準マーク
    [SerializeField] private Texture target_no_marker;              //禁止マーク

    private bool hook_shot_joint_need_update;//FixedUpdate中でhook_shot_jointの状態更新が必要かどうかを表すフラグ
    private bool hook_shot_current;//ワイヤーを射出中かどうかを表すフラグ
    private float wire_current;//現在のワイヤーの長さ...この値をFixedUpdate中でSpringJointのmaxDistanceにセットする
    private readonly Vector3[] wire_end = new Vector3[2];//Player側と接着点側の末端
    private Vector3 world_launch_port_center;//Hook_Shotをワールド座標に変換したもの

    //スクリプト上でコンポーネントを参照するための宣言
    private Transform cameraTransform;
    private LineRenderer hook_shot_renderer;
    //Spring Jointは二つのオブジェクトをバネのように繋ぐ
    private SpringJoint hook_shot_joint;

    void Awake()
    {
        //それぞれ宣言した変数にUnity上の情報を取得
        this.cameraTransform = Camera.main.transform;
        this.hook_shot_renderer = this.GetComponent<LineRenderer>();
        //launch_portCenterの座標をworld_launch_port_Centerに変換
        this.world_launch_port_center = this.transform.TransformPoint(this.launch_portCenter);
    }
    void Update()
    {
        hook_shot_string();
        Wire_drawing();
    }
    void hook_shot_string()
    {
        //launch_portCenterの座標をworld_launch_port_Centerに変換
        this.world_launch_port_center = transform.TransformPoint(this.launch_portCenter);

        //カメラの前方を取得
        var cameraForward = this.cameraTransform.forward;

        //カメラからレイを飛ばす
        var cameraRay = new Ray(this.cameraTransform.position, cameraForward);

        //そのレイの衝突点に向かうレイを求める。これを糸の射出方向とする
        //{ 原点,Physics.Raycast (Vector3 origin(rayの開始地点), Vector3 direction(rayの向き),float distance(rayの発射距離), int layerMask(レイヤマスクの設定) ? レイがコライダーにヒットした時 ‐ Hooｋ_Shotのワールド座標) : カメラの前方 }
        //PositiveInfinityは無限　Physics.Raycast(cameraRay, out var focus, float.PositiveInfinity, this.interactiveLayers)が存在する場合ヒットしたコライダーとHook_Shotの座標のベクトルを求めレイを飛ばしている。存在しない場合は見ている方向の前方に飛ばす。
        var aimingRay = new Ray(this.world_launch_port_center, Physics.Raycast(cameraRay, out var focus, float.PositiveInfinity, this.interactiveLayers) ? focus.point - this.world_launch_port_center : cameraForward);

        //射出方向のmaxhook_shot_distance以内の距離に接着可能な物体があれば、糸を射出できる
        //(Vector3 origin(rayの開始地点), Vector3 direction(rayの向き),float distance(rayの発射距離), int layerMask(レイヤマスクの設定))
        if (Physics.Raycast(aimingRay, out var aimingTarget, this.maxhook_shot_distance, this.interactiveLayers))
        {
            //レイの表示を照準マークに変更
            //画像変更の際にはtextureを付ける
            this.target_current.texture = this.target_marker;

            //その状態で左クリックが押された場合
            if (Input.GetMouseButtonDown(0))
            {
                //ワイヤーの接着点末端を設定
                this.wire_end[1] = aimingTarget.point;

                //ワイヤー射出中フラグを立てる
                this.hook_shot_current = true;

                //ワイヤーの長さを設定
                this.wire_current = Vector3.Distance(this.world_launch_port_center, aimingTarget.point);

                //hook_shot_jointの更新フラグを立てる
                this.hook_shot_joint_need_update = true;
            }
        }
        else
        {
            //レイの表示を禁止マークに変更
            this.target_current.texture = this.target_no_marker;
        }

        //ワイヤーを射出中の状態でマウスが押されたら
        if (this.hook_shot_current)
        {
            //wire_shortenの長さをまで縮めさせる
            this.wire_current = this.wire_shorten;
            //フラグを立てる
            this.hook_shot_joint_need_update = true;
        }

        //発射ボタンが離された場合
        if (Input.GetMouseButtonUp(0))
        {
            //ワイヤーを射出中フラグをfalseに変更
            this.hook_shot_current = false;
            //hook_shot_jointの更新フラグを立てる
            this.hook_shot_joint_need_update = true;
        }
        //ワイヤーの状態を更新する
        this.WireUpdate();
    }
    private void WireUpdate()
    {
        //ワイヤーをを射出中ならhook_shot_Rendererをアクティブにして描画させ、さもなければ非表示にする
        if (this.hook_shot_renderer.enabled = this.hook_shot_current)
        {
            //ワイヤーを射出中の場合のみ処理を行う
            //ワイヤーのキャラクター側末端を設定
            this.wire_end[0] = this.world_launch_port_center;

            //キャラクターと接着点の間に障害物があるかどうか
            //Linecastは始点と終点を設定してそこに線を引き、コライダーがヒットした場合
            if (Physics.Linecast(this.wire_end[0], this.wire_end[1], out var obstacle, this.interactiveLayers))
            {
                //障害物があれば、接着点を障害物に変更する
                this.wire_end[1] = obstacle.point;
                //(プレイヤー側ワイヤーの末端とワイヤーの先端の距離)、現在のワイヤーの長さの値から最小値だけを返す。
                this.wire_current = Mathf.Min(Vector3.Distance(this.wire_end[0], this.wire_end[1]), this.wire_current);
                //hook_shot_jointの更新フラグを立てる
                this.hook_shot_joint_need_update = true;
            }
            //ワイヤーの描画設定を行う
            this.hook_shot_renderer.SetPositions(this.wire_end);
            //hook_shot_jointのネイピア数を求めている　
            var gbValue = Mathf.Exp(this.hook_shot_joint != null ? -Mathf.Max(Vector3.Distance(this.wire_end[0], this.wire_end[1]) - this.wire_current, 0.0f) : 0.0f);
            //ワイヤーの色を宣言
            var wire_color = new Color(1.0f, gbValue, gbValue);
            //
            this.hook_shot_renderer.startColor = wire_color;
            //
            this.hook_shot_renderer.endColor = wire_color;
        }
    }
    private void Wire_drawing()
    {
        //更新不要なら何もしない
        if (!this.hook_shot_joint_need_update)
        {
            return;
        }
        //射出中かどうかを判定
        if (this.hook_shot_current)
        {
            //射出中で、かつまだhook_shot_jointが張られていなければ張る
            if (this.hook_shot_joint == null)
            {
                this.hook_shot_joint = this.gameObject.AddComponent<SpringJoint>();
                this.hook_shot_joint.autoConfigureConnectedAnchor = false;
                this.hook_shot_joint.anchor = this.launch_portCenter;
                this.hook_shot_joint.spring = this.spring;
                this.hook_shot_joint.damper = this.damper;
            }
            //hook_shot_jointの自然長と接続先を設定する
            this.hook_shot_joint.maxDistance = this.wire_current;
            this.hook_shot_joint.connectedAnchor = this.wire_end[1];
        }
        else
        {
            //射出中でなければhook_shot_jointを削除し、引っぱりを起こらなくする
            Destroy(this.hook_shot_joint);
            this.hook_shot_joint = null;
        }
        //更新が終わったので、「SpringJoint要更新」フラグを折る
        this.hook_shot_joint_need_update = false;
    }
}
