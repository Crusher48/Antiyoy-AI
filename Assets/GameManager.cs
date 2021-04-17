using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Main;
    public int activeTeam = 0;
    public List<ProvinceManagerScript> activeProvinces;
    public int MAX_TEAMS = 12;
    private void Awake()
    {
        Main = this;
        //StartTurn(); (started by the world generator instead
    }
    //starts the next turn
    public void EndTurn()
    {
        //get the next team
        int nextTeam = activeTeam;
        //look for active provinces
        List<ProvinceManagerScript> allProvinces = new List<ProvinceManagerScript>(GameObject.FindObjectsOfType<ProvinceManagerScript>());
        activeProvinces = new List<ProvinceManagerScript>();
        int breaker = 0;
        do
        {
            breaker++;
            nextTeam++; if (nextTeam > MAX_TEAMS) nextTeam = 1;
            foreach (ProvinceManagerScript province in allProvinces)
            {
                if (province.team == nextTeam)
                    activeProvinces.Add(province);
            }
        } while (activeProvinces.Count == 0 && breaker < 100);
        activeTeam = nextTeam;
        //upkeep and initialize province turn
        foreach (ProvinceManagerScript province in activeProvinces)
        {
            province.StartProvinceTurn();
        }
        //TODO: if province is ran by an AI, run the AI, else let the player play it
    }
}
