using Unity.Netcode;
using UnityEngine;

public class ProjectileOnDestroy : MonoBehaviour
{
    ulong ownerClientId;

    public void SetOwner(ulong ownerClientId)
    {
        this.ownerClientId = ownerClientId;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<NetworkObject>(out NetworkObject obj))
        {
            if (ownerClientId == obj.OwnerClientId)
            {
                return;
            }
        }

        if (other.TryGetComponent<Health>(out Health health))
        {
            Managers.Pool.Push(gameObject);
        }
    }
}
