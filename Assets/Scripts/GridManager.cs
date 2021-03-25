using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Main;
    public Grid grid;
    private void Awake()
    {
        GridManager.Main = this;
        grid = GetComponent<Grid>();
    }
    //snaps the target to the center of a grid coordinate and returns it
    public Vector3Int GetGridPosition(Vector3 worldPosition)
    {
        return grid.WorldToCell(worldPosition);
    }
    //convert to cubic coordinates (easier to use for operations)
    public Vector3Int CellToCubicPosition(Vector3Int cellPosition)
    {
        Vector3Int cubicPosition = Vector3Int.zero;
        cubicPosition.x = cellPosition.x - (cellPosition.y - (cellPosition.y & 1)) / 2;
        cubicPosition.z = cellPosition.y;
        cubicPosition.y = -cubicPosition.x - cubicPosition.z;
        return cubicPosition;
    }
    //convert back to axial coordinates
    public Vector3Int CubicToCellPosition(Vector3Int cubicPosition)
    {
        Vector3Int cellPosition = Vector3Int.zero;
        cellPosition.x = cubicPosition.x + (cubicPosition.z - (cubicPosition.z & 1)) / 2;
        cellPosition.y = cubicPosition.z;
        return cellPosition;
    }
    //snaps the target to the center of a grid coordinate and returns it
    public Vector3 GetWorldPosition(Vector3Int cellPosition)
    {
        return grid.CellToWorld(cellPosition);
    }
    //snaps a world position to the nearest grid point
    public Vector3 SnapToGrid(Vector3 worldPosition)
    {
        return grid.CellToWorld(grid.WorldToCell(worldPosition));
    }
    //gets all grid points within the given range of the target
    public List<Vector3> GetAllPointsInRangeOfTarget(Vector3 position, int range)
    {
        List<Vector3> points = new List<Vector3>();
        Vector3Int centerPosition = grid.WorldToCell(position);
        //hexagon circle formula, does y first then x
        for (int x = -10; x <= 10; x++)
        {
            for (int y = -10; y <= 10; y++)
            {
                Vector3Int potentialPoint = centerPosition + new Vector3Int(x, y, 0);
                if (AreGridPointsInRange(centerPosition, potentialPoint, range))
                    points.Add(grid.CellToWorld(potentialPoint));
            }
        }
        /*
        for (int y = -range; y <= range; y++)
        {
            for (int x = -range+(Math.Abs(y)/2)+centerPosition.y%2; x <= range-((Math.Abs(y)+1)/2)+centerPosition.y%2; x++)
            {
                points.Add(grid.CellToWorld(centerPosition + new Vector3Int(x, y, 0)));
            }
        }
        */
        return points;
    }
    //determine if the two grid points are in range
    public bool AreGridPointsInRange(Vector3Int point1, Vector3Int point2, int range)
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
    //gets all objects on a point
    public List<GameObject> GetObjectsAtGridPoint(Vector3Int point)
    {
        List<GameObject> returnVals = new List<GameObject>();
        print(point);
        print(GetWorldPosition(point));
        print((Vector2)(GetWorldPosition(point)));
        Collider2D[] hits = Physics2D.OverlapPointAll((Vector2)(GetWorldPosition(point)),Physics2D.AllLayers);
        foreach (Collider2D hit in hits)
        {
            print("Collider Hit!");
            returnVals.Add(hit.gameObject);
        }
        return returnVals;
    }
}
