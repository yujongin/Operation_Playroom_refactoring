using System.Collections;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class Archer : Character
{
    [SerializeField] CinemachineCamera aimCamera;
    [SerializeField] GameObject aimCanvas;
    [SerializeField] GameObject arrowObject;
    [SerializeField] public int damage;

    //public NetworkVariable<bool> isAiming = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    bool isAiming;
    float xRotation = 0;
    float mouseSensitivity = 100;

    Quaternion lastAimRotation;

    public override void Start()
    {
        base.Start();
        InitializeCharacterStat();
    }

    // 공격 메서드
    public override void Attack()
    {
        // 화살 발사
        StartCoroutine(ShootAndReloadRoutine());
        PlaySFXServerRpc(2, 0.5f);

    }

    public override void TakeDamage()
    {
        base.TakeDamage();

        ResetAimmingClientRpc();
    }

    [ClientRpc]
    void ResetAimmingClientRpc()
    {
        if (isHoldingItem) return;
        holdItemAble = true;

        aimCanvas.SetActive(false);

        SetAvatarLayerWeight(0);

        aimCamera.Priority = -10;

        transform.rotation = lastAimRotation;
        currentRotation = lastAimRotation;

        isAiming = false;
    }

    // 이동 메서드
    public override void Move(CinemachineCamera cam, Rigidbody rb)
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        float scaleFactor = transform.localScale.y;
        float adjustedMoveSpeed = moveSpeed * scaleFactor;

        // 카메라 방향에 따른 이동
        Transform referenceCam = isAiming ? aimCamera.transform : cam.transform; // 기준으로 할 카메라 선택
        Vector3 moveDirection = referenceCam.right * moveX + referenceCam.forward * moveZ;
        moveDirection.y = 0;

        Vector3 velocity = moveDirection.normalized * adjustedMoveSpeed;
        velocity.y = rb.linearVelocity.y;

        rb.linearVelocity = velocity;

        // 애니메이션 적용
        float speed = moveDirection.magnitude > 0.1f ? 1f : 0f;
        SetFloatAnimation("Move", speed);

        // 조준중일때
        if (isAiming)
        {
            RotateView(); // 1인칭 회전 적용
        }
        // 조준중이 아닐때
        else
        {
            // 일정 움직임이 있을때만 회전값 변경
            if (moveDirection.magnitude > 0.1f)
            {
                currentRotation = Quaternion.LookRotation(moveDirection);
            }
           

            // 회전 적용 (회전 값은 계속 유지됨)
            rb.rotation = Quaternion.Normalize(Quaternion.Slerp(rb.rotation, currentRotation, Time.deltaTime * 10f));
        }

    }

    // 키 입력 메서드
    public override void HandleInput()
    {
        // 조준 시작
        if (Input.GetButtonDown("Aim"))
        {
            if (isHoldingItem) return;
            holdItemAble = false; 

            // 조준점 활성화
            aimCanvas.SetActive(true);

            // 정면 조준
            transform.rotation = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0);

            xRotation = 0;

            aimCamera.transform.position = transform.position + transform.forward * 0.05f + transform.up * 0.13f;
            aimCamera.transform.rotation = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0);


            aimCamera.Priority = 10;

            // 조준 애니메이션 실행
            SetAvatarLayerWeight(1);
            SetTriggerAnimation("Aim");
            isAiming = true;
        }
        // 조준 해제
        if (Input.GetButtonUp("Aim"))
        {
            if (isHoldingItem) return;
            holdItemAble = true;

            aimCanvas.SetActive(false);

            SetAvatarLayerWeight(0);

            aimCamera.Priority = -10;

            transform.rotation = lastAimRotation;
            currentRotation = lastAimRotation;

            isAiming = false;
        }
        // 조준중이면
        if (isAiming)
        {
            // 발사
            if (Input.GetButtonDown("Attack"))
            {
                if (isAiming && attackAble)
                {
                    Attack();
                }

                transform.rotation = lastAimRotation;
                currentRotation = lastAimRotation;

            }
        }
        // 줍기
        if (Input.GetButtonDown("Interact"))
        {
            Interaction();
        }
    }

    // 상호작용 메서드
    public override void Interaction()
    {
        // 아이템을 들고 있으면
        if (isHoldingItem)
        {
            Drop();
        }
        else
        {
            PickUp();
        }
    }

    // 1인칭 회전 메서드
    void RotateView()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 위아래 회전
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 40f); // 시야 제한

        // 캐릭터와 카메라 회전
        aimCamera.transform.rotation = Quaternion.Euler(xRotation, transform.eulerAngles.y, 0f);
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y + mouseX, 0f);

        lastAimRotation = transform.rotation;
    }

    // 장전 루틴
    IEnumerator ShootAndReloadRoutine()
    {
        SetTriggerAnimation("BowAttack");
        aimCamera.GetComponent<ProjectileLauncher>().ShootArrow(aimCamera.transform);
        attackAble = false;

        yield return new WaitForSeconds(0.5f);

        SetTriggerAnimation("Aim");

        yield return new WaitForSeconds(1f);

        attackAble = true;
    }

    public void InitializeCharacterStat()
    {
        if (Managers.Data.UnitDic.TryGetValue(201001, out Data.UnitData archerData))
        {
            health.SetHp((int)archerData.HP);
            damage = (int)archerData.Atk;
        }
        else
        {
            Debug.LogError("Archer 데이터(ID: 201001)를 찾을 수 없습니다.");
        }
    }

}

