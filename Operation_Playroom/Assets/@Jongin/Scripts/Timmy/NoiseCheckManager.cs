using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using static Define;

public class NoiseCheckManager : NetworkBehaviour
{
    public Image sleepGauge;
    public Image noiseGauge;

    private NetworkVariable<float> sleep = new NetworkVariable<float>(100f); // 초기 수면 게이지
    private float noise; // 개별 클라이언트의 소음 값

    private NetworkVariable<float> totalNoise = new NetworkVariable<float>(0); // 서버에서 관리하는 총 소음 값

    TimmyDirection timmyDirection;
    public override void OnNetworkSpawn()
    {
        timmyDirection = GameObject.FindFirstObjectByType<TimmyDirection>();
    }
    void Update()
    {

        if (IsClient)
        {
            noiseGauge.fillAmount = totalNoise.Value / 30f; // 개별 클라이언트의 UI 업데이트
            sleepGauge.fillAmount = sleep.Value / 100f; // 서버의 sleep 값 기반으로 UI 업데이트
        }
    }

    public void AddNoiseGage(float value)
    {
        if (IsClient) // 클라이언트에서 서버에 요청
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
        totalNoise.Value += noiseValue; // 서버에서 전체 noise 값 관리
        totalNoise.Value = Mathf.Clamp(totalNoise.Value, 0, 30);
    }

    void FixedUpdate()
    {
        if (IsServer) // 서버에서만 sleep 값 조정
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
            change = -0.5f * Time.deltaTime; // 변화 없음
        }
        else if (totalNoise.Value >= 20 && totalNoise.Value <= 30)
        {
            change = -1f * Time.deltaTime; // 초당 1 감소
        }
        //else if (totalNoise.Value >= 20 && totalNoise.Value <= 30)
        //{
        //    change = -1.5f * Time.deltaTime; // 초당 2 감소
        //}

        sleep.Value = Mathf.Clamp(sleep.Value + change, 0, 100); // sleep 값 범위 제한
        totalNoise.Value = Mathf.Max(0, totalNoise.Value - (2f * Time.deltaTime)); // 점진적으로 noise 감소
        if(sleep.Value <= 0)
        {
            timmyDirection.timmyState = ETimmyState.Move;
            sleep.Value = 100;
            totalNoise.Value = 0;
            timmyDirection.StartTimmy();
        }
    }
}
