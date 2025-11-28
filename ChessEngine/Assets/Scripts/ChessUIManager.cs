using System;
using System.Collections.Generic;
using Core;
using UnityEngine;

public class ChessUIManager : MonoBehaviour
{
    
    private float SqrSize = 1f;
    private GameObject[,] board = new GameObject[8, 8];
    private GameObject[,] validSquares = new GameObject[8, 8];
    private GameObject[,] activePieces = new GameObject[8, 8];

    // Square Colors
    [SerializeField]
    private Color lightSquares;
    [SerializeField] 
    private Color darkSquares;
    [SerializeField] private Color validSquareColors;
    private Material lightSquareMat;
    private Material darkSquareMat;
    private Material validSquareMat;

    private List<Move> validSqrs;
    
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
            worldPost.z = -1f;
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
            Debug.Log(coords);
            validSqrs = game.GenerateValidMoves(coords);
            for (int i = 0; i < validSqrs.Count; i++)
            {
                var di = validSqrs[i].toi;
                var dj = validSqrs[i].toj;
                var p = validSquares[di, dj].transform.position;
                validSquares[di, dj].transform.position = new Vector3(p.x, p.y, -0.5f);
            }
            _isDragged = true;
            _draggedPiece = activePieces[coords.y, coords.x];
            _dragStartCoords = coords;
        }

        if (Input.GetMouseButtonUp(0))
        {
            var worldPost = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var currentCoords = ConvertCorrdstoBoardIndex(worldPost);
            for (int i = 0; i < validSqrs.Count; i++)
            {
                var di = validSqrs[i].toi;
                var dj = validSqrs[i].toj;
                var p = validSquares[di, dj].transform.position;
                validSquares[di, dj].transform.position = new Vector3(p.x, p.y, 1f);
            }
            if(!_isDragged) return;
            _isDragged = false;
            if (!(currentCoords.x < 0 || currentCoords.x >= 8 || currentCoords.y < 0 || currentCoords.y >= 8) && !(currentCoords.x==_dragStartCoords.x && currentCoords.y==_dragStartCoords.y))
            {
                var moveI = -1;
                for (int i = 0; i < validSqrs.Count; i++)
                {
                    if (validSqrs[i].toi == currentCoords.y && validSqrs[i].toj == currentCoords.x)
                    {
                        moveI = i;
                        break;
                    }
                }

                if (moveI != -1)
                {
                game.MakeMove(validSqrs[moveI]);
                UpdateVisuals(validSqrs[moveI]);
                }
                else
                {
                    ResetPosition();
                }
            }
            else
            {
                ResetPosition();
            }
            validSqrs = null;
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

        activePieces[move.toi, move.toj] = activePieces[move.fromi, move.fromj];
        activePieces[move.fromi, move.fromj] = null;
        
        if (move.moveType == Move.MoveType.KingSideCastle)
        {
            activePieces[move.toi, move.toj - 1] = activePieces[move.toi, move.toj + 1];
            activePieces[move.toi, move.toj + 1] = null;
        }else if (move.moveType == Move.MoveType.QueenSideCastle)
        {
            activePieces[move.toi, move.toj + 1] = activePieces[move.toi, move.toj - 2];
            activePieces[move.toi, move.toj - 2] = null;
        }
        else if(move.moveType == Move.MoveType.Enpassant)
        {
            activePieces[move.fromi, move.toj] = null;
        }else if (move.moveType == Move.MoveType.PromotetoQueen)
        {
            Destroy(activePieces[move.toi, move.toj]);
            var piece = Instantiate(move.pieceColor == Piece.PieceColor.White ? wq : bq);
            activePieces[move.toi, move.toj] = piece;
        }else if (move.moveType == Move.MoveType.PromotetoBishop)
        {
            Destroy(activePieces[move.toi, move.toj]);
            var piece = Instantiate(move.pieceColor == Piece.PieceColor.White ? wb : bb);
            activePieces[move.toi, move.toj] = piece;
        }else if (move.moveType == Move.MoveType.PromotetoKnight)
        {
            Destroy(activePieces[move.toi, move.toj]);
            var piece = Instantiate(move.pieceColor == Piece.PieceColor.White ? wn : bn);
            activePieces[move.toi, move.toj] = piece;
        }else if (move.moveType == Move.MoveType.PromotetoRook)
        {
            Destroy(activePieces[move.toi, move.toj]);
            var piece = Instantiate(move.pieceColor == Piece.PieceColor.White ? wr : br);
            activePieces[move.toi, move.toj] = piece;
        }

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if(activePieces[i, j]==null) continue;
                activePieces[i, j].transform.position = board[i, j].transform.position;
            }
        }
        
        

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

        if (validSquareMat != null)
        {
            validSquareMat.color = validSquareColors;
        }
    }

    private void InitializeValues()
    {
        lightSquareMat = new Material(Shader.Find("Unlit/Color"));
        lightSquareMat.color = lightSquares;
        darkSquareMat = new Material(Shader.Find("Unlit/Color"));
        darkSquareMat.color = darkSquares;
        validSquareMat = new Material(Shader.Find("Unlit/Color"));
        validSquareMat.color = validSquareColors;
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
                var valSqr = CreateSquare(tempX, tempY, SqrSize, validSquareMat);
                valSqr.transform.position = new Vector3(valSqr.transform.position.x, valSqr.transform.position.y, 1f);
                validSquares[i, j] = valSqr;
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
                pieceObj.transform.position = new Vector3(board[i, j].transform.position.x, board[i, j].transform.position.y, -0.75f);
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
