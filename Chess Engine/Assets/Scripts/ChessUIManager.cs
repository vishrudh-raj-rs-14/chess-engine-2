using UnityEngine;

public class ChessUIManager : MonoBehaviour
{
    
    private float SqrSize = 1f;
    private GameObject[,] board = new GameObject[8, 8];

    [SerializeField]
    private Color lightSquares;

    [SerializeField] 
    private Color darkSquares;


    private Material lightSquareMat;
    private Material darkSquareMat;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lightSquareMat = new Material(Shader.Find("Unlit/Color"));
        lightSquareMat.color = lightSquares;
        darkSquareMat = new Material(Shader.Find("Unlit/Color"));
        darkSquareMat.color = darkSquares;
        DrawBoard();
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
        float startY = pos.y - 4 * SqrSize;

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

            tempY += SqrSize;
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
