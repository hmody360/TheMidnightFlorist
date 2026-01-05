using UnityEngine;

public class GameLeaver : MonoBehaviour, Iinteractable


{
    private string _actionName;

    public string ActionName
    {
        get { return _actionName; }
        set { _actionName = value; }
    }

    private void Start()
    {
        ActionName = "Leave? (Go To Main Menu)";
    }

    public void Interact()
    {
        GameManager.instance.QuitToMainMenu();
    }
}
