using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundController : MonoBehaviour
{
    public AudioMixer audioMixer; 
    public Slider masterSlider;   
    public Slider bgmSlider;      
    public Slider sfxSlider;

    //private const string MASTER_VOLUME = "MasterVolume";
    //private const string BGM_VOLUME = "BGMVolume";
    //private const string SFX_VOLUME = "SFXVolume";

    void Start()
    {
        // ����� ���� �ҷ����� 
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.5f);

        // �����̴� �̺�Ʈ ����
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        // �ʱ� ���� ���� ����
        SetMasterVolume(masterSlider.value);
        SetBGMVolume(bgmSlider.value);
        SetSFXVolume(sfxSlider.value);
    }

    public void SetMasterVolume(float volume)
    {
        float dB = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
        audioMixer.SetFloat("MasterVolume", dB);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetBGMVolume(float volume)
    {
        float dB = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
        audioMixer.SetFloat("BGMVolume", dB);
        PlayerPrefs.SetFloat("BGMVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        float dB = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
        audioMixer.SetFloat("SFXVolume", dB);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
    // ��������
    //void OnApplicationQuit()
    //{
    //    PlayerPrefs.Save();
    //}
}
