using System;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Sun")]
    [SerializeField] private Light _sun;

    [Header("Time Settings")]
    [SerializeField] private float _dayLengthInSeconds = 60f;
    [SerializeField] private float _currentTimeOfDay;

    [Header("SkyBoxes")]
    [SerializeField] Material[] _skyboxList;

    private Material _currentSkyBox;

    private AudioManager _audioManager;
    private void Start()
    {
        _sun = RenderSettings.sun;
        _audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
    }
    // Update is called once per frame
    void Update()
    {
        TimeCycle();
        UpdateSunRotation();
        UpdateSkybox();
    }

    private void TimeCycle()
    {
        _currentTimeOfDay += Time.deltaTime / _dayLengthInSeconds;
        if (_currentTimeOfDay > 1f)
        {
            _currentTimeOfDay = 0f;
        }
    }

    private void UpdateSunRotation()
    {
        float sunAngle = Mathf.Lerp(-90f, 270f, _currentTimeOfDay);
        _sun.transform.rotation = Quaternion.Euler(sunAngle, 90f, 0f);
    }

    private void UpdateSkybox()
    {
        Material targetSkyBox = GetSkyBoxBasedOnTime();

        if (_currentSkyBox != targetSkyBox)
        {
            _currentSkyBox = targetSkyBox;
            RenderSettings.skybox = _currentSkyBox;
            DynamicGI.UpdateEnvironment(); // This is to update the Lighting Alongside the Skybox change.

            if(_currentSkyBox == _skyboxList[0])
            {
                _audioManager.ChangeAmbienceSounds(1);
            }else if (_currentSkyBox == _skyboxList[2])
            {
                _audioManager.ChangeAmbienceSounds(2);
            }
        }
    }

    private Material GetSkyBoxBasedOnTime()
    {
        if(_currentTimeOfDay < 0.25f)
        {
            
            return _skyboxList[0];
        }else if(_currentTimeOfDay < 0.5f)
        {
            return _skyboxList[1];
        }else if(_currentTimeOfDay < 0.75f)
        {
            return _skyboxList[2];
        }
        else
        {
            return _skyboxList[3];
        }
    }

}
