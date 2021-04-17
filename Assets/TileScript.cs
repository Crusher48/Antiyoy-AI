using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{
    public ProvinceManagerScript owner;
    public int team = 0;
    public static Color[] TeamColors = {
        new Color(0.5f, 0.5f, 0.5f),
        new Color(0.25f, 0.25f, 1f),
        new Color(1f, 0.25f, 0.25f),
        new Color(0.25f, 1f, 0.25f),
        new Color(0.8f,0.8f,0.25f),
        new Color(0.8f,0.25f,0.8f),
        new Color(1f,0.5f,0.1f)};
    // Start is called before the first frame update
    void Start()
    {
        transform.parent = GameObject.Find("Grid").transform;
    }

    public void ChangeTeam(int team, ProvinceManagerScript owner)
    {
        //change team
        GetComponent<SpriteRenderer>().color = TeamColors[team];
        this.team = team;
        //change owner
        if (this.owner != null) this.owner.controlledTiles.Remove(this);
        this.owner = owner;
        this.owner.controlledTiles.Add(this);
    }    
}
