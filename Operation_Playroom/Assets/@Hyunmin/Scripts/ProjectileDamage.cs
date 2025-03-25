using Unity.Netcode;
using UnityEngine;

public class ProjectileDamage : MonoBehaviour
{
    [SerializeField] int damage;

    ulong ownerClientId;
    int ownerTeam;

    NoiseCheckManager noise;
    public void SetOwner(ulong ownerClientId, int ownerTeam, int damage)
    {
        this.ownerClientId = ownerClientId;
        this.ownerTeam = ownerTeam;
        this.damage = damage;
    }

    void OnTriggerEnter(Collider other)
    {
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
            Owner myTeam = ownerTeam == 0 ? Owner.Blue : Owner.Red;

            if (myTeam != building.buildingOwner)
            {
                building.TakeDamage(damage, ownerClientId);

                if (noise == null)
                    noise = FindFirstObjectByType<NoiseCheckManager>();
                noise.SubmitNoiseTo(1.5f);

            }
            Managers.Pool.Push(gameObject);
        }

        // ��� �� Ÿ�� �� ������
        if (other.TryGetComponent<Health>(out Health health))
        {
            health.TakeDamage(damage, ownerClientId);

            if (noise == null)
                noise = FindFirstObjectByType<NoiseCheckManager>();
            noise.SubmitNoiseTo(1.5f);

            Managers.Pool.Push(gameObject);
        }

    }
}
