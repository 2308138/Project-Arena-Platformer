using System;
using UnityEngine;

public class Gem : MonoBehaviour, IItem
{
    [SerializeField] public static event Action<int> OnGemCollect;
    [SerializeField] public int worth = 0;

    public void Collect()
    {
        OnGemCollect.Invoke(worth);
        Destroy(gameObject);
    }
}