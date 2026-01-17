using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Analytics;
using Unity.VisualScripting;

public class CustomerProp : MonoBehaviour
{
    private NavMeshAgent _agent;
    private Animator _animator;
    private AudioSource _audioSource;
    private int _currentPointIndex = 0;

    [SerializeField] private Gender _gender;

    // Model Randoms
    [SerializeField] private GameObject[] _BeardList;
    [SerializeField] private GameObject[] _HairList;
    [SerializeField] private GameObject[] _mustacheList;
    [SerializeField] private GameObject[] _NoseList;
    [SerializeField] private Material[] _clothesMaterialList;
    [SerializeField] private Material[] _skinMaterialList;
    [SerializeField] private SkinnedMeshRenderer _clothesMaterialToChange;
    [SerializeField] private SkinnedMeshRenderer _WomanHandSkinToChange;

    [SerializeField] List<Transform> _goLocations;

    [SerializeField] private float stoppingDistance = 0.5f;


    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateLook();
        GoToStartPoint();
        _agent.stoppingDistance = stoppingDistance;
    }

    // Update is called once per frame
    void Update()
    {
        Patrol();
    }

    private void GoToStartPoint()
    {

        _agent.SetDestination(_goLocations[0].position);
        _currentPointIndex = 0;
        _animator.SetBool("isWalking", true);
        _audioSource.Play();
    }

    private void Patrol()
    {

        if (!_agent.pathPending && _agent.hasPath && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            _currentPointIndex = (_currentPointIndex == 0) ? 1 : 0;
            GenerateLook();
            _agent.SetDestination(_goLocations[_currentPointIndex].position);
            _agent.speed = Random.Range(1.5f, 2.1f);
        }
    }

    private void GenerateLook()
    {
        GameObjectListHider(_NoseList);
        int randomNoseIndex = Random.Range(-1, _NoseList.Length);
        int randomClothesIndex = Random.Range(-1, _clothesMaterialList.Length);
        int randomSkinIndex = Random.Range(0, _skinMaterialList.Length);

        if (randomNoseIndex != -1)
        {
            _NoseList[randomNoseIndex].SetActive(true);
            _NoseList[randomNoseIndex].GetComponent<SkinnedMeshRenderer>().material = _skinMaterialList[randomSkinIndex];
        }

        if (randomClothesIndex != -1)
        {
            Material[] tempMatList = _clothesMaterialToChange.sharedMaterials;
            tempMatList[0] = _clothesMaterialList[randomClothesIndex];
            _clothesMaterialToChange.sharedMaterials = tempMatList;
        }

        Material[] tempSkinMatList = _clothesMaterialToChange.sharedMaterials;
        tempSkinMatList[1] = _skinMaterialList[randomSkinIndex];
        _clothesMaterialToChange.sharedMaterials = tempSkinMatList;




        if (_gender == Gender.Male)
        {
            GameObjectListHider(_BeardList);
            GameObjectListHider(_HairList);
            GameObjectListHider(_mustacheList);
            int randomBeardIndex = Random.Range(-1, _BeardList.Length);
            int randomHairIndex = Random.Range(-1, _HairList.Length);
            int randomMustacheIndex = Random.Range(-1, _mustacheList.Length);

            if (randomBeardIndex != -1)
            {
                _BeardList[randomBeardIndex].SetActive(true);
            }

            if (randomHairIndex != -1)
            {
                _HairList[randomHairIndex].SetActive(true);
            }

            if (randomMustacheIndex != -1)
            {
                _mustacheList[randomMustacheIndex].SetActive(true);
            }

        }
        else
        {
            _WomanHandSkinToChange.material = _skinMaterialList[randomSkinIndex];
        }
    }

    private void GameObjectListHider(GameObject[] ListToHide)
    {
        foreach (GameObject obj in ListToHide)
        {
            obj.SetActive(false);
        }
    }
}
