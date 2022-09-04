using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/MartialStageSequence", order = 1)]
public class MartialStageSequence : ScriptableObject
{
    public string sequenceName;
    public List<MartialStage> stages;
}
