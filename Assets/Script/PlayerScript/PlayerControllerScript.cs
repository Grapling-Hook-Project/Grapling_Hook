using UnityEngine;
using UnityEngine.UI;
public class PlayerControllerScript : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private Transform hookshotTransform;
    [SerializeField] Transform wire;
    [SerializeField] GameObject enabledAim;
    [SerializeField] GameObject disabledBim;
    [SerializeField] float hookshotTrowSpeed = 1000;
    [SerializeField] float diration = 100;

    [SerializeField] Rigidbody rb;

    private Camera playerCamera;
    private GameObject targetObj;
    private float cameraVerticalAngle;
    private float hookshotSize;
    private Vector3 shotPoint;

    [SerializeField, Min(0), Tooltip("X,Z軸に対しての疑似摩擦です。")]
    float moveForceMultiplierXZ = 500f;

    [SerializeField, Min(0), Tooltip("Y軸に対しての疑似摩擦です。")]
    float moveForceMultiplierY = 500f;

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


    bool quickFall;

    //Vector3 wallHitPosition;

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
        enabledAim.SetActive(false);
        disabledBim.SetActive(true);

        quickFall = false;
        //Time.timeScale = 0.2f;
    }

    private void Update()
    {

        //Debug.Log(quickFall);
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
        CharacterFrictionMove();
        HookshotMove();
        GravitySpeedUp();
    }

#if UNITY_EDITOR
    //常にゲーム画面左上にステートを表示。
    private void OnGUI()
    {
        GUI.Label(new Rect(40, 40, 100, 50), "" + _currentState);
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

        Debug.Log(_horizontalInput);
    }

    private void CharacterFrictionMove()
    {
        if (_currentState is State.HookshotFlyingPlayer or State.HookshotStandByTargetObj) return;

        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;

        Vector3 moveVector = MOVESPEED * (cameraRight.normalized * _horizontalInput + cameraForward.normalized * _verticalInput);

        //上昇時に摩擦をかける
        if (rb.velocity.y > 0)
        {
            quickFall = false;
            rb.AddForce(moveForceMultiplierY * -new Vector3(0, rb.velocity.y, 0));
        }

        //移動がなければ一定速度に近くなるように力を加える
        rb.AddForce(moveForceMultiplierXZ * (moveVector - new Vector3(rb.velocity.x, 0, rb.velocity.z)));
    }

    private void HookShotStart()
    {
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit raycastHit, diration))
        {
            //hitした座標
            shotPoint = raycastHit.point;
            targetObj = raycastHit.collider.gameObject;

            //切り替え
            if (targetObj.CompareTag("anchorPoint"))
            {
                enabledAim.SetActive(true);
                disabledBim.SetActive(false);
            }
            else
            {
                enabledAim.SetActive(false);
                disabledBim.SetActive(true);
            }

            if (Input.GetMouseButtonDown(0))
            {

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
                rb.useGravity = true;

                hookshotSize = 0f;

                hookshotTransform.gameObject.SetActive(true);

                hookshotTransform.localScale = Vector3.zero;
            }
        }
        else
        {
            enabledAim.SetActive(false);
            disabledBim.SetActive(true);
        }
    }

    private void HookshotThrow()
    {
        //対象のTransformを設定し、その方向へ向かせる
        hookshotTransform.LookAt(shotPoint);

        //float hookshotTrowSpeed = 10000f;

        //サイズの計算
        hookshotSize += hookshotTrowSpeed * Time.deltaTime;
        hookshotTransform.localScale = new Vector3(1, 1, hookshotSize);

        if (hookshotSize * wire.localScale.z > Vector3.Distance(transform.position, shotPoint))
        {
            hookshotSize = Vector3.Distance(transform.position, shotPoint) / wire.localScale.z;
            _currentState = State.HookshotFlyingPlayer;
            hookshotTransform.localScale = new Vector3(1, 1, hookshotSize);

            //親の移動の影響を受けてしまうため親をもとに戻す
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

        float reachHookshotDistance = 2.5f;

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
            //gravityPoint = transform.position.y;
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
            //掴まり表現のためOFFにしていた重力を復活させる
            rb.useGravity = true;

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
            //Debug.Log("床と接触");
            _currentState = State.Normal;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_currentState == State.HookshotFlyingPlayer)
        {
            hookshotTransform.gameObject.SetActive(false);
            _currentState = State.Normal;
        }

        if (_currentState is not State.HookshotFlyingCut) return;

        Debug.Log("OBJ接触");
        quickFall = true;
    }

    private void GravitySpeedUp()
    {
        var ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        var distance = 1.0f;

        if (Physics.Raycast(ray, distance))
        {
            //Debug.Log("床と接触");
            quickFall = false;

            if (_currentState is not State.HookshotFlyingCut) return;

            _currentState = State.Normal;
        }
        if (quickFall == true || _currentState == State.HookshotFlyingCut)
        {
            //Debug.Log("急降下");
            rb.AddForce(new Vector3(0, -100f, 0));
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
