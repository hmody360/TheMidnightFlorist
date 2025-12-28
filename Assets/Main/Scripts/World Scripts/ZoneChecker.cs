using UnityEngine;

public class ZoneChecker : MonoBehaviour
{
    public bool isInZone;
    public string tagToCheck;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagToCheck))
        {
            isInZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(tagToCheck))
        {
            isInZone = false;
        }
    }
}
