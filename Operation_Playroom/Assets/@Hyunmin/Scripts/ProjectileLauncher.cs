using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ProjectileLauncher : NetworkBehaviour
{
    [SerializeField] GameObject serverProjectile;
    [SerializeField] GameObject clientProjectile;
    [SerializeField] Character character;

    public float flightTime = 3f;

    public void ShootArrow(Transform shootPoint)
    {
        FireDummyProjectile(shootPoint.position, shootPoint.forward);
        FireServerRpc(shootPoint.position, shootPoint.forward);
    }

    [ServerRpc]
    void FireServerRpc(Vector3 spawnPoint, Vector3 direction)
    {
        // 서버에서 실제 발사체 생성(데미지 처리)
        GameObject arrow = Managers.Pool.Pop(serverProjectile);

        Archer archer = GetComponentInParent<Archer>();
        arrow.GetComponent<ProjectileDamage>().SetOwner(OwnerClientId, character.team.Value, archer.damage);

        arrow.GetComponent<Projectile>().Launch(spawnPoint, direction);

        // 클라이언트에 동기화
        FireClientRpc(spawnPoint, direction); 
    }

    [ClientRpc]
    void FireClientRpc(Vector3 spawnPoint, Vector3 direction)
    {
        // 클라이언트에서 더미 화살 생성(시각적 처리)
        if(IsOwner) return;

        FireDummyProjectile(spawnPoint, direction);
    }

    void FireDummyProjectile(Vector3 spawnPoint, Vector3 direction)
    {
        GameObject arrow = Managers.Pool.Pop(clientProjectile);
        arrow.GetComponent<ProjectileOnDestroy>().SetOwner(OwnerClientId);

        arrow.GetComponent<Projectile>().Launch(spawnPoint, direction);
    }

}
