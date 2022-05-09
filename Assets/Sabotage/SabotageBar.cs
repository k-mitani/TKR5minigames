using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SabotageBar : MonoBehaviour
{
    private Rigidbody2D rb;
    public float barSpeed;
    public int remainingBalls = 3;
    public SabotageBall ballPrefab;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start");

        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        var h = Input.GetAxis("Horizontal");
        rb.velocity = Vector2.right * h * barSpeed;

        if (remainingBalls > 0 && Input.GetKeyDown(KeyCode.Space))
        {
            remainingBalls--;
            var ball = Instantiate(ballPrefab);
            var ballPos = transform.position;
            ballPos.y += 0.5f;
            ball.transform.position = ballPos;
        }
    }
}
