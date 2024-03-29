using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobManager : MonoBehaviour
{
    private static JobManager _instance;

    private Dictionary<JobType, int> _jobWeights;
    private float _jobWeightNextCheck;
    [SerializeField] private float _jobWeightCheckTickSize = 4f;
    private HashSet<int> _foodJobs;
    private HashSet<int> _lumberJobs;
    private HashSet<int> _buildJobs;
    private HashSet<int> _magicJobs;
    private HashSet<int> _inProgressJobs;
    private HashSet<int> _priorityJobs;

    void Awake()
    {
		if(_instance == null) _instance = this;
		else
		{
			Destroy(this);
			return;
        }
        _jobWeights = new Dictionary<JobType, int>();
        _foodJobs = new HashSet<int>();
        _lumberJobs = new HashSet<int>();
        _buildJobs = new HashSet<int>();
        _magicJobs = new HashSet<int>();
        _inProgressJobs = new HashSet<int>();
        _priorityJobs = new HashSet<int>();

        _jobWeights.Add(JobType.Idle, 5);
        for(int i=1; i<System.Enum.GetValues(typeof(JobType)).Length; i++)
        {
            _jobWeights.Add((JobType)i, -100);
        }
        _jobWeightNextCheck = Time.time;
        //_jobWeights[JobType.Build] = -100;
        //_jobWeights[JobType.Magic] = -100;
        StartCoroutine(AssessJobWeightsCoroutine());

        #if UNITY_EDITOR
            StartCoroutine(DebugLogging());
        #endif
    }
    IEnumerator DebugLogging()
    {
        yield return new WaitForSeconds(2f);
        Debug.Log($"JobManager: Info: _lumberJobs ");
        foreach(int index in _lumberJobs)
        {
            Job job = GameManager.GetJobById(index);
            Debug.Log($"JobManager: Info: (index: {job.index}, jobType: {job.jobType},  "
                    + $"targetObjectId: {job.targetObjectId}, inProgress: {job.inProgress}, workerId: {job.workerId})");
        }
        while(true)
        {
            yield return new WaitForSeconds(3f);
            Debug.Log($"JobManager: _foodJobs: {Tools.LogHashSet(_foodJobs)}");
            Debug.Log($"JobManager: _lumberJobs: {Tools.LogHashSet(_lumberJobs)}");
            Debug.Log($"JobManager: _buildJobs: {Tools.LogHashSet(_buildJobs)}");
            Debug.Log($"JobManager: _magicJobs: {Tools.LogHashSet(_magicJobs)}");
            Debug.Log($"JobManager: _inProgressJobs: {Tools.LogHashSet(_inProgressJobs)}");
            Debug.Log($"JobManager: _priorityJobs: {Tools.LogHashSet(_priorityJobs)}");
        }
    }

    public static int QueueJob(Job job, bool newJob = true)
    {
        if(newJob) job.index = GameManager.GenerateId(IdType.Job, null, job);
        else GameManager.AddId(IdType.Job, job.index, null, job);
        
        //Debug.Log("JobManager: New job added (" + job.index + ", " + job.targetObjectId + ")");
        
        if(job.inProgress)
        {
            _instance._inProgressJobs.Add(job.index);
        }

        else switch(job.jobType)
        {
            case JobType.Food:
                _instance._foodJobs.Add(job.index);
                break;
            case JobType.Cut:
                _instance._lumberJobs.Add(job.index);
                break;
            case JobType.Build:
                _instance._buildJobs.Add(job.index);
                break;
            case JobType.Magic:
                _instance._magicJobs.Add(job.index);
                break;
            default:
                break;
        }
        return job.index;
    }

    public static void RemoveJob(int jobIndex, bool finished = false)
    {
        //Debug.Log($"JobManager: Removing job index {jobIndex}, finished({finished})");
        if(jobIndex == -1) return;

        Job job = GameManager.GetJobById(jobIndex);

        #if UNITY_EDITOR
            Debug.Log(  $"JobManager: Removing job exists: {job != null})");
            Debug.Log(  $"JobManager: Removing job data: (jobType: {job.jobType}, targetObjectId: {job.targetObjectId}, "
                        + $"inProgress: {job.inProgress}, workerId: {job.workerId})");
        #endif

        if(job.inProgress)
        {
            _instance._inProgressJobs.Remove(jobIndex);
            if(!finished)
                GameManager.GetObjectById(IdType.Character, job.workerId).GetComponent<Villager>().CancelJob();
        }
        else switch(job.jobType)
        { // wrong type?
            case JobType.Food:
                _instance._foodJobs.Remove(jobIndex);
                break;
            case JobType.Cut:
                _instance._lumberJobs.Remove(jobIndex);
                break;
            case JobType.Build:
                _instance._buildJobs.Remove(jobIndex);
                break;
            case JobType.Magic:
                _instance._magicJobs.Remove(jobIndex);
                break;
            default:
                break;
        }

        GameManager.RemoveId(IdType.Job, jobIndex);
    }

    private static void AssessJobWeights()
    {
        int weight = 40;
        // Food job
        if(_instance._foodJobs.Count < 1) weight = -100;
        else if(GameManager.GetResource(Resource.Food) > 15) weight = 4;
        else if(GameManager.GetResource(Resource.Food) > 8) weight = 8;
        _instance._jobWeights[JobType.Food] = weight;

        // Lumber job
        weight = 10;
        if(_instance._lumberJobs.Count < 1) weight = -100;
        else if(GameManager.GetResource(Resource.Lumber) > 25) weight = 4;
        else if(GameManager.GetResource(Resource.Lumber) > 15) weight = 7;
        _instance._jobWeights[JobType.Cut] = weight;

        // Build job
        weight = 20;
        if(_instance._buildJobs.Count < 1) weight = -100;
        else if(_instance._buildJobs.Count > 5) weight = 40;
        _instance._jobWeights[JobType.Build] = weight;

        // Magic job
        weight = 15;
        if(_instance._magicJobs.Count < 1) weight = -100;
        else if(GameManager.GetResource(Resource.Magic) > 24) weight = 3;
        else if(GameManager.GetResource(Resource.Magic) > 12) weight = 10;
        _instance._jobWeights[JobType.Magic] = weight;
    }
    IEnumerator AssessJobWeightsCoroutine()
    {
        if(Time.time >= _jobWeightNextCheck)
        {
            AssessJobWeights();
            yield return new WaitForSeconds(_jobWeightCheckTickSize);
        }
        else
        {
            yield return new WaitForSeconds(_jobWeightCheckTickSize - Time.time);
        }
        StartCoroutine(AssessJobWeightsCoroutine());
    }

    public static Job AssignJob(Villager villager)
    {
        Vector3 villagerLocation = villager.gameObject.transform.position;
        Job job = null;
        int randomVal = Random.Range(0, 11);
        JobType jobType = JobType.Idle;
        int jobWeight = _instance._jobWeights[jobType] + randomVal;

        // Check if a job is already supposed to assigned to this villager
        foreach(int id in _instance._inProgressJobs)
        {
            job = GameManager.GetJobById(GetClosestJobFromSet(
                            villagerLocation, _instance._inProgressJobs));
            if(job.workerId == villager.characterId)
            {
                return job;
            }
        }

        // Choose jobtype based on weights and random chance
        for(int i=1; i<System.Enum.GetValues(typeof(JobType)).Length; i++)
        {
            int newJobWeight = _instance._jobWeights[(JobType)i] + Random.Range(0, 5);

            if(villager.jobEfficiency[(JobType)i] <= 0f)
                newJobWeight -= 100;
            else newJobWeight = (int)((float)newJobWeight * villager.jobEfficiency[(JobType)i]);

            if(newJobWeight > jobWeight)
            {
                jobWeight = newJobWeight;
                jobType = (JobType)i;
            }
        }

        // Pick job, make sure it exists
        switch(jobType)
        {
            case JobType.Food:
                if(_instance._foodJobs.Count > 0)
                {
                    job = GameManager.GetJobById(GetClosestJobFromSet(
                                villagerLocation, _instance._foodJobs));
                    job.inProgress = true;
                    GameManager.UpdateJobInProgress(job.index);
                    _instance._foodJobs.Remove(job.index);
                    _instance._inProgressJobs.Add(job.index);
                }
                else jobType = JobType.Idle;
                break;
            case JobType.Cut:
                if(_instance._lumberJobs.Count > 0)
                {
                    job = GameManager.GetJobById(GetClosestJobFromSet(
                                villagerLocation, _instance._lumberJobs));
                    job.inProgress = true;
                    GameManager.UpdateJobInProgress(job.index);
                    _instance._lumberJobs.Remove(job.index);
                    _instance._inProgressJobs.Add(job.index);
                }
                else jobType = JobType.Idle;
                break;
            case JobType.Build:
                if(_instance._buildJobs.Count > 0)
                {
                    job = GameManager.GetJobById(GetClosestJobFromSet(
                                villagerLocation, _instance._buildJobs));
                    job.inProgress = true;
                    GameManager.UpdateJobInProgress(job.index);
                    _instance._buildJobs.Remove(job.index);
                    _instance._inProgressJobs.Add(job.index);
                }
                else jobType = JobType.Idle;
                break;
            case JobType.Magic:
                if(_instance._magicJobs.Count > 0)
                {
                    job = GameManager.GetJobById(GetClosestJobFromSet(
                                villagerLocation, _instance._magicJobs));
                    job.inProgress = true;
                    GameManager.UpdateJobInProgress(job.index);
                    _instance._magicJobs.Remove(job.index);
                    _instance._inProgressJobs.Add(job.index);
                }
                else jobType = JobType.Idle;
                break;
            default:
                break;
        }
        
        if(jobType == JobType.Idle)
        {
            //Debug.Log("JobManager.AssignJob assigned idle job");
            return null;
        }
        if(job == null)
        {
            //Debug.LogError($"JobManager.AssignJob can't handle jobtype ({jobType})");
            return null;
        }
        job.workerId = villager.characterId;
        return job;
    }

    public static void FinishRepeatingJob(Job job)
    {
        //Debug.Log("FinishRepeatingJob");
        GameManager.AddResource(job.resource, job.rewardValue);
        job.inProgress = false;
        
        switch(job.jobType)
        {
            case JobType.Food:
                _instance._inProgressJobs.Remove(job.index);
                _instance._foodJobs.Add(job.index);
                break;
            case JobType.Cut:
                _instance._inProgressJobs.Remove(job.index);
                _instance._lumberJobs.Add(job.index);
                break;
            case JobType.Build:
                _instance._inProgressJobs.Remove(job.index);
                _instance._buildJobs.Add(job.index);
                break;
            case JobType.Magic:
                _instance._inProgressJobs.Remove(job.index);
                _instance._magicJobs.Add(job.index);
                break;
            default:
                break;
        }
    }

    public static int GetClosestJobFromSet(Vector3 villagerLocation, HashSet<int> set)
    {
        float distance = 1000000;
        int result = -1;
        foreach (var i in set){
            float newDistance = Vector3.Distance(villagerLocation, 
                                                GameManager.GetJobById(i).position);
            if(newDistance < distance)
            {
                distance = newDistance;
                result = i;
            } 
        }
        return result;
    }
}
