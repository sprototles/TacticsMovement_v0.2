using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public enum EnumTileType_Campaign { None, Grass, Sand, Forest, City, Mountain, River }

public class Tile_Campaign : MonoBehaviour
{

    #region Variables

    /// <summary>
    /// 
    /// </summary>
    public EnumTileType_Campaign enumTileType_Campaign;

    public TileManager_Campaign tileManager_Campaign;

    [Space]
    public int nation;

    /// <summary>
    /// list of units on THIS tile
    /// </summary>
    [Header("Units")]
    public List<TacticsMove_Campaign> unitsOnTile = new List<TacticsMove_Campaign>();


    /// <summary>
    /// can unit walk on THIS tile ?
    /// </summary>
    [Header("Movement")]
    public bool walkable;

    /// <summary>
    /// tile under selected unit ; rename [currentSelectedTile] ???
    /// </summary>
    public bool current = false;

    /// <summary>
    /// if TRUE, THIS tile is target destination tile for selected moving unit
    /// </summary>
    public bool target = false;

    /// <summary>
    /// every tile player can select as a "target"
    /// </summary>
    public bool selectable = false;

    /// <summary>
    /// is THIS tile part of path for moving unit ?
    /// </summary>
    public bool isPath = false;

    /// <summary>
    /// list of all neighbourd tiles to THIS tile
    /// </summary>
    public List<Tile_Campaign> adjacencyList = new List<Tile_Campaign>();

    //Needed BFS (breadth first search)
    /// <summary>
    /// TRUE if THIS tile was searched in BFS
    /// </summary>
    public bool visited = false;

    /// <summary>
    /// req. for generating path
    /// </summary>
    public Tile_Campaign parent = null;

    /// <summary>
    /// OBSOLETE ???
    /// </summary>
    public int distance = 0;

    /// <summary>
    /// how much distance it will take to walk over this tile
    /// </summary>
    public int tileDistance;

    //For A*
    /// <summary>
    /// f(n) = g(n) + h(n)
    /// </summary>
    public float f = 0;

    /// <summary>
    /// cost of the path from the start node to n
    /// </summary>
    public float g = 0;

    /// <summary>
    /// heuristic estimation of cost of the cheapest path from n to the goal
    /// </summary>
    public float h = 0;

