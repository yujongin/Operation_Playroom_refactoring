using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class King : Character
{
    //private SoldierSpawner soldierSpawner;

    public override void OnNetworkSpawn()
    {
        base.Start();
        //soldierSpawner = GetComponent<SoldierSpawner>();
        //soldierSpawner = new GameObject($"SoldierSpawner_{OwnerClientId}").AddComponent<SoldierSpawner>();
        //soldierSpawner.InitializeSpawner(this.transform);
    }
    void Update()
    {
        if (!IsOwner) return;
        HandleInput();
    }

    // 키 입력 메서드
    public override void HandleInput()
    {
        // 공격
        if (Input.GetButtonDown("Attack"))
        {
            Attack();
        }
        // E 버튼 누르면
        if (Input.GetButtonDown("Interact"))
        {
            //CommandSoldiersServerRpc();
            Debug.Log("E눌림");
        }
        // Q 버튼 누르면
        if (Input.GetButtonDown("Interact"))
        {
            //RetreatSoldiersServerRpc();
            Debug.Log("Q눌림");
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            //if (IsOwner)
            //{
            //    if (soldierSpawner != null)
            //    {
            //        soldierSpawner.AddSoldierServerRpc(1);
            //    }
            //}
            Debug.Log($"F키 눌림 by {OwnerClientId}");
        }
    }

    // 공격 메서드
    public override void Attack()
    {
        // 검 휘두르며 공격
        Debug.Log("Sword Attack");
    }
    // 상호작용 메서드
    public override void Interaction()
    {
        // 줍기
        Debug.Log("King Interaction");
    }
    
    //[ServerRpc]
    //private void CommandSoldiersServerRpc()
    //{
    //    // 주변 병사들을 찾아서 행동 지시
    //    GameObject[] soldiers = GameObject.FindGameObjectsWithTag("Soldier");
    //    foreach (var soldierObj in soldiers)
    //    {
    //        Soldier soldier = soldierObj.GetComponent<Soldier>(); // 탐색한 콜라이더 중 soldier 컴포넌트를 가진 오브젝트만 가지고 옴 
    //        if (soldier != null && soldier.currentState is FollowingState) // 병사가 명령을 받을 수 있는 상태라면 
    //        {
    //            float distance = Vector3.Distance(transform.position, soldier.transform.position);
    //            if (distance <= 5)
    //            {
    //                // 주변 적 확인
    //                GameObject enemy = FindNearestEnemy(); // 주변 적 찾는 함수 호출
    //                if (enemy != null)
    //                {
    //                    soldier.SetState(1, enemy.GetComponent<NetworkObject>().NetworkObjectId); // 적이 있을 경우 AttackingState로 변경
    //                }
    //                else
    //                {
    //                    // 주변 자원 확인
    //                    GameObject item = FindNearestItem();
    //                    if (item != null)
    //                    {
    //                        soldier.SetState(2, item.GetComponent<NetworkObject>().NetworkObjectId); // 자원이 있다면..
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}
    //[ServerRpc]
    //private void RetreatSoldiersServerRpc()
    //{
    //    // 공격 중인 병사들에게 후퇴 명령
    //    GameObject[] attackingSoldiers = GameObject.FindGameObjectsWithTag("Soldier");
    //    foreach (var attackingSoldierObj in attackingSoldiers)
    //    {
    //        Soldier soldier = attackingSoldierObj.GetComponent<Soldier>();
    //        if (soldier != null && soldier.currentState is AttackingState) // 공격 중인 병사 들 
    //        {
    //            soldier.SetState(0, 0); // 따라오라
    //        }
    //    }
    //}

    // 적을 찾는 메서드 (범위 내 가장 가까운 적)
    //private GameObject FindNearestEnemy()
    //{
    //    Collider[] enemies = Physics.OverlapSphere(transform.position, 1f, LayerMask.GetMask("Enemy")); // 나중에 콜라이더로 수정
    //    GameObject nearestEnemy = null; // 가장 가까운 적을 저장할 오브젝트
    //    float minDistance = Mathf.Infinity; // 가장 가까운 거리를 저장. 초기 값은 무한대로 설정

    //    foreach (Collider enemy in enemies) // 탐색된 모든 적 순회
    //    {
    //        float distance = Vector3.Distance(transform.position, enemy.transform.position); // 왕, 각 적의 거리를 계산
    //        if (distance < minDistance) // 현재 계산된 거리가 최소 거리보다 작으면 
    //        {
    //            minDistance = distance; // 최소거리를 현재 계산 거리로 업데이트
    //            nearestEnemy = enemy.gameObject; // 가까운 적 오브젝트에 현재 적의 오브젝트로 저장 
    //        }
    //    }
    //    return nearestEnemy; // 가까운 적 오브젝트를 반환
    //}

    //// 자원 찾기 (범위 내 가장 가까운 자원)
    //private GameObject FindNearestItem()
    //{
    //    Collider[] item = Physics.OverlapSphere(transform.position, 1f, LayerMask.GetMask("Item")); // (왕의 위치를 중심으로, ~)
    //    GameObject nearestItem = null; // 가까운 자원을 저장할 오브젝트
    //    float minDistance = Mathf.Infinity; // 가장 가까운 거리를 저장, 초기값은 무한대 

    //    foreach (Collider resource in item) // 탐색된 모든 자원 순회하며
    //    {
    //        float distance = Vector3.Distance(transform.position, resource.transform.position); // 왕과 각 자원간 거리 계산
    //        if (distance < minDistance) // 현재 계산된거리가 최소 거리보다 작으면
    //        {
    //            minDistance = distance; // 최소거리에 현재 계산 거리 업데이트
    //            nearestItem = resource.gameObject; // 오브젝트에 자원 저장
    //        }
    //    }
    //    return nearestItem; // 최소거리자원 오브젝트 반환
    //}
}


