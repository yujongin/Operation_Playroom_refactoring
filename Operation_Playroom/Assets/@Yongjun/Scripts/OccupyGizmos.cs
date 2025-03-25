using UnityEngine;

public class OccupyGizmos : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(false);
    }

    void OnDrawGizmos()
    {
        float radius = 0.15f;

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
