using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject spawnContainer;
    public GameObject[] obstaclePrefabs;
    public GameObject[] powerUpPrefabs;
    public float[] spawnRate = { 1.25f };
    private float timeSinceLastSpawned;
    public Vector2[] startingPosition;
    public Vector2 pickUpStartingPosition;
    public int powerUpSpawnCount = 5;
    int spawnCounter = 0;
    int currentLevel = 0;

    // Start is called before the first frame update
    void Start()
    {
            
    }
    public void SetLevel(int level)
    {
        currentLevel = level ;
        currentLevel = math.clamp(currentLevel, 0, obstaclePrefabs.Length + 1);
    }

    // Update is called once per frame
    void Update()
    {
        int size = obstaclePrefabs.Length;
        timeSinceLastSpawned += Time.deltaTime;

        if(GameController.instance.isGameOver == false && timeSinceLastSpawned >= spawnRate[currentLevel])
        {
            //Reset the timer
            timeSinceLastSpawned = 0.0f;
            spawnCounter += 1;

            //Check if the spawn counter is 
            if (spawnCounter >= powerUpSpawnCount)
            {
                //Create the game object
                Instantiate(powerUpPrefabs[0], pickUpStartingPosition, Quaternion.identity, spawnContainer.transform);
                spawnCounter = 0;
                return;
            }

            int random = UnityEngine.Random.Range(0, currentLevel + 1);

            //Create the game object
            Instantiate(obstaclePrefabs[random], startingPosition[random], Quaternion.identity, spawnContainer.transform);
        }
    }
}
