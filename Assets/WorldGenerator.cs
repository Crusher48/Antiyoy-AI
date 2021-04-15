using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public GameObject hexPrefab;
    public GameObject capitalPrefab;
    int worldGenStage = 1; //stage 1: generate hexes, stage 2: generate trees, stage 3: generate capitals, stage 4: done
    // Start is called before the first frame update
    void Start()
    {
        //for now, just generate a big blob of hexes
        foreach (Vector3 point in GridManager.GetAllWorldPointsInRangeOfTarget(Vector3.zero, 10))
        {
            GameObject newHex = Instantiate(hexPrefab,point,Quaternion.identity);
            var gridLocation = GridManager.GetGridPosition(point);
        }
        //handle spawn positions
        List<Vector3Int> spawnPositions = new List<Vector3Int> { new Vector3Int(-10,0,0), new Vector3Int(10,0,0),
                new Vector3Int(-5,-10,0), new Vector3Int(-5,10,0), new Vector3Int(5,-10,0), new Vector3Int(5,10,0) };
        int teamToSpawn = 1;
        foreach (Vector3Int position in spawnPositions)
        {
            TileScript hex = GridManager.GetHexAtGridPoint(position);
            if (hex != null)
            {
                hex.ChangeTeam(teamToSpawn);
                //spawn a capital
                GameObject newCapital = Instantiate(capitalPrefab, GridManager.GetWorldPosition(position), Quaternion.identity);
                //initialize the capital and have it also take nearby tiles
                newCapital.GetComponent<ProvinceManagerScript>().InitializeProvince(teamToSpawn, true);
                teamToSpawn++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
