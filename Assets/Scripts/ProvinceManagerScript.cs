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
            if (unit.GetComponent<ProvinceManagerScript>() == null)
                Destroy(unit.gameObject);
        }
        controlledUnits.Clear();
        money = 0;
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
    //merges with another friendly province
    public void MergeProvince(ProvinceManagerScript otherProvince)
    {
        ProvinceManagerScript winningProvince, losingProvince;
        if (otherProvince.controlledTiles.Count > this.controlledTiles.Count) //the larger province wins the merger
        {
            winningProvince = otherProvince;
            losingProvince = this;
        }
        else
        {
            
            winningProvince = this;
            losingProvince = otherProvince;
        }
        foreach (var tile in losingProvince.controlledTiles)
            tile.owner = winningProvince;
        foreach (var unit in losingProvince.controlledUnits)
            unit.owner = winningProvince;
        winningProvince.money += losingProvince.money;
        Destroy(losingProvince.gameObject);
    }
    //the province has been split, create a new province
    public void SplitProvince(IEnumerable<Vector3Int> newTerritory)
    {
        List<Vector3Int> newTerritoryList = new List<Vector3Int>(newTerritory);
        //pick a random location to place the capital
        Vector3Int chosenPosition; int counter = 50;
        do
        {
            chosenPosition = newTerritoryList[Random.Range(0, newTerritoryList.Count)];
            if (GridManager.GetUnitAtGridPoint(chosenPosition) == null) 
                counter = 0;
            else 
                counter--;
        }
        while (counter < 0);
        if (GridManager.GetUnitAtGridPoint(chosenPosition) != null) Destroy(GridManager.GetUnitAtGridPoint(chosenPosition));
        ProvinceManagerScript newCapital = Instantiate(GameManager.Main.capital, GridManager.GetWorldPosition(chosenPosition), Quaternion.identity).GetComponent<ProvinceManagerScript>();
        newCapital.team = team;
        foreach (Vector3Int position in newTerritoryList)
        {
            TileScript tile = GridManager.GetHexAtGridPoint(position);
            if (tile == null || tile.owner != this) print("Invalid tile in split!");
            tile.ChangeTeam(newCapital.team, newCapital);
            UnitScript unit = GridManager.GetUnitAtGridPoint(position);
            if (unit != null) unit.SetOwner();
        }
    }
}
