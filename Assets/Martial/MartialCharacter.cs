using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MartialCharacter : MonoBehaviour
{
    public int prowess;
    public GameObject moveRange;
    public GameObject attackRange;
    public Transform shadowChild;
    public Transform shadow;

    public Rigidbody rb;

    public float MaxMoveAmount => prowess / 25f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        var scale = moveRange.transform.localScale;
        scale *= MaxMoveAmount;
        scale.y = moveRange.transform.localScale.y;
        moveRange.transform.localScale = scale;

        moveRange.SetActive(false);
        attackRange.SetActive(false);
        shadow.gameObject.SetActive(false);
    }

    public void ShowMoveRange()
    {
        moveRange.transform.position = transform.position;
        moveRange.SetActive(true);
    }
    public void HideMoveRange()
    {
        moveRange.SetActive(false);
    }

    public void ShowAttackRange()
    {
        attackRange.SetActive(true);
    }
    public void HideAttackRange()
    {
        attackRange.SetActive(false);
    }

    public void ShowShadow()
    {
        CopyTransforms(shadowChild.transform, transform);
        shadow.gameObject.SetActive(true);
    }
    public void HideShadow()
    {
        shadow.gameObject.SetActive(false);
    }

    private void CopyTransforms(Transform a, Transform b)
    {
        for (int i = 0; i < a.childCount; i++)
        {
            var childA = a.GetChild(i);
            var childB = b.GetChild(i);
            childA.localPosition = childB.localPosition;
            childA.localScale = childB.localScale;
            childA.localRotation = childB.localRotation;
            CopyTransforms(childA, childB);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
