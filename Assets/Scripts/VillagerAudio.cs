using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VillagerAudio", menuName = "ScriptableObject/VillagerAudio")]
public class VillagerAudio : ScriptableObject
{
    public AudioEvent walkingOnGrass;
    public AudioEvent walkingOnSand;
    public AudioEvent cutTrees;
    public AudioEvent gatheringFood;
    public AudioEvent construction;
    public AudioEvent magic;
    public AudioEvent idle;
}
