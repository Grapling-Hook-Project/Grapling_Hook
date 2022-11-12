using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerControllerScript : MonoBehaviour
{
    [SerializeField] private float mouseSensitivity = 100000f;
    [SerializeField] private Transform hookshotTransform;
    [SerializeField] Transform wire;

    const float moveSpeed = 20f;
    const float fryMoveSpeed = 1f;
    const float downForce = -60f;
    const float fryDownForce = -20f;

    private CharacterController characterController;
    private Camera playerCamera;
    private GameObject targetObj;
    private float cameraVerticalAngle;
    private float charcterVelocityY;
    private float hookshotSize;
    //private float currentSpeed;
    private State CurrentState;
    private Vector3 shotPoint;
    private Vector3 characterVelocityMomentum;

    Rigidbody rb;
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
        //取得
        characterController = GetComponent<CharacterController>();
        playerCamera = transform.Find("Main Camera").GetComponent<Camera>();
        //マウス固定
        Cursor.lockState = CursorLockMode.Locked;
        //変更
        CurrentState = State.Normal;
        hookshotTransform.gameObject.SetActive(false);
        //Time.timeScale = 0.2f;
    }
    private void Update()
    {
        Debug.Log(CurrentState);
        switch (CurrentState)
        {
            default:
            case State.Normal:
                CharacterLook();
                CharacterMovement(downForce, moveSpeed);
                HookShotStart();
                break;
            case State.HookshotThrown:
                CharacterLook();
                CharacterMovement(fryDownForce, fryMoveSpeed);
                HookshotThrow();
                break;
            case State.HookshotFlyingPlayer:
                CharacterLook();
                HookshotMovement();
                break;
            case State.HookshotFlyingCut:
                CharacterLook();
                CharacterMovement(fryDownForce, fryMoveSpeed);
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
                CharacterMovement(downForce, moveSpeed);
                HookShotStart();
                ResetGravityEffect();
                break;
        }
    }
    private void CharacterLook()
    {
        float lookX = Input.GetAxisRaw("Mouse X");
        float lookY = Input.GetAxisRaw("Mouse Y");

        transform.Rotate(new Vector3(0f, lookX * mouseSensitivity, 0f), Space.Self);

        cameraVerticalAngle -= lookY * mouseSensitivity;

        cameraVerticalAngle = Mathf.Clamp(cameraVerticalAngle, -89f, 89f);

        playerCamera.transform.localEulerAngles = new Vector3(cameraVerticalAngle, 0, 0);
    }
    private void CharacterMovement(float gravityDownForce, float moveSpeed_)
    {
        //Vector3 forward = Camera.main.transform.TransformDirection(Vector3.forward);
        Vector3 right = Camera.main.transform.TransformDirection(Vector3.right);
        Vector3 moveDirection = Input.GetAxis("Horizontal") * right + Input.GetAxis("Vertical") * Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        moveDirection *= moveSpeed_;

        //Vector3 characterVelocity = transform.right * moveX * moveSpeed + transform.forward * moveZ * moveSpeed;

        charcterVelocityY += gravityDownForce * Time.deltaTime;

        moveDirection.y = charcterVelocityY;

        moveDirection += characterVelocityMomentum;

        characterController.Move(moveDirection * Time.deltaTime);
    }
    private void ResetGravityEffect()
    {
        charcterVelocityY = 0f;
    }
    private void HookShotStart()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit raycastHit))
            {
                //hitした座標を格納
                shotPoint = raycastHit.point;
                targetObj = raycastHit.collider.gameObject;

                if (targetObj.CompareTag("anchorPoint"))
                {
                    CurrentState = State.HookshotThrown;
                }
                else
                { 
                    CurrentState = State.HookshotMistake;
                }

                hookshotSize = 0f;

                hookshotTransform.gameObject.SetActive(true);

                hookshotTransform.localScale = Vector3.zero;
            }
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
            CurrentState = State.HookshotFlyingPlayer;
        }
    }
    private void HookshotMovement()
    {
        hookshotTransform.LookAt(targetObj.transform);

        //二点間のベクトルの正規化
        Vector3 hookshotDirection = (targetObj.transform.position - transform.position).normalized;

        float hookshotSpeedMin = 10f;
        float hookshotSpeedMax = 40f;

        //縮小スピード
        float hookshotSpeed = Mathf.Clamp(Vector3.Distance(transform.position, hookshotDirection), hookshotSpeedMin, hookshotSpeedMax);

        //加速
        float hookshotSpeedMultiplier = 2f;

        characterController.Move(hookshotDirection * hookshotSpeed * hookshotSpeedMultiplier * Time.deltaTime);

        //ワイヤーの縮小
        hookshotSize -= (hookshotDirection * hookshotSpeed * hookshotSpeedMultiplier * Time.deltaTime).magnitude / wire.localScale.z;
        if (hookshotSize < 0) hookshotSize = 0;
        hookshotTransform.localScale = new Vector3(1, 1, hookshotSize);

        float reachHookshotDistance = 2f;

        //ワイヤーが特定の距離まで縮まった場合
        if (Vector3.Distance(transform.position, targetObj.transform.position) < reachHookshotDistance)
        {
            hookshotTransform.gameObject.SetActive(false);

            //待機モーション
            HookshotStandBy();
        }
        //途中で切断
        HookshotFlyingStop();
    }
    private void HookshotStandBy()
    {
        //ステート変更
        CurrentState = State.HookshotStandByTargetObj;
        //親子関係を付ける
        gameObject.transform.parent = targetObj.transform;
        //重力リセット
        ResetGravityEffect();
        //ワイヤーを非表示
        hookshotTransform.gameObject.SetActive(false);
    }
    private void HookshotFlyingStop()
    {
        if (Input.GetMouseButtonDown(1))
        {
            //ステート変更
            CurrentState = State.HookshotFlyingCut;
            //重力リセット
            ResetGravityEffect();
            //ワイヤーを非表示
            hookshotTransform.gameObject.SetActive(false);
        }
    }
    private void HookshotStop()
    {
        hookshotTransform.LookAt(targetObj.transform);

        if (Input.GetMouseButtonDown(1))
        {
            gameObject.transform.parent = null;
            hookshotTransform.LookAt(targetObj.transform);
            CurrentState = State.Normal;
            ResetGravityEffect();
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
            ResetGravityEffect();
            CurrentState = State.Normal;
        }
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
