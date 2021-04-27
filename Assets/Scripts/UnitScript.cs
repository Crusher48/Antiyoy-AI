using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitScript : MonoBehaviour
{
    public ProvinceManagerScript owner;
    public int buildCost = 0; //how much the unit costs to build
    public int upkeep = 0; //how much the unit costs (or gives) per turn
    public int powerLevel = 0; //the unit's power level, the attacker must have a higher power level to capture a unit or hex
    public bool mobile = true; //whether the unit can move or is a building
    public bool canMove = true; //whether the unit is ready to move
    public int MOVE_RANGE = 4; //the movement range of units
    //public int team = 0; //units always have the same team as the tile directly underneath them
    private void Awake()
    {
        Vector3Int gridPosition = GridManager.GetGridPosition(transform.position);
        if (!GridManager.unitPool.ContainsKey(gridPosition))
            GridManager.unitPool.Add(gridPosition, this);
        else
            GridManager.unitPool[gridPosition] = this;
        SetOwner();
    }
    public void DestroyUnit()
    {
        //if (GridManager.grid == null) return; //prevent errors on scene change
        if (owner != null)
            owner.controlledUnits.Remove(this);
        if (GetComponent<ProvinceManagerScript>() != null)
            GetComponent<ProvinceManagerScript>().SackProvince();
        GridManager.unitPool[GridManager.GetGridPosition(transform.position)] = null;
        Destroy(this.gameObject);
    }
    public int GetTeam()
    {
        TileScript currentTile = GridManager.GetHexAtGridPoint(GridManager.GetGridPosition(transform.position));
        return currentTile.team;
    }
    public void SetOwner()
    {
        if (owner != null)
            owner.controlledUnits.Remove(this);
        TileScript currentTile = GridManager.GetHexAtGridPoint(GridManager.GetGridPosition(transform.position));
        if (currentTile == null)
        {
            Debug.LogError("Spawning in the void!");
            print(transform.position);
        }
        owner = currentTile.owner;
        if (owner != null)
            owner.controlledUnits.Add(this);
    }
    //attempts to move the unit, returns false if the unit failed to move
    public bool MoveUnit(Vector3Int targetPosition)
    {
        Vector3Int oldPosition = GridManager.GetGridPosition(transform.position);
        if (!(mobile && canMove)) return false; //we can't move if we can't move
        if (targetPosition == GridManager.GetGridPosition(transform.position)) return false; //no, you cannot move into yourself to upgrade
        int currentTeam = GetTeam();
        TileScript targetTile = GridManager.GetHexAtGridPoint(targetPosition);
        UnitScript targetUnit = GridManager.GetUnitAtGridPoint(targetPosition);
        //check conditions
        if (targetTile != null)
        {
            if (targetTile.team == currentTeam) //if moving to a friendly tile
            {
                if (targetUnit != null)
                {
                    if (!targetUnit.mobile) return false; //can't move on top of a building, but can move through it
                    //merge move handling
                    GameObject mergedUnit = GameManager.Main.GetUnit(this.powerLevel + targetUnit.powerLevel);
                    if (mergedUnit == null) return false; //the resulting unit would be too powerful
                    bool targetCanMove = targetUnit.canMove;
                    //destroy the old units first
                    DestroyUnit();
                    targetUnit.DestroyUnit();
                    //create the new merged unit
                    GameObject newUnit = Instantiate(mergedUnit, GridManager.GetWorldPosition(targetPosition), Quaternion.identity);
                    GridManager.unitPool[targetPosition] = newUnit.GetComponent<UnitScript>();
                    newUnit.GetComponent<UnitScript>().canMove = targetCanMove; //merged unit can move if the unit being merged into could move
                    return true;
                }
                else
                {
                    GridManager.unitPool[oldPosition] = null;
                    transform.position = GridManager.GetWorldPosition(targetPosition);
                    if (!GridManager.unitPool.ContainsKey(targetPosition))
                        GridManager.unitPool.Add(targetPosition, this);
                    else
                        GridManager.unitPool[targetPosition] = this;
                }
            }
            else
            {
                List<Vector3Int> adjacentPositions = GridManager.GetAllGridPointsInRange(targetPosition, 1);
                //look for defenders
                foreach (var adjacentPosition in adjacentPositions)
                {
                    UnitScript possibleDefender = GridManager.GetUnitAtGridPoint(adjacentPosition);
                    //move fails if a defender exists, is on the same team as the target tile, and it has power level equal to or greater than our own
                    if (possibleDefender != null && possibleDefender.GetTeam() == targetTile.team && powerLevel <= possibleDefender.powerLevel) return false;
                }
                //destroy the unit we attacked
                if (targetUnit != null)
                {
                    targetUnit.DestroyUnit();
                }
                GridManager.unitPool[oldPosition] = null;
                targetTile.ChangeTeam(currentTeam, owner);
                transform.position = GridManager.GetWorldPosition(targetPosition);
                if (!GridManager.unitPool.ContainsKey(targetPosition))
                    GridManager.unitPool.Add(targetPosition, this);
                else
                    GridManager.unitPool[targetPosition] = this;
                //split and merge functionality
                foreach (var adjacentPosition in adjacentPositions)
                {
                    TileScript adjacentTile = GridManager.GetHexAtGridPoint(adjacentPosition);
                    if (adjacentTile == null) continue;
                    if (adjacentTile.team == currentTeam)
                    {
                        if (adjacentTile.owner != owner) //if the other tile is not our tile
                        {
                            owner.MergeProvince(adjacentTile.owner);
                        }
                    }
                    else
                    {
                        var connectedPositions = GridManager.GetAllConnectedTiles(GridManager.GetGridPosition(adjacentTile.transform.position), 99, false);
                        if (adjacentTile.owner != null && !connectedPositions.Contains(GridManager.GetGridPosition(adjacentTile.owner.transform.position)))
                        {
                            adjacentTile.owner.SplitProvince(connectedPositions);
                        }
                    }
                }
            }
        }
        canMove = false; //we've moved, now we can't move anymore
        return true;
    }
    public HashSet<Vector3Int> GetAllValidMovePositions()
    {
        if (!mobile) return new HashSet<Vector3Int>();
        return GridManager.GetAllConnectedTiles(GridManager.GetGridPosition(transform.position), 4);
    }
}
