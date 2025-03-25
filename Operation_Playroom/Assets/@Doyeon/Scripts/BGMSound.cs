using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class BGMSound : MonoBehaviour
{
    public static BGMSound instance;

    public AudioMixer audioMixer;
    private AudioSource audioSource;

    public AudioClip[] mainSceneClip; // 3개의 오디오 클립을 담을 배열
    public AudioClip gameSceneClip;
    private int currentClipIndex = 0; // 현재 재생 중인 클립 인덱스

    private string[] scenesMusic = { "LoadingScene", "MainScene", "LobbyScene" };

    void Awake()
    {
        if (instance != null) { return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("AudioSource가 없어서 자동으로 추가");
        }
        
        audioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("BGM")[0];
        
        //audioSource.clip = backgroundMusic;
        audioSource.loop = true;

        StartCoroutine(PlayAudioLoop());

        CheckScenePlayMusic();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    IEnumerator PlayAudioLoop()
    {
        while (true)
        {
            audioSource.clip = mainSceneClip[currentClipIndex];
            audioSource.Play();

            yield return new WaitForSeconds(audioSource.clip.length);

            currentClipIndex = (currentClipIndex + 1) % mainSceneClip.Length;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckScenePlayMusic();
    }
    void CheckScenePlayMusic()
    {
        if (audioSource == null) return;
        string currentScene = SceneManager.GetActiveScene().name;

        bool isPlayMusic = System.Array.Exists(scenesMusic, sceneName => sceneName == currentScene);

        if (isPlayMusic && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
        else if (!isPlayMusic && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
