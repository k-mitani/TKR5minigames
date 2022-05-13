using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SabotageBlock : MonoBehaviour
{
    public int hpMax = 3;
    public int hp;

    private new SpriteRenderer renderer;

    // Start is called before the first frame update
    void Start()
    {
        hp = hpMax;
        renderer = GetComponent<SpriteRenderer>();
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (hp == 3)
        {
            renderer.color = Color.green;
        }
        else if (hp == 2)
        {
            renderer.color = Color.yellow;
        }
        else
        {
            renderer.color = Color.red;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("SabotageBall"))
        {
            if (hp > 1)
            {
                hp--;
                UpdateColor();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
