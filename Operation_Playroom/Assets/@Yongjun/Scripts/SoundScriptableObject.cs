using UnityEngine;

[CreateAssetMenu(fileName = "SoundScriptableObject", menuName = "Create SoundScriptableObject")]

public class SoundScriptableObject : ScriptableObject
{
    [Header("Sound Files")]
    public AudioClip[] soundClips;
}
