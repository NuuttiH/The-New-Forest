using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lumber : PlaceableObject
{
    public override void FinishGrowth()
    {
        if(_cutDownjobIndex != -1) return;

        _cutDownjobIndex = JobManager.QueueJob(
            new Job( JobType.Cut, _cutDownResourceType,
                     this.buildingId, this.transform.position, 
                     _woodCuttingTime, _woodCuttingDistance), true);
    }
}
