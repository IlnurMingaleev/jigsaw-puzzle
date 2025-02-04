using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Puzzle;
using Ui;
using UniRx;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BoardGen : MonoBehaviour,IDisposable
{
    public string ImageFilename;
    public Transform nonActiveParentForTiles;
    public Transform activeParentForTiles;

    public Image buttonPrefab;
    public Transform scrollViewContentTransform;
    public bool LoadingFinished { get; set; } = false;

    // The opaque sprite. 
    Sprite mBaseSpriteOpaque;

    // The transparent (or Ghost sprite)
    Sprite mBaseSpriteTransparent;

    // The game object that holds the opaque sprite.
    // This should be SetActive to false by default.
    GameObject mGameObjectOpaque;

    // The game object that holds the transparent sprite.
    GameObject mGameObjectTransparent;
    
    private Dictionary<int,GameObject> puzzleByHash = new Dictionary<int,GameObject>();
    
    private List<GameObject> uiPuzzles = new List<GameObject>();
    private CompositeDisposable disposables = new CompositeDisposable();
    Sprite LoadBaseTexture()
    {
        Texture2D tex = SpriteUtils.LoadTexture(ImageFilename);
        if (!tex.isReadable)
        {
            Debug.Log("Error: Texture is not readable");
            return null;
        }

        if(tex.width % Tile.TileSize != 0 || tex.height % Tile.TileSize != 0)
        {
            Debug.Log("Error: Image must be of size that is multiple of <" + Tile.TileSize + ">");
            return null;
        }

        // Add padding to the image.
        Texture2D newTex = new Texture2D(
            tex.width + Tile.Padding * 2, 
            tex.height + Tile.Padding * 2, 
            TextureFormat.ARGB32, 
            false);

        // Set the default colour as white
        for (int x = 0; x < newTex.width; ++x)
        {
            for (int y = 0; y < newTex.height; ++y)
            {
                newTex.SetPixel(x, y, Color.white);
            }
        }

        // Copy the colours.
        for (int x = 0; x < tex.width; ++x)
        {
            for (int y = 0; y < tex.height; ++y)
            {
                Color color = tex.GetPixel(x, y);
                color.a = 1.0f;
                newTex.SetPixel(x + Tile.Padding, y + Tile.Padding, color);
            }
        }
        newTex.Apply();

        Sprite sprite = SpriteUtils.CreateSpriteFromTexture2D(
            newTex,
            0,
            0,
            newTex.width,
            newTex.height);
        return sprite;
    }

    Sprite CreateTransparentView()
    {
        Texture2D tex = mBaseSpriteOpaque.texture;

        // Add padding to the image.
        Texture2D newTex = new Texture2D(
            tex.width,
            tex.height,
            TextureFormat.ARGB32,
            false);

        //for (int x = Tile.Padding; x < Tile.Padding + Tile.TileSize; ++x)
        for (int x = 0; x < newTex.width; ++x)
        {
            //for (int y = Tile.Padding; y < Tile.Padding + Tile.TileSize; ++y)
            for (int y = 0; y < newTex.height; ++y)
            {
                Color c = tex.GetPixel(x, y);
                if (x > Tile.Padding && x < (newTex.width - Tile.Padding) && y > Tile.Padding && y < newTex.height - Tile.Padding)
                {
                    c.a = 0.2f;
                }
                newTex.SetPixel(x, y, c);
            }
        }

        newTex.Apply();

        Sprite sprite = SpriteUtils.CreateSpriteFromTexture2D(
            newTex,
            0,
            0,
            newTex.width,
            newTex.height);
        return sprite;
    }
    
    #region Coroutines to load create the board.
    public void CreateJigsawBoardUsingCoroutines()
    {
        mBaseSpriteOpaque = LoadBaseTexture();

        NumTilesX = mBaseSpriteOpaque.texture.width / Tile.TileSize;
        NumTilesY = mBaseSpriteOpaque.texture.height / Tile.TileSize;

        mGameObjectOpaque = new GameObject();
        mGameObjectOpaque.name = ImageFilename + "_Opaque";
        mGameObjectOpaque.AddComponent<SpriteRenderer>().sprite = mBaseSpriteOpaque;
        mGameObjectOpaque.GetComponent<SpriteRenderer>().sortingLayerName = "Opaque";
        mGameObjectOpaque.AddComponent<FullImage>();
        

        StartCoroutine(Coroutine_CreateJigsawBoard());
    }

    IEnumerator Coroutine_CreateJigsawBoard()
    {
        mBaseSpriteTransparent = CreateTransparentView();
        mGameObjectTransparent = new GameObject();
        mGameObjectTransparent.name = ImageFilename + "_Transparent";
        mGameObjectTransparent.AddComponent<SpriteRenderer>().sprite = mBaseSpriteTransparent;
        mGameObjectTransparent.GetComponent<SpriteRenderer>().sortingLayerName = "Transparent";
        mGameObjectTransparent.AddComponent<FullImage>();
        yield return null;

        yield return StartCoroutine(Coroutine_CreateJigsawTiles());

        // Hide the mBaseSpriteOpaque game object.
        mGameObjectOpaque.gameObject.SetActive(false);

        LoadingFinished = true;
    }


    IEnumerator Coroutine_CreateJigsawTiles()
    {
        Texture2D baseTexture = mBaseSpriteOpaque.texture;
        NumTilesX = baseTexture.width / Tile.TileSize;
        NumTilesY = baseTexture.height / Tile.TileSize;

        mTiles = new Tile[NumTilesX, NumTilesY];
        mTileGameObjects = new GameObject[NumTilesX, NumTilesY];
        for (int i = 0; i < NumTilesX; ++i)
        {
            for (int j = 0; j < NumTilesY; ++j)
            {
                Tile tile = new Tile(baseTexture);
                tile.xIndex = i;
                tile.yIndex = j;

                // Left side tiles
                if (i == 0)
                {
                    tile.SetPosNegType(Tile.Direction.LEFT, Tile.PosNegType.NONE);
                }
                else
                {
                    // We have to create a tile that has LEFT direction opposite operation 
                    // of the tile on the left's RIGHT direction operation.
                    Tile leftTile = mTiles[i - 1, j];
                    Tile.PosNegType rightOp = leftTile.GetPosNegType(Tile.Direction.RIGHT);
                    tile.SetPosNegType(Tile.Direction.LEFT, rightOp == Tile.PosNegType.NEG ? Tile.PosNegType.POS : Tile.PosNegType.NEG);
                }

                // Bottom side tiles
                if (j == 0)
                {
                    tile.SetPosNegType(Tile.Direction.DOWN, Tile.PosNegType.NONE);
                }
                else
                {
                    // We have to create a tile that has LEFT direction opposite operation 
                    // of the tile on the left's RIGHT direction operation.
                    Tile downTile = mTiles[i, j - 1];
                    Tile.PosNegType rightOp = downTile.GetPosNegType(Tile.Direction.UP);
                    tile.SetPosNegType(Tile.Direction.DOWN, rightOp == Tile.PosNegType.NEG ? Tile.PosNegType.POS : Tile.PosNegType.NEG);
                }

                // Right side tiles
                if (i == NumTilesX - 1)
                {
                    tile.SetPosNegType(Tile.Direction.RIGHT, Tile.PosNegType.NONE);
                }
                else
                {
                    float toss = Random.Range(0.0f, 1.0f);
                    if (toss < 0.5f)
                    {
                        tile.SetPosNegType(Tile.Direction.RIGHT, Tile.PosNegType.POS);
                    }
                    else
                    {
                        tile.SetPosNegType(Tile.Direction.RIGHT, Tile.PosNegType.NEG);
                    }
                }

                // Up side tiles
                if (j == NumTilesY - 1)
                {
                    tile.SetPosNegType(Tile.Direction.UP, Tile.PosNegType.NONE);
                }
                else
                {
                    float toss = Random.Range(0.0f, 1.0f);
                    if (toss < 0.5f)
                    {
                        tile.SetPosNegType(Tile.Direction.UP, Tile.PosNegType.POS);
                    }
                    else
                    {
                        tile.SetPosNegType(Tile.Direction.UP, Tile.PosNegType.NEG);
                    }
                }

                tile.Apply();

                mTiles[i, j] = tile;

                // Create a game object for the tile.
                mTileGameObjects[i, j] = Tile.CreateGameObjectFromTile(tile);

                if (nonActiveParentForTiles != null)
                {
                    mTileGameObjects[i, j].transform.SetParent(nonActiveParentForTiles);
                    GameObject gObject = mTileGameObjects[i, j];
                    buttonPrefab.sprite = mTileGameObjects[i, j].GetComponent<SpriteRenderer>().sprite;
                    GameObject go = Instantiate(buttonPrefab.gameObject, scrollViewContentTransform, true);
                    //puzzleByHash.Add(buttonPrefab.sprite.GetHashCode(),mTileGameObjects[i,j]);
                    Button button = go.GetComponent<Button>();
                    button
                        .OnClickAsObservable()
                        .Subscribe(_ => OnUIPuzzleClick(gObject,button))
                        .AddTo(disposables);
                    uiPuzzles.Add(go);

                }
                yield return null;
            }
        }
        Shuffle();
    }
    #endregion

    public void CreateJigsawBoard()
    {
        mBaseSpriteOpaque = LoadBaseTexture();
        mGameObjectOpaque = new GameObject();
        mGameObjectOpaque.name = ImageFilename + "_Opaque";
        mGameObjectOpaque.AddComponent<SpriteRenderer>().sprite = mBaseSpriteOpaque;
        mGameObjectOpaque.GetComponent<SpriteRenderer>().sortingLayerName = "Opaque";

        mBaseSpriteTransparent = CreateTransparentView();
        mGameObjectTransparent = new GameObject();
        mGameObjectTransparent.name = ImageFilename + "_Transparent";
        mGameObjectTransparent.AddComponent<SpriteRenderer>().sprite = mBaseSpriteTransparent;
        mGameObjectTransparent.GetComponent<SpriteRenderer>().sortingLayerName = "Transparent";

        CreateJigsawTiles();

        // Hide the mBaseSpriteOpaque game object.
        mGameObjectOpaque.gameObject.SetActive(false);
    }

    protected void Start()
    {
        CreateJigsawBoard();
    }

    public int NumTilesX { get; private set; }
    public int NumTilesY { get; private set; }

    protected Tile[,] mTiles = null;
    protected GameObject[,] mTileGameObjects = null;

    void CreateJigsawTiles()
    {
        Texture2D baseTexture = mBaseSpriteOpaque.texture;
        NumTilesX = baseTexture.width / Tile.TileSize;
        NumTilesY = baseTexture.height / Tile.TileSize;

        mTiles = new Tile[NumTilesX, NumTilesY];
        mTileGameObjects = new GameObject[NumTilesX, NumTilesY];
        for (int i = 0; i < NumTilesX; ++i)
        {
            for(int j = 0; j < NumTilesY; ++j)
            {
                Tile tile = new Tile(baseTexture);
                tile.xIndex = i;
                tile.yIndex = j;

                // Left side tiles
                if (i == 0)
                {
                    tile.SetPosNegType(Tile.Direction.LEFT, Tile.PosNegType.NONE);
                }
                else
                {
                    // We have to create a tile that has LEFT direction opposite operation 
                    // of the tile on the left's RIGHT direction operation.
                    Tile leftTile = mTiles[i - 1, j];
                    Tile.PosNegType rightOp = leftTile.GetPosNegType(Tile.Direction.RIGHT);
                    tile.SetPosNegType(Tile.Direction.LEFT, rightOp == Tile.PosNegType.NEG ? Tile.PosNegType.POS : Tile.PosNegType.NEG);
                }

                // Bottom side tiles
                if (j == 0)
                {
                    tile.SetPosNegType(Tile.Direction.DOWN, Tile.PosNegType.NONE);
                }
                else
                {
                    // We have to create a tile that has LEFT direction opposite operation 
                    // of the tile on the left's RIGHT direction operation.
                    Tile downTile = mTiles[i, j-1];
                    Tile.PosNegType rightOp = downTile.GetPosNegType(Tile.Direction.UP);
                    tile.SetPosNegType(Tile.Direction.DOWN, rightOp == Tile.PosNegType.NEG ? Tile.PosNegType.POS : Tile.PosNegType.NEG);
                }

                // Right side tiles
                if (i == NumTilesX - 1)
                {
                    tile.SetPosNegType(Tile.Direction.RIGHT, Tile.PosNegType.NONE);
                }
                else
                {
                    float toss = Random.Range(0.0f, 1.0f);
                    if(toss < 0.5f)
                    {
                        tile.SetPosNegType(Tile.Direction.RIGHT, Tile.PosNegType.POS);
                    }
                    else
                    {
                        tile.SetPosNegType(Tile.Direction.RIGHT, Tile.PosNegType.NEG);
                    }
                }

                // Up side tiles
                if (j == NumTilesY - 1)
                {
                    tile.SetPosNegType(Tile.Direction.UP, Tile.PosNegType.NONE);
                }
                else
                {
                    float toss = Random.Range(0.0f, 1.0f);
                    if (toss < 0.5f)
                    {
                        tile.SetPosNegType(Tile.Direction.UP, Tile.PosNegType.POS);
                    }
                    else
                    {
                        tile.SetPosNegType(Tile.Direction.UP, Tile.PosNegType.NEG);
                    }
                }

                tile.Apply();

                mTiles[i, j] = tile;

                // Create a game object for the tile.
                mTileGameObjects[i, j] = Tile.CreateGameObjectFromTile(tile);

                if (nonActiveParentForTiles != null)
                {
                    mTileGameObjects[i, j].transform.SetParent(nonActiveParentForTiles);
                    
                }
            }
            
        }
       
    }

    #region Other public functions
    public void ShowOpaqueImage(bool flag)
    {
        mGameObjectOpaque.SetActive(flag);
    }
    #endregion
    
    public void Shuffle() {
        for (int i = 0; i < uiPuzzles.Count; i++) {
            int randomIndex = Random.Range(0, uiPuzzles.Count);
            GameObject temp = uiPuzzles[i];
            uiPuzzles[i] = uiPuzzles[randomIndex];
            uiPuzzles[randomIndex] = temp;
        }

        // Update the Hierarchy to reflect the shuffled order
        foreach (GameObject gameObject in uiPuzzles) {
            gameObject.transform.SetSiblingIndex(uiPuzzles.IndexOf(gameObject));
        }
    }
    public void OnUIPuzzleClick(GameObject gameObj,Button button)
    {
        gameObj.transform.position = mGameObjectTransparent.transform.position;
        gameObj.transform.SetParent(activeParentForTiles);
        Destroy(button.gameObject);
    }

    public void Dispose()
    {
        disposables.Clear();
    }
}
