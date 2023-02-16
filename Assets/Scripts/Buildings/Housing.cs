using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Housing : PlaceableObject
{
    [SerializeField] private int _populationHousing = 1;
    private bool _populationLimitApplied = false;

    public override void FinishConstruction()
    {
        GameManager.AdjustPopulationLimit(_populationHousing);
        _populationLimitApplied = true;
    }
    public override void PrepUnplace()
    {
        if(_populationLimitApplied) GameManager.AdjustPopulationLimit(-_populationHousing);
    }
}
