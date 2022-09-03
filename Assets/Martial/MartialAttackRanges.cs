using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MartialAttackRanges : MonoBehaviour
{
    public Transform sword;
    public Transform boldLine;
    public Transform threeLine;

    public IEnumerable<MeshCollider> EnumerateRangeColliders(System.Func<MartialAttackRanges, Transform> selector)
    {
        var target = selector(this);

        var collider = target.GetComponent<MeshCollider>();
        if (collider != null) yield return collider;
        for (int i = 0; i < target.childCount; i++)
        {
            var c = target.GetChild(i).GetComponent<MeshCollider>();
            if (c != null) yield return c;
        }
    }

    public void ShowMain()
    {
        sword.GetComponent<Renderer>().enabled = true;
    }

    public void Show(System.Func<MartialAttackRanges, Transform> selector)
    {
        foreach (var item in EnumerateRangeColliders(selector))
        {
            item.GetComponent<Renderer>().enabled = true;
        }
    }

    public void HideAll()
    {
        sword.GetComponent<Renderer>().enabled = false;
        boldLine.GetComponent<Renderer>().enabled = false;
        for (int i = 0; i < threeLine.childCount; i++)
        {
            threeLine.GetChild(i).GetComponent<Renderer>().enabled = false;
        }
    }
}
