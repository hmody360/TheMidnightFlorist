using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject _mainMenuHolder;
    [SerializeField] private GameObject _settingsHolder;
    [SerializeField] private GameObject _howToPlayDayHolder;
    [SerializeField] private GameObject _howToPlayNightHolder;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ShowMainMenu();
        Cursor.lockState = CursorLockMode.Confined;
    }


    private void ShowMainMenu()
    {
        _mainMenuHolder.SetActive(true);
    }

    private void HideMainMenu()
    {
        _mainMenuHolder.SetActive(false);
    }

    private void ShowSettings()
    {
        _settingsHolder.SetActive(true);
    }

    private void HideSettings()
    {
        _settingsHolder.SetActive(false);
    }

    private void ShowHowToPlayDay()
    {
        _howToPlayDayHolder.SetActive(true);
    }

    private void HideHowToPlayDay()
    {
        _howToPlayDayHolder.SetActive(false);
    }

    private void ShowHowToPlayNight()
    {
        _howToPlayNightHolder.SetActive(true);
    }

    private void HideHowToPlayNight()
    {
        _howToPlayNightHolder.SetActive(false);
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

    public void StartGame()
    {
        SceneManager.LoadScene("FlowershopScene");
    }

    public void ExitGame()
    {
        Application.Quit();
    }

}
