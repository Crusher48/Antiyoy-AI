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
    public int GetTeam()
    {
        TileScript currentTile = GridManager.GetHexAtGridPoint(GridManager.GetGridPosition(transform.position));
        return currentTile.team;
    }
    public void SetOwner()
    {
        TileScript currentTile = GridManager.GetHexAtGridPoint(GridManager.GetGridPosition(transform.position));
        owner = currentTile.owner;
        owner.controlledUnits.Add(this);
    }
    public void MoveUnit(Vector3Int targetPosition)
    {
        if (!(mobile || canMove)) return; //we can't move if we can't move
        int currentTeam = GetTeam();
        TileScript targetTile = GridManager.GetHexAtGridPoint(targetPosition);
        if (targetTile != null)
        {
            transform.position = GridManager.GetWorldPosition(targetPosition);
            targetTile.ChangeTeam(currentTeam,owner);
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
                                if (unit == null) //make sure we wouldn't move on top of a unit
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
