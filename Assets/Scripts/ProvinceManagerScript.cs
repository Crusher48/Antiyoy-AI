using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProvinceManagerScript : MonoBehaviour
{
    public int team = 0;
    public HashSet<TileScript> controlledTiles;
    public HashSet<UnitScript> controlledUnits;
    public int money = 0;
    bool isDestroyed = false;
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
                UnitScript unit = GridManager.GetUnitAtGridPoint(position);
                if (hex != null)
                {
                    hex.ChangeTeam(team,this);
                    controlledTiles.Add(hex);
                }
                if (unit != null)
                {
                    unit.SetOwner();
                }
            }
        }

    }
    //province turn-start update
    public void StartProvinceTurn()
    {
        if (isDestroyed)
        {
            return;
        }
        foreach (var unit in controlledUnits)
        {
            if (unit != null && unit.GetTeam() != team)
            {
                Debug.LogWarning("Unit is an imposter!");
                print(unit);
                print(team);
                print(unit.transform.position);
                print(GridManager.GetUnitAtGridPoint(GridManager.GetGridPosition(unit.transform.position)));
                Destroy(unit.gameObject);
            }
        }
        GridManager.unitPool[GridManager.GetGridPosition(transform.position)] = GetComponent<UnitScript>();
        /*
        if (GridManager.GetHexAtGridPoint(GridManager.GetGridPosition(transform.position)).team != team)
        {
            Debug.LogError("Province Bug! " + transform.position);
            print(GridManager.GetUnitAtGridPoint(GridManager.GetGridPosition(transform.position)));
        }
        if (GridManager.GetUnitAtGridPoint(GridManager.GetGridPosition(transform.position)) != GetComponent<UnitScript>())
        {
            Debug.LogError("Invalid province unit assignment!" + transform.position);
            print(GridManager.GetUnitAtGridPoint(GridManager.GetGridPosition(transform.position)));
        }
        */
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
        var deletedUnits = new HashSet<UnitScript>(controlledUnits);
        foreach (UnitScript unit in deletedUnits)
        {
            if (unit != null && unit.mobile)
                unit.DestroyUnit();
        }
        controlledUnits.Clear();
        money = 0;
    }
    //creates and deploys a new unit
    public void SpawnUnit(GameObject unitPrefab,Vector3Int position)
    {
        UnitScript unitBlueprint = unitPrefab.GetComponent<UnitScript>();
        int buildCost = unitBlueprint.buildCost;
        if (unitBlueprint.upkeep < 0) //towns are more expensive the more of them you have
            buildCost += 2 * GetTownCount();
        if (money < buildCost) return; //we need enough money to actually build the unit
        //create the unit
        TileScript targetTile = GridManager.GetHexAtGridPoint(position);
        UnitScript targetUnit = GridManager.GetUnitAtGridPoint(position);
        //if tile is clear, just spawn there
        if (targetTile.owner == this && targetUnit == null)
        {
            GameObject newUnit = Instantiate(unitPrefab, GridManager.GetWorldPosition(position), Quaternion.identity);
            money -= buildCost;
        }
        else //initialize the unit on this province tile and then attempt to move it
        {
            GameObject newUnit = Instantiate(unitPrefab, this.transform.position, Quaternion.identity);
            if (newUnit.GetComponent<UnitScript>().MoveUnit(position))
                money -= buildCost;
            else
                newUnit.GetComponent<UnitScript>().DestroyUnit();
            GridManager.unitPool[GridManager.GetGridPosition(transform.position)] = GetComponent<UnitScript>(); //because the unit moved off of this tile, it potentially cleared the unit pool value, re-add that
        }
    }
    //gets the amount of mobile units of the given tier (or all if tier is left at 0
    public int GetUnitCount(int tier = 0)
    {
        int unitAmount = 0;
        foreach (UnitScript unit in controlledUnits)
        {
            if (unit.mobile && (tier == 0 || unit.powerLevel == tier))
                unitAmount++;
        }
        return unitAmount;
    }
    //gets the amount of towns already in this province
    public int GetTownCount()
    {
        int towns = 0;
        foreach (UnitScript unit in controlledUnits)
        {
            if (unit.upkeep < 0)
                towns++;
        }
        return towns;
    }
    //merges with another friendly province
    public void MergeProvince(ProvinceManagerScript otherProvince)
    {
        //print("Merging province!");
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
        if (winningProvince.team != losingProvince.team)
        {
            Debug.LogError("Coup attempted! " + winningProvince.transform.position + losingProvince.transform.position);
        }
        var switchingTiles = new HashSet<TileScript>(losingProvince.controlledTiles);
        foreach (var tile in switchingTiles)
            tile.ChangeTeam(winningProvince.team, winningProvince);
        foreach (var unit in losingProvince.controlledUnits)
            unit.owner = winningProvince;
        winningProvince.money += losingProvince.money;
        GameManager.Main.activeProvinces.Remove(losingProvince);
        if (losingProvince == null) print("Losing Province is Null!");
        else losingProvince.GetComponent<UnitScript>().DestroyUnit();
    }
    //the province has been split, create a new province
    public void SplitProvince(IEnumerable<Vector3Int> newTerritory)
    {
        //print("Splitting province!");
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
        if (GridManager.GetUnitAtGridPoint(chosenPosition) != null) GridManager.GetUnitAtGridPoint(chosenPosition).DestroyUnit(); //if forced to override a unit, destroy it
        ProvinceManagerScript newCapital = Instantiate(GameManager.Main.capital, GridManager.GetWorldPosition(chosenPosition), Quaternion.identity).GetComponent<ProvinceManagerScript>();
        newCapital.team = team;
        foreach (Vector3Int position in newTerritoryList)
        {
            TileScript tile = GridManager.GetHexAtGridPoint(position);
            if (tile == null || tile.owner != this)
            {
                Debug.LogError("Invalid tile in split! " + tile.transform.position);
                print("Current Owner: " + tile.owner.transform.position);
                print("Expected Owner: " + transform.position);
            }
            tile.ChangeTeam(newCapital.team, newCapital);
            UnitScript unit = GridManager.GetUnitAtGridPoint(position);
            if (unit != null) unit.SetOwner();
        }
    }
    public void SackProvince()
    {
        isDestroyed = true;
    }
}
