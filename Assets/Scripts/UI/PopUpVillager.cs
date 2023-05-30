using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopUpVillager : PopUpMenu
{
    private Villager _villagerScript;

    public override void InitAdvanced()
    {
        _villagerScript = _bossObject.GetComponent<Villager>();
        UpdateJobText();
        Events.onJobChange += UpdateJobText;
    }
    void OnDestroy()
    {
        Events.onJobChange -= UpdateJobText;
    }


    private void UpdateJobText()
    {
        Job job = _villagerScript.GetJob();
        JobType jobType;
        string jobDescription = "";

        if(job == null)
        {
            jobType = JobType.Idle;
        }
        else jobType = job.jobType;

        switch(jobType)
        {
            case JobType.Idle:
                jobDescription = "Idling...";
                break;
            case JobType.Food:
                jobDescription = "Going to gather food from ";
                break;
            case JobType.Cut:
                jobDescription = "Going to cut down ";
                break;
            case JobType.Build:
                switch(job.buildJobType)
                {
                    case BuildJobType.Cut:
                        jobDescription = "Going to cut down ";
                        break;
                    case BuildJobType.Deconstruct:
                        jobDescription = "Going to tear down ";
                        break;
                    case BuildJobType.ConstructCraft:
                    case BuildJobType.ConstructMagic:
                        jobDescription = "Going to construct ";
                        break;
                    default:
                        jobDescription = "Error";
                        break;
                }
                break;
            case JobType.Magic:
                jobDescription = "Going to harness magical power ";
                break;
            default:
                break;
        }
        if(jobType != JobType.Idle)
        {
            GameObject targetObject = GameManager.GetObjectById(IdType.Building, job.targetObjectId);
            if(targetObject != null) jobDescription += targetObject.GetComponent<PlaceableObject>().Name;
            else jobDescription += "???";
        }
        _titleText.text = jobDescription;
    }
}
