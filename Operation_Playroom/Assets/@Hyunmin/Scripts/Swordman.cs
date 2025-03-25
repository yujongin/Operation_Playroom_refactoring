using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Swordman : Character
{
    [SerializeField] GameObject swordHitbox;
    [SerializeField] int damage;
    public float attackCooldown = 1f;

    IEnumerator attackRoutine;

    public override void Start()
    {
        base.Start();
        InitializeCharacterStat();
    }

    // ���� �޼���
    public override void Attack()
    {
        // �� �ֵθ��� ����
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
        }
        attackRoutine = SwordAttack();
        StartCoroutine(SwordAttack());
    }

    // Ű �Է� �޼���
    public override void HandleInput()
    {
        // ����
        if (Input.GetButtonDown("Attack"))
        {
            // �� �ֵθ��� ����
            if (attackAble)
            {
                Attack();

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
        // ������ ��������
        if (isHoldingItem)
        {
            Drop();
        }
        // ������ ���
        else
        {
            PickUp();
        }
    }

    // Į ���� �ڷ�ƾ
    IEnumerator SwordAttack()
    {
        swordHitbox.GetComponent<WeaponDamage>().SetOwner(OwnerClientId, team.Value, damage);

        SetAvatarLayerWeight(1); // ��ü ���������� ����

        attackAble = false; // ����� ��Ȱ��ȭ
        holdItemAble = false;

        SetTriggerAnimation("SwordAttack"); // ���� ��� ����

        yield return new WaitForSeconds(0.4f);

        EnableHitboxServerRpc(true);
        PlaySFXServerRpc(5, 0.25f);

        yield return new WaitForSeconds(0.5f);

        EnableHitboxServerRpc(false);

        yield return new WaitForSeconds(0.4f);

        attackAble = true; // ���� ����
        holdItemAble = true;

        SetAvatarLayerWeight(0); // ��ü ������ ����
    }

    [ServerRpc]
    void EnableHitboxServerRpc(bool state)
    {
        EnableHitboxClientRpc(state);
    }

    [ClientRpc]
    void EnableHitboxClientRpc(bool state)
    {
        swordHitbox.GetComponent<Collider>().enabled = state;
    }

    // ĳ���� ������ �ʱ�ȭ�ϴ� �޼���
    public void InitializeCharacterStat()
    {
        if (Managers.Data.UnitDic.TryGetValue(201000, out Data.UnitData swordmanData))
        {
            health.SetHp((int)swordmanData.HP);
            damage = (int)swordmanData.Atk;
        }
        else
        {
            Debug.LogError("Swordman ������(ID: 201000)�� ã�� �� �����ϴ�.");
        }
    }
}
