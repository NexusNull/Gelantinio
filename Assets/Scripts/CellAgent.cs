using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class CellAgent : Agent
{
    Camera playerCamera;

    // Start is called before the first frame update
    void Start()
    {
        playerCamera = this.gameObject.GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void AgentAction(float[] vectorAction, string textAction) {

        // If no brain exists the player may control the PlayerCell, otherwise the brains has the control
        if (this.brain.name != "CellPlayerBrain") {

            // has brain -> brain controls
            Vector3 controlSignal = new Vector3(vectorAction[0],
                                                vectorAction[1],
                                                0f);
            if (controlSignal.magnitude > 1) controlSignal.Normalize();
            transform.position = transform.position + controlSignal;
        } else {

            // has no brain -> player controls via mouse
            Vector3 controlSignal = Vector3.zero;
            Vector3 mousePosition = playerCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;
            Debug.Log(mousePosition);
        }
    }
}
