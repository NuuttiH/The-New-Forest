using System;
using UnityEngine;

public enum JobType {Idle, Food, Cut, Build, Magic}
public enum BuildJobType {None, Cut, Deconstruct, ConstructCraft, ConstructMagic}

[Serializable]
public class Job
{
    public int index;
    public JobType jobType;
    public Resource resource;
    public int targetObjectId;
    public Vector3 position;
    public float length;
    public float workDistance;
    public int followUpJobId;
    public BuildJobType buildJobType;
    public bool inProgress;
    public bool repeatable;
    public int workerId;
    public int otherIndex;
    public int rewardValue;

    public Job( JobType jobType, Resource resource, 
                int targetObjectId, Vector3 position, 
                float length, float workDistance, 
                int followUpJobId = -1, BuildJobType buildJobType = BuildJobType.None,
                bool inProgress = false, bool repeatable = false, 
                int workerId = -1, int otherIndex = -1,
                int rewardValue = 0 )
    {
        this.index = -1;
        this.jobType = jobType;
        this.resource = resource;
        this.targetObjectId = targetObjectId;
        this.position = position;
        this.length = length;
        this.workDistance = workDistance;
        this.followUpJobId = followUpJobId;
        this.buildJobType = buildJobType;
        this.inProgress = inProgress;
        this.repeatable = repeatable;
        this.workerId = workerId;
        this.otherIndex = otherIndex;
        this.rewardValue = rewardValue;
    }
    public Job(){}
}
