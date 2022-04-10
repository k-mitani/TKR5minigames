using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpeechTimeBar : MonoBehaviour
{
    private float timeBarTotalLength;
    public Transform topRight;
    public Transform right;
    public Transform bottom;
    public Transform left;
    public Transform topLeft;
    private (Transform, float)[] segments;

    void Awake()
    {
        segments = new[]
        {
            (topRight, 5f),
            (right, 2.25f),
            (bottom, 9.78f),
            (left, 2.25f),
            (topLeft, 5f),
        };
        timeBarTotalLength = segments.Sum(x => x.Item2);
        DrawBar(1, 1);
    }

    public void DrawBar(float time, float timeMax, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
    {
        var elappsedRatio = (timeMax - time) / timeMax;
        var elappsedTotalLength = elappsedRatio * timeBarTotalLength;

        var accLength = 0f;
        for (int i = 0; i < segments.Length; i++)
        {
            var segment = segments[i].Item1;
            var len = segments[i].Item2;
            var remainingLength = elappsedTotalLength - accLength;
            segment.localScale = new Vector3(
                Mathf.Clamp(remainingLength, 0, len),
                segment.localScale.y,
                segment.localScale.z);

            accLength += len;
        }
    }
}
