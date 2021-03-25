using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public GameObject hexPrefab;
    // Start is called before the first frame update
    void Start()
    {
        //for now, just generate a big blob of hexes
        foreach (Vector3 point in GridManager.Main.GetAllPointsInRangeOfTarget(Vector3.zero, 10))
        {
            GameObject newHex = Instantiate(hexPrefab,point,Quaternion.identity);
            var gridLocation = GridManager.Main.GetGridPosition(point);
        }
        //handle spawn positions
        List<Vector3Int> spawnPositions = new List<Vector3Int> { new Vector3Int(-10,0,0), new Vector3Int(10,0,0),
                new Vector3Int(-5,-10,0), new Vector3Int(-5,10,0), new Vector3Int(5,-10,0), new Vector3Int(5,10,0) };
        int teamToSpawn = 1;
        foreach (Vector3Int position in spawnPositions)
        {
            List<GameObject> hexes = GridManager.Main.GetObjectsAtGridPoint(position);
            foreach (GameObject hex in hexes)
            {
                TileScript tileComponent = hex.GetComponent<TileScript>();
                if (tileComponent != null)
                {
                    //change the team of the start tile
                    tileComponent.ChangeTeam(teamToSpawn);
                    teamToSpawn++;
                    //TODO: Add capital building
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
