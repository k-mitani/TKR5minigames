using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeechHPMeter : MonoBehaviour
{
    public int hpMax = 5;
    public int hp;
    public MeshRenderer heartPrefab;
    public MeshRenderer noHeartPrefab;
    public float heartsMargin;
    public Color color;
    private MeshRenderer[] hearts;
    private MeshRenderer[] noHearts;

    // Start is called before the first frame update
    void Start()
    {
        hp = hpMax;
        hearts = new MeshRenderer[hpMax];
        noHearts = new MeshRenderer[hpMax];
        for (int i = 0; i < hpMax; i++)
        {
            var heart = Instantiate(heartPrefab, transform);
            heart.transform.position += Vector3.right * heartsMargin * i;
            heart.material.color = color;
            hearts[i] = heart;
            
            var noHeart = Instantiate(noHeartPrefab, transform);
            noHeart.transform.position = heart.transform.position;
            noHeart.material.color = color;
            noHearts[i] = noHeart;
        }
    }

    public void Update()
    {
        for (int i = 0; i < hpMax; i++)
        {
            hearts[i].gameObject.SetActive(i < hp);
            noHearts[i].gameObject.SetActive(i >= hp);
        }
    }
}
