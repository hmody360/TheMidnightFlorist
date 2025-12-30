using UnityEngine;

public class RayInetractor : MonoBehaviour
{
    private Camera _camera;
    private Ray _rayToCast;
    private UIManager _UIManagerInstance;

    [SerializeField] private Outline _lastHitOutline;
    [SerializeField] private float _maxDistance = 6f;
    [SerializeField] LayerMask _interactible;
    [SerializeField] private InteractionPromptUI _interactionPromptUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _camera = Camera.main;
        _UIManagerInstance = UIManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        _rayToCast = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f)); //Create a RayCast from the center of the camera

        if (Physics.Raycast(_rayToCast, out RaycastHit hit, _maxDistance, _interactible) && hit.collider.gameObject.layer == 6) //if the ray cast hits an interactible object display an outline, if the player interacts, do the interaction set for this item
        {
            GameObject currentGameObject = hit.collider.gameObject;
            Outline currentOutlined = currentGameObject.GetComponent<Outline>();
            Iinteractable item = currentGameObject.transform.GetComponent<Iinteractable>();

            if(_UIManagerInstance != null && item != null)
            {
                UIManager.instance.ChangeCrossHair(1);
                UIManager.instance.setPromptText(item.ActionName, Color.white);
            }

            if (currentOutlined != _lastHitOutline) //Disable outlines once not looking at the current item
            {
                DisableCurrentOutline();
                _lastHitOutline = currentOutlined;
                EnableCurrentOutline();
            }


            if (Input.GetMouseButtonDown(0))
            {
                

                if (item != null)
                {
                    item.Interact();
                }
                Debug.Log("Looking at:" + hit.transform.name);
            }
        }
        else
        {
            if (_UIManagerInstance != null)
            {
                UIManager.instance.ChangeCrossHair(0);
                UIManager.instance.setPromptText(string.Empty, Color.white);
            }

            DisableCurrentOutline();
        }
    }

    private void DisableCurrentOutline()
    {
        if(_lastHitOutline != null)
        {
            _lastHitOutline.enabled = false;
            _lastHitOutline = null;
        }
    }

    private void EnableCurrentOutline()
    {
        if(_lastHitOutline != null)
        {
            _lastHitOutline.enabled = true;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(_rayToCast);

    }
}
