using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Animator、Rigidbody、LineRendererを必須としている
// Animator...糸を射出しているかどうかに応じて右手を前に出したり戻したりするのに使用
// Rigidbody...スクリプト中で直接操作してはいないが、SpringJointの動作に必要
// LineRenderer...糸を画面上に描画するために使用
[RequireComponent(typeof(Animator), typeof(Rigidbody), typeof(LineRenderer))]
public class PlayerData : MonoBehaviour
{
    #region 変数
    [SerializeField, Tooltip("糸を伸ばせる最大距離")] private float maxDis = 100.0f;
    [SerializeField] private LayerMask interactiveLayers; // 糸をくっつけられるレイヤー
    [SerializeField] private Vector3 casterCenter = new Vector3(0.0f, 0.5f, 0.0f); // オブジェクトのローカル座標で表した糸の射出位置
    [SerializeField, Tooltip("SpringJointのspring")] private float spring = 50.0f; // 糸の物理的挙動を担当するSpringJointのspring
    [SerializeField, Tooltip("SpringJointのdamper")] private float damper = 20.0f; // 糸の物理的挙動を担当するSpringJointのdamper
    [SerializeField, Tooltip("糸を縮めた時の自然長")] private float equilibriumLength = 1.0f; // 糸を縮めた時の自然長
    [SerializeField, Tooltip("腕位置の遷移時間")] private float ikTransitionTime = 0f; // 糸の射出中に右手を前に伸ばしたり、糸を外した時に右手を戻したりする時の腕位置の遷移時間
    [SerializeField, Tooltip("照準マーク・禁止マークに切り替える")] private RawImage reticle; // 糸を張れるかどうかの状況に合わせて、このRawImageの表示を照準マーク・禁止マークに切り替える
    [SerializeField, Tooltip("照準マーク")] private Texture reticleImageValid; // 照準マーク
    [SerializeField, Tooltip("禁止マーク")] private Texture reticleImageInvalid; // 禁止マーク

    // 各種コンポーネントへの参照
    private Animator animator;
    private Transform cameraTransform;
    private LineRenderer lineRenderer;
    private SpringJoint springJoint;

    // 右手を伸ばす・戻す動作のスムージングのための...
    private float currentIkWeight; // 現在のウェイト
    private float targetIkWeight; // 目標ウェイト
    private float ikWeightVelocity; // ウェイト変化率

    private bool casting; // 糸が射出中かどうかを表すフラグ
    private bool needsUpdateSpring; // FixedUpdate中でSpringJointの状態更新が必要かどうかを表すフラグ
    private float stringLength; // 現在の糸の長さ...この値をFixedUpdate中でSpringJointのmaxDistanceにセットする
    private readonly Vector3[] stringAnchor = new Vector3[2]; // SpringJointのキャラクター側と接着点側の末端
    private Vector3 worldCasterCenter; // casterCenterをワールド座標に変換したもの


    #endregion

    private void Awake()
    {
        // スクリプト上で使用するコンポーネントへの参照を取得する
        animator = this.GetComponent<Animator>();
        cameraTransform = Camera.main.transform;
        lineRenderer = this.GetComponent<LineRenderer>();

        // worldCasterCenterはUpdate中でも毎回更新しているが、Awake時にも初回更新を行った
        // ちなみに今回のキャラクターの場合は、キャラクターのCapsuleCollider中心と一致するようにしている
        worldCasterCenter = this.transform.TransformPoint(this.casterCenter);
    }

