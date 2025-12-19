using UnityEngine;

public class RayInetractor : MonoBehaviour
{
    private Camera _camera;
    private Ray _rayToCast;
    [SerializeField] private Outline _lastHitOutline;
    [SerializeField] private float _maxDistance = 6f;
    [SerializeField] LayerMask _interactible;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        _rayToCast = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f)); //Create a RayCast from the center of the camera

        if (Physics.Raycast(_rayToCast, out RaycastHit hit, _maxDistance, _interactible)) //if the ray cast hits an interactible object display an outline, if the player interacts, do the interaction set for this item
        {
            GameObject currentGameObject = hit.collider.gameObject;
            Outline currentOutlined = currentGameObject.GetComponent<Outline>();
            Iinteractable item = currentGameObject.transform.GetComponent<Iinteractable>();

            UIManager.instance.ChangeCrossHair(1);

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
            UIManager.instance.ChangeCrossHair(0);
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
