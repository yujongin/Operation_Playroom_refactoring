using UnityEngine;

[CreateAssetMenu(fileName = "OccupyScriptableObject", menuName = "Create OccupyScriptableObject")]

public class OccupyScriptableObject : ScriptableObject
{
    [Header("Initialization Prefab")]
    public GameObject buildingPrefabTeamRed; // �ǹ� ������
    public GameObject buildingPrefabTeamBlue;
}
