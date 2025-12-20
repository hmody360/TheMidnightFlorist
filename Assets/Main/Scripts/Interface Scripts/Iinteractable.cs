using UnityEngine;

public interface Iinteractable //Interface Script for all Interactible Items.
{
    public string ActionName { get; set; }
    public void Interact();
}
