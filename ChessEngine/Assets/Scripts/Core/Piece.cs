namespace Core
{
    public class Piece
    {

        public enum PieceType
        {
            None,
            Pawn,
            Bishop,
            Knight,
            Rook,
            Queen,
            King
        }

        public enum PieceColor
        {
            None,
            White,
            Black
        }
        
        private bool _isNull = true;
        private PieceType  _pieceType;
        private PieceColor _pieceColor;

        public Piece()
        {
            _isNull = true;
            _pieceColor = PieceColor.None;
            _pieceType = PieceType.None;
        }
        
        public Piece(PieceColor color, PieceType piece)
        {
            _isNull = false;
            _pieceColor = color;
            _pieceType = piece;
        }

        public bool Exists()
        {
            return _isNull == false;
        }
        
        public PieceColor GetPieceColor()
        {
            if (_isNull) return PieceColor.None;
            return this._pieceColor;
        }

        public PieceType GetPieceType()
        {
            if (_isNull) return PieceType.None;
            return this._pieceType;
        }
        
        
    }
}