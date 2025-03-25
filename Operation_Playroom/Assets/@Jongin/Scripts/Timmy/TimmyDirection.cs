using DG.Tweening;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class TimmyDirection : NetworkBehaviour
{
    public Image fadeImage;
    private Color imageColor;

    public CinemachineCamera[] cameras;
    private int activeCameraIndex = 0;

    public GameObject sleepTimmyPrefab;
    public GameObject moveTimmyPrefab;

    SleepTimmy sleepTimmy;
    MoveTimmy moveTimmy;

    public ETimmyState timmyState;


    public NetworkVariable<float> fadeAlpha = new NetworkVariable<float>(0);
    public NetworkVariable<int> cameraIndex = new NetworkVariable<int>(0);
    OccupyManager occupyManager;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            occupyManager = FindFirstObjectByType<OccupyManager>();
            GameObject sleepTimmyObject = Instantiate(sleepTimmyPrefab);
            sleepTimmyObject.GetComponent<NetworkObject>().Spawn();
            sleepTimmy = sleepTimmyObject.GetComponent<SleepTimmy>();

            timmyState = ETimmyState.Sleep;

        }
        if (IsClient)
        {
            imageColor = fadeImage.color;
            imageColor.a = 0; // ���� �� ����
            fadeImage.color = imageColor;

            cameraIndex.OnValueChanged -= ActiveCamera;
            cameraIndex.OnValueChanged += ActiveCamera;

            fadeAlpha.OnValueChanged -= ChangeImageAlpha;
            fadeAlpha.OnValueChanged += ChangeImageAlpha;
        }
    }

    public override void OnNetworkDespawn()
    {
        cameraIndex.OnValueChanged -= ActiveCamera;
        fadeAlpha.OnValueChanged -= ChangeImageAlpha;
        if (sleepTimmy != null && sleepTimmy.GetComponent<NetworkObject>().IsSpawned)
        {
            sleepTimmy.GetComponent<NetworkObject>().Despawn();
        }
        if (moveTimmy != null && moveTimmy.GetComponent<NetworkObject>().IsSpawned)
        {
            moveTimmy.GetComponent<NetworkObject>().Despawn();
        }
    }


    public void StartTimmy()
    {
        Sequence timmySequence = DOTween.Sequence();

        //Fade in
        timmySequence.Append(DOTween.To(() => fadeAlpha.Value, x => SetAlpha(x), 1f, 0.5f));
        timmySequence.AppendCallback(() =>
        {
            //ȭ�� ��ȯ
            cameraIndex.Value = 1;
            GameManager.Instance.AllPlayerRespawn();
        });
        timmySequence.AppendInterval(1f);
        //fade out
        timmySequence.Append(DOTween.To(() => fadeAlpha.Value, x => SetAlpha(x), 0f, 0.5f));


        timmySequence.AppendCallback(() =>
        {
            sleepTimmy.animator.SetTrigger("WakeUp");
        });
        timmySequence.AppendInterval(3f);

        //�ڴ� Ƽ�̿� �����̴� Ƽ�� ��ü
        timmySequence.Append(DOTween.To(() => fadeAlpha.Value, x => SetAlpha(x), 1f, 0.5f));
        timmySequence.AppendCallback(() =>
        {
            //�ڴ� Ƽ�� �ʱ�ȭ �� ����
            sleepTimmy.GetComponent<Animator>().SetTrigger("Sleep");
            sleepTimmy.timmyActive.Value = false;
            if (moveTimmy == null)
            {
                GameObject moveTimmyObject = Instantiate(moveTimmyPrefab);
                moveTimmyObject.GetComponent<NetworkObject>().Spawn();
                moveTimmy = moveTimmyObject.GetComponent<MoveTimmy>();
            }
            moveTimmy.path = occupyManager.GetRandomPoints();
            moveTimmy.timmyActive.Value = true;
            cameraIndex.Value = 2;
        });

        timmySequence.AppendInterval(1f);

        timmySequence.Append(DOTween.To(() => fadeAlpha.Value, x => SetAlpha(x), 0f, 0.5f));

        //Ƽ�� �����̱�
        timmySequence.AppendCallback(() =>
        {
            if (moveTimmy.path.Count > 0)
            {
                moveTimmy.GetComponent<MoveTimmy>().CallTimmy(FinishTimmy);
            }
            else
            {
                GameManager.Instance.CallNotiTextClientRpc("������ �ǹ��� ����..");
                FinishTimmy();
            }
        });



    }

    public void FinishTimmy()
    {
        Sequence timmySequence = DOTween.Sequence();

        //���� ȭ�� ���� �� Ƽ�� �ʱ�ȭ
        timmySequence.AppendInterval(2.5f);
        timmySequence.Append(DOTween.To(() => fadeAlpha.Value, x => SetAlpha(x), 1f, 0.5f));
        timmySequence.AppendCallback(() =>
        {
            cameraIndex.Value = 0;
            sleepTimmy.timmyActive.Value = true;
            moveTimmy.timmyActive.Value = false;
            moveTimmy.ResetTimmy();
            GameManager.Instance.AllPlayerStopClientRpc(true);
            timmyState = ETimmyState.Sleep;
        });
        timmySequence.AppendInterval(1f);

        timmySequence.Append(DOTween.To(() => fadeAlpha.Value, x => SetAlpha(x), 0f, 0.5f));

    }

    private void SetAlpha(float alpha)
    {
        fadeAlpha.Value = alpha;
    }

    public void ChangeImageAlpha(float oldValue, float newValue)
    {
        imageColor.a = newValue;
        fadeImage.color = imageColor;
    }

    public void ActiveCamera(int oldIndex, int newIndex)
    {
        foreach (var camera in cameras)
        {
            camera.Priority = 0;
        }
        cameras[newIndex].Priority = 10;
        activeCameraIndex = newIndex;
    }
}
