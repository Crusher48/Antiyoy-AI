using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProvinceManagerScript : MonoBehaviour
{
    public int team = 0;
    public List<TileScript> controlledTiles;
    public List<UnitScript> controlledUnits;
    public int money = 0;
    //called to initialize the province
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
    public void DestroyProvince()
    {

    }
    //handles the money for a turn
    public void HandleMoney()
    {
        money += controlledTiles.Count; //+1 money per tile
        foreach (UnitScript unit in controlledUnits) //subtract upkeep for each unit
            money -= unit.upkeep;
        if (money <= 0)
            DestroyProvince();
    }
}
