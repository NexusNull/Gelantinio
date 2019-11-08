using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMovement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    [SerializeField] public float speed = default;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("a"))
        {
            transform.position = transform.position + new Vector3(-1f * speed,0,0);
        }
        if (Input.GetKey("d"))
        {
            transform.position = transform.position + new Vector3(1f * speed, 0, 0);
        }
        if (Input.GetKey("w"))
        {
            transform.position = transform.position + new Vector3(0, 1f * speed, 0);
        }
        if (Input.GetKey("s"))
        { 
            transform.position = transform.position + new Vector3(0, -1f * speed, 0);
        }
    }
}
