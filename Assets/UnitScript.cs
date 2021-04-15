using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitScript : MonoBehaviour
{
    public int upkeep = 0; //how much the unit costs (or gives) per turn
    public int powerLevel = 0; //the unit's power level, the attacker must have a higher power level to capture a unit or hex
    public bool mobile = false; //whether the unit can move or is a building
    //public int team = 0; //units always have the same team as the tile directly underneath them
    public int GetTeam()
    {
        return GridManager.GetHexAtGridPoint(GridManager.GetGridPosition(transform.position)).team;
    }
}
