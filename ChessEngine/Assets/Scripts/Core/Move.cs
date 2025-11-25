namespace Core
{
    public class Move
    {
        private enum MoveType
        {
            Regular,
            KingSideCastle,
            QueenSideCastle,
            Enpassant,
            PromotetoQueen,
            PromotetoKing,
            PromotetoBishop,
            PromotetoKnight,
            PromotetoRook
        }

        private Piece.PieceColor _pieceColor;

        public int fromi;
        public int fromj;
        public int toi;
        public int toj;

        public Move(int fi, int fj, int ti, int tj)
        {
            fromi = fi;
            fromj = fj;
            toi = ti;
            toj = tj;
        }
        
        
        

    }
}