using System.Collections.Generic;

namespace Core
{
    public class GameState
    {
        public Piece capturedPiece;
        public Piece.PieceColor turn;
        public Dictionary<Piece.PieceColor, bool> canCastleKingSide;
        public Dictionary<Piece.PieceColor, bool> canCastleQueenSide;
        public int enPassantFile;
        public int noOfMoves;

        public GameState()
        {
            noOfMoves = 0;
            capturedPiece = null;
            turn = Piece.PieceColor.White;
            canCastleKingSide = new Dictionary<Piece.PieceColor, bool>()
            {
                { Piece.PieceColor.White, true },
                { Piece.PieceColor.Black, true }
            };
            
            canCastleQueenSide = new Dictionary<Piece.PieceColor, bool>()
            {
                { Piece.PieceColor.White, true },
                { Piece.PieceColor.Black, true }
            };
            enPassantFile = -1;

        }
        
        public GameState(GameState state)
        {
            noOfMoves = state.noOfMoves;
            capturedPiece = state.capturedPiece;
            turn = state.turn;
            canCastleKingSide = new Dictionary<Piece.PieceColor, bool>(state.canCastleKingSide);
            canCastleQueenSide = new Dictionary<Piece.PieceColor, bool>(state.canCastleQueenSide);
            enPassantFile = state.enPassantFile;
        }
    }
}