using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class OccupySystem : NetworkBehaviour
{
    // 적재한 자원
    [SerializeField] NetworkVariable<int> redTeamResourceCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] NetworkVariable<int> blueTeamResourceCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> hasBuilding = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // 채워야 할 자원
    const int resourceFillCount = 3;

    // 점령지 초기 상태
    public Owner currentOwner = Owner.Neutral; 

    // 이미지 위치
    [SerializeField] Image redTeamResourceCountImage;
    [SerializeField] Image blueTeamResourceCountImage;

    ResourceSpawner resourceSpawner;
    OccupyManager occupyManager;

    void Update()
    {
        if (IsServer)
            DetectResources();
    }

    public override void OnNetworkSpawn()
    {
        resourceSpawner = GameObject.FindFirstObjectByType<ResourceSpawner>();
        occupyManager = GameObject.FindFirstObjectByType<OccupyManager>();

        if (!IsServer)
        {
            redTeamResourceCount.OnValueChanged += (oldValue, newValue) => UpdateVisuals();
            blueTeamResourceCount.OnValueChanged += (oldValue, newValue) => UpdateVisuals();
        }
    }

    void DetectResources() // 점령지 내 자원 감지
    {
        if (currentOwner != Owner.Neutral) return;

        Collider[] colliders = Physics.OverlapSphere(transform.position, transform.localScale.x / 2);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Item"))
            {
                // Debug.Log($"{collider.name} 자원 감지 성공!"); 

                ResourceData data = collider.GetComponent<ResourceData>();
                if (data.isColliderEnable.Value)
                {
                    data.isColliderEnable.Value = false;
                    data.resourceCollider.enabled = false;
                }

                ulong resourceId = collider.GetComponent<NetworkObject>().NetworkObjectId;
                if (data.CurrentOwner == Owner.Red) redTeamResourceCount.Value++;
                else if (data.CurrentOwner == Owner.Blue) blueTeamResourceCount.Value++;
                
                Managers.Pool.Push(collider.gameObject);
                PushObjectClientRpc(resourceId);
                resourceSpawner.currentSpawnCount--;
                CheckOwnership(data.lastHoldClientId);
            }
        }
    }

    [ClientRpc(RequireOwnership = false)]
    public void PushObjectClientRpc(ulong resourceId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(resourceId, out NetworkObject resourceObject))
        {
            resourceObject.gameObject.SetActive(false);
            for (int i = 0; i < resourceObject.transform.childCount; i++)
            {
                resourceObject.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    void CheckOwnership(ulong clientId) // 점령지 소유권 검사
    {
        if (currentOwner == Owner.Neutral)
        {
            if (redTeamResourceCount.Value >= resourceFillCount)
                ChangeOwnership(Owner.Red,clientId);
            else if (blueTeamResourceCount.Value >= resourceFillCount)
                ChangeOwnership(Owner.Blue, clientId);
        }
    }

    void ChangeOwnership(Owner newOwner, ulong clientId)
    {
        currentOwner = newOwner;

        if (newOwner == Owner.Red)
        {
            occupyManager.UpdateOccupyCount(Owner.Red, 1);
        }
        else if (newOwner == Owner.Blue)
        {
            occupyManager.UpdateOccupyCount(Owner.Blue, 1);
        }
        PlayData buildPlayerData = GameManager.Instance.userPlayDatas[clientId];
        buildPlayerData.build++;
        GameManager.Instance.userPlayDatas[clientId] = buildPlayerData;

        InstantiateBuilding(newOwner);
        ChangeOwnershipClientRpc(newOwner);
    }

    [ClientRpc]
    void ChangeOwnershipClientRpc(Owner newOwner) // 중립 점령지의 소유권을 팀으로 변경 클라RPC
    {
        currentOwner = newOwner;
        if (newOwner == Owner.Neutral)
        {
            GetComponent<Renderer>().material.color = new Color(0, 0, 0);
        }
        else
        {
            GetComponent<Renderer>().material.color = (newOwner == Owner.Red) ? Color.red : Color.blue;
        }
    }

    void InstantiateBuilding(Owner newOwner)
    {
        ResetFillAmount();
        GameObject building = (newOwner == Owner.Red) ?
            Managers.Resource.Instantiate("BuildingRed", null, true) :
            Managers.Resource.Instantiate("BuildingBlue", null, true);

        building.transform.localPosition = new Vector3(0f, -40f, 0f);
        building.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);

        NetworkObject networkObject = building.GetComponent<NetworkObject>();
        networkObject.TrySetParent(transform.GetComponent<NetworkObject>());
        ActiveNetworkObjectClientRpc(networkObject.NetworkObjectId, true);
        building.GetComponent<Building>().BuildingInit();

        if (IsServer)
        {
            hasBuilding.Value = true;
        }
    }

    [ClientRpc]
    public void ActiveNetworkObjectClientRpc(ulong networkObjectId, bool isActive)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject no))
        {
            no.gameObject.SetActive(isActive);
        }
    }

    void UpdateVisuals() // 자원 적재 시각 효과
    {
        redTeamResourceCountImage.fillAmount = Mathf.Clamp((float)redTeamResourceCount.Value / resourceFillCount, 0f, 1f);
        blueTeamResourceCountImage.fillAmount = Mathf.Clamp((float)blueTeamResourceCount.Value / resourceFillCount, 0f, 1f);
    }

    void ResetFillAmount() // 자원 적재 시각 효과 초기화
    {
        redTeamResourceCount.Value = 0;
        blueTeamResourceCount.Value = 0;
    }

    public void ResetOwnership() 
    {
        if (currentOwner == Owner.Red)
        {
            occupyManager.UpdateOccupyCount(Owner.Red, -1);
        }
        else if (currentOwner == Owner.Blue)
        {
            occupyManager.UpdateOccupyCount(Owner.Blue, -1);
        }

        currentOwner = Owner.Neutral;
        ChangeOwnershipClientRpc(currentOwner);

        redTeamResourceCount.Value = 0;
        blueTeamResourceCount.Value = 0;

        if (IsServer)
        {
            hasBuilding.Value = false;
        }
    }
}