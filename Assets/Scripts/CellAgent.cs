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
    private float boostTimer;
    public float boostCooldown;
    public float boostSpeed;
	private float windowSize;

    // Start is called before the first frame update
    void Start() {
        PlayerCamera = this.gameObject.GetComponentInChildren<Camera>();
        rBody = this.gameObject.GetComponent<Rigidbody2D>();
        rBody.freezeRotation = true;
        radius = startRadius;
		mapManager = GameObject.Find("Map").GetComponent<MapManager>();
        speed = startSpeed;
		windowSize = 10 * radius;
        PlayerCamera.orthographicSize = WindowSize(radius);
    }
    private float WindowSize(float cellsize)
    {
          return (148.41f - Mathf.Pow(2.718f, -cellsize*0.01f+5))*2f+4;
    }
    public override float[] Heuristic()
    {
        Debug.Log(WindowSize(radius));
		if(PlayerCamera.orthographicSize < WindowSize(radius))
        {
			PlayerCamera.orthographicSize += 0.01f;
		}
		Vector3 mousePosition = PlayerCamera.ScreenToWorldPoint(Input.mousePosition) - this.gameObject.transform.position;
        mousePosition.z = 0f;
		if (mousePosition.magnitude > 1)
            mousePosition.Normalize();
        var action = new float[2];
        action[0] = mousePosition[0];
        action[1] = mousePosition[1];
        return action;
    }

    public override void AgentAction(float[] vectorAction, string textAction) {
		if(PlayerCamera.orthographicSize < windowSize)
        {
			PlayerCamera.orthographicSize += 0.05f;
		}
		Vector2 controlSignal = new Vector2(vectorAction[0], vectorAction[1]);
		if (controlSignal.magnitude > 1)
			controlSignal.Normalize();
		rBody.velocity = controlSignal * speed;
    }
	
    public override void AgentReset()
    {
        base.AgentReset();
        setRadius(startRadius);
		PlayerCamera.orthographicSize = windowSize;
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
		if(this.GetComponent<BehaviorParameters>().behaviorName == "Behavior"){
			//...
        } else if(this.GetComponent<BehaviorParameters>().behaviorName == "Behavior2"){
			//Additional Observations that are extreme when the agent is close to one of the wall.
			//Close to left/right wall
			float distX = Mathf.Clamp((mapManager.xSize / 2) + transform.position.x - radius, 0, 1) - Mathf.Clamp((mapManager.xSize / 2) - transform.position.x - radius, 0, 1);
			//Close to top/bottom wall
			float distY = Mathf.Clamp((mapManager.ySize / 2) + transform.position.y - radius, 0, 1) - Mathf.Clamp((mapManager.ySize / 2) - transform.position.y - radius, 0, 1);
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
        return closest;
    }
    
    //Returns position of the closest  Cell
    GameObject findClosestVirus()
    {
        GameObject[] allCell = GameObject.FindGameObjectsWithTag("Virus");
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

    void shrink(float mass)
    {
        mass = -mass;
        setRadius(Mathf.Sqrt(mass * mass + radius * radius));
        AddReward(-mass);
    }

    void setRadius(float newRadius) {
        radius = newRadius;
        // Starts at startSpeed and converges nicely and stricly monotoniously falling against convergeSpeed
        speed = (startSpeed - convergeSpeed) * (0.05f * startRadius + 1) / (0.05f * radius + 1) + convergeSpeed;

        // Sets world scale, according to radius of the cell
        transform.localScale = new Vector3(radius * 2, radius * 2, 1f);
    }

    private void boost()
    {
        if (boostTimer > boostCooldown)
        {
            boostTimer = 0;


        }
    }
}

