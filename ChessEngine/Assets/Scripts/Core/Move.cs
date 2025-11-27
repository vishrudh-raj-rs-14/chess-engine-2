namespace Core
{
    public class Move
    {
        public enum MoveType
        {
            Regular,
            KingSideCastle,
            QueenSideCastle,
            Enpassant,
            PromotetoQueen,
            PromotetoBishop,
            PromotetoKnight,
            PromotetoRook
        }

        public Piece.PieceColor pieceColor;
        public MoveType moveType;

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
        
        public Move(int fi, int fj, int ti, int tj, MoveType mType)
        {
            fromi = fi;
            fromj = fj;
            toi = ti;
            toj = tj;
            moveType = mType;
        }

        
        

    }
}