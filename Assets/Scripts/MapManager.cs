using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField] public int maxFood = 200;
    [SerializeField] public int xSize = 200;
    [SerializeField] public int ySize = 200;
    [SerializeField] public GameObject backgroundPrefab = default;
    [SerializeField] public GameObject backgroundContainer = default;
    [SerializeField] public GameObject wall = default;
    // Start is called before the first frame update
    void Start()
    {
        setupMapBackground();
        setupMapBorder();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void spawnFood()
    {


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