    private void Update()
    {
        // まず画面中心から真っ正面に伸びるRayを求め、さらにworldCasterCenterから
        // そのRayの衝突点に向かうRayを求める...これを糸の射出方向とする
        this.worldCasterCenter = this.transform.TransformPoint(this.casterCenter);
        var cameraForward = this.cameraTransform.forward;
        var cameraRay = new Ray(this.cameraTransform.position, cameraForward);
        var aimingRay = new Ray(
            this.worldCasterCenter,
            Physics.Raycast(cameraRay, out var focus, float.PositiveInfinity, this.interactiveLayers)
                ? focus.point - this.worldCasterCenter
                : cameraForward);

        // 射出方向のmaximumDistance以内の距離に糸接着可能な物体があれば、糸を射出できると判断する
        if (Physics.Raycast(aimingRay, out var aimingTarget, this.maxDis, this.interactiveLayers))
        {
            // reticleの表示を照準マークに変え...
            this.reticle.texture = this.reticleImageValid;

            // その状態で糸発射ボタンが押されたら...
            if (Input.GetMouseButtonDown(0))
            {
                this.stringAnchor[1] = aimingTarget.point; // 糸の接着点末端を設定
                this.casting = true; // 「糸を射出中」フラグを立てる
                this.targetIkWeight = 1.0f; // IK目標ウェイトを1にする...つまり右手を射出方向に伸ばそうとする
                this.stringLength = Vector3.Distance(this.worldCasterCenter, aimingTarget.point); // 糸の長さを設定
                this.needsUpdateSpring = true; // 「SpringJoint要更新」フラグを立てる
            }
        }
        else
        {
            // 糸接着不可能なら、reticleの表示を禁止マークに変える
            this.reticle.texture = this.reticleImageInvalid;
        }

        // 糸を射出中の状態で糸収縮ボタンが押されたら、糸の長さをequilibriumLengthまで縮めさせる
        if (this.casting && Input.GetMouseButtonDown(1))
        {
            this.stringLength = this.equilibriumLength;
            this.needsUpdateSpring = true;
        }

        // 糸発射ボタンが離されたら...
        if (Input.GetMouseButtonDown(0))
        {
            this.casting = false; // 「糸を射出中」フラグを折る
            this.targetIkWeight = 0.0f; // IK目標ウェイトを0にする...つまり右手を自然姿勢に戻そうとする
            this.needsUpdateSpring = true; // 「SpringJoint要更新」フラグを立てる
        }

        // 右腕のIKウェイトをなめらかに変化させる
        this.currentIkWeight = Mathf.SmoothDamp(
            this.currentIkWeight,
            this.targetIkWeight,
            ref this.ikWeightVelocity,
            this.ikTransitionTime);

        // 糸の状態を更新する
        this.UpdateString();
    }

    private void UpdateString()
    {
        // 糸を射出中ならlineRendererをアクティブにして糸を描画させ、さもなければ非表示にする
        if (this.lineRenderer.enabled = this.casting)
        {
            // 糸を射出中の場合のみ処理を行う
            // 糸のキャラクター側末端を設定し...
            this.stringAnchor[0] = this.worldCasterCenter;

            // キャラクターと接着点の間に障害物があるかをチェックし...
            if (Physics.Linecast(
                this.stringAnchor[0],
                this.stringAnchor[1],
                out var obstacle,
                this.interactiveLayers))
            {
                // 障害物があれば、接着点を障害物に変更する
                // これにより、糸が何かに触れればそこにくっつくようになるので
                // 糸全体が粘着性があるかのように振る舞う
                this.stringAnchor[1] = obstacle.point;
                this.stringLength = Mathf.Min(
                    Vector3.Distance(this.stringAnchor[0], this.stringAnchor[1]),
                    this.stringLength);
                this.needsUpdateSpring = true;
            }

            // 糸の描画設定を行う
            // 糸の端点同士の距離とstringLengthとの乖離具合によって糸を赤く塗る
            // つまり糸が赤くなっていれば、SpringJointが縮もうとしていることを示す
            this.lineRenderer.SetPositions(this.stringAnchor);
            var gbValue = Mathf.Exp(
                this.springJoint != null
                    ? -Mathf.Max(Vector3.Distance(this.stringAnchor[0], this.stringAnchor[1]) - this.stringLength, 0.0f)
                    : 0.0f);
            var stringColor = new Color(1.0f, gbValue, gbValue);
            this.lineRenderer.startColor = stringColor;
            this.lineRenderer.endColor = stringColor;
        }
    }

    // 右腕の姿勢を設定し、右腕から糸を出しているように見せる
    private void OnAnimatorIK(int layerIndex)
    {
        this.animator.SetIKPosition(AvatarIKGoal.RightHand, this.stringAnchor[1]);
        this.animator.SetIKPositionWeight(AvatarIKGoal.RightHand, this.currentIkWeight);
    }

    // SpringJointの状態を更新する
    private void FixedUpdate()
    {
        // 更新不要なら何もしない
        if (!this.needsUpdateSpring)
        {
            return;
        }

        // 糸射出中かどうかを判定し...
        if (this.casting)
        {
            // 射出中で、かつまだSpringJointが張られていなければ張り...
            if (this.springJoint == null)
            {
                this.springJoint = this.gameObject.AddComponent<SpringJoint>();
                this.springJoint.autoConfigureConnectedAnchor = false;
                this.springJoint.anchor = this.casterCenter;
                this.springJoint.spring = this.spring;
                this.springJoint.damper = this.damper;
            }

            // SpringJointの自然長と接続先を設定する
            this.springJoint.maxDistance = this.stringLength;
            this.springJoint.connectedAnchor = this.stringAnchor[1];
        }
        else
        {
            // 射出中でなければSpringJointを削除し、糸による引っぱりを起こらなくする
            Destroy(this.springJoint);
            this.springJoint = null;
        }

        // 更新が終わったので、「SpringJoint要更新」フラグを折る
        this.needsUpdateSpring = false;
    }
}
