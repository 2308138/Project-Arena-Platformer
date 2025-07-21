using UnityEngine;

[System.Serializable]

public class LootItem
{
    public GameObject itemPrefab;
    [Range(0F, 100F)] public float dropChance;
}