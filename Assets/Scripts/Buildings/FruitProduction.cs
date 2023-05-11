using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitProduction : PlaceableObject
{
    [SerializeField] private GameObject _fruitPrefab;
    [SerializeField] private GameObject[] _fruits;
    [SerializeField] private float _fruitGrowTime = 20f;
    [SerializeField] private float _fruitGrowthTics = 5f;
    [SerializeField] private float _fruitPickingTime = 4f;
    [SerializeField] private float _fruitPickingDistance = 1.5f;
    [SerializeField] private Resource _fruitResourceType = Resource.Food;
    [SerializeField] private int _fruitFoodValue = 1;
    private int[] _fruitJobIndex;
    private Vector3 _fruitOriginalScale;
    private float[] _fruitGrowthProgress;
    private float _fruitTicSize;
    [SerializeField] private bool _selfDestructAfterUse = true;
    [SerializeField] private int _useCycles = 8;
    private int _cycles = 0;
    private bool _beingDestroyed = false;
    
    
    public override void FinishPlacing()
    {
        Debug.Log("FinishPlacing (" + this.gameObject.name + "...");
        _fruitJobIndex = new int[_fruits.Length];
        _fruitGrowthProgress = new float[_fruits.Length];
        _fruitTicSize = 1f / _fruitGrowthTics;
        _fruitOriginalScale = _fruits[0].transform.localScale;
        for(int i=0; i<_fruits.Length; i++)
        {
            _fruits[i].transform.localScale = new Vector3(0f, 0f, 0f);
        }
    }

    public override void PrepUnplace()
    {
        for(int i=0; i<_fruits.Length; i++)
        {
            Unregister(_fruitJobIndex[i], i, false);
        }
    }

    public override void FinishGrowth()
    {
        // Start fruit growth
        for(int i=0; i<_fruits.Length; i++)
        {
            StartCoroutine(ManageFruitGrowth(i));
        }
    }

    IEnumerator ManageFruitGrowth(int i)
    {
        // Check expiration
        if(_selfDestructAfterUse) _cycles++;
        if(_cycles > _useCycles)
        {
            if(_beingDestroyed) yield break;
            bool destroy = true;
            foreach(float growth in _fruitGrowthProgress) 
                if(growth != 0) destroy = false;
            if(destroy)
            {
                _beingDestroyed = true;
                MessageLog.NewMessage(new MessageData(
                    $"{_objectInfo.name} was foraged completely and disappeared", MessageType.Unimportant));
                yield return null;
                Unplace();
            }
            yield break;
        }

        // Grow fruit
        //_fruitGrowthProgress[i] = 0f;
        _fruits[i].transform.localScale = new Vector3(0f, 0f, 0f);
        
        while(_fruitGrowthProgress[i] < 1f)
        {
            yield return new WaitForSeconds(
                1f / GameManager.GetGrowthMultiplier() * _fruitGrowTime / _fruitGrowthTics);
            _fruits[i].transform.localScale += (_fruitOriginalScale * _fruitTicSize);
            _fruitGrowthProgress[i] += _fruitTicSize;
        }
        
        Job newJob = new Job(   JobType.Food, _fruitResourceType,
                                this.buildingId, _fruits[i].transform.position, 
                                _fruitPickingTime, _fruitPickingDistance);
        newJob.otherIndex = i;
        int jobIndex = JobManager.QueueJob(newJob, true);
        _fruitJobIndex[i] = jobIndex;
    }

    public void Unregister(int index, int otherIndex, bool rewardFood = true)
    {
        if(rewardFood) GameManager.AddResource(Resource.Food, _fruitFoodValue);
        JobManager.RemoveJob(index);
        _fruitGrowthProgress[otherIndex] = 0f;
        _fruitJobIndex[otherIndex] = -1;
        StartCoroutine(ManageFruitGrowth(otherIndex));
    }
}
