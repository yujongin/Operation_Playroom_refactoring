using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerRespawnManager : NetworkBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        
        StartCoroutine(SpawnPlayerRoutine());
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return; 

        PlayerController.OnPlayerSpawn -= HandlePlayerSpawn;
        PlayerController.OnPlayerDespawn -= HandlePlayerDespawn;
    }

    private void HandlePlayerSpawn(PlayerController player)
    {
        player.GetComponent<Health>().OnDie -= HandlePlayerDie;
        player.GetComponent<Health>().OnDie += HandlePlayerDie;
    }

    private void HandlePlayerDespawn(PlayerController player)
    {
        player.GetComponent<Health>().OnDie -= HandlePlayerDie;
    }

    private void HandlePlayerDie(Health sender)
    {
        PlayerController player = sender.GetComponent<PlayerController>();

        StartCoroutine(RespawnPlayerRoutine(player));
    }

    // ������ ��ƾ
    IEnumerator RespawnPlayerRoutine(PlayerController player)
    {
        if (player.GetComponent<KingTest>() != null) yield break;

        float respawnTime = 10f;
        while (respawnTime > 0)
        {
            UpdateTimerTextClientRpc(player.NetworkObject, respawnTime);
            yield return new WaitForSeconds(1f);
            respawnTime -= 1f;
        }

        Character character = player.GetComponent<Character>();
        Health health = player.GetComponent<Health>();

        GameTeam gameTeam = (GameTeam)character.team.Value;
        Vector3 spawnPosition = SpawnPoint.GetSpawnPoint(gameTeam, GameRole.King);

        // �������� ��ġ ����
        player.transform.position = spawnPosition;

        // �ٽ� �÷��� ���� ����
        player.isPlayable = true;
        health.InitializeHealth();
        character.InitializeAnimator();

        // Ŭ���̾�Ʈ�� ��ġ ����ȭ
        UpdatePlayerStateClientRpc(player.NetworkObject, spawnPosition);
    }

    [ClientRpc]
    void UpdateTimerTextClientRpc(NetworkObjectReference playerRef, float time)
    {
        if (playerRef.TryGet(out NetworkObject playerObj))
        {
            if (playerObj.IsOwner)
            {
                if (timerText != null)
                {
                    timerText.gameObject.SetActive(true);
                    timerText.text = time.ToString();
                }
            }
        }
    }

    [ClientRpc]
    void UpdatePlayerStateClientRpc(NetworkObjectReference playerRef, Vector3 position)
    {
        if (playerRef.TryGet(out NetworkObject playerObj))
        {
            PlayerController player = playerObj.GetComponent<PlayerController>();
            Character character = player.GetComponent<Character>();
            Health health = player.GetComponent<Health>();

            // Ŭ���̾�Ʈ���� ��ġ, ����, �ִϸ����� �ʱ�ȭ
            playerObj.transform.position = position;
            player.isPlayable = true;
            health.InitializeHealth();
            character.InitializeAnimator();

            // Ÿ�̸� ����
            if (playerObj.IsOwner)
            {
                if (timerText != null)
                {
                    timerText.gameObject.SetActive(false);
                }
            }
        }
    }

    // �÷��̾� ���� ��ƾ
    IEnumerator SpawnPlayerRoutine()
    {
        yield return new WaitForSeconds(5f);

        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (PlayerController player in players)
        {
            HandlePlayerSpawn(player);
        }

        PlayerController.OnPlayerSpawn -= HandlePlayerSpawn;
        PlayerController.OnPlayerDespawn -= HandlePlayerDespawn;
        PlayerController.OnPlayerSpawn += HandlePlayerSpawn;
        PlayerController.OnPlayerDespawn += HandlePlayerDespawn;
    }

    // �÷��̾�� �������� ����
    public void ReTransformPostion()
    {
        // �÷��̾� ã��
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        Vector3 spawnPosition = Vector3.zero;

        // ĳ���� �� ��ġ�� �̵�
        foreach (PlayerController player in players)
        {
            Character character = player.GetComponent<Character>();
            GameTeam gameTeam = (GameTeam)character.team.Value;

            if (player.GetComponent<Swordman>())
            {
                spawnPosition = SpawnPoint.GetSpawnPoint(gameTeam, GameRole.Swordman);
            }
            else if (player.GetComponent<Archer>())
            {
                spawnPosition = SpawnPoint.GetSpawnPoint(gameTeam, GameRole.Archer);
            }
            else if (player.GetComponent<KingTest>())
            {
                spawnPosition = SpawnPoint.GetSpawnPoint(gameTeam, GameRole.King);
            }

            player.transform.position = spawnPosition;
            UpdatePlayerTransformClientRpc(player.NetworkObject, spawnPosition);
        }

    }

    // Ŭ���̾�Ʈ���� ��ġ �ʱ�ȭ
    [ClientRpc]
    void UpdatePlayerTransformClientRpc(NetworkObjectReference playerRef, Vector3 position)
    {
        if (playerRef.TryGet(out NetworkObject playerObj))
        {
            
            playerObj.transform.position = position;
        }
    }
}
