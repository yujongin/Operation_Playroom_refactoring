using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class ResourceSpawner : NetworkBehaviour
{
    [SerializeField] GameObject resourceParent;
    [SerializeField] int initSpawnCount;
    [SerializeField] LayerMask layerMask;

    public int currentSpawnCount;
    public override void OnNetworkSpawn() 
    {
        if (IsServer)
        {
            InitSpawnResource(initSpawnCount);
            StartCoroutine(SpawnResourceRoutine());
        }
    }

    void Update()
    {

    }
    Collider[] itemBuffer = new Collider[1];
    public void InitSpawnResource(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnResource();
        }
    }

    IEnumerator SpawnResourceRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (currentSpawnCount < initSpawnCount)
            {
                SpawnResource();
            }
        }
    }

    Vector3 GetRandomSpawnPos()
    {
        int attempts = 0; // 시도 횟수를 추적하는 변수

        while (100 > attempts)
        {
            Vector3 randomPosition = new Vector3(Random.Range(-4f, 4f), 0, Random.Range(-4f, 4f));
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPosition, out hit, 1.0f, NavMesh.AllAreas))
            {
                int numColliders = Physics.OverlapSphereNonAlloc(hit.position, 0.5f, itemBuffer, layerMask);
                if (numColliders == 0)
                {
                    return hit.position;
                }
                else
                {
                    attempts++;
                }
            }
        }

        return Vector3.zero;
    }

    [ClientRpc]
    private void NotifyResourceSpawnedClientRpc(ulong networkObjectId)
    {
        GameObject resourceObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId].gameObject;
        resourceObject.SetActive(true);
        StartCoroutine(DelayActiveChild(resourceObject));
    }
    public void SpawnResource()
    {
        Vector3 randomPos = GetRandomSpawnPos();
        if (randomPos == Vector3.zero) return;

        GameObject go = Managers.Resource.Instantiate("ResourcePrefab", null, true);
        if (go.transform.parent != resourceParent)
            go.GetComponent<NetworkObject>().TrySetParent(resourceParent, true);
        go.transform.position = new Vector3(randomPos.x, 0, randomPos.z);

        DelayActiveChild(go);
        NotifyResourceSpawnedClientRpc(go.GetComponent<NetworkObject>().NetworkObjectId);
        currentSpawnCount++;
    }  

    IEnumerator DelayActiveChild(GameObject go)
    {
        yield return new WaitForSeconds(0.5f);
        go.transform.GetChild(0).gameObject.SetActive(true);
        go.GetComponent<ResourceData>().resourceCollider.enabled = true;
    }
}
