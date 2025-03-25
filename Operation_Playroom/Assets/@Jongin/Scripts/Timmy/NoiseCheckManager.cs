using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class NoiseCheckManager : NetworkBehaviour
{
    public Image sleepGauge;
    public Image noiseGauge;

    private NetworkVariable<float> sleep = new NetworkVariable<float>(100f); // �ʱ� ���� ������
    private float noise; // ���� Ŭ���̾�Ʈ�� ���� ��

    private NetworkVariable<float> totalNoise = new NetworkVariable<float>(0); // �������� �����ϴ� �� ���� ��

    TimmyDirection timmyDirection;
    public override void OnNetworkSpawn()
    {
        timmyDirection = GameObject.FindFirstObjectByType<TimmyDirection>();
    }
    void Update()
    {

        if (IsClient)
        {
            noiseGauge.fillAmount = totalNoise.Value / 30f; // ���� Ŭ���̾�Ʈ�� UI ������Ʈ
            sleepGauge.fillAmount = sleep.Value / 100f; // ������ sleep �� ������� UI ������Ʈ
        }
    }

    public void AddNoiseGage(float value)
    {
        if (IsClient) // Ŭ���̾�Ʈ���� ������ ��û
        {
            SubmitNoiseToServerRpc(value);
        }
    }

    public void SubmitNoiseTo(float noiseValue)
    {
        if (timmyDirection.timmyState != ETimmyState.Sleep) return;
        totalNoise.Value += noiseValue;
        totalNoise.Value = Mathf.Clamp(totalNoise.Value, 0, 30);
    }

    [ServerRpc(RequireOwnership = false)]
    void SubmitNoiseToServerRpc(float noiseValue)
    {
        if (timmyDirection.timmyState != ETimmyState.Sleep) return;
        totalNoise.Value += noiseValue; // �������� ��ü noise �� ����
        totalNoise.Value = Mathf.Clamp(totalNoise.Value, 0, 30);
    }

    void FixedUpdate()
    {
        if (IsServer) // ���������� sleep �� ����
        {
            DecreaseSleepGage();
        }
    }

    void DecreaseSleepGage()
    {
        if (timmyDirection.timmyState != ETimmyState.Sleep) return;
        
        float change = 0f;

        if (totalNoise.Value >= 5 && totalNoise.Value < 20)
        {
            change = -0.5f * Time.deltaTime; // ��ȭ ����
        }
        else if (totalNoise.Value >= 20 && totalNoise.Value <= 30)
        {
            change = -1f * Time.deltaTime; // �ʴ� 1 ����
        }
        //else if (totalNoise.Value >= 20 && totalNoise.Value <= 30)
        //{
        //    change = -1.5f * Time.deltaTime; // �ʴ� 2 ����
        //}

        sleep.Value = Mathf.Clamp(sleep.Value + change, 0, 100); // sleep �� ���� ����
        totalNoise.Value = Mathf.Max(0, totalNoise.Value - (2f * Time.deltaTime)); // ���������� noise ����
        if(sleep.Value <= 0)
        {
            timmyDirection.timmyState = ETimmyState.Move;
            sleep.Value = 100;
            totalNoise.Value = 0;
            timmyDirection.StartTimmy();
        }
    }
}
