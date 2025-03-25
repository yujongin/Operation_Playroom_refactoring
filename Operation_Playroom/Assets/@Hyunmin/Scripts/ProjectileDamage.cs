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
        // 본인을 타격했을경우 리턴
        if (other.TryGetComponent<NetworkObject>(out NetworkObject obj))
        {
            if (ownerClientId == obj.OwnerClientId)
            {
                return;
            }
        }

        // 같은 팀 타격 시 리턴
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

        // 건물 타격 시 데미지
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

        // 상대 팀 타격 시 데미지
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
