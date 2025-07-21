using System;
using UnityEngine;

public class HealthItem : MonoBehaviour, IItem
{
    [SerializeField] public int healAmount = 0;

    [SerializeField] public static event Action<int> OnHealthCollect;

    public void Collect()
    {
        OnHealthCollect.Invoke(healAmount);
        Destroy(gameObject);
    }
}