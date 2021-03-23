using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public GameObject hexPrefab;
    // Start is called before the first frame update
    void Start()
    {
        foreach (Vector3 point in GridManager.Main.GetAllPointsInRangeOfTarget(Vector3.zero, 10))
        {
            GameObject newHex = Instantiate(hexPrefab);
            newHex.transform.position = point;
            var gridLocation = GridManager.Main.GetGridPosition(point);
            
            if ((Mathf.Abs(gridLocation.x) == 10 || Mathf.Abs(gridLocation.x) == 5) &&
                (Mathf.Abs(gridLocation.y) == 10 || Mathf.Abs(gridLocation.y) == 0))
            {
                newHex.GetComponent<TileScript>().ChangeTeam(1);
                print(gridLocation);
            }
            List<Vector3Int> spawnPositions = new List<Vector3Int> { new Vector3Int(-10,0,0), new Vector3Int(10,0,0), 
                new Vector3Int(-5,-10,0), new Vector3Int(-5,10,0), new Vector3Int(5,-10,0), new Vector3Int(5,10,0) };
            foreach (Vector3Int position in spawnPositions)
            {
                List<GameObject> hexes = GridManager.Main.GetObjectsAtGridPoint(position);
                foreach (GameObject hex in hexes)
                    hex.GetComponent<TileScript>().ChangeTeam(2);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
