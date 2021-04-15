using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Main;
    public int activeTeam = 0;
    int MAX_TEAMS = 12;
    //starts a turn
    void StartTurn(int team)
    {
        activeTeam = team;
        //get all provinces that are of the active team
        List<ProvinceManagerScript> allProvinces = new List<ProvinceManagerScript>(GameObject.FindObjectsOfType<ProvinceManagerScript>());
        List<ProvinceManagerScript> activeProvinces = new List<ProvinceManagerScript>();
        foreach (ProvinceManagerScript province in allProvinces)
        {
            if (province.team == activeTeam)
                activeProvinces.Add(province);
        }
        //upkeep and initialize province turn
        foreach (ProvinceManagerScript province in activeProvinces)
        {
            province.StartProvinceTurn();
        }
        //TODO: if province is ran by an AI, run the AI, else let the player play it
    }
    //ends the turn and starts the next one
    public void EndTurn()
    {

    }
}
