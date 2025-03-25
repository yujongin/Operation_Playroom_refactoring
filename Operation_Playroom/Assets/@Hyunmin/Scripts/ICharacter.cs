using Unity.Cinemachine;
using UnityEngine;

public interface ICharacter
{
    void Move(CinemachineCamera cam, Rigidbody rb); // �̵�
    void Attack(); // ����
    void Interaction(); // ��ȣ�ۿ�(�ݱ�)
    void TakeDamage(); // �ǰ�
}
