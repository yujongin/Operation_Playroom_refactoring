using System;
using System.Collections;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{

    public static Action<PlayerController> OnPlayerSpawn;
    public static Action<PlayerController> OnPlayerDespawn;
    public bool isPlayable;

    Vector3 velocity;
    bool isGrounded;

    Rigidbody rb;
    Character character;

    void Start()
    {
        character = GetComponent<Character>();
        rb = GetComponent<Rigidbody>();

        // ī�޶� Ȱ��ȭ
        if (IsOwner)
        {
            StartCoroutine(CamRoutine());
        }

    }
    void FixedUpdate()
    {
        if (!IsOwner) return;
        if (!IsOwner || !isPlayable) return;

        character.Move(character.cam.gameObject.GetComponent<CinemachineCamera>(), rb); // ĳ���� �̵�

    }

    private void Update()
    {
        if (!IsOwner) return;
        if (!IsOwner || !isPlayable) return;

        character.HandleInput(); // ĳ���� �Է�ó��
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            OnPlayerSpawn?.Invoke(this);
        }

        if (!IsOwner) return;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            OnPlayerDespawn?.Invoke(this);
        }
    }

    // ���� �ִ� �ó׸ӽ� ī�޶� ã�Ƽ� �Ҵ�
    void AssignCamera()
    {
        character.cam = FindFirstObjectByType<CinemachineFreeLookModifier>();

        if (character.cam != null)
        {
            character.cam.transform.position = transform.position;
            character.cam.gameObject.GetComponent<CinemachineCamera>().Follow = transform;
            character.cam.gameObject.GetComponent<CinemachineCamera>().LookAt = transform;
        }
        else
        {
            Debug.LogError("Cinemachine Camera�� ã�� �� �����ϴ�.");
        }
    }

    // ī�޶� �Ҵ� ��ƾ
    IEnumerator CamRoutine()
    {
        yield return new WaitUntil(() => FindFirstObjectByType<CinemachineCamera>() != null);
        isPlayable = true;
        AssignCamera();
    }

}
