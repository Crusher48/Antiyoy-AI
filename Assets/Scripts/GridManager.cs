using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static Grid grid;
    private void Awake()
    {
        grid = GetComponent<Grid>();
    }
    //snaps the target to the center of a grid coordinate and returns it
    public static Vector3Int GetGridPosition(Vector3 worldPosition)
    {
        return grid.WorldToCell(worldPosition);
    }
    //convert to cubic coordinates (easier to use for operations)
    public static Vector3Int CellToCubicPosition(Vector3Int cellPosition)
    {
        Vector3Int cubicPosition = Vector3Int.zero;
        cubicPosition.x = cellPosition.x - (cellPosition.y - (cellPosition.y & 1)) / 2;
        cubicPosition.z = cellPosition.y;
        cubicPosition.y = -cubicPosition.x - cubicPosition.z;
        return cubicPosition;
    }
    //convert back to axial coordinates
    public static Vector3Int CubicToCellPosition(Vector3Int cubicPosition)
    {
        Vector3Int cellPosition = Vector3Int.zero;
        cellPosition.x = cubicPosition.x + (cubicPosition.z - (cubicPosition.z & 1)) / 2;
        cellPosition.y = cubicPosition.z;
        return cellPosition;
    }
    //snaps the target to the center of a grid coordinate and returns it
    public static Vector3 GetWorldPosition(Vector3Int cellPosition)
    {
        return grid.CellToWorld(cellPosition);
    }
    //snaps a world position to the nearest grid point
    public static Vector3 SnapToGrid(Vector3 worldPosition)
    {
        return grid.CellToWorld(grid.WorldToCell(worldPosition));
    }
    //gets all grid points within the given grid range of the target
    public static List<Vector3Int> GetAllGridPointsInRange(Vector3Int centerPosition, int range)
    {
        List<Vector3Int> points = new List<Vector3Int>();
        //hexagon circle formula, does y first then x
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                Vector3Int potentialPoint = centerPosition + new Vector3Int(x, y, 0);
                if (AreGridPointsInRange(centerPosition, potentialPoint, range))
                    points.Add(potentialPoint);
            }
        }
        return points;
    }
    //gets all world points within the given grid range of the target
    public static List<Vector3> GetAllWorldPointsInRange(Vector3 position, int range)
    {
        List<Vector3> points = new List<Vector3>();
        Vector3Int centerPosition = grid.WorldToCell(position);
        //hexagon circle formula, does y first then x
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                Vector3Int potentialPoint = centerPosition + new Vector3Int(x, y, 0);
                if (AreGridPointsInRange(centerPosition, potentialPoint, range))
                    points.Add(grid.CellToWorld(potentialPoint));
            }
        }
        return points;
    }
    //determine if the two grid points are in range
    public static bool AreGridPointsInRange(Vector3Int point1, Vector3Int point2, int range)
    {
        Vector3Int point1Cubic = CellToCubicPosition(point1);
        Vector3Int point2Cubic = CellToCubicPosition(point2);
        if (Mathf.Max(Math.Abs(point1Cubic.x - point2Cubic.x), Math.Abs(point1Cubic.y - point2Cubic.y), Math.Abs(point1Cubic.z - point2Cubic.z)) <= range)
            return true;
        /*
        int xOffset = point1.x - point2.x;
        int yOffset = point1.y - point2.y;
        if (Math.Abs(yOffset) <= range)
        {
            if (xOffset >= -range + (Math.Abs(yOffset) / 2) && xOffset <= range - ((Math.Abs(yOffset) + 1) / 2))
                return true;
        }
        */
        return false;
    }
    //gets the hex for a grid point, returns null if there's no hex
    public static TileScript GetHexAtGridPoint(Vector3Int point)
    {
        //print(point);
        //print(GetWorldPosition(point));
        //print((Vector2)(GetWorldPosition(point)));
        Collider2D[] hits = Physics2D.OverlapPointAll((Vector2)(GetWorldPosition(point)), Physics2D.AllLayers);
        foreach (Collider2D hit in hits)
        {
            TileScript tileScript = hit.gameObject.GetComponent<TileScript>();
            if (tileScript != null)
                return tileScript;
        }
        return null;
    }
    //gets the unit on a grid point
    public static UnitScript GetUnitAtGridPoint(Vector3Int point)
    {
        //print(point);
        //print(GetWorldPosition(point));
        //print((Vector2)(GetWorldPosition(point)));
        Collider2D[] hits = Physics2D.OverlapPointAll((Vector2)(GetWorldPosition(point)),Physics2D.AllLayers);
        foreach (Collider2D hit in hits)
        {
            UnitScript unitScript = hit.gameObject.GetComponent<UnitScript>();
            if (unitScript != null)
                return unitScript;
        }
        return null;
    }
    //gets all hexes of the same team connected to the hex
    public static HashSet<Vector3Int> GetAllConnectedTiles(Vector3Int gridPosition, int maxRange, bool extendIntoOpposition = true)
    {
        TileScript startTile = GetHexAtGridPoint(gridPosition);
        if (startTile == null) return null;
        //A*-like, we have explored positions and frontier positions
        HashSet<Vector3Int> exploredPositions = new HashSet<Vector3Int>(); //fully exploerd positions
        HashSet<Vector3Int> frontierPositions = new HashSet<Vector3Int>(); //positions on the frontier that we can still move from
        frontierPositions.Add(gridPosition);
        HashSet<Vector3Int> newFrontierPositions = new HashSet<Vector3Int>(); //positions that will be added to the frontier at the end of the cycle
        for (int stage = 1; stage <= maxRange; stage++)
        {
            if (frontierPositions.Count == 0) break;
            //iterate through each node in frontier positions, getting the adjacent points to it and adding it to the list
            foreach (Vector3Int frontierPoint in frontierPositions)
            {
                foreach (Vector3Int point in GetAllGridPointsInRange(frontierPoint, 1))
                {
                    //if the point isn't in explored positions or frontier positions
                    if (!(exploredPositions.Contains(point) || frontierPositions.Contains(point)))
                    {
                        TileScript hex = GetHexAtGridPoint(point);
                        UnitScript unit = GetUnitAtGridPoint(point);
                        if (hex != null)
                        {
                            if (hex.team == startTile.team) //if friendly, add to the next frontier
                            {
                                //if (unit == null || unit.mobile) //make sure we wouldn't move on top of a unit
                                newFrontierPositions.Add(point);
                            }
                            else if (extendIntoOpposition)//hostile hex, add to the explored nodes
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
