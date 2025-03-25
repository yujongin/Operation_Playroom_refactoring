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

    // Ű �Է� �޼���
    public override void HandleInput()
    {
        // ����
        if (Input.GetButtonDown("Attack"))
        {
            Attack();
        }
        // E ��ư ������
        if (Input.GetButtonDown("Interact"))
        {
            //CommandSoldiersServerRpc();
            Debug.Log("E����");
        }
        // Q ��ư ������
        if (Input.GetButtonDown("Interact"))
        {
            //RetreatSoldiersServerRpc();
            Debug.Log("Q����");
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
            Debug.Log($"FŰ ���� by {OwnerClientId}");
        }
    }

    // ���� �޼���
    public override void Attack()
    {
        // �� �ֵθ��� ����
        Debug.Log("Sword Attack");
    }
    // ��ȣ�ۿ� �޼���
    public override void Interaction()
    {
        // �ݱ�
        Debug.Log("King Interaction");
    }
    
    //[ServerRpc]
    //private void CommandSoldiersServerRpc()
    //{
    //    // �ֺ� ������� ã�Ƽ� �ൿ ����
    //    GameObject[] soldiers = GameObject.FindGameObjectsWithTag("Soldier");
    //    foreach (var soldierObj in soldiers)
    //    {
    //        Soldier soldier = soldierObj.GetComponent<Soldier>(); // Ž���� �ݶ��̴� �� soldier ������Ʈ�� ���� ������Ʈ�� ������ �� 
    //        if (soldier != null && soldier.currentState is FollowingState) // ���簡 ����� ���� �� �ִ� ���¶�� 
    //        {
    //            float distance = Vector3.Distance(transform.position, soldier.transform.position);
    //            if (distance <= 5)
    //            {
    //                // �ֺ� �� Ȯ��
    //                GameObject enemy = FindNearestEnemy(); // �ֺ� �� ã�� �Լ� ȣ��
    //                if (enemy != null)
    //                {
    //                    soldier.SetState(1, enemy.GetComponent<NetworkObject>().NetworkObjectId); // ���� ���� ��� AttackingState�� ����
    //                }
    //                else
    //                {
    //                    // �ֺ� �ڿ� Ȯ��
    //                    GameObject item = FindNearestItem();
    //                    if (item != null)
    //                    {
    //                        soldier.SetState(2, item.GetComponent<NetworkObject>().NetworkObjectId); // �ڿ��� �ִٸ�..
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}
    //[ServerRpc]
    //private void RetreatSoldiersServerRpc()
    //{
    //    // ���� ���� ����鿡�� ���� ���
    //    GameObject[] attackingSoldiers = GameObject.FindGameObjectsWithTag("Soldier");
    //    foreach (var attackingSoldierObj in attackingSoldiers)
    //    {
    //        Soldier soldier = attackingSoldierObj.GetComponent<Soldier>();
    //        if (soldier != null && soldier.currentState is AttackingState) // ���� ���� ���� �� 
    //        {
    //            soldier.SetState(0, 0); // �������
    //        }
    //    }
    //}

    // ���� ã�� �޼��� (���� �� ���� ����� ��)
    //private GameObject FindNearestEnemy()
    //{
    //    Collider[] enemies = Physics.OverlapSphere(transform.position, 1f, LayerMask.GetMask("Enemy")); // ���߿� �ݶ��̴��� ����
    //    GameObject nearestEnemy = null; // ���� ����� ���� ������ ������Ʈ
    //    float minDistance = Mathf.Infinity; // ���� ����� �Ÿ��� ����. �ʱ� ���� ���Ѵ�� ����

    //    foreach (Collider enemy in enemies) // Ž���� ��� �� ��ȸ
    //    {
    //        float distance = Vector3.Distance(transform.position, enemy.transform.position); // ��, �� ���� �Ÿ��� ���
    //        if (distance < minDistance) // ���� ���� �Ÿ��� �ּ� �Ÿ����� ������ 
    //        {
    //            minDistance = distance; // �ּҰŸ��� ���� ��� �Ÿ��� ������Ʈ
    //            nearestEnemy = enemy.gameObject; // ����� �� ������Ʈ�� ���� ���� ������Ʈ�� ���� 
    //        }
    //    }
    //    return nearestEnemy; // ����� �� ������Ʈ�� ��ȯ
    //}

    //// �ڿ� ã�� (���� �� ���� ����� �ڿ�)
    //private GameObject FindNearestItem()
    //{
    //    Collider[] item = Physics.OverlapSphere(transform.position, 1f, LayerMask.GetMask("Item")); // (���� ��ġ�� �߽�����, ~)
    //    GameObject nearestItem = null; // ����� �ڿ��� ������ ������Ʈ
    //    float minDistance = Mathf.Infinity; // ���� ����� �Ÿ��� ����, �ʱⰪ�� ���Ѵ� 

    //    foreach (Collider resource in item) // Ž���� ��� �ڿ� ��ȸ�ϸ�
    //    {
    //        float distance = Vector3.Distance(transform.position, resource.transform.position); // �հ� �� �ڿ��� �Ÿ� ���
    //        if (distance < minDistance) // ���� ���ȰŸ��� �ּ� �Ÿ����� ������
    //        {
    //            minDistance = distance; // �ּҰŸ��� ���� ��� �Ÿ� ������Ʈ
    //            nearestItem = resource.gameObject; // ������Ʈ�� �ڿ� ����
    //        }
    //    }
    //    return nearestItem; // �ּҰŸ��ڿ� ������Ʈ ��ȯ
    //}
}


