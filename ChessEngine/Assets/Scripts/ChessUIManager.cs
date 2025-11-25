using System;
using Core;
using UnityEngine;

public class ChessUIManager : MonoBehaviour
{
    
    private float SqrSize = 1f;
    private GameObject[,] board = new GameObject[8, 8];
    private GameObject[,] activePieces = new GameObject[8, 8];

    // Square Colors
    [SerializeField]
    private Color lightSquares;
    [SerializeField] 
    private Color darkSquares;
    private Material lightSquareMat;
    private Material darkSquareMat;
    
    // Pieces
    [SerializeField] private GameObject wk;
    [SerializeField] private GameObject wq;
    [SerializeField] private GameObject wb;
    [SerializeField] private GameObject wn;
    [SerializeField] private GameObject wr;
    [SerializeField] private GameObject wp;
    [SerializeField] private GameObject bk;
    [SerializeField] private GameObject bq;
    [SerializeField] private GameObject bb;
    [SerializeField] private GameObject bn;
    [SerializeField] private GameObject br;
    [SerializeField] private GameObject bp;


    [SerializeField]
    private string startPositionFen;

    private GameManager game;
    
    private float startX;
    private float startY;

    private bool _isDragged;
    private Vector2Int _dragStartCoords;
    private GameObject _draggedPiece;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeValues();
        game = new GameManager(startPositionFen);
        DrawBoard();
        SetupPieces();
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
        if (_isDragged)
        {
            var worldPost = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPost.z = 0;
            _draggedPiece.transform.position = worldPost;
        }

    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var worldPost = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var coords = ConvertCorrdstoBoardIndex(worldPost);
            if(coords.x<0 || coords.x>=8 || coords.y<0 || coords.y>=8) return;
            if(activePieces[coords.y, coords.x]==null) return;
            _isDragged = true;
            _draggedPiece = activePieces[coords.y, coords.x];
            _dragStartCoords = coords;
        }

        if (Input.GetMouseButtonUp(0))
        {
            var worldPost = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var currentCoords = ConvertCorrdstoBoardIndex(worldPost);
            if(!_isDragged) return;
            _isDragged = false;
            if (!(currentCoords.x < 0 || currentCoords.x >= 8 || currentCoords.y < 0 || currentCoords.y >= 8))
            {
                var move = new Move(_dragStartCoords.y, _dragStartCoords.x, currentCoords.y, currentCoords.x);
                game.MakeMove(move);
                UpdateVisuals(move);
            }
            else
            {
                ResetPosition();
            }

            _draggedPiece = null;
            _dragStartCoords = new Vector2Int();
        }
    }

    private void UpdateVisuals(Move move)
    {
        if(move.toi==move.fromi && move.toj==move.fromj) return;
        var toPiece = activePieces[move.toi, move.toj];
        if (toPiece != null)
        {
            Destroy(toPiece);
        }

        _draggedPiece.transform.position = board[move.toi, move.toj].transform.position;        
        activePieces[move.toi, move.toj] = activePieces[move.fromi, move.fromj];
        activePieces[move.fromi, move.fromj] = null;

    }

    private void ResetPosition()
    {
        _draggedPiece.transform.position = board[_dragStartCoords.y, _dragStartCoords.x].transform.position;
    }

    private void OnValidate()
    {
        if (lightSquareMat != null)
        {
            lightSquareMat.color = lightSquares;
        }

        if (darkSquareMat != null)
        {
            darkSquareMat.color = darkSquares;
        }
    }

    private void InitializeValues()
    {
        lightSquareMat = new Material(Shader.Find("Unlit/Color"));
        lightSquareMat.color = lightSquares;
        darkSquareMat = new Material(Shader.Find("Unlit/Color"));
        darkSquareMat.color = darkSquares;
    }

    void DrawBoard()
    {

        Vector2 pos = transform.position;
        startX = pos.x - 4 * SqrSize;
        startY = pos.y + 4 * SqrSize;

        float tempX = startX;
        float tempY = startY;
        
        

        for (int i = 0; i < 8; i++)
        {
            tempX = startX;
            for (int j = 0; j < 8; j++)
            {
                var sqr = CreateSquare(tempX, tempY, SqrSize, (i + j) % 2 == 0 ? lightSquareMat : darkSquareMat);
                board[i, j] = sqr;
                tempX += SqrSize;
            }

            tempY -= SqrSize;
        }
        

    }

    void SetupPieces()
    {
        var tempBoard = game.GetBoard();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                var piece = tempBoard[i, j];
                if(piece==null) continue;
                if(!piece.Exists()) continue;
                GameObject pref = null;
                if (piece.GetPieceColor() == Piece.PieceColor.White)
                {
                    if (piece.GetPieceType() == Piece.PieceType.Bishop) pref = wb;
                    if (piece.GetPieceType() == Piece.PieceType.Knight) pref = wn;
                    if (piece.GetPieceType() == Piece.PieceType.Rook) pref = wr;
                    if (piece.GetPieceType() == Piece.PieceType.Pawn) pref = wp;
                    if (piece.GetPieceType() == Piece.PieceType.Queen) pref = wq;
                    if (piece.GetPieceType() == Piece.PieceType.King) pref = wk;
                } else if (piece.GetPieceColor() == Piece.PieceColor.Black)
                {
                    if (piece.GetPieceType() == Piece.PieceType.Bishop) pref = bb;
                    if (piece.GetPieceType() == Piece.PieceType.Knight) pref = bn;
                    if (piece.GetPieceType() == Piece.PieceType.Rook) pref = br;
                    if (piece.GetPieceType() == Piece.PieceType.Pawn) pref = bp;
                    if (piece.GetPieceType() == Piece.PieceType.Queen) pref = bq;
                    if (piece.GetPieceType() == Piece.PieceType.King) pref = bk;
                }

                if (pref == null) continue;
                GameObject pieceObj = Instantiate(pref);
                pieceObj.transform.position = board[i, j].transform.position;
                activePieces[i, j] = pieceObj;
            }
        }
    }

    void DestroyAllPieces()
    {
        GameObject[] pieces = GameObject.FindGameObjectsWithTag("Pieces");
        for (int i = 0; i < pieces.Length; i++)
        {
            Destroy(pieces[i]);
        }
    }

    GameObject CreateSquare(float x, float y, float size, Material mat)
    {
        GameObject sqr = GameObject.CreatePrimitive(PrimitiveType.Quad);
        sqr.transform.position = new Vector3(x, y, 0);
        sqr.transform.localScale = new Vector3(size, size, 1);
        sqr.GetComponent<MeshRenderer>().material = mat;

        return sqr;

    }


    Vector2Int ConvertCorrdstoBoardIndex(Vector3 coords)
    {
        float x = (coords.x - (startX - (SqrSize / 2))) / SqrSize;
        float y = (startY+(SqrSize/2) - coords.y) / SqrSize;

        return new Vector2Int((int)x, (int)y);
    }
    
    
}
