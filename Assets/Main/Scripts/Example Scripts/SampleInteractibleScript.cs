using UnityEngine;

public class SampleInteractibleScript : MonoBehaviour, Iinteractable
{
    private Collider _colldier;
    private Rigidbody _rigidBody;
    public string ActionName => "I'm Cubie You Took Me!";
    [SerializeField] private bool isEqipped = false;



    private void Awake()
    {
        _colldier = GetComponent<Collider>();
        _rigidBody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if(isEqipped && Input.GetKeyDown(KeyCode.Q))
        {
            transform.SetParent(null);
            _rigidBody.isKinematic = false;
            _colldier.enabled = true;
            isEqipped = false;
        }
    }

    public void Interact()
    {
        if (!isEqipped)
        {
            transform.SetParent(GameObject.FindGameObjectWithTag("ItemSlot").transform);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            _rigidBody.isKinematic = true;
            _colldier.enabled = false;
            Debug.Log(ActionName);
            isEqipped = true;
        }

    }
}
