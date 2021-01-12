using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsMove_Campaign : MonoBehaviour
{

    #region Variables

    /// <summary>
    /// 
    /// </summary>
    public int nation;

    /// <summary>
    /// max distance unit can move
    /// </summary>
    [Header("Stats")]
    public int move = 8;

    /// <summary>
    /// <= move ; reset in NewTurn
    /// </summary>
    public int remainingMoves;

    /// <summary>
    /// 
    /// </summary>
    public float moveSpeed = 2.0f;

    /// <summary>
    /// 
    /// </summary>
    public float jumpVelocity = 4.5f;

    /// <summary>
    /// 
    /// </summary>
    public bool isMoving = false;

    /// <summary>
    /// distance from top of tile to center of the player
    /// </summary>
    public float halfHeight;

    /// <summary>
    /// height of unit during movement
    /// </summary>
    public float movementHeight;

    /// <summary>
    /// up/down tile distance for jumping
    /// </summary>
    public float jumpHeight = 2;

    public Vector3 targetVector3 = new Vector3();
    public Vector3 velocity = new Vector3();
    public Vector3 heading = new Vector3();
    public Vector3 jumpTarget = new Vector3();

    /// <summary>
    /// when player select THIS unit or tile on THIS unit = TRUE
    /// </summary>
    public bool unitIsSelected = false;

    [Header("Graphics")]
    public Renderer render;
    public Material material;


    /// <summary>
    /// 
    /// </summary>
    [Header("Movement Campaign")]
    public TileManager_Campaign tileManager_Campaign = null;

    /// <summary>
    /// list of tiles accesible to THIS object
    /// </summary>
    public List<Tile_Campaign> selectableTiles = new List<Tile_Campaign>();

    /// <summary>
    /// tile under THIS unit
    /// </summary>
    public Tile_Campaign currentTile;

    /// <summary>
    /// destination tile for THIS if moving
    /// </summary>
    public Tile_Campaign targetTile;

    /// <summary>
    /// 
    /// </summary>
    public List<Tile_Campaign> path = new List<Tile_Campaign>();

    /// <summary>
    /// 
    /// </summary>
    public List<Tile_Campaign> pathGhost = new List<Tile_Campaign>();

    #endregion


    #region Unity Callbacks (Awake, Start)

    private void Awake()
    {
        Init_Awake();
    }

    public void Init_Awake()
    {

        if (!CompareTag("TacticsMove_Campaign"))
        {
            tag = "TacticsMove_Campaign";
        }

        if (render == null)
        {
            if ((render = GetComponent<Renderer>()) == null)
                Debug.LogError("render\n" + this, gameObject);
        }

        if (material == null)
        {
            if ((material = render.material) == null)
                Debug.LogError("material\n" + this, gameObject);
        }

        // assign tileManager
        if (tileManager_Campaign == null)
        {
            if ((tileManager_Campaign = GameObject.FindGameObjectWithTag("TileManager_Campaign").GetComponent<TileManager_Campaign>()) == null)
                Debug.LogError("tileManager_Campaign\n" + this, gameObject);
        }

        halfHeight = GetComponent<Collider>().bounds.extents.y;

        material.color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
    }

    private void Start()
    {
        Init_Start();
    }

    public void Init_Start()
    {
        targetTile = null;
        unitIsSelected = false;

        // RearangeUnitPosition();
        // new turn
        NewTurn();

        // late start ???
    }

    private void Update()
    {
        // Update_TileMove();
    }

    #endregion

    public void NewTurn()
    {
        remainingMoves = move;
    }

    #region A*

    /// <summary>
    /// 
    /// </summary>
    public void FindSelectableTiles()
    {
        // Debug.Log("<color=yellow> FindSelectableTiles </color>\n\n",gameObject);

        foreach (Tile_Campaign tile in tileManager_Campaign.list_TileCampaign)
        {
            tile.Reset();
        }

        if (currentTile == null)
        {
            GetCurrentTile();
        }

        currentTile.walkable = true;

        Queue<Tile_Campaign> process = new Queue<Tile_Campaign>();

        process.Enqueue(currentTile);

        currentTile.visited = true;

        while (process.Count > 0)
        {
            Tile_Campaign t = process.Dequeue();

            selectableTiles.Add(t);
            t.selectable = true;

            // FIX HERE !!!

            if (t.distance < remainingMoves)
            {
                foreach (Tile_Campaign tile in t.adjacencyList)
                {
                    if (!tile.visited && tile.walkable)
                    {
                        tile.parent = t;
                        tile.visited = true;
                        tile.distance = tile.tileDistance + t.distance;
                        process.Enqueue(tile);
                    }
                }
            }
        }
    }



    void CalculatePath()
    {
        // Tile_Campaign targetTile = GetTargetTile(gameObject);

        FindPath(targetTile);
    }


    public void GetCurrentTile()
    {
        if(currentTile == null)
        {
            currentTile = GetTargetTile(gameObject);
            currentTile.current = true;
        }
    }

    public Tile_Campaign GetTargetTile(GameObject target)
    {
        RaycastHit hit;
        Tile_Campaign tile = null;

        if (Physics.Raycast(target.transform.position, -Vector3.up, out hit, 1))
        {
            tile = hit.collider.GetComponent<Tile_Campaign>();
        }

        return tile;
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    protected Tile_Campaign FindLowestF(List<Tile_Campaign> list)
    {
        Tile_Campaign lowest = list[0];

        foreach (Tile_Campaign t in list)
        {
            if (t.f < lowest.f)
            {
                lowest = t;
            }
        }

        list.Remove(lowest);

        return lowest;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    protected Tile_Campaign FindEndTile(Tile_Campaign t)
    {
        Stack<Tile_Campaign> tempPath = new Stack<Tile_Campaign>();

        Tile_Campaign next = t.parent;
        while (next != null)
        {
            tempPath.Push(next);
            next = next.parent;
        }

        if (tempPath.Count <= move)
        {
            return t.parent;
        }

        Tile_Campaign endTile = null;
        for (int i = 0; i <= move; i++)
        {
            endTile = tempPath.Pop();
        }

        return endTile;
    }

    /// <summary>
    /// when THIS player is selected and will click on tile, tile became a targetTile
    /// </summary>
    /// <param name="target"></param>
    protected void FindPath(Tile_Campaign target)
    {
        // ComputeAdjacencyLists(jumpHeight, target);
        GetCurrentTile();

        List<Tile_Campaign> openList = new List<Tile_Campaign>();
        List<Tile_Campaign> closedList = new List<Tile_Campaign>();

        openList.Add(currentTile);
        //currentTile.parent = ??
        currentTile.h = Vector3.Distance(currentTile.transform.position, target.transform.position);
        currentTile.f = currentTile.h;

        while (openList.Count > 0)
        {
            Tile_Campaign t = FindLowestF(openList);

            closedList.Add(t);

            if (t == target)
            {
                targetTile = FindEndTile(t);
                // MoveToTile(targetTile);
                return;
            }

            foreach (Tile_Campaign tile in t.adjacencyList)
            {
                if (closedList.Contains(tile))
                {
                    //Do nothing, already processed
                }
                else if (openList.Contains(tile))
                {
                    float tempG = t.g + Vector3.Distance(tile.transform.position, t.transform.position);

                    if (tempG < tile.g)
                    {
                        tile.parent = t;

                        tile.g = tempG;
                        tile.f = tile.g + tile.h;
                    }
                }
                else
                {
                    tile.parent = t;

                    tile.g = t.g + Vector3.Distance(tile.transform.position, t.transform.position);
                    tile.h = Vector3.Distance(tile.transform.position, target.transform.position);
                    tile.f = tile.g + tile.h;

                    openList.Add(tile);
                }
            }
        }

        //todo - what do you do if there is no path to the target tile?
        Debug.Log("Path not found");
    }

    #endregion


    #region Mouse Events

    #region Unity OnMouseEvents

    private void OnMouseEnter()
    {
        OnMouseEnterTile();
    }

    private void OnMouseExit()
    {
        OnMouseExitTile();
    }

    private void OnMouseOver()
    {
        OnMouseOverTile();

        // Left mouse button
        if (Input.GetMouseButtonDown(0))
        {
            OnLeftMouseClick();
        }

        // Right mouse button
        if (Input.GetMouseButtonDown(1))
        {
            OnRightMouseClick();
        }
    }

    #endregion

    public void OnMouseEnterTile()
    {
        Debug.Log("<color=yellow> OnMouseEnterTile </color>\n Unit " + this + " \n", gameObject);
    }

    public void OnMouseOverTile()
    {
        // Debug.Log("<color=yellow> OnMouseOverTile </color>\n Unit " + this + " \n", gameObject);
    }

    public void OnMouseExitTile()
    {
        Debug.Log("<color=yellow> OnMouseExitTile </color>\n Unit " + this + " \n", gameObject);
    }

    public void OnLeftMouseClick()
    {
        Debug.Log("<color=yellow> OnLeftMouseClick </color>\n Unit " + this + " \n", gameObject);

        tileManager_Campaign.clickedTile = currentTile;

        // 1 or more units are on currentTile
        if(currentTile.unitsOnTile.Count > 0)
        {
            // tileManager_Campaign.Call_RearangeUnitPosition();
            // only one unit is on tile
            tileManager_Campaign.list_SelectedUnits.Clear();

            // more units are on tile, get shortest distance available for this group of units
            foreach (TacticsMove_Campaign units in currentTile.unitsOnTile)
            {
                if (!tileManager_Campaign.list_SelectedUnits.Contains(units))
                {
                    tileManager_Campaign.list_SelectedUnits.Add(units);
                    // TODO: do for all selected units
                    units.unitIsSelected = true;
                }
            }

            FindSelectableTiles();

            tileManager_Campaign.UpdateTileColor(true);
        }
    }

    public void OnRightMouseClick()
    {
        Debug.Log("<color=yellow> OnRightMouseClick </color>\n Unit " + this + " \n", gameObject);


        tileManager_Campaign.UpdateTileColor(false);
    }

    #endregion





}
