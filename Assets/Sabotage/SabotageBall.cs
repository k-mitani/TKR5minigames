using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SabotageBall : MonoBehaviour
{
    private Rigidbody2D rb;
    public float ballSpeed = 5;
    public int tetst;
    public ContactPoint2D[] contactPoints = new ContactPoint2D[1];
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(1, 1).normalized * ballSpeed;
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("OnTriggerEnter2D");
        if (!collision.gameObject.CompareTag("SabotageBall"))
        {
            var v = rb.velocity;
            // バーとぶつかったら衝突場所に合わせて向きを変える。
            if (collision.gameObject.CompareTag("SabotageBar"))
            {
                var diff = (transform.position - collision.gameObject.transform.position).normalized;
                diff.y += 0.3f;
                v = diff.normalized * ballSpeed;
            }
            else if (collision.gameObject.CompareTag("SabotageWallSide"))
            {
                v.x = -v.x;
            }
            else if (collision.gameObject.CompareTag("SabotageWallAbove"))
            {
                v.y = -v.y;
            }
            else
            {
                var contact = collision.GetContact(0);
                var offset = contact.point - (Vector2)collision.gameObject.transform.position;
                var isSideContact = Mathf.Abs(offset.x) < Mathf.Abs(offset.y);
                Debug.Log("isSide: " + isSideContact);
                if (isSideContact)
                {
                    v.x = -v.x;
                }
                else
                {
                    v.y = -v.y;
                }
            }
            rb.velocity = v;
        }
    }
}
