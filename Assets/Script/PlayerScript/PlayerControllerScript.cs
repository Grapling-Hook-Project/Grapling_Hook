using UnityEngine;

public class PlayerControllerScript : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 100000f;
    [SerializeField] private Transform hookshotTransform;
    [SerializeField] Transform wire;

    [SerializeField] Rigidbody rb;

    private Camera playerCamera;
    private GameObject targetObj;
    private float cameraVerticalAngle;
    private float hookshotSize;
    private Vector3 shotPoint;

    [SerializeField, Min(0),Tooltip("値が大きいほど疑似摩擦が大きいものになります。上げすぎには注意してください")] 
    float moveForceMultiplier = 500f;

    //現在接触しているコライダー
    Collider _currentCollider;

    //ステート
    private State _currentState;

    //入力値
    float _horizontalInput;
    float _verticalInput;

    //移動速度
    const float MOVESPEED = 20f;

    //ワイヤーの縮小スピード
    const float HOOKSHOT_SPEED_MIN = 10f;
    const float HOOKSHOT_SPEED_MAX = 40f;
    const float HOOKSHOT_SPEED_MULTIPLIER = 2f;

    private enum State
    {
        Normal,
        HookshotThrown,
        HookshotFlyingPlayer,
        HookshotFlyingCut,
        HookshotMistake,
        HookshotStandBy,
        HookshotStandByTargetObj,
    }

    private void Awake()
    {
        playerCamera = Camera.main;

        //マウス固定
        Cursor.lockState = CursorLockMode.Locked;
        //変更
        _currentState = State.Normal;
        hookshotTransform.gameObject.SetActive(false);
        //Time.timeScale = 0.2f;
    }

    private void Update()
    {
        //Debug.Log(CurrentState);

        switch (_currentState)
        {
            default:
            case State.Normal:
                CharacterLook();
                InputMoveAxis();
                HookShotStart();
                break;
            case State.HookshotThrown:
                CharacterLook();
                InputMoveAxis();
                HookshotThrow();
                break;
            case State.HookshotFlyingPlayer:
                CharacterLook();
                HookshotMovement();
                break;
            case State.HookshotFlyingCut:
                CharacterLook();
                InputMoveAxis();
                HookShotStart();
                HookshotCutParabola();
                break;
            case State.HookshotStandByTargetObj:
                CharacterLook();
                HookShotStart();
                HookshotStop();
                break;
            //アニメーション追加用
            case State.HookshotMistake:
                CharacterLook();
                InputMoveAxis();
                HookShotStart();
                break;
        }
    }

    private void FixedUpdate()
    {
        CharacterMove();
        HookshotMove();
    }

#if UNITY_EDITOR
    //常にゲーム画面左上にステートを表示するものです。
    //ビルド時にはこの部分だけビルドされない設定にしていますが、不要なら削除してください
    private void OnGUI()
    {
        GUI.Label(new Rect(20, 20, 100, 50), "" + _currentState);
    }
