using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class WeaponDamage : NetworkBehaviour
{
    [SerializeField] int damage;

    ulong ownerClientId;
    int ownerTeam;

    bool isCollision;
    NoiseCheckManager noise;
    public void SetOwner(ulong ownerClientId, int ownerTeam, int damage)
    {
        this.ownerClientId = ownerClientId;
        this.ownerTeam = ownerTeam;
        this.damage = damage;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isCollision) 
        {
            // ������ �ƴ� ��� ����
            if (!IsOwner) return;

            // ������ Ÿ��������� ����
            if (other.TryGetComponent<NetworkObject>(out NetworkObject obj))
            {
                if (ownerClientId == obj.OwnerClientId)
                {
                    return;
                }
            }

            // ���� �� Ÿ�� �� ����
            if (other.TryGetComponent<Character>(out Character character))
            {
                if (character.team.Value == -1)
                {
                    return;
                }
                if (ownerTeam == character.team.Value)
                {
                    return;
                }
            }

            // �ǹ� Ÿ�� �� ������
            if (other.TryGetComponent<Building>(out Building building))
            {
                // ���� �� �ǹ� Ÿ�� ��
                Owner myTeam = ownerTeam == 0 ? Owner.Blue : Owner.Red;

                if (myTeam != building.buildingOwner)
                {
                    isCollision = true;

                    building.TakeDamageServerRpc(damage, ownerClientId);
                    GetComponentInParent<Character>().SwordSound();

                    if(noise ==null)
                        noise = FindFirstObjectByType<NoiseCheckManager>();
                    noise.AddNoiseGage(1.5f);

                    StartCoroutine(ResetCollisionRoutine());
                }
            }

            // ��� �� Ÿ�� �� ������
            if (other.TryGetComponent<Health>(out Health health))
            {
                isCollision = true;
                GetComponentInParent<Character>().SwordSound();

                if (health.IsServer)
                {
                    health.TakeDamage(damage, ownerClientId);
                }
                else
                {
                    health.TakeDamageServerRpc(damage, ownerClientId);
                }
                if (noise == null)
                    noise = FindFirstObjectByType<NoiseCheckManager>();
                noise.AddNoiseGage(1.5f);

                StartCoroutine(ResetCollisionRoutine());
            }
        }
    }

    // ������ �ݶ��̴��� ��Ÿ���� �����ϴ� ��ƾ
    IEnumerator ResetCollisionRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        isCollision = false;
    }
}
