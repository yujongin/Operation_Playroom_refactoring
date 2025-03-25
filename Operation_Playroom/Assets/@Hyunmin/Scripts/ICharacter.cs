using Unity.Cinemachine;
using UnityEngine;

public interface ICharacter
{
    void Move(CinemachineCamera cam, Rigidbody rb); // 이동
    void Attack(); // 공격
    void Interaction(); // 상호작용(줍기)
    void TakeDamage(); // 피격
}
