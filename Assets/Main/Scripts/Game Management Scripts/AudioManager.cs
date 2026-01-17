using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioMixer audioMixer;

    [SerializeField] private Slider _MasterSider;
    [SerializeField] private Slider _MusicSider;
    [SerializeField] private Slider _SFXSider;

    [SerializeField] private AudioSource[] _audioSourceList;
    [SerializeField] private AudioClip[] _audioClips;


    private void Start()
    {
        //Get the previously set Sound Options and apply them to the respective Audio Group.
        float masterVolume = PlayerPrefs.GetFloat("MasterVol");
        float musicVolume = PlayerPrefs.GetFloat("MusicVol");
        float SFXVolume = PlayerPrefs.GetFloat("SFXVol");

        audioMixer.SetFloat("MasterVol", Mathf.Log10(masterVolume) * 20);
        audioMixer.SetFloat("MusicVol", Mathf.Log10(musicVolume) * 20);
        audioMixer.SetFloat("SFXVol", Mathf.Log10(SFXVolume) * 20);

        // Adding Listeners to Volume Sliders
        if (_MasterSider != null)
        {
            _MasterSider.value = masterVolume;
            SetMasterVolume(_MasterSider.value);
            _MasterSider.onValueChanged.AddListener(SetMasterVolume);
        }

        if (_MusicSider != null)
        {
            _MusicSider.value = musicVolume;
            SetMusicVolume(_MusicSider.value);
            _MusicSider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (_SFXSider != null)
        {
            _SFXSider.value = SFXVolume;
            SetSFXVolume(_SFXSider.value);
            _SFXSider.onValueChanged.AddListener(SetSFXVolume);
        }
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVol", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MasterVol", volume);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVol", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MusicVol", volume);
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFXVol", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVol", volume);
    }

    public void ChangeGameMusic(int clipIndex)
    {
        if (_audioSourceList.Length == 0 || _audioClips.Length - 1 < clipIndex)
        {
            return;
        }

        _audioSourceList[0].clip = _audioClips[clipIndex];
        _audioSourceList[0].Play();
    }

    public void ChangeAmbienceSounds(int clipIndex)
    {
        if (_audioSourceList.Length == 0 || _audioClips.Length - 1 < clipIndex)
        {
            return;
        }

        _audioSourceList[1].clip = _audioClips[clipIndex];
        _audioSourceList[1].Play();
    }


}
