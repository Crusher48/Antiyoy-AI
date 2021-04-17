using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public GameObject hexPrefab;
    public GameObject capitalPrefab;
    List<Vector3Int> spawnPositions = new List<Vector3Int> { new Vector3Int(-10,0,0), new Vector3Int(-5,10,0),
                 new Vector3Int(5,10,0), new Vector3Int(10,0,0), new Vector3Int(5,-10,0), new Vector3Int(-5,-10,0), };
    public float holeChance = 0.25f;
    // Start is called before the first frame update
    void Start()
    {
        //for now, just generate a big blob of hexes
        foreach (Vector3Int point in GridManager.GetAllGridPointsInRange(Vector3Int.zero, 15))
        {
            if (Random.Range(0f, 1f) < holeChance && !spawnPositions.Contains(point)) continue;
            GameObject newHex = Instantiate(hexPrefab, GridManager.GetWorldPosition(point), Quaternion.identity);
            //var gridLocation = GridManager.GetGridPosition(point);
        }
        //handle spawn positions
        int teamToSpawn = 1;
        foreach (Vector3Int position in spawnPositions)
        {
            TileScript hex = GridManager.GetHexAtGridPoint(position);
            if (hex != null)
            {
                //spawn a capital
                GameObject newCapital = Instantiate(capitalPrefab, GridManager.GetWorldPosition(position), Quaternion.identity);
                //initialize the capital and have it also take nearby tiles
                newCapital.GetComponent<ProvinceManagerScript>().InitializeProvince(teamToSpawn, true);
                teamToSpawn++;
                if (teamToSpawn > GameManager.Main.MAX_TEAMS) teamToSpawn = 1;
            }
        }

        //when the world is generated, start the game
        GameManager.Main.EndTurn();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
