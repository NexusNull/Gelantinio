using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class CellAgent : Agent
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void AgentAction(float[] vectorAction, string textAction) {

        Vector3 controlSignal = new Vector3(vectorAction[0],
                                            vectorAction[1],
                                            0f);
        if (controlSignal.magnitude > 1) controlSignal.Normalize();
        transform.position = transform.position + controlSignal;
    }
}