    [Header("Graphics")]
    public Renderer render;
    public Material material;
    [HideInInspector]    public Texture texture_None;
    [HideInInspector]    public Texture texture_Grass;
    [HideInInspector]    public Texture texture_Sand;
    [HideInInspector]    public Texture texture_Forest;
    [HideInInspector]    public Texture texture_City;
    [HideInInspector]    public Texture texture_Mountain;
    [HideInInspector]    public Texture texture_River;

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        Init_Awake();
    }

    public void Init_Awake()
    {
        // set name for easier navigation in hierarchy
        name = "Tile_[" + Mathf.RoundToInt(transform.localPosition.x) + "," + Mathf.RoundToInt(transform.localPosition.z) + "]";

        // LOAD MANAGER
        if (tileManager_Campaign == null)
        {
            if ((tileManager_Campaign = GameObject.Find("TileManager_Campaign").GetComponent<TileManager_Campaign>()) == null)
                Debug.LogError("TileManager_Campaign", gameObject);
        }

        // SET TAG
        if (!CompareTag("Tile_Campaign"))
        {
            tag = "Tile_Campaign";
        }

        // RENDERER
        if (render == null)
        {
            if ((render = GetComponent<Renderer>()) == null)
                Debug.LogError("renderer", gameObject);
        }

        // MATERIAL
        if (material == null)
        {
            if ((material = render.material) == null)
                Debug.LogError("material", gameObject);
        }

        // set texture tiling
        material.mainTextureScale = new Vector2(1, 1);

        // TEXTURES
        if (texture_None == null)
        {
            if ((texture_None = Resources.Load("Material/Tile/texture_None") as Texture) == null)
                Debug.LogWarning("Cannot find texture_None", gameObject);
        }

        if (texture_Grass == null)
        {
            if ((texture_Grass = Resources.Load("Material/Tile/texture_Grass") as Texture) == null)
                Debug.LogWarning("Cannot find texture_Grass", gameObject);
        }

        if (texture_Sand == null)
        {
            if ((texture_Sand = Resources.Load("Material/Tile/texture_Sand") as Texture) == null)
                Debug.LogWarning("Cannot find texture_Sand", gameObject);
        }

        if (texture_Forest == null)
        {
            if ((texture_Forest = Resources.Load("Material/Tile/texture_Forest") as Texture) == null)
                Debug.LogWarning("Cannot find texture_Forest", gameObject);
        }

        if (texture_City == null)
        {
            if ((texture_City = Resources.Load("Material/Tile/texture_City") as Texture) == null)
                Debug.LogWarning("Cannot find texture_City", gameObject);
        }

        if (texture_Mountain == null)
        {
            if ((texture_Mountain = Resources.Load("Material/Tile/texture_Mountain") as Texture) == null)
                Debug.LogWarning("Cannot find texture_Mountain", gameObject);
        }

        if (texture_River == null)
        {
            if ((texture_River = Resources.Load("Material/Tile/texture_River") as Texture) == null)
                Debug.LogWarning("Cannot find texture_River", gameObject);
        }
        
        Reset();
        Call_FillAdjacenctList();

    }

    private void Start()
    {
        Init_Start();
    }


    public void Init_Start()
    {
        // assign tile to tileManager
        if (!tileManager_Campaign.list_TileCampaign.Contains(this))
        {
            tileManager_Campaign.list_TileCampaign.Add(this);
        }

        SetTileColor_Enum();
    }

    #endregion


    private void OnValidate()
    {
        // Init_Awake();
        // SetTileColor();
    }

    #region Tile Color

    /// <summary>
    /// 
    /// </summary>
    [ContextMenu("01 - SetTileColor_Enum")]
    public void SetTileColor_Enum()
    {
        // TODO
        /*
         * if advancedGraphics
         *      load transparent tile
         * 
         */
        material.mainTexture = texture_None;

        switch (enumTileType_Campaign)
        {
            case EnumTileType_Campaign.None:
                //material.mainTexture = texture_None;
                material.color = Color.black;
                walkable = false;
                tileDistance = 99;
                Debug.LogWarning("TileType not set\n " + this +"\n\n", gameObject);
                break;
            case EnumTileType_Campaign.Grass:
                //material.mainTexture = texture_Grass;
                material.color = Color.green;
                walkable = true;
                tileDistance = 1;
                break;
            case EnumTileType_Campaign.Sand:
                //material.mainTexture = texture_Sand;
                material.color = Color.red;
                walkable = true;
                tileDistance = 3;
                break;
            case EnumTileType_Campaign.Forest:
                //material.mainTexture = texture_Forest;
                material.color = new Color(0.125f, 0.5f, 0.2f, 1);
                walkable = true;
                tileDistance = 2;
                break;
            case EnumTileType_Campaign.City:
                //material.mainTexture = texture_City;
                material.color = Color.yellow;
                walkable = true;
                tileDistance = 1;
                break;
            case EnumTileType_Campaign.Mountain:
                //material.mainTexture = texture_Mountain;
                material.color = Color.grey;
                walkable = false;
                tileDistance = 99;
                break;
            case EnumTileType_Campaign.River:
                //material.mainTexture = texture_River;
                material.color = Color.blue;
                walkable = false;
                tileDistance = 99;
                break;

            default:
                Debug.LogError("Incorrect TileType\n" +this  + "\n\n",gameObject);
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetTileColor_Move()
    {
        if (!walkable)
        {
            material.color = Color.grey;
        }
        else if (target || isPath)  // fix
        {
            material.color = Color.green;
        }
        else if (selectable)
        {
            material.color = Color.red;
        }
        else if (current)
        {
            material.color = Color.magenta;
        }
        else
        {
            material.color = Color.white;
        }
    }






    #endregion

    #region A*

    /// <summary>
    /// 
    /// </summary>
    public void Reset()
    {
        current = false;
        target = false;
        selectable = false;

        visited = false;
        parent = null;
        distance = 0;

        f = g = h = 0;
    }

    /// <summary>
    /// called at the start of the game to hard-write adjacency list
    /// </summary>
    public void Call_FillAdjacenctList()
    {
        adjacencyList.Clear();
        FillAdjacencyList(Vector3.right);
        FillAdjacencyList(Vector3.forward);
        FillAdjacencyList(-Vector3.forward);
        FillAdjacencyList(-Vector3.right);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="direction"></param>
    private void FillAdjacencyList(Vector3 direction)
    {
        Vector3 halfExtents = new Vector3(0.25f, (1 + 0) / 2.0f, 0.25f);
        Collider[] colliders = Physics.OverlapBox(transform.position + direction, halfExtents);

        foreach (Collider item in colliders)
        {
            Tile_Campaign tile = item.GetComponent<Tile_Campaign>();
            if (tile != null)
            {
                adjacencyList.Add(tile);
            }
        }
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
        Debug.Log("<color=yellow> OnMouseEnterTile </color>\n Tile " + this + " \n", gameObject);
    }

    public void OnMouseOverTile()
    {
        // Debug.Log("<color=yellow> OnMouseOverTile </color>\n Tile " + this + " \n", gameObject);
    }

    public void OnMouseExitTile()
    {
        Debug.Log("<color=yellow> OnMouseExitTile </color>\n Tile " + this + " \n", gameObject);
    }

    public void OnLeftMouseClick()
    {
        Debug.Log("<color=yellow> OnLeftMouseClick </color>\n Tile " + this + " \n", gameObject);
    }

    public void OnRightMouseClick()
    {
        Debug.Log("<color=yellow> OnRightMouseClick </color>\n Tile " + this + " \n", gameObject);


        tileManager_Campaign.UpdateTileColor(false);
    }

    #endregion





}
