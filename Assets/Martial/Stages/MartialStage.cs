using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/MartialStage", order = 1)]
public class MartialStage : ScriptableObject
{
    public string stageName;
    public List<MartialCharacterData> enemies;
}
