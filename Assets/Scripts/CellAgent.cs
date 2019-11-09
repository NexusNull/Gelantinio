using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class CellAgent : Agent
{
    Camera PlayerCamera;
	public float radius;
    public float speed;
    private Rigidbody2D rBody;

    // Start is called before the first frame update
    void Start()
    {
        PlayerCamera = this.gameObject.GetComponentInChildren<Camera>();
        rBody = this.gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //Sets Cell's size, speed and camera size according to it's radius
        transform.localScale = new Vector3(radius * 2, radius * 2, 1);
        speed = 3 / Mathf.Pow(radius, 0.5f);
        PlayerCamera.orthographicSize = 10 * radius;

        // has no brain -> player controls via mouse
        if (!this.brain)
        {

            Vector3 controlSignal = Vector3.zero;
            Vector3 mousePosition = PlayerCamera.ScreenToWorldPoint(Input.mousePosition) - this.gameObject.transform.position;

            mousePosition.z = 0f;
            if (mousePosition.magnitude > 1)
                mousePosition.Normalize();
            //Debug.Log(mousePosition);
            rBody.velocity = mousePosition * speed;
            //transform.position = transform.position + mousePosition * speed;
        }
    }

    public override void AgentAction(float[] vectorAction, string textAction) {
				
        // If no brain exists the player may control the PlayerCell, otherwise the brain has the control
        if (this.brain.name == "CellPlayerBrain") {
            
            // has brain -> brain controls
            Vector2 controlSignal = new Vector3(vectorAction[0],
                                                vectorAction[1]);

            if (controlSignal.magnitude > 1)
                controlSignal.Normalize();

            rBody.AddForce(controlSignal);
            // transform.position = transform.position + controlSignal * speed;
         
        }

    }
}
