using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource[] _audioSourceList;
    [SerializeField] private AudioClip[] _audioClips;


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
