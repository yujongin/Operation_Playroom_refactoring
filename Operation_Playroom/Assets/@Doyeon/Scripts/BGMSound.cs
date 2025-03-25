using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class BGMSound : MonoBehaviour
{
    public static BGMSound instance;

    public AudioMixer audioMixer;
    private AudioSource audioSource;

    public AudioClip[] mainSceneClip; // 3���� ����� Ŭ���� ���� �迭
    public AudioClip gameSceneClip;
    private int currentClipIndex = 0; // ���� ��� ���� Ŭ�� �ε���

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
            Debug.Log("AudioSource�� ��� �ڵ����� �߰�");
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
