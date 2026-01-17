using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject _mainMenuHolder;
    [SerializeField] private GameObject _settingsHolder;
    [SerializeField] private GameObject _howToPlayDayHolder;
    [SerializeField] private GameObject _howToPlayNightHolder;
    [SerializeField] private CinemachineCamera[] _mainMenuCameras;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ShowMainMenu();
        Cursor.lockState = CursorLockMode.Confined;
    }


    private void ShowMainMenu()
    {
        _mainMenuHolder.SetActive(true);
        ChangeCameraBasedOnIndex(0);
    }

    private void HideMainMenu()
    {
        _mainMenuHolder.GetComponent<ImageUIFader>().Hide();

    }

    private void ShowSettings()
    {
        _settingsHolder.SetActive(true);
        ChangeCameraBasedOnIndex(1);
    }

    private void HideSettings()
    {
        _settingsHolder.GetComponent<ImageUIFader>().Hide();
    }

    private void ShowHowToPlayDay()
    {
        _howToPlayDayHolder.SetActive(true);
        
    }

    private void HideHowToPlayDay()
    {
        _howToPlayDayHolder.GetComponent<ImageUIFader>().Hide();
    }

    private void ShowHowToPlayNight()
    {
        _howToPlayNightHolder.SetActive(true);
    }

    private void HideHowToPlayNight()
    {
        _howToPlayNightHolder.GetComponent<ImageUIFader>().Hide();
    }

    public void OpenSettingsPanel()
    {
        HideMainMenu();
        ShowSettings();
    }

    public void CloseSettingsPanel()
    {
        ShowMainMenu();
        HideSettings();
    }

    public void OpenHowToPlayDay()
    {
        HideMainMenu();
        ShowHowToPlayDay();
        StartCoroutine(GoThroughOtherCamera(1, 2));
    }

    public void CloseHowToPlayDay()
    {
        ShowMainMenu();
        HideHowToPlayDay();
        StartCoroutine(GoThroughOtherCamera(1, 0));
    }

    public void OpenHowToPlayNight()
    {
        HideHowToPlayDay();
        ShowHowToPlayNight();
        ChangeCameraBasedOnIndex(3);
    }

    public void CloseHowToPlayNight()
    {
        ShowMainMenu();
        HideHowToPlayNight();
        StartCoroutine(GoThroughOtherCamera(1, 0));
    }

    public void GoBackToHowToPlayDay()
    {
        HideHowToPlayNight();
        ShowHowToPlayDay();
        ChangeCameraBasedOnIndex(2);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("FlowershopScene");
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    // Camera Transitions

    private void ChangeCameraBasedOnIndex(int index)
    {
        if (index >= _mainMenuCameras.Length)
        {
            Debug.LogError("Camera Index is not in the Camera List");
            return;
        }
        for (int i = 0; i < _mainMenuCameras.Length; i++)
        {
            if (i == index)
            {
                _mainMenuCameras[i].Priority = 1;
            }
            else
            {
                _mainMenuCameras[i].Priority = 0;
            }
        }
    }

    private IEnumerator GoThroughOtherCamera(int throughCameraIndex, int finalCameraIndex)
    {
        ChangeCameraBasedOnIndex(throughCameraIndex);
        yield return new WaitForSeconds(1.3f);
        ChangeCameraBasedOnIndex(finalCameraIndex);
    }

}
