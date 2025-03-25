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

        // 버튼 이벤트 연결
        button.onClick.AddListener(OnButtonClicked);

        audioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("VFX")[0];

        // 호버링 이벤트 설정
        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => { OnButtonHovered(); });
        trigger.triggers.Add(entry);
    }
    

    
    // 호버링 사운드
    public void OnButtonHovered()
    {
        audioSource.PlayOneShot(hoverSound);
    }

    // 클릭 사운드
    public void OnButtonClicked()
    {
        audioSource.PlayOneShot(clickSound);
    }

    
    
}
