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

    // 리스폰 루틴
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

        // 서버에서 위치 설정
        player.transform.position = spawnPosition;

        // 다시 플레이 가능 설정
        player.isPlayable = true;
        health.InitializeHealth();
        character.InitializeAnimator();

        // 클라이언트에 위치 동기화
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

            // 클라이언트에서 위치, 상태, 애니메이터 초기화
            playerObj.transform.position = position;
            player.isPlayable = true;
            health.InitializeHealth();
            character.InitializeAnimator();

            // 타이머 해제
            if (playerObj.IsOwner)
            {
                if (timerText != null)
                {
                    timerText.gameObject.SetActive(false);
                }
            }
        }
    }

    // 플레이어 스폰 루틴
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

    // 플레이어들 본진으로 스폰
    public void ReTransformPostion()
    {
        // 플레이어 찾기
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        Vector3 spawnPosition = Vector3.zero;

        // 캐릭터 별 위치로 이동
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

    // 클라이언트에서 위치 초기화
    [ClientRpc]
    void UpdatePlayerTransformClientRpc(NetworkObjectReference playerRef, Vector3 position)
    {
        if (playerRef.TryGet(out NetworkObject playerObj))
        {
            
            playerObj.transform.position = position;
        }
    }
}
