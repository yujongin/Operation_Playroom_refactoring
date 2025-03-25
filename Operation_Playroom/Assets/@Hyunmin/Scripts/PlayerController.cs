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

        // 카메라 활성화
        if (IsOwner)
        {
            StartCoroutine(CamRoutine());
        }

    }
    void FixedUpdate()
    {
        if (!IsOwner) return;
        if (!IsOwner || !isPlayable) return;

        character.Move(character.cam.gameObject.GetComponent<CinemachineCamera>(), rb); // 캐릭터 이동

    }

    private void Update()
    {
        if (!IsOwner) return;
        if (!IsOwner || !isPlayable) return;

        character.HandleInput(); // 캐릭터 입력처리
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

    // 씬에 있는 시네머신 카메라를 찾아서 할당
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
            Debug.LogError("Cinemachine Camera를 찾을 수 없습니다.");
        }
    }

    // 카메라 할당 루틴
    IEnumerator CamRoutine()
    {
        yield return new WaitUntil(() => FindFirstObjectByType<CinemachineCamera>() != null);
        isPlayable = true;
        AssignCamera();
    }

}
