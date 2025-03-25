using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    static Dictionary<GameTeam, List<SpawnPoint>> spawnPoints = new Dictionary<GameTeam, List<SpawnPoint>>();
    [SerializeField] GameTeam team;
    [SerializeField] GameRole role;

    private void OnEnable()
    {
        if (spawnPoints.ContainsKey(team))
        {
            spawnPoints[team].Add(this);
        }
        else
        {
            spawnPoints.Add(team, new List<SpawnPoint> { this });
        }
    }

    public static void Clear()
    {
        spawnPoints.Clear();
    }

    public static Vector3 GetSpawnPoint(GameTeam team, GameRole role)
    {
        if (spawnPoints.Count == 0)
        {
            return new Vector3(0, 0.5f, 0);
        }

        Vector3 spawnPos = new Vector3(0, 0.5f, 0);
        var result = spawnPoints[team].Where(x => x.role == role).ToList();
        if (result.Count > 0)
        {
            spawnPos = new Vector3(result[0].transform.position.x, 0.12f, result[0].transform.position.z);
        }

        return spawnPos;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = team == GameTeam.Blue ? Color.blue : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }
}
