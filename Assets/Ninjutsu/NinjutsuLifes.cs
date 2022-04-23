using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NinjutsuLifes : MonoBehaviour
{
    private Transform[] lifes;

    public int _lifeCount;
    public int LifeCount
    {
        get => _lifeCount;
        set
        {
            _lifeCount = value;
            Draw();
        }
    }

    void Awake()
    {
        lifes = new[]
        {
            transform.Find("Life1"),
            transform.Find("Life2"),
            transform.Find("Life3"),
        };
    }
    void Start()
    {
        Draw();
    }

    private void Draw()
    {
        for (int i = 0; i < lifes.Length; i++)
        {
            var heart = lifes[i].Find("Heart");
            var noHeart = lifes[i].Find("NoHeart");
            heart.gameObject.SetActive(i < LifeCount);
            noHeart.gameObject.SetActive(i >= LifeCount);
        }
    }

}
