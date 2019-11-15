using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class CellAgent : Agent
{
	public float startRadius;
    public float radius;
    Camera PlayerCamera;
    public float startSpeed;
    public float convergeSpeed;
    private float speed;
    private Rigidbody2D rBody;
    public float growthSpeed;
	private GameObject map;
    private MapManager mapManager;
	int frames;

    // Start is called before the first frame update
    void Start() {
        PlayerCamera = this.gameObject.GetComponentInChildren<Camera>();
        rBody = this.gameObject.GetComponent<Rigidbody2D>();
        rBody.freezeRotation = true;
        radius = startRadius;
		mapManager = GameObject.Find("Map").GetComponent<MapManager>();
        speed = startSpeed;
        PlayerCamera.orthographicSize = 10 * radius;
		frames = 0;
    }

    // Update is called once per frame
    void Update() {

        Vector3 food = findClosestFood();
        //Debug.Log(new Vector2(food.x - transform.position.x, food.y - transform.position.y));

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
            Debug.Log(vectorAction);
            // has brain -> brain controls
            Vector2 controlSignal = new Vector3(vectorAction[0],
                                                vectorAction[1]);

            if (controlSignal.magnitude > 1)
                controlSignal.Normalize();

            rBody.velocity = controlSignal * speed;
         
        } else {
			//Vector3 controlSignal = new Vector3(Mathf.Sin(vectorAction[0]*2*Mathf.PI), Mathf.Cos(vectorAction[1]*2*Mathf.PI), 0);
            Vector2 controlSignal = new Vector3(vectorAction[0],
                                    vectorAction[1]);
            controlSignal.Normalize();
            rBody.velocity = controlSignal * speed;
		}
    }

    public override void AgentReset()
    {
        base.AgentReset();
        setRadius(startRadius);
        float x = Random.Range(-1*(float)mapManager.xSize/2 + 1, (float)mapManager.xSize/2 - 1);
		float y = Random.Range(-1*(float)mapManager.xSize/2 + 1, (float)mapManager.xSize/2 - 1);
        transform.position = new Vector3(x,y,0);
    }

    //Input Vector for the ml-agents neural network
    public override void CollectObservations(){
        AddVectorObs(radius);
        Vector3 food = findClosestFood();
		
        Vector2 foodPos = new Vector2(food.x - transform.position.x, food.y - transform.position.y);
        float distRight = Mathf.Clamp((mapManager.xSize/2)-transform.position.x,0,10);
        float distLeft = Mathf.Clamp((mapManager.xSize / 2) + transform.position.x, 0, 10);
        float distTop = Mathf.Clamp((mapManager.ySize / 2) - transform.position.y, 0, 10);
        float distBottom = Mathf.Clamp((mapManager.ySize / 2) + transform.position.y, 0, 10);
        
        if(this.brain.name == "CellLearningBrain2"){
            Debug.Log("Food: [x:" + foodPos.x + ", y:" + foodPos.y + "] top:" + distTop + " right:" + distRight + " bottom:" + distBottom + " left:" + distLeft);
        } else if(this.brain.name == "CellLearningBrain"){
            Debug.Log("Food: [x:" + foodPos.x + ", y:" + foodPos.y + "] top:" + distTop + " right:" + distRight + " bottom:" + distBottom + " left:" + distLeft);
            //Distance to right wall
            
		    AddVectorObs(distRight);
		    //Distance to left wall
		    AddVectorObs(distLeft);
		    //Distance to top wall
		    AddVectorObs(distTop);
		    //Distance to bottom wall
		    AddVectorObs(distBottom);
            
            AddVectorObs(foodPos);
        }

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
			
			//Ends Training episode after eating food
			if(this.brain){
				AddReward(1.0f);
			}
        }

    }

    // Work in progress, to be extended when adding other cells
    public void swallow(/*smaller cell*/) {
		float otherRadius = 0f; // Radius of the swallowed cell or growthSpeed if swallowing food.
		grow(otherRadius);
	}
	
	// Grows so, that the total volume stays the same
	void grow(float mass) {
		setRadius(Mathf.Sqrt(mass*mass + radius*radius));
	}

    void setRadius(float newRadius) {
        radius = newRadius;
        // Starts at startSpeed and converges nicely and stricly monotoniously falling against convergeSpeed
        speed = (startSpeed - convergeSpeed) * (0.05f * startRadius + 1) / (0.05f * radius + 1) + convergeSpeed;

        // Sets world scale, according to radius of the cell
        transform.localScale = new Vector3(radius * 2, radius * 2, 1f);
        PlayerCamera.orthographicSize = 10 * radius;
    }
}
