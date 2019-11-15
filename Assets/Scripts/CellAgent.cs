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

    // Start is called before the first frame update
    void Start() {
        PlayerCamera = this.gameObject.GetComponentInChildren<Camera>();
        rBody = this.gameObject.GetComponent<Rigidbody2D>();
        rBody.freezeRotation = true;
        radius = startRadius;
		mapManager = GameObject.Find("Map").GetComponent<MapManager>();
        speed = startSpeed;
        PlayerCamera.orthographicSize = 10 * radius;
    }

    // Update is called once per frame
    void Update() {
        // has no brain -> player controls via mouse
        if (!this.brain && this.name == "PlayerCell(agent)") {

            Vector3 controlSignal = Vector3.zero;
            Vector3 mousePosition = PlayerCamera.ScreenToWorldPoint(Input.mousePosition) - this.gameObject.transform.position;

            mousePosition.z = 0f;
            if (mousePosition.magnitude > 1)
                mousePosition.Normalize();
            //Debug.Log(mousePosition);
            rBody.velocity = mousePosition * speed;
            //transform.position = transform.position + mousePosition * speed;
        }
			
		if((mapManager.xSize/2)+transform.position.x < radius*1.1){
			Debug.Log((mapManager.xSize/2)+transform.position.x);
		}
    }

    public override void AgentAction(float[] vectorAction, string textAction) {
				
        // If no brain exists the player may control the PlayerCell, otherwise the brain has the control
        if (this.brain.name == "CellPlayerBrain") {
            //Debug.Log(vectorAction);
            // has brain -> brain controls
            Vector2 controlSignal = new Vector3(vectorAction[0],
                                                vectorAction[1]);

            if (controlSignal.magnitude > 1)
                controlSignal.Normalize();

            rBody.velocity = controlSignal * speed;
         
        } else {
			//Vector3 controlSignal = new Vector3(Mathf.Sin(vectorAction[0]*2*Mathf.PI), Mathf.Cos(vectorAction[1]*2*Mathf.PI), 0);
            Vector2 controlSignal = new Vector2(vectorAction[0],
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
        Vector3 food = findClosestFood();
        GameObject cell = findClosestCell();
			AddVectorObs(new Vector2(food.x - transform.position.x, food.y - transform.position.y));
			AddVectorObs(new Vector2(cell.GetComponent<Transform>().position.x - transform.position.x, cell.GetComponent<Transform>().position.y - transform.position.y));
			AddVectorObs(cell.GetComponent<CellAgent>().radius < radius);
		if(this.brain.name == "CellLearningBrain"){
			//...
        } else if(this.brain.name == "CellLearningBrain2"){
			//Additional Observations that are extreme when the agent is close to one of the wall.
			//Close to left/right wall
			float distX = Mathf.Clamp((mapManager.xSize / 2) + transform.position.x, 0, radius * 1.125f) - Mathf.Clamp((mapManager.xSize / 2) - transform.position.x, 0, radius * 1.125f);
					    //Close to top/bottom wall
			float distY = Mathf.Clamp((mapManager.ySize / 2) + transform.position.y, 0, radius * 1.125f) - Mathf.Clamp((mapManager.ySize / 2) - transform.position.y, 0, radius * 1.125f);
		    AddVectorObs(new Vector2(distX, distY));
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
        //Debug.Log(this.name + " - Closest Food: " + closestPosition);
        return closestPosition;
	}

    //Returns position of the closest  Cell
    GameObject findClosestCell()
    {
        GameObject[] allCell = GameObject.FindGameObjectsWithTag("Cell");
        float smallestDistance = Mathf.Infinity;
        GameObject closest = this.gameObject;
        foreach (GameObject cell in allCell)
        {
            float distance = Vector3.Distance(cell.transform.position, transform.position);
            if (distance < smallestDistance && cell.name != this.name)
            {
                smallestDistance = distance;
                closest = cell;
            }
        }
        //Debug.Log(this.name + " : " + closest.name + " - " + closest.transform.position);
        return closest;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "Food") {
            grow(growthSpeed);
            int x = mapManager.xSize;
            int y = mapManager.ySize;
            collision.gameObject.GetComponent<Transform>().position = new Vector2(Random.Range(-(float)x / 2 + 1, (float)x / 2 - 1), Random.Range(-(float)y / 2 + 1, (float)y / 2 - 1));
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Cell")
        {
            if (collision.gameObject.GetComponent<CellAgent>().radius > radius)
            {
                AddReward(-10.0f);
                AgentReset();
            }
            else if (collision.gameObject.GetComponent<CellAgent>().radius < radius)
            {
                swallow(collision.gameObject);
            }
        }
    }

    // Work in progress, to be extended when adding other cells
    public void swallow(GameObject otherCell) {
        float size = otherCell.GetComponent<CellAgent>().radius;
        grow(size);
	}
	
	// Grows so, that the total volume stays the same
	void grow(float mass) {
		setRadius(Mathf.Sqrt(mass*mass + radius*radius));
        AddReward(mass);
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

