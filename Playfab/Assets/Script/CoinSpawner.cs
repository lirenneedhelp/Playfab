using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [SerializeField] GameObject CoinPrefab;
    public float minSpawnInterval = 20f;
    public float maxSpawnInterval = 40f;
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("SpawnCoin", Random.Range(minSpawnInterval, maxSpawnInterval), Random.Range(minSpawnInterval, maxSpawnInterval));
    }

    private void SpawnCoin()
    {
        // Check if the current instance is the master client before spawning
        if (PhotonNetwork.IsMasterClient)
        {
            // Spawn the coin using PhotonNetwork
            PhotonNetwork.Instantiate(CoinPrefab.name, transform.position, Quaternion.identity);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
