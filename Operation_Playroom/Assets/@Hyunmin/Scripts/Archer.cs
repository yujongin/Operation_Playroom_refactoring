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

    // ���� �޼���
    public override void Attack()
    {
        // ȭ�� �߻�
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

    // �̵� �޼���
    public override void Move(CinemachineCamera cam, Rigidbody rb)
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        float scaleFactor = transform.localScale.y;
        float adjustedMoveSpeed = moveSpeed * scaleFactor;

        // ī�޶� ���⿡ ���� �̵�
        Transform referenceCam = isAiming ? aimCamera.transform : cam.transform; // �������� �� ī�޶� ����
        Vector3 moveDirection = referenceCam.right * moveX + referenceCam.forward * moveZ;
        moveDirection.y = 0;

        Vector3 velocity = moveDirection.normalized * adjustedMoveSpeed;
        velocity.y = rb.linearVelocity.y;

        rb.linearVelocity = velocity;

        // �ִϸ��̼� ����
        float speed = moveDirection.magnitude > 0.1f ? 1f : 0f;
        SetFloatAnimation("Move", speed);

        // �������϶�
        if (isAiming)
        {
            RotateView(); // 1��Ī ȸ�� ����
        }
        // �������� �ƴҶ�
        else
        {
            // ���� �������� �������� ȸ���� ����
            if (moveDirection.magnitude > 0.1f)
            {
                currentRotation = Quaternion.LookRotation(moveDirection);
            }
           

            // ȸ�� ���� (ȸ�� ���� ��� ������)
            rb.rotation = Quaternion.Normalize(Quaternion.Slerp(rb.rotation, currentRotation, Time.deltaTime * 10f));
        }

    }

    // Ű �Է� �޼���
    public override void HandleInput()
    {
        // ���� ����
        if (Input.GetButtonDown("Aim"))
        {
            if (isHoldingItem) return;
            holdItemAble = false; 

            // ������ Ȱ��ȭ
            aimCanvas.SetActive(true);

            // ���� ����
            transform.rotation = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0);

            xRotation = 0;

            aimCamera.transform.position = transform.position + transform.forward * 0.05f + transform.up * 0.13f;
            aimCamera.transform.rotation = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0);


            aimCamera.Priority = 10;

            // ���� �ִϸ��̼� ����
            SetAvatarLayerWeight(1);
            SetTriggerAnimation("Aim");
            isAiming = true;
        }
        // ���� ����
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
        // �������̸�
        if (isAiming)
        {
            // �߻�
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
        // �ݱ�
        if (Input.GetButtonDown("Interact"))
        {
            Interaction();
        }
    }

    // ��ȣ�ۿ� �޼���
    public override void Interaction()
    {
        // �������� ��� ������
        if (isHoldingItem)
        {
            Drop();
        }
        else
        {
            PickUp();
        }
    }

    // 1��Ī ȸ�� �޼���
    void RotateView()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // ���Ʒ� ȸ��
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 40f); // �þ� ����

        // ĳ���Ϳ� ī�޶� ȸ��
        aimCamera.transform.rotation = Quaternion.Euler(xRotation, transform.eulerAngles.y, 0f);
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y + mouseX, 0f);

        lastAimRotation = transform.rotation;
    }

    // ���� ��ƾ
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
            Debug.LogError("Archer ������(ID: 201001)�� ã�� �� �����ϴ�.");
        }
    }

}

