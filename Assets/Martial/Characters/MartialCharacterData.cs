using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/MartialCharacterData", order = 1)]
public class MartialCharacterData : ScriptableObject
{
    public string characterName;
    public int prowess;
    public int kiaiMax;
    public List<MartialSpecialActionTag> specialActions;
}
