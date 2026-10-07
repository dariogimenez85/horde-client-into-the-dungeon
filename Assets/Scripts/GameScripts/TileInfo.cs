using TMPro;
using UnityEngine;

public class TileInfo : MonoBehaviour
{
    // (fila, columna)
    public Vector2Int indice;
    public bool isTrap = false;
    public int scoreToGive = 0;

    private SpriteRenderer mySpriteRenderer;
    private SpriteRenderer childSpriteRenderer;
    private SpriteMask spriteMask;
    private TextMeshPro myTextMesh;
    private MeshRenderer myMeshRenderer;


    private void Start()
    {
        SettingTileInfo();
    }

    public void BecameTrap(string strVal)
    {
        print("Became Trap");

        if (myTextMesh == null) SettingTileInfo();

        isTrap = true;
        myTextMesh.text = strVal;
        childSpriteRenderer.gameObject.SetActive(true);
    }

    public void BecameNormal()
    {
        print("Became Normal");
        isTrap = false;
        myTextMesh.text = "";
        scoreToGive = 0;
        childSpriteRenderer.gameObject.SetActive(false);
        transform.GetChild(2).gameObject.SetActive(false); // desabilitamos la animacion de puntos
        transform.GetChild(3).gameObject.SetActive(false); // deshabilitamos la celda verde(visual)
    }

    // if is valid cell to move, we activate the green cell(visual)-> OnReconnect
    public void ActiveGreenCell()
    {
        transform.GetChild(3).gameObject.SetActive(true); // habilitamos la celda verde(visual)
    }

    private void SettingTileInfo()
    {
        print("Setup TileInfo");
        myTextMesh = GetComponentInChildren<TextMeshPro>();
        myMeshRenderer = myTextMesh.GetComponent<MeshRenderer>();
        myMeshRenderer.sortingLayerName = "MaskLigth";
        myMeshRenderer.sortingOrder = 3;
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        spriteMask = GetComponent<SpriteMask>();
        spriteMask.sprite = mySpriteRenderer.sprite;
        childSpriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }
}