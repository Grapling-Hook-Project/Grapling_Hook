using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
左マウスクリックでワイヤー発射
　  発射中に左マウスを離すとワイヤーが縮む→①
　　発射中に左マウスをクリックすると停止→②
　　発射中に右クリックするとワイヤーを解除→③
 */
public class New_Hook_Contoroller : MonoBehaviour
{
    enum HookShotState
    {
        //未発射の状態
        wireDefault,
        //発射された状態
        wireShot,
        //ワイヤーが縮む状態
        wireReel,
        //ワイヤーの縮が止まった状態
        wireReelStop,
    }

    [SerializeField] GameObject player;
    [SerializeField] GameObject playerArm;
    [SerializeField] bool playerSpeed;
    [SerializeField] float maxDistance = 30.0f;
    [SerializeField] private float spring = 5.0f;
    [SerializeField] private float damper = 1.0f;
    [SerializeField] private Texture targetMarker;
    [SerializeField] private Texture noTargetMarker;
    [SerializeField] private RawImage targetCurrent;
    [SerializeField] private LayerMask wireJudgmentLayers;//ワイヤーを接着可能か判断するレイヤー
    [SerializeField] private Vector3 anchorPosition = new Vector3(0.0f, 0.5f, 0.0f);//ワイヤーの到達地点

    const float shrinkSpeed = 8.5f;
    private float wireDistance;
    private bool wireCurrent;//射出しているかの確認
    private bool springJointUpdate;//springJointの更新が必要かどうかの確認
    private Vector3 worldAnchorPosition;
    private readonly Vector3[] wireEnd = new Vector3[2];//Player側と接着点側の末端
    private Transform playerArmTransform;
    private SpringJoint hookShotJoint;
    private LineRenderer wireRenderer;

    HookShotState keyHookCurrent;
    void Awake()
    {
        playerArmTransform = Camera.main.transform;
        wireRenderer = GetComponent<LineRenderer>();
        targetCurrent.texture = noTargetMarker;
        keyHookCurrent = HookShotState.wireDefault;
        worldAnchorPosition = transform.TransformPoint(anchorPosition);
    }
    void Update()
    {
        HookRay();
        WireJoint();
    }
    private void HookRay()
    {
        var hookForwrad = this.playerArmTransform.forward;

        //腕からRayを発射
        Ray armRay = new Ray(playerArmTransform.position, hookForwrad);

        //レイの衝突点に向かうレイを発射
        Ray aimRay = new Ray(worldAnchorPosition, 
            Physics.Raycast(armRay, out var target, float.PositiveInfinity, wireJudgmentLayers) ? target.point - worldAnchorPosition : hookForwrad);

        if (Physics.Raycast(aimRay, out var targetObj, maxDistance))
        {
            targetCurrent.texture = targetMarker;

            if (Input.GetMouseButton(0))
            {
                //現在の状態の更新
                keyHookCurrent = HookShotState.wireShot;

                wireCurrent = true;
                springJointUpdate = true;

                //ワイヤーの末端設定
                wireEnd[1] = targetObj.transform.position;

                //ワイヤーの長さ設定
                wireDistance = Vector3.Distance(playerArm.transform.position, targetObj.transform.position);
            }
            //①
            if (Input.GetMouseButtonUp(0))
            {
                keyHookCurrent = HookShotState.wireReel;

                anchorPosition = new Vector3(0.0f, 0.5f, 0.0f);
                //anchorPosition = (targetObj.transform.localPosition + anchorPosition);
                if(hookShotJoint!=null)
                hookShotJoint.anchor = anchorPosition;

                KeyHookShot();
            }

            Debug.DrawRay(aimRay.origin, aimRay.direction, Color.red);
        }
        else
        {
            wireCurrent = false;

            targetCurrent.texture = noTargetMarker;

            //現在の状態の更新
            keyHookCurrent = HookShotState.wireDefault;
        }

        if (Input.GetMouseButtonDown(1))
        {
            keyHookCurrent = HookShotState.wireReelStop;
            KeyHookShot();
        }
    }
    private void KeyHookShot()
    {
        //発射中に左マウスを離すとワイヤーが縮む
        if (keyHookCurrent == HookShotState.wireReel)
        {
            //非同期処理
            if (hookShotJoint != null)
                hookShotJoint.connectedAnchor = wireEnd[1];

            StartCoroutine(WireShrink());
            springJointUpdate = true;
        }

        //発射中に左マウスをクリックすると停止
        if (keyHookCurrent == HookShotState.wireReelStop)
        {
            StopCoroutine(WireShrink());
            Destroy(hookShotJoint);
            hookShotJoint = null;
        }
        WireUpdate();
    }
    private IEnumerator WireShrink()
    {
        while (0 < wireDistance)
        {
            //Time.deltaTime : 1 = wireDistance : shrinkSpeed
            wireDistance -= shrinkSpeed * Time.deltaTime;
            wireDistance = Mathf.Max(wireDistance, 0);
            //最大距離を現在の距離に設定
            if (hookShotJoint != null)
                hookShotJoint.maxDistance = wireDistance;
            yield return null;
        }
    }
    private void WireUpdate()
    {
        if (wireRenderer.enabled == wireCurrent)
        {
            //末端をplayerに設定
            wireEnd[0] = playerArmTransform.position;

            //始点と終点間に線を引き、コライダーがヒットした場合
            if (Physics.Linecast(wireEnd[0], wireEnd[1], out var obstacle, wireJudgmentLayers))
            {
                //障害物があれば、接着点を障害物に変更する
                wireEnd[1] = obstacle.transform.position;
                //(プレイヤー側ワイヤーの末端とワイヤーの先端の距離)、現在のワイヤーの長さの値から最小値だけを返す。
                wireDistance = Mathf.Min(Vector3.Distance(wireEnd[0], wireEnd[1]), wireDistance);

                if (gameObject.TryGetComponent<SpringJoint>(out var sj))
                {
                    hookShotJoint = sj;
                }
                else
                {
                    hookShotJoint = gameObject.AddComponent<SpringJoint>();
                    //接続先のアンカーの位置を自動的に計算するべきか
                    hookShotJoint.autoConfigureConnectedAnchor = false;
                    //最終地点の変更
                    hookShotJoint.anchor = anchorPosition;
                    //バネの強さを設定
                    hookShotJoint.spring = spring;
                    //バネの強さを抑える設定
                    hookShotJoint.damper = damper;
                    //最大距離を現在の距離に設定
                    hookShotJoint.maxDistance = wireDistance;
                    //中心点をワイヤーの末端に変更
                    hookShotJoint.connectedAnchor = wireEnd[1];

                }

                hookShotJoint.connectedBody = obstacle.collider.GetComponent<Rigidbody>();
                springJointUpdate = true;
            }
        }
        wireRenderer.SetPositions(this.wireEnd);

        wireRenderer.startColor = Color.black;
        wireRenderer.endColor = Color.yellow;


    }
    private void WireJoint()
    {
        if (!springJointUpdate)
        {
            return;
        }

        if (wireCurrent == true)
        {
            //射出中かつhookShotJointが張られて以内場合
            if (hookShotJoint != null)
            {
            }

        }
        else
        {
            if (hookShotJoint != null)
            {
            }
        }

        springJointUpdate = false;
    }
}
