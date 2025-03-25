using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class GameManager : NetworkBehaviour
{
    private static GameManager instance;

    public static GameManager Instance { get { return instance; } }

    int myTeam;

    public NetworkVariable<float> remainTime = new NetworkVariable<float>();
    public TMP_Text notiText;
    public TMP_Text timerText;
    public Image circleImage;
    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject drawPanel;

    public GameObject[] doors;
    public GameObject[] roleImages;
    public GameObject[] playDataPanelResultImages;

    public CinemachineCamera[] kingCams;

    public GameObject resultPlayDataPanel;
    public Transform bluePlayDataUIParent;
    public GameObject bluePlayDataUIPrefab;
    public Transform redPlayDataUIParent;
    public GameObject redPlayDataUIPrefab;

    EGameState gameState;

    Sequence textSequence;

    PlayerController[] players;

    PlayerRespawnManager respawnManager;
    OccupyManager occupyManager;

    public Dictionary<ulong, PlayData> userPlayDatas;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public override void OnNetworkSpawn()
    {
        gameState = EGameState.Ready;
        textSequence = DOTween.Sequence();
        textSequence.Append(notiText.DOFade(1, 1));
        textSequence.AppendInterval(2f);
        textSequence.Append(notiText.DOFade(0, 1))
            .SetAutoKill(false).Pause();

        respawnManager = FindFirstObjectByType<PlayerRespawnManager>();
        occupyManager = FindFirstObjectByType<OccupyManager>();

        if (IsServer)
        {
            remainTime.Value = 420f;
            StartCoroutine(CallReadyMessage());

        }
        else
        {
            ActiveRoleInfoImage();
            myTeam = (int)ClientSingleton.Instance.UserData.userGamePreferences.gameTeam;
            circleImage.rectTransform.DOSizeDelta(new Vector2(5000, 5000), 3f);
            remainTime.OnValueChanged -= OnChangeTimer;
            remainTime.OnValueChanged += OnChangeTimer;
        }

    }

    public override void OnNetworkDespawn()
    {
        remainTime.OnValueChanged -= OnChangeTimer;
    }


    void Update()
    {
        if (gameState != EGameState.Play) return;
    }

    [ClientRpc]
    private void SubmitPlayDataClientRpc(PlayData data, ClientRpcParams rpcParams = default)
    {
        MakePlayDataUI(data.team, data);
    }

    void MakePlayDataUI(GameTeam team, PlayData data)
    {
        GameObject playDataUI;
        Transform parent;
        if (team == GameTeam.Blue)
        {
            parent = bluePlayDataUIParent;
            playDataUI = Instantiate(bluePlayDataUIPrefab, bluePlayDataUIParent);
        }
        else
        {
            parent = redPlayDataUIParent;
            playDataUI = Instantiate(redPlayDataUIPrefab, redPlayDataUIParent);
        }

        playDataUI.transform.Find("Name").GetComponent<TMP_Text>().text = data.name.Split('#')[0];
        playDataUI.transform.Find("Kill").GetComponent<TMP_Text>().text = data.kill.ToString();
        playDataUI.transform.Find("Death").GetComponent<TMP_Text>().text = data.death.ToString();
        playDataUI.transform.Find("Build").GetComponent<TMP_Text>().text = data.build.ToString();
        playDataUI.transform.Find("Destroy").GetComponent<TMP_Text>().text = data.destroy.ToString();

        int toIndex = (int)data.role;
        if (parent.transform.childCount <= toIndex)
        {
            return;
        }
        playDataUI.transform.SetSiblingIndex(toIndex);
    }

    private string GetFormattedTime(float time)
    {
        int min = Mathf.FloorToInt(time / 60);
        int sec = Mathf.FloorToInt(time % 60);
        return sec >= 10 ? $"{min} : {sec}" : $"{min} : 0{sec}";
    }

    IEnumerator TimerRoutine()
    {
        while (remainTime.Value > 0)
        {
            yield return new WaitForSeconds(1f);
            remainTime.Value -= 1.0f;
        }
        TimeOverClientRpc();
        foreach (var playData in userPlayDatas.Values)
        {
            SubmitPlayDataClientRpc(playData);
        }
        yield return null;
    }

    public void OnChangeTimer(float oldVal, float newVal)
    {
        timerText.text = GetFormattedTime(newVal);
    }
    IEnumerator CallReadyMessage()
    {
        float time = 15;
        CallNotiTextClientRpc("곧 게임이 시작됩니다!");
        while (time > 1)
        {
            yield return new WaitForSeconds(1);
            time--;

            if (time <= 10)
            {
                CallNotiTextClientRpc(time.ToString());
            }
            yield return null;
        }
        yield return new WaitForSeconds(1);
        CallNotiTextClientRpc("Start!");

        userPlayDatas = new Dictionary<ulong, PlayData>();
        foreach (var clientIdToUserData in ServerSingleton.Instance.clientIdToUserData)
        {
            ulong clientId = clientIdToUserData.Key;
            UserData userData = clientIdToUserData.Value;
            PlayData playData = new PlayData(userData.userName, userData.userGamePreferences.gameRole, userData.userGamePreferences.gameTeam);
            userPlayDatas.Add(clientId, playData);
        }

        DoorOpenClientRpc();
        gameState = EGameState.Play;
        StartCoroutine(TimerRoutine());
    }
    public void ActiveRoleInfoImage()
    {
        int role = (int)ClientSingleton.Instance.UserData.userGamePreferences.gameRole;

        roleImages[role].SetActive(true);
    }

    [ClientRpc]
    public void DoorOpenClientRpc()
    {
        for (int i = 0; i < doors.Length; i++)
        {
            doors[i].GetComponentInChildren<Collider>().enabled = false;  
            doors[i].SetActive(false);
        }
    }
    [ClientRpc]
    public void AllPlayerStopClientRpc(bool isStop, bool isSoldierSpawn = false)
    {
        players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        for (int i = 0; i < players.Length; i++)
        {
            players[i].isPlayable = isStop;
            if (!isSoldierSpawn) continue;


            if (players[i].gameObject.GetComponent<KingTest>() != null)
            {
                KingTest king = players[i].gameObject.GetComponent<KingTest>();
                StartCoroutine(SoldierSpawnDelay(king));
            }

        }
    }

    IEnumerator SoldierSpawnDelay(KingTest king)
    {
        yield return new WaitForSeconds(1);
        king.CommandSoldierToWarp();
    }

    public void AllPlayerRespawn()
    {
        AllPlayerStopClientRpc(false, true);
        //리스폰
        respawnManager.ReTransformPostion();

    }

    public void OnKingDead(Health health)
    {
        KingDeadRoutineClientRpc(health.GetComponent<Character>().team.Value);
        foreach (var playData in userPlayDatas.Values)
        {
            SubmitPlayDataClientRpc(playData);
        }
    }

    [ClientRpc]
    void KingDeadRoutineClientRpc(int team)
    {
        Sequence kingDeadSeq = DOTween.Sequence();
        kingDeadSeq.SetUpdate(true);
        kingDeadSeq.AppendCallback(() =>
        {
            kingCams[team].Priority = 10;
            Time.timeScale = 0.5f;

        })
        .AppendInterval(3f)
        .AppendCallback(() =>
        {
            AllPlayerStopClientRpc(false);
            Time.timeScale = 1f;
        })
        .AppendInterval(1f)
        .AppendCallback(() =>
        {
            kingCams[team].Priority = 0;
            if (myTeam == team)
            {
                losePanel.SetActive(true);
            }
            else
            {
                winPanel.SetActive(true);
            }


            //foreach (var playData in userPlayDatas.Values)
            //{
            //    SubmitPlayDataClientRpc(playData);
            //}
        })
        .AppendInterval(3f)
        .AppendCallback(() =>
        {
            if (team == 0)
            {
                playDataPanelResultImages[1].SetActive(true);
            }
            else
            {
                playDataPanelResultImages[0].SetActive(true);
            }
            resultPlayDataPanel.SetActive(true);
        })
        .AppendInterval(10f)
        .AppendCallback(() =>
        {
            circleImage.rectTransform.DOSizeDelta(new Vector2(0, 0), 1f);
        })
        .AppendInterval(2f)
        .AppendCallback(() =>
         {
             NetworkManager.Singleton.Shutdown();
         });
    }

    [ClientRpc]
    void TimeOverClientRpc()
    {
        Sequence gameOverSeq = DOTween.Sequence();
        gameOverSeq.AppendCallback(() =>
        {
            AllPlayerStopClientRpc(false);
        })
        .AppendInterval(1f)
        .AppendCallback(() =>
        {
            int redPoint = occupyManager.redTeamOccupyCount.Value;
            int bluePoint = occupyManager.blueTeamOccupyCount.Value;

            if (redPoint == bluePoint)
            {
                drawPanel.SetActive(true);
                playDataPanelResultImages[2].SetActive(true);
            }
            else
            {
                int winner = redPoint > bluePoint ? 1 : 0;

                if (myTeam == winner)
                {
                    winPanel.SetActive(true);
                }
                else
                {
                    losePanel.SetActive(true);
                }
                if (winner == 0)
                {
                    playDataPanelResultImages[0].SetActive(true);
                }
                else
                {
                    playDataPanelResultImages[1].SetActive(true);
                }
            }


        })
        .AppendInterval(3f)
        .AppendCallback(() =>
        {
            resultPlayDataPanel.SetActive(true);
        })
        .AppendInterval(10f)

        .AppendCallback(() =>
        {
            circleImage.rectTransform.DOSizeDelta(new Vector2(0, 0), 1f);
        })
        .AppendInterval(2f)
        .AppendCallback(() =>
        {
            NetworkManager.Singleton.Shutdown();
        });
    }

    [ClientRpc]
    public void CallNotiTextClientRpc(string text)
    {
        notiText.text = text;
        textSequence.Restart();
    }
}

[Serializable]
public struct PlayData : INetworkSerializable
{
    public string name;
    public GameRole role;
    public GameTeam team;
    public int kill;
    public int death;
    public int build;
    public int destroy;

    public PlayData(string name, GameRole role, GameTeam team)
    {
        this.name = name;
        this.role = role;
        this.team = team;
        this.kill = 0;
        this.death = 0;
        this.build = 0;
        this.destroy = 0;
        this.role = role;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref role);
        serializer.SerializeValue(ref team);
        serializer.SerializeValue(ref kill);
        serializer.SerializeValue(ref death);
        serializer.SerializeValue(ref build);
        serializer.SerializeValue(ref destroy);
    }
}