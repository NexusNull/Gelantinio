using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class CellAgent : Agent
{
	public float startRadius;
    public float radius;
    Camera PlayerCamera;
    public float maxSpeed;
    public float minSpeed;
    private float speed;
    private Rigidbody2D rBody;
    public float growthSpeed;
	private GameObject map;
    private MapManager mapManager;

    // Start is called before the first frame update
    void Start() {
        PlayerCamera = this.gameObject.GetComponentInChildren<Camera>();
        rBody = this.gameObject.GetComponent<Rigidbody2D>();
        rBody.freezeRotation = true;
        radius = startRadius;
		mapManager = GameObject.Find("Map").GetComponent<MapManager>();
    }

    // Update is called once per frame
    void Update() {
        //Sets Cell's size, speed and camera size according to it's radius
        transform.localScale = new Vector3(radius * 2, radius * 2, 1f);

        if(Mathf.Abs(radius - startRadius) >= 0.01f) {
            // This expression is maxSpeed at startRadius and converges against maxSpeed - minSpeed 
            // (need to be fixed, so it does converge agains minSpeed) with growing radius
            speed = -(minSpeed * Mathf.Pow(radius - startRadius, 2f) / (Mathf.Pow(radius - startRadius, 2f) + 16*(radius - startRadius)) ) + maxSpeed;
        } else {
            // If radius == startRadius the expression is not defined, so we need to set it to maxSpeed manually then
            speed = maxSpeed;
        }

        PlayerCamera.orthographicSize = 10 * radius;

        // has no brain -> player controls via mouse
        if (!this.brain) {

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
	
	//Input Vector for the ml-agents neural network
	public override void CollectObservations(){
		AddVectorObs(radius);
		Vector3 food = findClosestFood();
		AddVectorObs(new Vector2(food.x - transform.position.x, food.y - transform.position.y));
		RaycastHit Right;
		RaycastHit ForwardRight;
		RaycastHit Forward;
		RaycastHit ForwardLeft;
		RaycastHit Left;
		RaycastHit BackwardLeft;
		RaycastHit Backward;
		RaycastHit BackwardRight;
		Physics.Raycast(transform.position, new Vector3(1,0,0), out Right);
		Physics.Raycast(transform.position, new Vector3(1,1,0), out ForwardRight);
		Physics.Raycast(transform.position, new Vector3(0,1,0), out Forward);
		Physics.Raycast(transform.position, new Vector3(-1,1,0), out ForwardLeft);
		Physics.Raycast(transform.position, new Vector3(-1,0,0), out Left);
		Physics.Raycast(transform.position, new Vector3(-1,-1,0), out BackwardLeft);
		Physics.Raycast(transform.position, new Vector3(0,-1,0), out Backward);
		Physics.Raycast(transform.position, new Vector3(1,-1,0), out BackwardRight);
		AddVectorObs(Right.distance);
		AddVectorObs(ForwardRight.distance);
		AddVectorObs(Forward.distance);
		AddVectorObs(ForwardLeft.distance);
		AddVectorObs(Left.distance);
		AddVectorObs(BackwardLeft.distance);
		AddVectorObs(Backward.distance);
		AddVectorObs(BackwardRight.distance);
	}
	
	//Returns position of the closest 
	Vector3 findClosestFood() {
		GameObject[] allFood = GameObject.FindGameObjectsWithTag("Food");
		float smallestDistance = Mathf.Infinity;
		Vector3 closestPosition = Vector3.zero;
		foreach (GameObject food in allFood){
			float distance = Vector3.Distance(food.transform.position, transform.position);
			if(distance < smallestDistance){
				smallestDistance = distance;
				closestPosition = food.transform.position;
			}
		}
		return closestPosition;
	}
	
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "Food") {
            grow(growthSpeed);
            int x = mapManager.xSize;
            int y = mapManager.ySize;
            collision.gameObject.GetComponent<Transform>().position = new Vector2(Random.Range(-(float)x / 2 + 1, (float)x / 2 - 1), Random.Range(-(float)y / 2 + 1, (float)y / 2 - 1));
        }
            
    }

    // Work in progress, to be extended when adding other cells
    public void swallow(/*smaller cell*/) {
		float otherRadius = 0f; // Radius of the swallowed cell or growthSpeed if swallowing food.
		grow(otherRadius);
	}
	
	// Grows so, that the total volume stays the same
	void grow(float mass) {
		radius = Mathf.Sqrt(mass*mass + radius*radius);
	}
}
