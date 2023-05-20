using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum VillagerType { None, Elf, Goblin }

[Serializable]
public class JobEfficiency {
    public JobType jobType;
    public float val;

    public JobEfficiency(JobType jobType, float val)
    {
        this.jobType = jobType;
        this.val = val;
    }
}

public enum CurrentAction { WaitingForAction, Moving, Cutting, Gathering, Constructing, Casting, Idle }

public class Villager : MonoBehaviour
{
    [HideInInspector] public int characterId;
    [SerializeField] private ObjectInfo _villagerInfo;
    [SerializeField] private VillagerType _villagerType;
    private bool _initialized = false;
    private NavMeshAgent _agent;
    [SerializeField] private Animator _animator;
    [SerializeField] private SkinnedMeshRenderer _mesh;
    [SerializeField] private int _colorChoice = 0;
    [SerializeField] private VillagerAudio _villagerAudio;

    private CurrentAction _currentAction = CurrentAction.WaitingForAction;
    private Job _job;
    private Vector3 _targetLocation;
    private float _waitTime;
    private bool _cancelJob;

    public float logicTic = 0.6f;

    [SerializeField] private Cost[] _upkeep;
    private bool _isFed = true;
    private float _baseSpeed;
    
    [SerializeField] private JobEfficiency[] _jobEffiency 
        = new JobEfficiency[] {
        new JobEfficiency(JobType.Idle, 1f),
        new JobEfficiency(JobType.Food, 1f),
        new JobEfficiency(JobType.Cut, 1f),
        new JobEfficiency(JobType.Build, 1f),
        new JobEfficiency(JobType.Magic, 1f),
    };
    
    public Dictionary<JobType, float> jobEfficiency;
    /*[SerializeField] public Dictionary<JobType, float> jobEfficiency 
        = new Dictionary<JobType, float> {
        { JobType.Idle, 1f },
        { JobType.Food, 1f },
        { JobType.Cut, 1f },
        { JobType.Build, 1f },
        { JobType.Magic, 1f }
    };*/


    void Start()
    {
        jobEfficiency = new Dictionary<JobType, float>();
        foreach(JobEfficiency je in _jobEffiency)
        {
            jobEfficiency.Add(je.jobType, je.val);
        }
        StartCoroutine(Initialize(null, 0.3f));
    }
    public void Init(CharacterSaveData data = null, float wait = 0.3f)
    {
        StartCoroutine(Initialize(data, wait));
    }
    IEnumerator Initialize(CharacterSaveData data, float wait)
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
                _cancelJob = false;
                // Get colorChoice smh
            }
            else
            {
                characterId = data.characterId;
                GameManager.AddId(IdType.Character, characterId, this.gameObject);

                _currentAction = data.currentAction;
                _job = data.job;
                _targetLocation = data.targetLocation;
                _waitTime = data.waitTime;
                _cancelJob = data.cancelJob;
                SetColor(data.colorChoice);

                if(_currentAction == CurrentAction.Moving)
                {
                    _agent.SetDestination(_targetLocation);
                    _animator.SetTrigger("IsRunning");
                }
            }

            GetComponent<OpenPopUpOnClick>().Init();
            StartCoroutine(AILogic());
            StartCoroutine(Upkeep());
            StartCoroutine(MakeNoise());
        } 
    }
    public CharacterSaveData FormSaveData()
    {
        return new CharacterSaveData(   
                        gameObject.transform.position, gameObject.transform.rotation, 
                        _villagerInfo.name, characterId,
                        _currentAction, _job, 
                        _targetLocation, _waitTime, 
                        _cancelJob, _colorChoice);
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
            _currentAction = CurrentAction.WaitingForAction;
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
        
        // Try to fetch a new job
        if(_currentAction == CurrentAction.WaitingForAction)   
        {
            _job = JobManager.AssignJob(this);
            if(_job != null)
            {
                _currentAction = CurrentAction.Moving;
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
            if(_currentAction == CurrentAction.Moving) // Move towards job, do it if close enough
            {
                float distance = Vector3.Distance(transform.position, _targetLocation);
                //Debug.Log("Job Distance: " + distance);

                if(distance < _job.workDistance) // Close enough, start job
                {
                    _animator.ResetTrigger("IsRunning");
                    _waitTime = _job.length * (1f / jobEfficiency[_job.jobType]);

                    switch(_job.jobType)
                    {
                        case JobType.Cut:
                            _animator.SetTrigger("IsAttacking");
                            _currentAction = CurrentAction.Cutting;
                            break;
                        case JobType.Food:
                            _animator.SetTrigger("IsCasting");
                            _currentAction = CurrentAction.Gathering;
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
                            _currentAction = CurrentAction.Constructing;
                            break;
                        case JobType.Magic:
                            _animator.SetTrigger("IsCasting");
                            _currentAction = CurrentAction.Casting;
                            break;
                        case JobType.Idle:
                        default:
                            _animator.SetTrigger("IsDoingIdleAction");
                            _currentAction = CurrentAction.Idle;
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
                Debug.Log($"Villager ({this.gameObject.name}), finished job (target:{targetObject.name})");
                switch(_job.jobType)
                {
                    case JobType.Cut:
                        targetObject.GetComponent<PlaceableObject>().Cut();
                        _animator.ResetTrigger("IsAttacking");
                        break;
                    case JobType.Food:
                        targetObject.GetComponent<FruitProduction>().Unregister(_job.index, _job.otherIndex);
                        _animator.ResetTrigger("IsCasting");
                        break;
                    case JobType.Build:
                        if(_job.resource == Resource.None)
                        {
                            // Building
                            _animator.ResetTrigger("IsCasting");
                            targetObject.GetComponent<PlaceableObject>().Construct();
                            JobManager.RemoveJob(_job.index, true);
                            //GameManager.RemoveId(IdType.Job, _job.index);
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
                _currentAction = CurrentAction.WaitingForAction;
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

    IEnumerator MakeNoise()
    {
        // Run upkeep once per minute
        float wait = 0.4f;
        while(true)
        {
            yield return new WaitForSeconds(wait);

            switch(_currentAction)
            {
                case CurrentAction.Moving:
                    if(GrassSystem.HasGrass(this.gameObject.transform.position)) // Check for grass
                        wait = Tools.PlayAudio(this.gameObject, _villagerAudio.walkingOnGrass);
                    else
                        wait = Tools.PlayAudio(this.gameObject, _villagerAudio.walkingOnSand);
                    break;
                case CurrentAction.Cutting:
                    wait = Tools.PlayAudio(this.gameObject, _villagerAudio.cutTrees);
                    break;
                case CurrentAction.Gathering:
                    wait = Tools.PlayAudio(this.gameObject, _villagerAudio.gatheringFood);
                    break;
                case CurrentAction.Constructing:
                    wait = Tools.PlayAudio(this.gameObject, _villagerAudio.construction);
                    break;
                case CurrentAction.Casting:
                    wait = Tools.PlayAudio(this.gameObject, _villagerAudio.magic);
                    break;
                case CurrentAction.Idle:
                    wait = Tools.PlayAudio(this.gameObject, _villagerAudio.idle);
                    break;
                case CurrentAction.WaitingForAction:
                default:
                    wait = 1.5f;
                    break;
            }
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
    public VillagerType GetVillagerType()
    {
        return _villagerType;
    }
}
