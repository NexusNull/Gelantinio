using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField] public int maxFood = 200;
    [SerializeField] public int maxVirus = 5;
    [SerializeField] public int xSize = 200;
    [SerializeField] public int ySize = 200;
    [SerializeField] public GameObject backgroundPrefab = default;
    [SerializeField] public GameObject backgroundContainer = default;
	[SerializeField] public GameObject foodPrefab = default;
	[SerializeField] public GameObject virusPrefab = default;
	[SerializeField] public GameObject foodContainer = default;	
	[SerializeField] public GameObject virusContainer = default;	
    [SerializeField] public GameObject wall = default;
	
    // Start is called before the first frame update
    void Start()
    {
        setupMapBackground();
		setupMapBorder();
		//Spawns 200 Food at the beginning of the game
		for(int i = 0; i < maxFood; i++){
			spawnFood();
		}
        for (int i = 0; i < maxVirus; i++)
        {
            spawnVirus();
        }
    }
	
	//Creates a food object and places it in a random position
    private void spawnFood()
    {
		float x = Random.Range(-1*(float)xSize/2 + 1, (float)xSize/2 - 1);
		float y = Random.Range(-1*(float)ySize/2 + 1, (float)ySize/2 - 1);
		Instantiate(foodPrefab, new Vector3(x, y, 0f), Quaternion.identity, foodContainer.transform);
    }

    //Creates a food object and places it in a random position
    private void spawnVirus()
    {
        float x = Random.Range(-1 * (float)xSize / 2 + 1, (float)xSize / 2 - 1);
        float y = Random.Range(-1 * (float)ySize / 2 + 1, (float)ySize / 2 - 1);
        Instantiate(virusPrefab, new Vector3(x, y, 0f),Quaternion.identity, virusContainer.transform);
    }
    private void setupMapBackground()
    {
        Vector2 size = backgroundPrefab.GetComponent<SpriteRenderer>().size;
        
        Vector2 startpoint = new Vector2(-xSize / 2, -ySize / 2);

        for(int i = 0; i < xSize / size.x + 1; i++)
        {
            for (int j = 0; j < ySize / size.y + 1; j++)
            {
                GameObject obj = Instantiate(backgroundPrefab, backgroundContainer.transform);
                obj.transform.position = new Vector3(startpoint.x+size.x*i, startpoint.y + size.y * j, 0);

            }
        }
    }

    private void setupMapBorder()
    {
        PolygonCollider2D collider = wall.GetComponent<PolygonCollider2D>();

        collider.SetPath(0, new Vector2[] {
            new Vector2(-(xSize / 2 + 1),  (ySize / 2 + 1)),
            new Vector2( (xSize / 2 + 1),  (ySize / 2 + 1)),
            new Vector2( (xSize / 2 + 1), -(ySize / 2 + 1)),
            new Vector2(-(xSize / 2 + 1), -(ySize / 2 + 1))
        });
        collider.SetPath(1, new Vector2[] {
            new Vector2(-(xSize / 2),  (ySize / 2)),
            new Vector2( (xSize / 2),  (ySize / 2)),
            new Vector2( (xSize / 2), -(ySize / 2)),
            new Vector2(-(xSize / 2), -(ySize / 2))
        });



    }


}
