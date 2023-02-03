using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lumber : PlaceableObject
{
    public override void FinishPlacing()
    {
        _cuttable = true;
        _cutByDefault = true;
    }

    public override void FinishGrowth()
    {
        _jobIndex = JobManager.QueueJob(
            new Job( JobType.Cut, _cutDownResourceType,
                     this.buildingId, this.transform.position, 
                     _woodCuttingTime, _woodCuttingDistance), true);
    }
}
