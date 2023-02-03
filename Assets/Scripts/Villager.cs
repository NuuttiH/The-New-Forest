using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Villager : MonoBehaviour
{
    [HideInInspector] public int characterId;
    private bool _initialized = false;
    private NavMeshAgent _agent;
    [SerializeField] private Animator _animator;
    [SerializeField] private SkinnedMeshRenderer _mesh;
    [SerializeField] private int _colorChoice = 0;

    private bool _working;
    private bool _moving;
    private Job _job;
    private Vector3 _targetLocation;
    private float _waitTime;
    private bool _cancelJob;

    public float logicTic = 0.6f;

    [SerializeField] private Cost[] _upkeep;
    private bool _isFed = true;
    private float _baseSpeed;
    
    //[SerializeField] 
    public JobType[] forbiddenJobs;


    void Start()
    {
        StartCoroutine(Initialize(null, 0.5f));
    }
    public void Init(CharacterSaveData data = null)
    {
        StartCoroutine(Initialize(data));
    }
    IEnumerator Initialize(CharacterSaveData data = null, float wait = 0.3f)
    {
        yield return new WaitForSeconds(wait);
        if(!_initialized)
        {
            _initialized = true;

            _agent = gameObject.GetComponent<NavMeshAgent>();
            _baseSpeed = _agent.speed;
            _waitTime = 0f;
            GetComponent<OpenPopUpOnClick>().Init();

            if(data == null)
            {
                characterId = GameManager.GenerateId(IdType.Character, this.gameObject);

                _working = false;
                _cancelJob = false;
                // Get colorChoice smh
            }
            else
            {
                characterId = data.characterId;
                GameManager.AddId(IdType.Character, characterId, this.gameObject);

                _working = data.working;
                _moving = data.moving;
                _job = data.job;
                _targetLocation = data.targetLocation;
                _waitTime = data.waitTime;
                _cancelJob = data.cancelJob;
                SetColor(data.colorChoice);

                if(_moving)
                {
                    _agent.SetDestination(_targetLocation);
                    _animator.SetTrigger("IsRunning");
                }
            }

            GetComponent<OpenPopUpOnClick>().Init();
            StartCoroutine(AILogic());
            StartCoroutine(Upkeep());
        } 
    }
    public CharacterSaveData FormSaveData()
    {
        return new CharacterSaveData(   
                        gameObject.transform.position, gameObject.transform.rotation, 
                        gameObject.name, characterId,
                        _working, _moving, 
                        _job, _targetLocation, 
                        _waitTime, _cancelJob,
                        _colorChoice);
    }

    public void CancelJob(bool val = true)
    {
        _cancelJob = val;
    }

    IEnumerator AILogic()
    {
        // Run AILogic once per logicTic
        yield return new WaitForSeconds(logicTic);
        StartCoroutine(AILogic());

        // If _cancelJob flag raised, cancel current job
        if(_cancelJob)
        {
            Debug.Log("Canceling job...");
            _working = false;
            _moving = false;
            _waitTime = 0f;
            _job = null;
            // Reset possible animation triggers
            foreach(var parameter in _animator.parameters)
            {
                //Debug.Log(parameter.name);
                if(parameter.type == AnimatorControllerParameterType.Bool)
                {
                    _animator.ResetTrigger(parameter.name);
                    //Debug.Log(parameter.name + " should be off");
                }
            }
            _agent.SetDestination(transform.position);
            _cancelJob = false;
            Events.onJobChange();
        }

        // If assigned wait time for action, wait instead of running logic
        if(_waitTime > 0)
        {
            _waitTime -= logicTic;
            yield break;
        }
        
        if(!_working)   // Try to fetch a new job if unemployed
        {
            _job = JobManager.AssignJob(this);
            if(_job != null)
            {
                _working = true;
                _moving = true;
                _targetLocation = new Vector3(  x: _job.position.x,
                                                y: transform.position.y,
                                                z: _job.position.z );
                _agent.SetDestination(_targetLocation);
                _animator.SetTrigger("IsRunning");
            }
            Events.onJobChange();
        }
        else    // On a job
        {
            if(_moving) // Move towards job, do it if close enough
            {
                float distance = Vector3.Distance(transform.position, _targetLocation);
                Debug.Log("Job Distance: " + distance);

                if(distance < _job.workDistance) // Close enough, start job
                {
                    _animator.ResetTrigger("IsRunning");
                    _moving = false;
                    _waitTime = _job.length;

                    switch(_job.jobType)
                    {
                        case JobType.Cut:
                            _animator.SetTrigger("IsAttacking");
                            break;
                        case JobType.Food:
                            _animator.SetTrigger("IsCasting");
                            break;
                        case JobType.Build:
                            if(_job.resource == Resource.None)
                            {
                                // Building
                                _animator.SetTrigger("IsCasting");
                            }
                            else
                            {
                                // Deconstructing
                                _animator.SetTrigger("IsAttacking");
                            }
                            break;
                        case JobType.Magic:
                            _animator.SetTrigger("IsCasting");
                            break;
                        case JobType.Idle:
                        default:
                            _animator.SetTrigger("IsDoingIdleAction");
                            break;
                    }
                    // Stop movement
                    _agent.SetDestination(transform.position);
                    //Quaternion rotation = Quaternion.LookRotation(_targetLocation);
                    //transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1f);
                }
            }
            else // Job finished, turn off animation
            {
                Debug.Log("Job finished: " + _job.jobType);
                GameObject targetObject = GameManager.GetObjectById(IdType.Building, _job.targetObjectId);
                switch(_job.jobType)
                {
                    case JobType.Cut:
                        targetObject.GetComponent<PlaceableObject>().Cut();
                        _animator.ResetTrigger("IsAttacking");
                        break;
                    case JobType.Food:
                        targetObject.GetComponent<FruitProduction>().Unregister(_job.otherIndex);
                        _animator.ResetTrigger("IsCasting");
                        break;
                    case JobType.Build:
                        if(_job.resource == Resource.None)
                        {
                            // Building
                            _animator.ResetTrigger("IsCasting");
                            targetObject.GetComponent<PlaceableObject>().Construct();
                            GameManager.RemoveId(IdType.Job, _job.index);
                        }
                        else
                        {
                            // Deconstructing
                            targetObject.GetComponent<PlaceableObject>().Cut();
                            _animator.ResetTrigger("IsAttacking");
                        }
                        break;
                    case JobType.Magic:
                        JobManager.FinishRepeatingJob(_job);
                        _animator.ResetTrigger("IsCasting");
                        break;
                    case JobType.Idle:
                    default:
                        _animator.ResetTrigger("IsDoingIdleAction");
                        break;
                }
                //if(!_job.repeatable) GameManager.RemoveId(IdType.Job, _job.index);
                _working = false;
                _waitTime = 0.5f; 
                _job = null;
                Events.onJobChange();
            }
        }
    }

    IEnumerator Upkeep()
    {
        // Run upkeep once per minute
        while(true)
        {
            yield return new WaitForSeconds(60f);

            _isFed = GameManager.TryPay(_upkeep);

            if(_isFed) _agent.speed = _baseSpeed;
            else _agent.speed = 0.6f * _baseSpeed;
        }
    }

    public void SetColor(int colorChoice)
    {
        _colorChoice = colorChoice;
        Debug.Log("TODO");

        // Change color based on colorChoice
        Material[] mat = new Material[1];
        if(colorChoice < _mesh.materials.Length)
        {
            mat[0] = _mesh.materials[colorChoice];
            _mesh.materials = mat;
        }
        else
        {
            Debug.Log($"Villager ({this.gameObject.name}), SetColor({colorChoice}): ERROR, SET COLOR VARIANT NOT FOUND");
        }
    }

    public Job GetJob()
    {
        return _job;
    }
}
