using Unity.Netcode;
using UnityEngine;

public class PlayerTest : NetworkBehaviour
{
    void Update()
    {
        if (!IsOwner) { return; }

        Vector3 moveDir = Vector3.zero;

        moveDir.x = Input.GetAxis("Horizontal");
        moveDir.z = Input.GetAxis("Vertical");

        float moveSpeed = 3;

        transform.Translate(moveDir * moveSpeed * Time.deltaTime);

    }
}