#endif

    private void CharacterLook()
    {
        float lookX = Input.GetAxisRaw("Mouse X");
        float lookY = Input.GetAxisRaw("Mouse Y");

        transform.Rotate(new Vector3(0f, lookX * mouseSensitivity, 0f), Space.Self);

        cameraVerticalAngle -= lookY * mouseSensitivity;

        cameraVerticalAngle = Mathf.Clamp(cameraVerticalAngle, -89f, 89f);

        playerCamera.transform.localEulerAngles = new Vector3(cameraVerticalAngle, 0, 0);
    }

    private void InputMoveAxis()
    {
        _horizontalInput = Input.GetAxis("Horizontal");
        _verticalInput = Input.GetAxis("Vertical");
    }

    private void CharacterMove()
    {
        //------------------------------------------------------------------------------
        //このメソッドを呼ぶと常に落下がゆっくりになります。
        //移動入力は普通に効きます。
        //入力をやめれば擬似的な摩擦のような感じです。
        //今は下記if文の条件で呼んでいるので常にゆっくりした落下になっています
        //よって、これを工夫して呼ぶようにしてください。
        //------------------------------------------------------------------------------

        if (_currentState is State.HookshotFlyingPlayer or State.HookshotStandByTargetObj) return;

        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        Vector3 moveVector = MOVESPEED * (cameraRight.normalized * _horizontalInput + cameraForward.normalized * _verticalInput);

        //移動がなければ一定速度に近くなるように力を加える
        rb.AddForce(moveForceMultiplier * (moveVector - rb.velocity));
    }

    private void HookShotStart()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit raycastHit))
        {
            //hitした座標を格納
            shotPoint = raycastHit.point;
            targetObj = raycastHit.collider.gameObject;

            if (targetObj.CompareTag("anchorPoint"))
            {
                SetCurrentCollider(raycastHit.collider);
                _currentState = State.HookshotThrown;
            }
            else
            {
                _currentState = State.HookshotMistake;
            }

            //掴まり表現のためOFFにしていた重力を復活させる。
            //この処理はState.HookshotStandByTargetObjを抜ける際に行いたいが、そういったものがないためここに直接書き込みます。
            rb.useGravity = true;

            hookshotSize = 0f;

            hookshotTransform.gameObject.SetActive(true);

            hookshotTransform.localScale = Vector3.zero;
        }

    }

    private void HookshotThrow()
    {
        //対象のTransformを設定し、その方向へ向かせる
        hookshotTransform.LookAt(shotPoint);

        float hookshotTrowSpeed = 100000f;

        //サイズの計算
        hookshotSize += hookshotTrowSpeed * Time.deltaTime;
        hookshotTransform.localScale = new Vector3(1, 1, hookshotSize);

        if (hookshotSize * wire.localScale.z > Vector3.Distance(transform.position, shotPoint))
        {
            hookshotSize = Vector3.Distance(transform.position, shotPoint) / wire.localScale.z;
            _currentState = State.HookshotFlyingPlayer;
            hookshotTransform.localScale = new Vector3(1, 1, hookshotSize);

            //親の移動の影響を受けてしまうため親をもとに戻す
            //今後このオブジェクトに永続的な親ができた場合に対応しきれていません。必要ないかも知れませんが対応したほうがいいです。
            gameObject.transform.parent = null;
        }
    }

    private void HookshotMovement()
    {
        hookshotTransform.LookAt(targetObj.transform);

        //二点間のベクトルの正規化
        Vector3 hookshotDirection = (targetObj.transform.position - transform.position).normalized;

        //縮小スピード
        float hookshotSpeed = Mathf.Clamp(Vector3.Distance(transform.position, hookshotDirection), HOOKSHOT_SPEED_MIN, HOOKSHOT_SPEED_MAX);

        //ワイヤーの縮小
        hookshotSize -= (HOOKSHOT_SPEED_MULTIPLIER * hookshotSpeed * Time.deltaTime * hookshotDirection).magnitude / wire.localScale.z;
        if (hookshotSize < 0) hookshotSize = 0;
        hookshotTransform.localScale = new Vector3(1, 1, hookshotSize);

        //[！！！注意！！！]
        //真横に掴まった際に範囲外のため、「2→2.5」に変更しました。レベルデザイン部分にかかわる変更のため要確認お願いします。
        float reachHookshotDistance = 2.5f;

        //ワイヤーが特定の距離まで縮まった場合
        Debug.Log(Vector3.Distance(transform.position, targetObj.transform.position));
        if (Vector3.Distance(transform.position, targetObj.transform.position) < reachHookshotDistance)
        {
            hookshotTransform.gameObject.SetActive(false);

            //待機モーション
            HookshotStandBy();
        }

        //途中で切断
        HookshotFlyingStop();
    }

    private void HookshotMove()
    {
        if (_currentState != State.HookshotFlyingPlayer) return;

        //二点間のベクトルの正規化
        Vector3 hookshotDirection = (targetObj.transform.position - transform.position).normalized;

        //縮小スピード
        float hookshotSpeed = Mathf.Clamp(Vector3.Distance(transform.position, hookshotDirection), HOOKSHOT_SPEED_MIN, HOOKSHOT_SPEED_MAX);

        //Updateで行っているワイヤーの縮小に遅れないようvelocityを直接書き換え
        rb.velocity = HOOKSHOT_SPEED_MULTIPLIER * hookshotSpeed * hookshotDirection;
    }

    private void HookshotStandBy()
    {
        //物体で静止する際は重力をオフにして擬似的な掴まりを表現
        rb.useGravity = false;

        //さらにビタッと止まるように変更。
        //仕様が異なる場合は仕様に沿って変更してください。
        rb.velocity = Vector3.zero;

        //ステート変更
        _currentState = State.HookshotStandByTargetObj;
        //親子関係を付ける
        gameObject.transform.parent = targetObj.transform;
        //ワイヤーを非表示
        hookshotTransform.gameObject.SetActive(false);
    }

    private void HookshotFlyingStop()
    {
        if (Input.GetMouseButtonDown(1))
        {
            //ステート変更
            _currentState = State.HookshotFlyingCut;
            //ワイヤーを非表示
            hookshotTransform.gameObject.SetActive(false);
        }
    }

    private void HookshotStop()
    {
        hookshotTransform.LookAt(targetObj.transform);

        if (Input.GetMouseButtonDown(1))
        {
            //掴まり表現のためOFFにしていた重力を復活させる。
            //この処理はState.HookshotStandByTargetObjを抜ける際に行いたいが、そういったものがないためここに直接書き込みます。
            rb.useGravity = true;

            //今後このオブジェクトに永続的な親ができた場合に対応しきれていません。必要ないかも知れませんが対応したほうがいいです。
            gameObject.transform.parent = null;
            hookshotTransform.LookAt(targetObj.transform);
            _currentState = State.Normal;
            hookshotTransform.gameObject.SetActive(false);
        }
    }

    private void HookshotCutParabola()
    {
        var ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);

        var distance = 1.2f;

        if (Physics.Raycast(ray, distance))
        {
            Debug.Log("床と接触");
            _currentState = State.Normal;
        }
    }

    private void SetCurrentCollider(Collider col)
    {
        //直前に付いていたコライダーのisTriggerをオンにする（上側に飛ぶ際に衝突してしまうのを回避するため）
        if (_currentCollider != null) _currentCollider.isTrigger = true;

        //現在のコライダーを設定し、isTriggerをオフにする
        _currentCollider = col;
        _currentCollider.isTrigger = false;
    }

    //private void HookshotStop()
    //{
    //    if (Input.GetMouseButtonDown(1))
    //    {
    //        hookshotTransform.LookAt(targetObj.transform);
    //        CurrentState = State.Normal;
    //        ResetGravityEffect();
    //        hookshotTransform.gameObject.SetActive(false);
    //    }
    //}
}
