using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class OccupyManager : NetworkBehaviour
{
    [SerializeField] GameObject occupyPrefab;
    [SerializeField] Transform occupyPoints;
    [SerializeField] Transform occupyPool;

    [SerializeField] TextMeshProUGUI redTeamOccupyCountText;
    [SerializeField] TextMeshProUGUI blueTeamOccupyCountText;

    public NetworkVariable<int> redTeamOccupyCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> blueTeamOccupyCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            GenerateOccupy();
        }

        if (IsClient)
        {
            redTeamOccupyCount.OnValueChanged += OnRedTeamOccupyCountChanged;
            blueTeamOccupyCount.OnValueChanged += OnBlueTeamOccupyCountChanged;

            UpdateUI();
        }
    }

    private void OnRedTeamOccupyCountChanged(int oldValue, int newValue)
    {
        UpdateUI();
    }

    private void OnBlueTeamOccupyCountChanged(int oldValue, int newValue)
    {
        UpdateUI();
    }

    private void GenerateOccupy()
    {
        foreach (Transform child in occupyPoints)
        {
            GameObject occupyInstance = Managers.Resource.Instantiate("Occupy");
            NetworkObject networkObject = occupyInstance.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                networkObject.TrySetParent(occupyPool.GetComponent<NetworkObject>());
            }
            occupyInstance.transform.position = child.position;
        }
    }

    public List<Transform> GetRandomPoints()
    {
        List<Transform> points = new List<Transform>();
        for (int i = 0; i < occupyPool.childCount; i++)
        {
            if (occupyPool.GetChild(i).TryGetComponent<OccupySystem>(out OccupySystem occupySystem))
            {
                if (occupySystem.currentOwner != Owner.Neutral)
                {
                    points.Add(occupyPool.GetChild(i));
                }
            }
        }

        if (points.Count <= 3)
        {
            return points;
        }
        else
        {
            while (points.Count > 3)
            {
                int num = Random.Range(0, points.Count);
                points.RemoveAt(num);
            }
        }

        return points;
    }

    public void UpdateOccupyCount(Owner owner, int amount)
    {
        if (IsServer)
        {
            if (owner == Owner.Red)
            {
                redTeamOccupyCount.Value += amount;
            }
            else if (owner == Owner.Blue)
            {
                blueTeamOccupyCount.Value += amount;
            }
        }
    }

    private void UpdateUI()
    {
        redTeamOccupyCountText.text = $"{redTeamOccupyCount.Value}";
        blueTeamOccupyCountText.text = $"{blueTeamOccupyCount.Value}";
    }
}