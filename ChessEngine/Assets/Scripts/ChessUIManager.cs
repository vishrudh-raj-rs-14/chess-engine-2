using Core;
using UnityEngine;

public class ChessUIManager : MonoBehaviour
{
    
    private float SqrSize = 1f;
    private GameObject[,] board = new GameObject[8, 8];

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
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lightSquareMat = new Material(Shader.Find("Unlit/Color"));
        lightSquareMat.color = lightSquares;
        darkSquareMat = new Material(Shader.Find("Unlit/Color"));
        darkSquareMat.color = darkSquares;
        game = new GameManager(startPositionFen);
        DrawBoard();
        SetupPieces();
    }

    // Update is called once per frame
    void Update()
    {
        lightSquareMat.color = lightSquares;
        darkSquareMat.color = darkSquares;


    }

    void DrawBoard()
    {

        Vector2 pos = transform.position;
        float startX = pos.x - 4 * SqrSize;
        float startY = pos.y + 4 * SqrSize;

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
                GameObject pieceObj = Instantiate(pref, board[i, j].transform);
                pieceObj.transform.localPosition = Vector3.zero;
            }
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
    
}
