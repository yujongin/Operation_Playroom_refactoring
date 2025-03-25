using UnityEngine;

[CreateAssetMenu(fileName = "BuildingScriptableObject", menuName = "Create BuildingScriptableObject")]
public class BuildingScriptableObject : ScriptableObject
{
    [Header("Information")]
    public int health;

    [Header("Meshes")]
    public Mesh defaultMesh;
    public Mesh brokenMesh;
    public Mesh hardBrokenMesh;
}
