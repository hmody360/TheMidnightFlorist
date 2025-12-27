using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PopInAnimation : MonoBehaviour
{
    public GameObject[] ObjectsToPopIn;
    public float PopInRate;
    private AudioSource _audioSource;
    public AudioClip _audioClip;


    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(DelayShow());
    }

    IEnumerator DelayShow()
    {
        foreach (GameObject obj in ObjectsToPopIn)
        {
            yield return new WaitForSeconds(PopInRate);
            obj.layer = 0;
            if(obj.transform.childCount == 1)
            {
                obj.transform.GetChild(0).gameObject.layer = 0;
            }
            _audioSource.PlayOneShot(_audioClip);
            Debug.Log("Flower Spawned");
        }
    }

}
