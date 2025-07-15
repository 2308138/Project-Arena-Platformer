using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] public enum ObjectType
    {
        SmallGem,
        BigGem,
        Enemy
    }

    [SerializeField] public Tilemap tileMap;
    [SerializeField] public GameObject[] objectPrefabs;
    [SerializeField] public float bigGemProbability = 0F;
    [SerializeField] public float enemyProbability = 0F;
    [SerializeField] public int maxObjects = 0;
    [SerializeField] public float gemLifeTime = 0F;
    [SerializeField] public float spawnInterval = 0F;

    [SerializeField] private List<Vector3> validSpawnPositions = new List<Vector3>();
    [SerializeField] private List<GameObject> spawnObjects = new List<GameObject>();
    [SerializeField] private bool isSpawning = false;

    private void Start()
    {
        GatherValidPositions();
        StartCoroutine(SpawnObjectsIfNeeded());
        GameController.OnReset += LevelChange;
    }

    private void Update()
    {
        if (!tileMap.gameObject.activeInHierarchy)
        {
            LevelChange();
        }

        if (!isSpawning && ActiveObjectCount() < maxObjects)
        {
            StartCoroutine(SpawnObjectsIfNeeded());
        }
    }

    private void LevelChange()
    {
        tileMap = GameObject.Find("Ground").GetComponent<Tilemap>();
        GatherValidPositions();
        DestroyAllSpawnedObjects();
    }

    private void GatherValidPositions()
    {
        validSpawnPositions.Clear();
        BoundsInt boundsInt = tileMap.cellBounds;
        TileBase[] allTiles = tileMap.GetTilesBlock(boundsInt);
        Vector3 start = tileMap.CellToWorld(new Vector3Int(boundsInt.xMin, boundsInt.yMin, 0));

        for (int x = 0; x < boundsInt.size.x; x++)
        {
            for (int y = 0; y < boundsInt.size.y; y++)
            {
                TileBase tile = allTiles[x + y * boundsInt.size.x];

                if (tile != null)
                {
                    Vector3 place = start + new Vector3(x + 0.5F, y + 2F, 0F);
                    validSpawnPositions.Add(place);
                }
            }
        }
    }

    private int ActiveObjectCount()
    {
        spawnObjects.RemoveAll(item => item == null);
        return spawnObjects.Count;
    }

    private IEnumerator SpawnObjectsIfNeeded()
    {
        isSpawning = true;
        while (ActiveObjectCount() < maxObjects)
        {
            SpawnObject();
            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
    }

    private bool PositionHasObject(Vector3 positionToCheck)
    {
        return spawnObjects.Any(checkObj => checkObj && Vector3.Distance(checkObj.transform.position, positionToCheck) < 1.0F);
    }

    private ObjectType RandomObjectType()
    {
        float randomChoice = Random.value;

        if (randomChoice <= enemyProbability)
            return ObjectType.Enemy;
        else if (randomChoice <= (enemyProbability + bigGemProbability))
            return ObjectType.BigGem;
        else
            return ObjectType.SmallGem;

    }

    private void SpawnObject()
    {
        if (validSpawnPositions.Count == 0) return;

        Vector3 spawnPosition = Vector3.zero;
        bool validPositionFound = false;

        while (!validPositionFound && validSpawnPositions.Count > 0)
        {
            int randomIndex = Random.Range(0, validSpawnPositions.Count);
            Vector3 potentialPosition = validSpawnPositions[randomIndex];
            Vector3 leftPosition = potentialPosition + Vector3.left;
            Vector3 rightPosition = potentialPosition + Vector3.right;

            if (!PositionHasObject(leftPosition) && !PositionHasObject(rightPosition))
            {
                spawnPosition = potentialPosition;
                validPositionFound = true;
            }

            validSpawnPositions.RemoveAt(randomIndex);
        }

        if (validPositionFound)
        {
            ObjectType objectType = RandomObjectType();
            GameObject gameObject = Instantiate(objectPrefabs[(int)objectType], spawnPosition, Quaternion.identity);
            spawnObjects.Add(gameObject);

            if (objectType != ObjectType.Enemy)
            {
                StartCoroutine(DestroyObjectAfterTime(gameObject, gemLifeTime));
            }
        }
    }

    private void DestroyAllSpawnedObjects()
    {
        foreach (GameObject obj in spawnObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        spawnObjects.Clear();
    }

    private IEnumerator DestroyObjectAfterTime(GameObject gameObject, float time)
    {
        yield return new WaitForSeconds(time);

        if (gameObject)
        {
            spawnObjects.Remove(gameObject);
            validSpawnPositions.Add(gameObject.transform.position);
            Destroy(gameObject);
        }
    }
}