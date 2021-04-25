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
    private void Start()
    {
        SetOwner();
    }
    private void OnDestroy()
    {
        if (owner != null)
            owner.controlledUnits.Remove(this);
    }
    public int GetTeam()
    {
        TileScript currentTile = GridManager.GetHexAtGridPoint(GridManager.GetGridPosition(transform.position));
        return currentTile.team;
    }
    public void SetOwner()
    {
        TileScript currentTile = GridManager.GetHexAtGridPoint(GridManager.GetGridPosition(transform.position));
        owner = currentTile.owner;
        if (owner != null)
            owner.controlledUnits.Add(this);
    }
    public void MoveUnit(Vector3Int targetPosition)
    {
        if (!(mobile || canMove)) return; //we can't move if we can't move
        int currentTeam = GetTeam();
        TileScript targetTile = GridManager.GetHexAtGridPoint(targetPosition);
        UnitScript targetUnit = GridManager.GetUnitAtGridPoint(targetPosition);
        //check conditions
        if (targetTile != null)
        {
            if (targetTile.owner == owner) //if moving to a friendly tile
            {
                if (targetUnit != null)
                {
                    if (!targetUnit.mobile) return; //can't move on top of a building, but can move through it
                    //merge move handling
                    GameObject mergedUnit = GameManager.Main.GetUnit(this.powerLevel + targetUnit.powerLevel);
                    if (mergedUnit == null) return; //the resulting unit would be too powerful
                    Instantiate(mergedUnit, targetUnit.transform.position, Quaternion.identity);
                    mergedUnit.GetComponent<UnitScript>().mobile = targetUnit.mobile; //merged unit can move if the unit being merged into could move
                    Destroy(gameObject);
                    Destroy(targetUnit.gameObject);
                }
            }
            else
            {
                //look for defenders
                foreach (var adjacentPosition in GridManager.GetAllGridPointsInRange(targetPosition, 1))
                {
                    UnitScript possibleDefender = GridManager.GetUnitAtGridPoint(adjacentPosition);
                    //move fails if a defender exists, is on the same team as the target tile, and it has power level equal to or greater than our own
                    if (possibleDefender != null && possibleDefender.GetTeam() == targetTile.team && powerLevel <= possibleDefender.powerLevel) return;
                }
                if (targetUnit != null) Destroy(targetUnit.gameObject); //destroy the unit we attacked
                transform.position = GridManager.GetWorldPosition(targetPosition);
                targetTile.ChangeTeam(currentTeam, owner);
            }
        }
        canMove = false; //we've moved, now we can't move anymore
    }
    public HashSet<Vector3Int> GetAllValidMovePositions()
    {
        if (!mobile) return new HashSet<Vector3Int>();
        //A*-like, we have explored positions and frontier positions
        HashSet<Vector3Int> exploredPositions = new HashSet<Vector3Int>(); //fully exploerd positions
        HashSet<Vector3Int> frontierPositions = new HashSet<Vector3Int>(); //positions on the frontier that we can still move from
        frontierPositions.Add(GridManager.GetGridPosition(transform.position));
        HashSet<Vector3Int> newFrontierPositions = new HashSet<Vector3Int>(); //positions that will be added to the frontier at the end of the cycle
        for (int stage = 1; stage <= MOVE_RANGE; stage++)
        {
            //iterate through each node in frontier positions, getting the adjacent points to it and adding it to the list
            foreach (Vector3Int frontierPoint in frontierPositions)
            {
                foreach (Vector3Int point in GridManager.GetAllGridPointsInRange(frontierPoint, 1))
                {
                    //if the point isn't in explored positions or frontier positions
                    if (!(exploredPositions.Contains(point) || frontierPositions.Contains(point)))
                    {
                        TileScript hex = GridManager.GetHexAtGridPoint(point);
                        UnitScript unit = GridManager.GetUnitAtGridPoint(point);
                        if (hex != null)
                        {
                            if (hex.team == GetTeam()) //if friendly, add to the next frontier
                            {
                                //if (unit == null || unit.mobile) //make sure we wouldn't move on top of a unit
                                newFrontierPositions.Add(point);
                            }
                            else //hostile hex, add to the explored nodes
                            {
                                exploredPositions.Add(point);
                            }
                        }
                    }
                }
            }
            //move the frontier into explored and the new frontier into the frontier
            exploredPositions.UnionWith(frontierPositions);
            frontierPositions = new HashSet<Vector3Int>(newFrontierPositions);
            newFrontierPositions.Clear();
        }
        //move remaining frontier nodes into explored
        exploredPositions.UnionWith(frontierPositions);
        return exploredPositions;
    }
}
