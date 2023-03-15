using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "00 ObjectInfo", menuName = "ScriptableObject/ObjectInfo/ObjectInfo")]
public class ObjectInfo : ScriptableObject
{
    public int id;
    public new string name;
    public Sprite sprite;
    [TextArea(2, 10)]
    public string description;
}
