using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharcterController : MonoBehaviour
{
    private const float NORMAL_FOV = 60f;
    private const float HOOKSHOT_FOV = 100f;

    [SerializeField] private float mouseSensitivity = 1f;
    [SerializeField] private Transform debugHitPointTransform;
    [SerializeField] private Transform hookshotTransform;

    private CharacterController characterController;
    private float cameraVerticalAngle;
    private float charcterVelocityY;
    private float hookshotSize;
    private Camera playerCamera;
    private CameraScript cameraFov;
    public WireColisionScript wireColisionScript;
    private State state;
    private Vector3 shotPoint;
    private Vector3 characterVelocityMomentum;

    private enum State
    {
        Normal,
        HookshotThrown,
        HookshotFlyingPlayer,
        HookshotStop
    }
    private void Awake()
    {
        //取得
        characterController = GetComponent<CharacterController>();
        playerCamera = transform.Find("Main Camera").GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        state = State.Normal;
        hookshotTransform.gameObject.SetActive(false);
    }
    private void Update()
    {
        switch (state)
        {
            default:
            case State.Normal:
                CharacterLook();
                CharacterMovement();
                HookShotStart();
                break;
            case State.HookshotThrown:
                HookshotThrow();
                CharacterLook();
                CharacterMovement();
                break;
            case State.HookshotFlyingPlayer:
                CharacterLook();
                HookshotMovement();
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
    private void CharacterMovement()
    {
        float moveZ = Input.GetAxisRaw("Horizontal");
        float moveX = Input.GetAxisRaw("Vertical");
        float moveSpeed = 20f;

        Vector3 characterVelocity = transform.right * moveX * moveSpeed + transform.forward * moveZ * moveSpeed;

        if (characterController.isGrounded)
        {
            charcterVelocityY = 0f;

            if (TestInputJump())
            {
                float jumpSeed = 30f;
                charcterVelocityY = jumpSeed;
            }
        }

        float gravityDownForce = -60f;

        charcterVelocityY += gravityDownForce * Time.deltaTime;

        characterVelocity.y = charcterVelocityY;

        characterVelocity += characterVelocityMomentum;

        characterController.Move(characterVelocity * Time.deltaTime);

        if (characterVelocityMomentum.magnitude >= 0f)
        {
            float momentumDrag = 10f;
            characterVelocityMomentum -= characterVelocityMomentum * momentumDrag * Time.deltaTime;
            if (characterVelocityMomentum.magnitude < .0f)
            {
                characterVelocityMomentum = Vector3.zero;
            }
        }
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
                debugHitPointTransform.position = raycastHit.point;
                shotPoint = raycastHit.point;
                hookshotSize = 0f;
                hookshotTransform.gameObject.SetActive(true);
                hookshotTransform.localScale = Vector3.zero;
                state = State.HookshotThrown;
            }
        }
    }
    private void HookshotThrow()
    {
        hookshotTransform.LookAt(shotPoint);

        float hookshotTrowSpeed = 1000f;

        hookshotSize += hookshotTrowSpeed * Time.deltaTime;

        hookshotTransform.localScale = new Vector3(1, 1, hookshotSize);

        Debug.Log("HookshotSize:" + hookshotSize);
        Debug.Log("Distance:" + Vector3.Distance(transform.position, shotPoint));

        if (hookshotSize > Vector3.Distance(transform.position, shotPoint))
        {
            state = State.HookshotFlyingPlayer;
        }
        //if (wireColisionScript.WireContact() == true)
        //{
        //    state = State.HookshotFlyingPlayer;
        //}
        //cameraFov.SetCameraFov(HOOKSHOT_FOV);
    }
    private void HookshotMovement()
    {
        hookshotTransform.LookAt(shotPoint);

        //二点間のベクトルの正規化
        Vector3 hookshotDirection = (shotPoint - transform.position).normalized;

        float hookshotSpeedMin = 10f;
        float hookshotSpeedMax = 40f;

        //
        float hookshotSpeed = Mathf.Clamp(Vector3.Distance(transform.position, hookshotDirection), hookshotSpeedMin, hookshotSpeedMax);

        //加速
        float hookshotSpeedMultiplier = 2f;

        characterController.Move(hookshotDirection * hookshotSpeed * hookshotSpeedMultiplier * Time.deltaTime);


        //ワイヤーの縮小
        hookshotSize -= (hookshotDirection * hookshotSpeed * hookshotSpeedMultiplier * Time.deltaTime).magnitude;
        hookshotTransform.localScale = new Vector3(1, 1, hookshotSize);

        float reachedHookshotDistance = 1f;
        if (Vector3.Distance(transform.position, shotPoint) < reachedHookshotDistance)
        {
            Debug.Log("実行");
            StopHookshot();
        }

        if (TestInputDownHookshot())
        {
            StopHookshot();
        }

        if (TestInputJump())
        {
            float momentExtraSpeed = 7f;
            float jumpSpeed = 40f;
            characterVelocityMomentum = hookshotDirection * hookshotSpeed * momentExtraSpeed;
            characterVelocityMomentum += Vector3.up * jumpSpeed;
            state = State.Normal;
            StopHookshot();
        }
    }
    private void StopHookshot()
    {
        state = State.Normal;
        ResetGravityEffect();
        hookshotTransform.gameObject.SetActive(false);
        //cameraFov.SetCameraFov(NORMAL_FOV);
    }
    private bool TestInputDownHookshot()
    {
        return Input.GetMouseButtonDown(0);
    }
    private bool TestInputJump()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }
}
