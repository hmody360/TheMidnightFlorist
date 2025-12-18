using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 10;
    public float runSpeed = 15;
    public float maxStamina = 20;
    public float currentStamina = 20;
    public float rotationSpeed = 3;
    public bool canMove = true;

    private Rigidbody _theRigidBody;
    private Quaternion _targetRotation;
    private float _currentSpeed;

    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private AudioSource[] _SFXSourceList;
    [SerializeField] private AudioClip[] _SFXClipList;
    [SerializeField] private bool _isWalking;
    [SerializeField] private bool _isSprinting = false;
    [SerializeField] private bool _canSprint;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        _theRigidBody = GetComponent<Rigidbody>(); //Getting Rigidbody from Player Object.
        _cameraTransform = Camera.main.transform;
    }

    void Start()
    {
        _targetRotation = transform.rotation;
        Cursor.lockState = CursorLockMode.Locked;
        _theRigidBody.freezeRotation = true; //This is to stop other game objects from affecting the player's rotation
        _currentSpeed = speed;
        currentStamina = maxStamina;
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove) //if player can move (Not Dead) allow them to jump, sprint and crouch
        {
            sprint();
        }
    }

    private void FixedUpdate()
    {
        if (canMove) //if player can move (Not Dead) allow them to move
        {
            moveAndRotate();
        }
    }

    private void moveAndRotate()
    {
        float Horizontal = Input.GetAxisRaw("Horizontal"); //Defining Char X Axis.
        float Vertical = Input.GetAxisRaw("Vertical"); //Defining Char Z Axis.

        _isWalking = (Horizontal != 0 || Vertical != 0); //Check if player is walking to play walkingSFX

        if (_isWalking && !_SFXSourceList[0].isPlaying) //if player is walking and the walking audio source is not playing, play it.
        {
            _SFXSourceList[0].Play();
        }
        else if (!_isWalking && _SFXSourceList[0].isPlaying) //if player STOPPED walking and the walking audio source is playing, stop it.
        {
            _SFXSourceList[0].Stop();
        }

        //Stamina Checking
        if (_isWalking && _isSprinting)
        {
            currentStamina -= Time.deltaTime;
        }
        else if (!_isSprinting)
        {
            if (currentStamina < maxStamina)
            {
                currentStamina += Time.deltaTime;
            }
        }

        // Camera Controls (for Realtive Movement)
        // Taking the Camera Forward and Right
        Vector3 cameraForward = _cameraTransform.forward;
        Vector3 cameraRight = _cameraTransform.right;

        //freezing the camera's y axis as we don't want it to be affected for the direction
        cameraForward.y = 0f;
        cameraRight.y = 0f;

        //Realtive Cam Direction
        Vector3 forwardRealtive = Vertical * cameraForward;
        Vector3 rightRealtive = Horizontal * cameraRight;

        Vector3 movementDir = (forwardRealtive + rightRealtive).normalized * _currentSpeed; //assigning movement with camera direction in mind, also using normalized to make movement in corner dierctions the same as normal directions (not faster)

        //Movement
        _theRigidBody.linearVelocity = new Vector3(movementDir.x, _theRigidBody.linearVelocity.y, movementDir.z); // Changing the velocity based on Horizontal and Vertical Movements alongside camera direction.

        //Rotation
        Vector3 lookDirection = cameraForward; //Taking the direction the camera is currently facing

        if (lookDirection != Vector3.zero) //on player movement
        {
            _targetRotation = Quaternion.LookRotation(lookDirection); // makes the target rotation that we want the player to move to
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotation, rotationSpeed * Time.deltaTime); //Using lerp to smooth the player rotation using current rotation, target rotaion and rotation speed.
    }

    private void sprint()
    {
        //Sprint Code
        if (_canSprint && Input.GetKey(KeyCode.LeftShift) && _isWalking)
        {
            _SFXSourceList[0].clip = _SFXClipList[1];
            _currentSpeed = runSpeed;
            _isSprinting = true;
        }
        else if ((Input.GetKeyUp(KeyCode.LeftShift) || !_canSprint || !_isWalking))
        {
            _SFXSourceList[0].clip = _SFXClipList[0];
            _currentSpeed = speed;
            _isSprinting = false;
        }

        //Stamina Code
        if (currentStamina <= 0)
        {
            currentStamina = 0;
            _canSprint = false;
            _SFXSourceList[1].PlayOneShot(_SFXClipList[2]);
        }

        if (currentStamina >= maxStamina)
        {
            currentStamina = maxStamina;
            _canSprint = true;
        }
    }
}
