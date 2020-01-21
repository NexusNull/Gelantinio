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
    public Collider2D wallCollider;
    public Collider2D triggerCollider;
    private float windowSize;

    // Start is called before the first frame update
    void Start() {
        PlayerCamera = this.gameObject.GetComponentInChildren<Camera>();
        rBody = this.gameObject.GetComponent<Rigidbody2D>();
        rBody.freezeRotation = true;
        radius = startRadius;
		mapManager = GameObject.Find("Map").GetComponent<MapManager>();
        speed = startSpeed;
		windowSize = WindowSize(radius);
        PlayerCamera.orthographicSize = WindowSize(radius);
        GameObject[] otherCells = GameObject.FindGameObjectsWithTag("Cell");
        Collider[] collider = this.transform.GetComponents<Collider>();
        Debug.Log(wallCollider);
        foreach (GameObject other in otherCells)
        {

            if(other != this)
            {
                Collider2D wallCollider = other.GetComponent<CellAgent>().wallCollider;
                Debug.Log(wallCollider);
                Physics2D.IgnoreCollision(this.wallCollider, wallCollider, true);
            }
        }
    }

    private void Update()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, -radius / 100);
    }

    private float WindowSize(float cellsize)
    {
          return (148.41f - Mathf.Pow(2.718f, -cellsize*0.01f+5))*2f+4;
    }
	
    public override float[] Heuristic()
    {
        if (Mathf.Round(PlayerCamera.orthographicSize * 100) < Mathf.Round(WindowSize(radius) * 100))
        {
			PlayerCamera.orthographicSize += 0.01f;
		} else if (Mathf.Round(PlayerCamera.orthographicSize) > Mathf.Round(WindowSize(radius)))
		{
			PlayerCamera.orthographicSize -= 0.01f;
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
		Vector3 virus = findClosestVirus();
        GameObject cell = findClosestCell();
		AddVectorObs(new Vector2(food.x - transform.position.x, food.y - transform.position.y));
		AddVectorObs(new Vector2(virus.x - transform.position.x, virus.y - transform.position.y));
		AddVectorObs(new Vector2(cell.GetComponent<Transform>().position.x - transform.position.x, cell.GetComponent<Transform>().position.y - transform.position.y));
		AddVectorObs(new Vector2(cell.GetComponent<CellAgent>().radius, radius));
			
		//Additional Observations that are extreme when the agent is close to one of the wall.
		//Close to left/right wall
		float distX = Mathf.Clamp((mapManager.xSize / 2) + transform.position.x - radius, 0, 1) - Mathf.Clamp((mapManager.xSize / 2) - transform.position.x - radius, 0, 1);
		//Close to top/bottom wall
		float distY = Mathf.Clamp((mapManager.ySize / 2) + transform.position.y - radius, 0, 1) - Mathf.Clamp((mapManager.ySize / 2) - transform.position.y - radius, 0, 1);
		AddVectorObs(new Vector2(distX, distY));
    }

    //Returns position of the closest Food
    Vector3 findClosestFood() {
		GameObject[] allFood = GameObject.FindGameObjectsWithTag("Food");
		float smallestDistance = Mathf.Infinity;
		Vector3 closestPosition = Vector3.zero;
		foreach (GameObject food in allFood)
		{
			float distance = Vector3.Distance(food.transform.position, transform.position);
			if(distance < smallestDistance){
				smallestDistance = distance;
				closestPosition = food.transform.position;
			}
		}
        return closestPosition;
	}
	
	//Returns position of the closest Virus
    Vector3 findClosestVirus()
    {
        GameObject[] allVirus = GameObject.FindGameObjectsWithTag("Virus");
        float smallestDistance = Mathf.Infinity;
        Vector3 closestPosition = Vector3.zero;
        foreach (GameObject virus in allVirus)
        {
            float distance = Vector3.Distance(virus.transform.position, transform.position);
            if (distance < smallestDistance)
            {
                smallestDistance = distance;
                closestPosition = virus.transform.position;
            }
        }
        return closestPosition;
    }

    //Returns position of the closest Cell
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
   
    private void OnTriggerStay2D(Collider2D collision) {
        if (collision.gameObject.tag == "Food") {
            grow(growthSpeed);
            int x = mapManager.xSize;
            int y = mapManager.ySize;
            collision.gameObject.GetComponent<Transform>().position = new Vector2(Random.Range(-(float)x / 2 + 1, (float)x / 2 - 1), Random.Range(-(float)y / 2 + 1, (float)y / 2 - 1));
        }
		else if (collision.gameObject.tag == "Virus" &&  Vector3.Distance(collision.gameObject.transform.position, this.transform.position) < radius)
		{
            CircleCollider2D collider = collision.gameObject.GetComponent<CircleCollider2D>();
            if (collider.radius < radius)
            {
                shrink(Mathf.Sqrt(radius * radius/2));
                Destroy(collision.gameObject);
            }

        } else if (collision.gameObject.tag == "Cell")
        {
            if (Vector3.Distance(collision.gameObject.transform.position, this.transform.position) < radius)
            {
                if (collision.gameObject.GetComponent<CellAgent>().radius > radius * 1.2f)
                    {
                        AddReward(-10.0f);
                        AgentReset();
                    }
                    else if (collision.gameObject.GetComponent<CellAgent>().radius * 1.1f < radius)
                    {
                        swallow(collision.gameObject);
                    }
            }
         
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

    }

    /**
     *  Gets called when from the other cell when it is being swallowed
     */
    public void swallowed()
    {
        AddReward(-10.0f);
        AgentReset();
    }
    public void swallow(GameObject otherCell) {
        CellAgent script = otherCell.GetComponent<CellAgent>();
        float size = otherCell.GetComponent<CellAgent>().radius;
        grow(size);
        script.swallowed();
	}
	
	// Grows so, that the total volume stays the same
	void grow(float mass) {
		setRadius(Mathf.Sqrt(mass * mass + radius * radius));
        AddReward(mass);
	}

	// Shrinks
    void shrink(float mass)
    {
        setRadius(Mathf.Sqrt(radius * radius - mass * mass));
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

