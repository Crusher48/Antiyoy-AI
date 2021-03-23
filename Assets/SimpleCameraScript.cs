using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCameraScript : MonoBehaviour
{
    public float moveSpeed = 5;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        transform.position += (Vector3)moveDirection * moveSpeed * Time.deltaTime;
    }
}
