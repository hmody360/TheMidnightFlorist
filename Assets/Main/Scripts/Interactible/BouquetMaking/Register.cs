using UnityEngine;

public class Register : MonoBehaviour, Iinteractable
{

    private string _actionName;
    private AudioSource _audioSource;
    private Animator _animator;
    private GameManager gameManager;

    [SerializeField] private AudioClip[] _audioClipList;
    [SerializeField] private Renderer _screenRenderer;
    [SerializeField] private Material[] _screenMatList;

    

    public string ActionName
    {
        get { return _actionName; }
        set { _actionName = value; }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _animator = GetComponent<Animator>();

    }

    void Start()
    {
        ActionName = "Open Flower Shop?";
        gameManager = GameManager.instance;
    }

    public void Interact()
    {
        if (!gameManager.getStoreStatus() && !gameManager.getFinishStatus())
        {
            gameManager.OpenStore();
            UIManager.instance.setPromptText("The Shop has been Opened!", Color.green, true);
            Material[] tempMatList = _screenRenderer.sharedMaterials;
            tempMatList[1] = _screenMatList[1];
            _screenRenderer.sharedMaterials = tempMatList;
            _audioSource.PlayOneShot(_audioClipList[0]);
            _animator.SetTrigger("SuccessTrigger");
            ActionName = "Close Flower Shop?";
        }
        else if (gameManager.getStoreStatus() && !gameManager.getFinishStatus())
        {
            bool canStoreClose = gameManager.CloseStore();

            if (canStoreClose)
            {
                Material[] tempMatList = _screenRenderer.sharedMaterials;
                tempMatList[1] = _screenMatList[2];
                _screenRenderer.sharedMaterials = tempMatList;
                UIManager.instance.setPromptText("The Shop has been Closed!", Color.red, true);
                _audioSource.PlayOneShot(_audioClipList[1]);
                _animator.SetTrigger("SuccessTrigger");
                ActionName = "Work More?";

            }
            else
            {
                UIManager.instance.setPromptText("You Haven't met the quota yet!", Color.red, true);
                _audioSource.PlayOneShot(_audioClipList[2]); // Stressed Sound
                _animator.SetTrigger("FailTrigger");
            }
        }
        else
        {
            UIManager.instance.setPromptText("Don't Overwork Yourself!", Color.red, true);
            _audioSource.PlayOneShot(_audioClipList[3]); // Yawn Sound
            _animator.SetTrigger("FailTrigger");
        }
    }
}
