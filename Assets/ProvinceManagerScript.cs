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
    public int GetIncome()
    {
        int upkeep = controlledTiles.Count; //+1 money per tile
        foreach (UnitScript unit in controlledUnits) //subtract upkeep for each unit
            upkeep -= unit.upkeep;
        return upkeep;
    }
    //initialize the province
    public void InitializeProvince(int team, bool gameStart = false)
    {
        this.team = team;
        if (gameStart)
        {
            money = 10; //add starting money to the province
            print(GridManager.GetGridPosition(transform.position));
            List<Vector3Int> nearbyPositions = GridManager.GetAllGridPointsInRange(GridManager.GetGridPosition(transform.position), 1);
            foreach (Vector3Int position in nearbyPositions)
            {
                TileScript hex = GridManager.GetHexAtGridPoint(position);
                if (hex != null)
                {
                    hex.ChangeTeam(team,this);
                    controlledTiles.Add(hex);
                }
            }
        }

    }
    //province turn-start update
    public void StartProvinceTurn()
    {
        //handle money
        money += GetIncome();
        if (money <= 0)
            BankruptProvince();
        //mobilize units
        foreach (UnitScript unit in controlledUnits)
            unit.canMove = true;
    }
    //the province goes bankrupt and all the units quit
    public void BankruptProvince()
    {
        foreach (UnitScript unit in controlledUnits)
        {
            Destroy(unit.gameObject);
        }
        controlledUnits.Clear();
    }
    //creates and deploys a new unit
    public void CreateUnit(GameObject unitPrefab,Vector3Int position)
    {
        int buildCost = unitPrefab.GetComponent<UnitScript>().buildCost;
        if (money < buildCost) return; //we need enough money to actually build the unit
        //create the unit
        GameObject newUnit = Instantiate(unitPrefab, GridManager.GetWorldPosition(position), Quaternion.identity);
        money -= buildCost;
    }
}
