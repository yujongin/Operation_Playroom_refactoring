using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
public class MoveTimmy : NetworkBehaviour
{
    public List<Transform> path = new List<Transform>();
    public NetworkVariable<bool> timmyActive = new NetworkVariable<bool>(true);
    public GameObject[] BuildingDummy;


    int pathIndex = 0;

    NavMeshAgent agent;
    Animator animator;
    bool isMove = false;

    Vector3 startPos;
    Quaternion startRot;

    public void OnSetActiveSelf(bool oldValue, bool newValue)
    {
        gameObject.SetActive(newValue);
    }
    public override void OnNetworkSpawn()
    {
        timmyActive.OnValueChanged -= OnSetActiveSelf;
        timmyActive.OnValueChanged += OnSetActiveSelf;
        //gameObject.SetActive(false);

        if (!IsServer) return;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        //timmyActive.Value = false;
        startPos = transform.position;
        startRot = transform.rotation;

    }
    public override void OnNetworkDespawn()
    {
        timmyActive.OnValueChanged -= OnSetActiveSelf;
    }
    public void ResetTimmy()
    {
        transform.position = startPos;
        transform.rotation = startRot;
    }
    public void CallTimmy(Action callback)
    {
        StartCoroutine(MoveTimmyToPath(callback));
    }

    IEnumerator MoveTimmyToPath(Action callback)
    {
        pathIndex = 0;
        while (pathIndex < path.Count)
        {
            if (!isMove)
            {
                MoveToPath(pathIndex);
                isMove = true;
                animator.SetTrigger("Walk");
            }
            else
            {
                if (HasReachedDestination(path[pathIndex].position))
                {
                    animator.SetTrigger("Lifting");
                    yield return new WaitForSeconds(1.5f);
                    SetBuildingDummyClientRpc(pathIndex);
                    path[pathIndex].GetComponentInChildren<Building>().DestructionBuilding();
                    yield return new WaitForSeconds(4.5f);
                    isMove = false;
                    pathIndex++;
                }
            }
            yield return null;
        }
        ResetBuildingDummyClientRpc();
        callback?.Invoke();
    }

    [ClientRpc]
    void SetBuildingDummyClientRpc(int pathIndex)
    {
        BuildingDummy[pathIndex].SetActive(true);
    }

    [ClientRpc]
    void ResetBuildingDummyClientRpc()
    {
        for (int i = 0; i < BuildingDummy.Length; i++)
        {
            BuildingDummy[i].SetActive(false);
        }
    }

    void MoveToPath(int index)
    {
        Vector3 dir = Vector3.Normalize(transform.position - path[index].position);
        agent.SetDestination(path[index].position);
    }

    bool HasReachedDestination(Vector3 currentPath)
    {
        // 에이전트가 경로를 가지고 있고, 남은 거리가 정지 거리보다 작으면 도착한 것으로 판단
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            return !agent.hasPath || agent.velocity.sqrMagnitude == 0f;
        }
        return false;
    }
}
