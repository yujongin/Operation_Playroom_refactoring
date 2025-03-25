using System.Collections;
using System.Linq;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Character : NetworkBehaviour, ICharacter
{
    [HideInInspector] public CinemachineFreeLookModifier cam;

    [SerializeField] GameObject targetItem;
    [SerializeField] GameObject weaponObject;
    [SerializeField] Material[] teamMaterials;
    [SerializeField] Material[] damageMaterials;
    [SerializeField] Renderer[] playerRenderers;
    [SerializeField] GameObject[] Icons;
    [SerializeField] SoundScriptableObject soundData;


    public NetworkVariable<int> team = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    float detectItemRange = 0.2f;
    AudioSource audioSource;
    Coroutine damageRoutine;

    protected bool attackAble;
    protected bool holdItemAble;
    protected bool isHoldingItem;
    protected float moveSpeed = 5;

    protected Animator animator;
    protected NetworkAnimator networkAnimator;
    protected Quaternion currentRotation;
    protected Health health;

    public virtual void Start()
    {

    }
    public override void OnNetworkSpawn()
    {
        animator = GetComponent<Animator>();
        networkAnimator = GetComponent<NetworkAnimator>();
        health = GetComponent<Health>();
        audioSource = GetComponent<AudioSource>();

        attackAble = true;
        holdItemAble = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (IsOwner)
        {
            team.Value = (int)ClientSingleton.Instance.UserData.userGamePreferences.gameTeam;
        }

        team.OnValueChanged += (oldValue, newValue) => OnTeamValueChanged(newValue);

        if (IsClient)
        {
            SyncMaterialsOnSpawn();
        }

        OnTeamValueChanged(team.Value);
    }

    public override void OnDestroy()
    {
        team.OnValueChanged -= (oldValue, newValue) => OnTeamValueChanged(newValue);
    }

    public abstract void Attack(); // ���� ����
    public abstract void Interaction(); // ��ȣ�ۿ� ����
    public abstract void HandleInput(); // Ű �Է� ����


    // �̵� �޼���
    public virtual void Move(CinemachineCamera cam, Rigidbody rb)
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        float scaleFactor = transform.localScale.y;
        float adjustedMoveSpeed = moveSpeed * scaleFactor;

        // ī�޶� ���⿡ ���� �̵�
        Vector3 moveDirection = cam.gameObject.transform.right * moveX + cam.gameObject.transform.forward * moveZ;
        moveDirection.y = 0;

        Vector3 velocity = moveDirection.normalized * adjustedMoveSpeed;
        velocity.y = rb.linearVelocity.y;

        rb.linearVelocity = velocity;

        // �ִϸ��̼� ����
        float speed = moveDirection.magnitude > 0.1f ? 1f : 0f;
        SetFloatAnimation("Move", speed);

        // ���� �������� �������� ȸ���� ����
        if (moveDirection.magnitude > 0.1f)
        {
            currentRotation = Quaternion.LookRotation(moveDirection);
        }

        // ȸ�� ���� (ȸ�� ���� ��� ������)
        rb.rotation = Quaternion.Normalize(Quaternion.Slerp(rb.rotation, currentRotation, Time.deltaTime * 10f));
    }

    // �ǰ� �޼���
    public virtual void TakeDamage()
    {
        if (health.isDead) return;

        if (damageRoutine != null)
        {
            StopCoroutine(damageRoutine);
        }
        damageRoutine = StartCoroutine(DamageRoutine());
    }

    // ������ ��ƾ
    IEnumerator DamageRoutine()
    {
        SetAvatarLayerWeightClientRpc(1);
        PlaySFXClientRpc(Random.Range(0, 2), 0.5f); // �ǰ� ȿ���� ���
        SetTriggerAnimationClientRpc("Damage");

        ApplyDamageMaterialClientRpc(team.Value);
        attackAble = false;

        yield return new WaitForSeconds(0.5f);

        ApplyMaterialClientRpc(team.Value);

        SetAvatarLayerWeightClientRpc(0);

        attackAble = true;
        damageRoutine = null;
    }

    void SyncMaterialsOnSpawn()
    {
        Character[] players = FindObjectsByType<Character>(FindObjectsSortMode.None);

        foreach (Character player in players)
        {
            if (player != this)
            {
                player.ApplyMaterial(player.team.Value);
            }
        }
    }

    void OnTeamValueChanged(int teamValue)
    {
        if (teamMaterials == null || teamMaterials.Length == 0)
        {
            return;
        }

        if (teamValue < 0 || teamValue >= teamMaterials.Length)
        {
            teamValue = 0;
        }

        if (Icons.Length > 0 && team.Value >= 0)
        {
            SetIcons(team.Value);
        }
        UpdateTeamMaterialClientRpc(teamValue);
        SyncMaterialsOnSpawn();
    }

    void ApplyMaterial(int teamIndex)
    {
        if (teamIndex < 0) return;
        Material targetMaterial = teamMaterials[teamIndex];
        foreach (var renderer in playerRenderers)
        {
            if (renderer != null)
            {
                renderer.material = targetMaterial;
            }
        }
    }

    [ClientRpc]
    void ApplyMaterialClientRpc(int teamIndex)
    {
        if (teamIndex < 0) return;
        foreach (var renderer in playerRenderers)
        {
            if (renderer != null)
            {
                renderer.material = teamMaterials[teamIndex];
            }
        }
    }

    [ClientRpc]
    void ApplyDamageMaterialClientRpc(int teamIndex)
    {
        if (teamIndex < 0) return;
        foreach (var renderer in playerRenderers)
        {
            if (renderer != null)
            {
                renderer.material = damageMaterials[teamIndex];
            }
        }
    }

    [ClientRpc]
    void UpdateTeamMaterialClientRpc(int teamIndex)
    {
        if (playerRenderers == null || playerRenderers.Length == 0)
        {
            Debug.LogError("Null");
            return;
        }

        Material targetMaterial = teamMaterials[teamIndex];

        foreach (var renderer in playerRenderers)
        {
            if (renderer != null)
            {
                renderer.material = new Material(teamMaterials[teamIndex]);
            }
        }
    }

    public void SetIcons(int teamIndex)
    {
        foreach (var icon in Icons)
        {
            icon.SetActive(false);
        }
        Icons[teamIndex].SetActive(true);
    }

    // ��� �޼���
    public void Die()
    {
        DropClientRpc();
        PlaySFXClientRpc(3, 0.75f);
        SetTriggerAnimation("Die");
    }

    // ������ �ݱ� �޼���
    public void PickUp()
    {
        targetItem = FindNearestItem();
        if (targetItem != null && holdItemAble)
        {
            attackAble = false;
            PickupItem();
        }
    }

    // ������ �������� �޼���
    public void Drop()
    {
        if (targetItem == null) return;

        attackAble = true;
        DropItem();
    }


    // ������ �������� �޼���
    [ClientRpc]
    public void DropClientRpc()
    {
        if (targetItem == null) return;

        attackAble = true;
        DropItem();
    }

    // �ڿ� ã�� (���� �� ���� ����� �ڿ�)
    GameObject FindNearestItem()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectItemRange);
        Collider[] item = colliders.Where(col => col.CompareTag("Item")).ToArray();

        GameObject nearestItem = null; // ����� �ڿ��� ������ ������Ʈ
        float minDistance = Mathf.Infinity; // ���� ����� �Ÿ��� ����, �ʱⰪ�� ���Ѵ� 

        foreach (Collider resource in item) // Ž���� ��� �ڿ� ��ȸ�ϸ�
        {
            float distance = Vector3.Distance(transform.position, resource.transform.position); // �հ� �� �ڿ��� �Ÿ� ���
            if (distance < minDistance) // ���� ���ȰŸ��� �ּ� �Ÿ����� ������
            {
                minDistance = distance; // �ּҰŸ��� ���� ��� �Ÿ� ������Ʈ
                nearestItem = resource.gameObject; // ������Ʈ�� �ڿ� ����
            }
        }
        return nearestItem; // �ּҰŸ��ڿ� ������Ʈ ��ȯ
    }

    // �������� �ݴ� �޼���
    void PickupItem()
    {
        // ���� ���߱� �� ����ִ� ����
        WeaponObjectActiveServerRpc(false);

        isHoldingItem = true;
        holdItemAble = false;

        // ������ ������Ʈ ��ġ��Ŵ
        targetItem.GetComponent<ResourceData>().SetParentOwnerserverRpc(GetComponent<NetworkObject>().NetworkObjectId, OwnerClientId, true, team.Value);
        targetItem.GetComponent<ResourceData>().lastHoldClientId = OwnerClientId;

        // �ݴ� �ִϸ��̼�
        SetAvatarLayerWeight(1);
        SetTriggerAnimation("Holding");

        PlaySFXServerRpc(4, 0.5f);
    }

    // �������� �������� �޼���
    void DropItem()
    {
        // ���� ���̱� �� ��� ���� ���� ����
        WeaponObjectActiveServerRpc(true);

        isHoldingItem = false;
        holdItemAble = true;

        // ������ ������Ʈ ��������
        targetItem.GetComponent<ResourceData>().SetParentOwnerserverRpc(GetComponent<NetworkObject>().NetworkObjectId, OwnerClientId, false, team.Value);
        targetItem = null;

        // �ִϸ��̼� ����
        SetAvatarLayerWeight(0);
        SetTriggerAnimation("Idle");

        PlaySFXServerRpc(4, 0.5f);
    }

    public void SwordSound()
    {
        PlaySFXServerRpc(2, 0.5f);
    }

    [ServerRpc]
    protected void PlaySFXServerRpc(int index, float volume = 1)
    {
        PlaySFXClientRpc(index, volume);
    }

    [ClientRpc]
    protected void PlaySFXClientRpc(int index, float volume = 1)
    {
        Debug.Assert(audioSource != null, $"{gameObject}: AudioSource is null");
        Debug.Assert(index >= 0 && index < soundData.soundClips.Length, $"{gameObject}: AudioClip is invalid");

        AudioClip clip = soundData.soundClips[index];
        Debug.Assert(clip != null, $"{gameObject}: AudioClip is not assigned");
        audioSource.volume = volume;
        audioSource.PlayOneShot(clip);
    }

    [ServerRpc]
    void WeaponObjectActiveServerRpc(bool state)
    {
        WeaponObjectActiveClientRpc(state);
    }

    [ClientRpc]
    void WeaponObjectActiveClientRpc(bool state)
    {
        weaponObject.SetActive(state);
    }

    public void InitializeAnimator()
    {
        networkAnimator.Animator.Rebind();
    }

    protected void SetAvatarLayerWeight(int value)
    {
        networkAnimator.Animator.SetLayerWeight(1, value);
    }

    protected void SetTriggerAnimation(string name)
    {
        networkAnimator.SetTrigger(name);
    }

    protected void SetFloatAnimation(string name, float value)
    {
        networkAnimator.Animator.SetFloat(name, value);
    }

    [ClientRpc]
    protected void SetAvatarLayerWeightClientRpc(int value)
    {
        networkAnimator.Animator.SetLayerWeight(1, value);
    }

    [ClientRpc]
    protected void SetTriggerAnimationClientRpc(string name)
    {
        networkAnimator.SetTrigger(name);
    }
}
