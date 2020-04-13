using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PrizeSpawner : NetworkBehaviour {

    float   lastSpawnTime,
            spawnCooldown;

    public GameObject pizzaPrefab;
    public Transform[] spawnPoints;

	// Use this for initialization
	void Start ()
    {
        lastSpawnTime = Time.realtimeSinceStartup;
        spawnCooldown = Random.Range(3, 10);
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (!isServer)
            return;

		if (Time.realtimeSinceStartup - lastSpawnTime > spawnCooldown)
        {
            lastSpawnTime = Time.realtimeSinceStartup;
            spawnCooldown = Random.Range(3, 10);

            Transform point = GetFreeSpwnPoint(0);

            if(point != null)
            {
                GameObject newPrize = GameObject.Instantiate(pizzaPrefab, point);
                newPrize.transform.localPosition = Vector2.zero;
                NetworkServer.Spawn(newPrize);
            }
        }
	}

    Transform GetFreeSpwnPoint(int startVal)
    {
        if(startVal < 0 || startVal > spawnPoints.Length)
            return null;

        Transform point = spawnPoints[Random.Range(startVal, spawnPoints.Length)];

        if (point.childCount == 0)
            return point;
        else
            return GetFreeSpwnPoint(++startVal);


    }
}
