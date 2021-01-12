using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager_Campaign : MonoBehaviour
{
    #region Variables

    /// <summary>
    /// 
    /// </summary>
    public int nationOnTurn;

    /// <summary>
    /// target tile for selected unit
    /// </summary>
    public Tile_Campaign selectedTile;

    /// <summary>
    /// 
    /// </summary>
    public Tile_Campaign clickedTile;

    /// <summary>
    /// 
    /// </summary>
    public List<TacticsMove_Campaign> list_movingUnits = new List<TacticsMove_Campaign>();

    /// <summary>
    /// list of all tiles on this map
    /// </summary>
    public List<Tile_Campaign> list_TileCampaign = new List<Tile_Campaign>();

    /// <summary>
    /// list of all active units on this map
    /// </summary>
    public List<TacticsMove_Campaign> list_UnitCampaign = new List<TacticsMove_Campaign>();

    /// <summary>
    /// list of all selected units on same tile
    /// </summary>
    public List<TacticsMove_Campaign> list_SelectedUnits = new List<TacticsMove_Campaign>();

    [Space]
    public Texture white;
    #endregion

    #region Unity Callbacks (Awake, Start)

    private void Awake()
    {
        Init_Awake();
    }

    public void Init_Awake()
    {
        if (!CompareTag("TileManager_Campaign"))
        {
            tag = "TileManager_Campaign";
        }

        if (white == null)
        {
            if ((white = Resources.Load("Material/GUI/white") as Texture) == null)
                Debug.LogWarning("Cannot find white", gameObject);
        }

        list_SelectedUnits.Clear();
        list_TileCampaign.Clear();
        list_movingUnits.Clear();

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("TacticsMove_Campaign"))
        {
            TacticsMove_Campaign tacticsMove_Campaign = go.GetComponent<TacticsMove_Campaign>();
            if (!list_UnitCampaign.Contains(tacticsMove_Campaign))
            {
                list_UnitCampaign.Add(tacticsMove_Campaign);
            }
        }

        nationOnTurn = -1;

    }

    private void Start()
    {
        Debug.Log("<color=green> TileManager \n Start()  </color>\n" + this, gameObject);
        Init_Start();
    }

    public void Init_Start()
    {
        // NewTurn();
    }


    #endregion


    /// <summary>
    /// if TRUE, set for MOVEMENT ; if FALSE, set for Enum
    /// </summary>
    /// <param name="movement">movement</param>
    public void UpdateTileColor(bool movement)
    {
        foreach (Tile_Campaign tile in list_TileCampaign)
        {
            if (movement)
            {
                tile.SetTileColor_Move();
            }
            else
            {
                tile.SetTileColor_Enum();
            }
        }
    }

}
