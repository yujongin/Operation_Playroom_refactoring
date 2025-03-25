using System.Globalization;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour
{
    public AudioClip hoverSound; 
    public AudioClip clickSound;
    public AudioMixer audioMixer;

    private AudioSource audioSource;

    public Button button;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // ��ư �̺�Ʈ ����
        button.onClick.AddListener(OnButtonClicked);

        audioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("VFX")[0];

        // ȣ���� �̺�Ʈ ����
        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => { OnButtonHovered(); });
        trigger.triggers.Add(entry);
    }
    

    
    // ȣ���� ����
    public void OnButtonHovered()
    {
        audioSource.PlayOneShot(hoverSound);
    }

    // Ŭ�� ����
    public void OnButtonClicked()
    {
        audioSource.PlayOneShot(clickSound);
    }

    
    
}
