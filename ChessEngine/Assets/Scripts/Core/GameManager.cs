using System.Linq;
using UnityEngine;

namespace Core
{
    public class GameManager
    {
        private Piece[,] _board = new Piece[8,8];

        public GameManager(string FEN)
        {
            int i = 0;
            int j = 0;
            for (int x = 0; x < FEN.Length; x++)
            {
                if (FEN[x] == '/')
                {
                    i++;
                    j = 0;
                    continue;
                }
                if (char.IsDigit(FEN[x]))
                {
                    j += FEN[x] - '0';
                    continue;
                }

                Piece.PieceColor color = char.IsLower(FEN[x]) ? Piece.PieceColor.Black : Piece.PieceColor.White;
                Piece.PieceType type = Piece.PieceType.None;
                char t = char.ToLower(FEN[x]);
                if (t == 'r') type = Piece.PieceType.Rook;
                else if (t == 'b') type = Piece.PieceType.Bishop;
                else if (t == 'n') type = Piece.PieceType.Knight;
                else if (t == 'p') type = Piece.PieceType.Pawn;
                else if (t == 'q') type = Piece.PieceType.Queen;
                else type = Piece.PieceType.King;
                _board[i, j] = new Piece(color, type);
                j++;
                
            }
        }

        public void MakeMove(Move move)
        {
            if(move.toi==move.fromi && move.toj==move.fromj) return;
            _board[move.toi, move.toj] = _board[move.fromi, move.fromj];
            _board[move.fromi, move.fromj] = null;
        }
        
        public Piece[,] GetBoard()
        {
            return _board;
        }
        
    }
}