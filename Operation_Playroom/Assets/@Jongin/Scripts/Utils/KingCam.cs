using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using static Define;
public class KingCam : MonoBehaviour
{
    public ETeam team;
    [SerializeField] GameObject target;
    void Start()
    {
        StartCoroutine(CamRoutine());
    }

    // 카메라 할당 루틴
    IEnumerator CamRoutine()
    {
        while (target == null)
        {
            yield return new WaitForSeconds(0.5f);
            KingTest[] kings = FindObjectsByType<KingTest>(FindObjectsSortMode.None);
            for (int i = 0; i < kings.Length; i++)
            {
                if (kings[i].team.Value == (int)team)
                {
                    target = kings[i].gameObject;
                }
                if (target != null)
                {
                    GetComponent<CinemachineCamera>().Follow = target.transform;
                    GetComponent<CinemachineCamera>().LookAt = target.transform;
                }
            }
        }
    }
}
