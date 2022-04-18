using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiningBox : MonoBehaviour
{
    public SpriteRenderer picture;
    
    public SpriteRenderer background;
    private Color backgroundNormalColor;
    public Color backgroundOkColor;
    public Color backgroundNgColor;

    public Transform curtain;
    private Vector3 curtainShowScale;
    private Vector3 curtainShowPosition;
    private Vector3 curtainHideScale;
    private Vector3 curtainHidePosition;
    // Start is called before the first frame update
    void Awake()
    {
        picture = transform.Find("Square").GetComponent<SpriteRenderer>();
        picture.transform.localScale = Vector3.one / 16 * 100;
        
        background = transform.Find("Background").GetComponent<SpriteRenderer>();
        backgroundNormalColor = background.color;

        curtain = transform.Find("Curtain").GetComponent<Transform>();
        curtainShowScale = curtain.localScale;
        curtainShowPosition = curtain.localPosition;
        curtainHideScale = new Vector3(curtainShowScale.x, 0, curtainShowScale.z);
        curtainHidePosition = curtain.localPosition - (Vector3.up * curtainShowScale.y);
    }

    public IEnumerator ShowCurtain(float duration)
    {
        curtain.localScale = curtainHideScale;
        curtain.localPosition = curtainShowPosition;
        var current = 0f;
        while (current < duration)
        {
            yield return null;
            current += Time.deltaTime;
            var y = Mathf.Lerp(curtainHideScale.y, curtainShowScale.y, current / duration);
            curtain.localScale = new Vector3(curtainShowScale.x, y, curtainShowScale.z);
        }
        curtain.localScale = curtainShowScale;
    }

    public IEnumerator HideCurtain(float duration)
    {
        background.color = backgroundNormalColor;
        curtain.localScale = curtainShowScale;
        curtain.localPosition = curtainShowPosition;
        var current = 0f;
        while (current < duration)
        {
            yield return null;
            current += Time.deltaTime;
            var ys = Mathf.Lerp(curtainShowScale.y, curtainHideScale.y, current / duration);
            curtain.localScale = new Vector3(curtainShowScale.x, ys, curtainShowScale.z);
            var yp = Mathf.Lerp(curtainShowPosition.y, curtainHidePosition.y, current / duration);
            curtain.localPosition = new Vector3(curtainShowPosition.x, yp, curtainShowPosition.z);
        }
        curtain.localScale = curtainHideScale;
        curtain.localPosition = curtainHidePosition;
    }

    public void SetBackgroundOkNg(bool ok)
    {
        if (ok) background.color = backgroundOkColor;
        else background.color = backgroundNgColor;
    }

    public Sprite Sprite
    {
        get => picture.sprite;
        set => picture.sprite = value;
    }
}
