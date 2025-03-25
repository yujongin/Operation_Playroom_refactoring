using UnityEngine;

[CreateAssetMenu(fileName = "EffectScriptableObject", menuName = "Create EffectScriptableObject")]
public class EffectScriptableObject : ScriptableObject
{
    [Header("Effects")]
    public GameObject buildEffect;
    public GameObject sparkleEffect;
    public GameObject destructionEffect;
    public GameObject buildingHitEffect;
}
