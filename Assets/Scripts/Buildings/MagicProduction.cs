using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicProduction : PlaceableObject
{
    [SerializeField] private float _jobTime = 20f;
    [SerializeField] private GameObject _jobLocation;
    [SerializeField] private Resource _resourceType = Resource.Magic;
    [SerializeField] private int _jobMagicValue = 3;
    [SerializeField] private float _jobDistance = 1.5f;
    

    public override void FinishConstruction()
    {
        if(_jobIndex != -1) return;

        Job newJob = new Job(   JobType.Magic, _resourceType,
                                this.buildingId, _jobLocation.transform.position, 
                                _jobTime, _jobDistance);
        newJob.rewardValue = _jobMagicValue;
        _jobIndex = JobManager.QueueJob(newJob, true);
    }
}
