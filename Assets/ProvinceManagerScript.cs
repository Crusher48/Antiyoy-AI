using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProvinceManagerScript : MonoBehaviour
{
    public int team = 0;
    public HashSet<TileScript> controlledTiles;
    public HashSet<UnitScript> controlledUnits;
    public int money = 0;
    //called to initialize the province
    private void Awake()
    {
        controlledTiles = new HashSet<TileScript>();
        controlledUnits = new HashSet<UnitScript>();
    }
    public void InitializeProvince(int team, bool gameStart = false)
    {
        this.team = team;
        if (gameStart)
        {
            print(GridManager.GetGridPosition(transform.position));
            List<Vector3Int> nearbyPositions = GridManager.GetAllGridPointsInRange(GridManager.GetGridPosition(transform.position), 1);
            foreach (Vector3Int position in nearbyPositions)
            {
                TileScript hex = GridManager.GetHexAtGridPoint(position);
                if (hex != null)
                {
                    hex.ChangeTeam(team);
                    controlledTiles.Add(hex);
                }
            }
        }

    }
    //province turn-start update
    public void StartProvinceTurn()
    {
        //handle money
        money += controlledTiles.Count; //+1 money per tile
        foreach (UnitScript unit in controlledUnits) //subtract upkeep for each unit
            money -= unit.upkeep;
        if (money <= 0)
            DestroyProvince();
        //mobilize units
        foreach (UnitScript unit in controlledUnits)
            unit.canMove = true;
    }
    public void DestroyProvince()
    {

    }
}
