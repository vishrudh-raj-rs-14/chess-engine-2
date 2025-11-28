using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core
{
    public class GameManager
    {
        private Piece[,] _board = new Piece[8,8];

        private GameState _curState;

        private Stack<GameState> states;
        

        public GameManager(string FEN)
        {
            states = new Stack<GameState>();
            _board = new Piece[8, 8];
            _curState = new GameState();

            string[] sections = FEN.Trim().Split(' ');

            // --- PART 1: PIECE PLACEMENT ---
            string piecePlacement = sections[0];
            int row = 0;
            int col = 0;
            for (int i = 0; i < piecePlacement.Length; i++)
            {
                char c = piecePlacement[i];
                if (c == '/')
                {
                    row++;
                    col = 0;
                }
                else if (char.IsDigit(c))
                {
                    col += (int)char.GetNumericValue(c);
                }
                else
                {
                    Piece.PieceColor color = char.IsUpper(c) ? Piece.PieceColor.White : Piece.PieceColor.Black;
                    Piece.PieceType type = Piece.PieceType.None;
                    switch (char.ToLower(c))
                    {
                        case 'p': type = Piece.PieceType.Pawn; break;
                        case 'r': type = Piece.PieceType.Rook; break;
                        case 'n': type = Piece.PieceType.Knight; break;
                        case 'b': type = Piece.PieceType.Bishop; break;
                        case 'q': type = Piece.PieceType.Queen; break;
                        case 'k': type = Piece.PieceType.King; break;
                    }
                    _board[row, col] = new Piece(color, type);
                    col++;
                }
            }

            // --- PART 2: ACTIVE COLOR ---
            // 'w' = White, 'b' = Black
            _curState.turn = (sections[1] == "w") ? Piece.PieceColor.White : Piece.PieceColor.Black;

            // --- PART 3: CASTLING RIGHTS ---
            // Reset to false first
            _curState.canCastleKingSide[Piece.PieceColor.White] = false;
            _curState.canCastleQueenSide[Piece.PieceColor.White] = false;
            _curState.canCastleKingSide[Piece.PieceColor.Black] = false;
            _curState.canCastleQueenSide[Piece.PieceColor.Black] = false;

            if (sections[2] != "-")
            {
                if (sections[2].Contains("K")) _curState.canCastleKingSide[Piece.PieceColor.White] = true;
                if (sections[2].Contains("Q")) _curState.canCastleQueenSide[Piece.PieceColor.White] = true;
                if (sections[2].Contains("k")) _curState.canCastleKingSide[Piece.PieceColor.Black] = true;
                if (sections[2].Contains("q")) _curState.canCastleQueenSide[Piece.PieceColor.Black] = true;
            }

            // --- PART 4: EN PASSANT TARGET ---
            // Example: "e3" means the file is 'e' (4). "-" means none.
            if (sections[3] == "-")
            {
                _curState.enPassantFile = -1;
            }
            else
            {
                // Convert 'a'->0, 'b'->1, etc.
                char fileChar = sections[3][0]; 
                _curState.enPassantFile = fileChar - 'a';
            }
            
            states.Push(new GameState(_curState));
        }

        public List<Move> GenerateAllValidMoves()
        {
            List<Move> validMoves = new List<Move>();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (_board[i, j] != null && _board[i, j].GetPieceColor() == _curState.turn)
                    {
                        validMoves.AddRange(GenerateValidMoves(new Vector2Int(j, i)));
                    }
                }
            }

            return validMoves;

        }

        public List<Move> GenerateValidMoves(Vector2Int sqr)
        {
            if (_board[sqr.y, sqr.x] == null) return new List<Move>();
            var piece = _board[sqr.y, sqr.x];
            var dir = piece.GetPieceColor() == Piece.PieceColor.White ? -1 : 1;
            if (piece.GetPieceColor() != _curState.turn) return new List<Move>();
            var validMoves = new List<Move>();
            if (piece.GetPieceType() == Piece.PieceType.Pawn)
            {
                if (_board[sqr.y + dir, sqr.x] == null)
                {
                    if (sqr.y + dir == 0 || sqr.y + dir == 7)
                    {
                        validMoves.Add(new Move(sqr.y, sqr.x, sqr.y + dir, sqr.x, Move.MoveType.PromotetoBishop));
                        validMoves.Add(new Move(sqr.y, sqr.x, sqr.y + dir, sqr.x, Move.MoveType.PromotetoKnight));
                        validMoves.Add(new Move(sqr.y, sqr.x, sqr.y + dir, sqr.x, Move.MoveType.PromotetoRook));
                        validMoves.Add(new Move(sqr.y, sqr.x, sqr.y + dir, sqr.x, Move.MoveType.PromotetoQueen));
                    }
                    else
                    {
                        validMoves.Add(new Move(sqr.y, sqr.x, sqr.y + dir, sqr.x));
                    }
                }

                if ((dir == -1 && sqr.y == 6) || (dir == 1 && sqr.y == 1))
                {
                    if (_board[sqr.y + dir, sqr.x] == null && _board[sqr.y + 2 * dir, sqr.x] == null)
                    {
                        validMoves.Add(new Move(sqr.y, sqr.x, sqr.y + 2*dir, sqr.x));
                    }
                }

                if (sqr.x > 0 && _board[sqr.y + dir, sqr.x - 1] != null &&
                    _board[sqr.y + dir, sqr.x - 1].GetPieceColor() != piece.GetPieceColor())
                {
                    validMoves.Add(new Move(sqr.y, sqr.x, sqr.y + dir, sqr.x-1));
                }
                
                if (sqr.x < 7 && _board[sqr.y + dir, sqr.x + 1] != null &&
                    _board[sqr.y + dir, sqr.x + 1].GetPieceColor() != piece.GetPieceColor())
                {
                    validMoves.Add(new Move(sqr.y, sqr.x, sqr.y + dir, sqr.x+1));
                }

                int enpassantRow = (_curState.turn == Piece.PieceColor.White) ? 3 : 4;
                if (_curState.enPassantFile != -1 && sqr.y == enpassantRow)
                {
                    if (sqr.x + 1 == _curState.enPassantFile)
                    {
                        validMoves.Add(new Move(sqr.y, sqr.x, sqr.y + dir, sqr.x + 1, Move.MoveType.Enpassant));
                    }else if (sqr.x - 1 == _curState.enPassantFile)
                    {
                        validMoves.Add(new Move(sqr.y, sqr.x, sqr.y + dir, sqr.x - 1, Move.MoveType.Enpassant));
                    }
                }
                
            }

            if (piece.GetPieceType() == Piece.PieceType.Queen || piece.GetPieceType() == Piece.PieceType.Rook)
            {
                GenerateFileRowMoves(sqr, validMoves);
            }
            
            
            if (piece.GetPieceType() == Piece.PieceType.Queen || piece.GetPieceType() == Piece.PieceType.Bishop)
            {
                GenerateDiagonalMoves(sqr, validMoves);
            }

            if (piece.GetPieceType() == Piece.PieceType.King)
            {
                for (int i = sqr.y - 1; i <= sqr.y + 1; i++)
                {
                    for (int j = sqr.x - 1; j <= sqr.x + 1; j++)
                    {
                        if(i<0 || j<0 || i>=8 || j>=8 || (sqr.x==j && sqr.y==i)) continue;
                        if(_board[i, j]!=null && _board[i, j].GetPieceColor()==piece.GetPieceColor()) continue;
                        validMoves.Add(new Move(sqr.y, sqr.x, i, j));
                    }
                }

                if (_curState.canCastleKingSide[_curState.turn])
                {
                    var clear = true;
                    if (_board[sqr.y, sqr.x + 1] != null || _board[sqr.y, sqr.x + 2] != null)
                    {
                        clear = false;
                    }
                    
                    if (isSqrUnderAttack(sqr, _curState.turn==Piece.PieceColor.White?Piece.PieceColor.Black:Piece.PieceColor.White)) clear = false;

                    if (isSqrUnderAttack(new Vector2Int(sqr.x + 1, sqr.y), _curState.turn==Piece.PieceColor.White?Piece.PieceColor.Black:Piece.PieceColor.White) ||
                        isSqrUnderAttack(new Vector2Int(sqr.x + 2, sqr.y), _curState.turn==Piece.PieceColor.White?Piece.PieceColor.Black:Piece.PieceColor.White))
                    {
                        clear = false;
                    }

                    if (clear)
                    {
                        validMoves.Add(new Move(sqr.y, sqr.x, sqr.y, sqr.x+2, Move.MoveType.KingSideCastle));
                    }
                }
                if (_curState.canCastleQueenSide[_curState.turn])
                {
                    var clear = true;
                    if (_board[sqr.y, sqr.x - 1] != null || _board[sqr.y, sqr.x - 2] != null || _board[sqr.y, sqr.x - 3]!=null)
                    {
                        clear = false;
                    }

                    if (isSqrUnderAttack(new Vector2Int(sqr.x - 1, sqr.y), _curState.turn==Piece.PieceColor.White?Piece.PieceColor.Black:Piece.PieceColor.White) ||
                        isSqrUnderAttack(new Vector2Int(sqr.x - 2, sqr.y), _curState.turn==Piece.PieceColor.White?Piece.PieceColor.Black:Piece.PieceColor.White))
                    {
                        clear = false;
                    }
                    
                    if (isSqrUnderAttack(sqr, _curState.turn==Piece.PieceColor.White?Piece.PieceColor.Black:Piece.PieceColor.White)) clear = false;

                    if (clear)
                    {
                        validMoves.Add(new Move(sqr.y, sqr.x, sqr.y, sqr.x-2, Move.MoveType.QueenSideCastle));
                    }
                }
                
            }
            
            if (piece.GetPieceType() == Piece.PieceType.Knight)
            {
                Vector2Int[] offsets = 
                {
                    new Vector2Int(1, 2), new Vector2Int(2, 1),
                    new Vector2Int(1, -2), new Vector2Int(2, -1),
                    new Vector2Int(-1, 2), new Vector2Int(-2, 1),
                    new Vector2Int(-1, -2), new Vector2Int(-2, -1)
                };

                foreach (var offset in offsets)
                {
                    int targetX = sqr.x + offset.x;
                    int targetY = sqr.y + offset.y;

                    if (targetX < 0 || targetX >= 8 || targetY < 0 || targetY >= 8) continue;
                    var targetPiece = _board[targetY, targetX];
                    if (targetPiece == null || targetPiece.GetPieceColor() != piece.GetPieceColor())
                    {
                        validMoves.Add(new Move(sqr.y, sqr.x, targetY, targetX));
                    }
                }
            }

            var kingsq = new Vector2Int();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if(_board[i, j]==null) continue;
                    if(_board[i, j].GetPieceColor()!=_curState.turn) continue;
                    if (_board[i, j].GetPieceType() == Piece.PieceType.King)
                    {
                        kingsq = new Vector2Int(j, i);
                        break;
                    }
                    
                }
            }
            
            var filteredMoves = new List<Move>();
            for (int i = 0; i < validMoves.Count; i++)
            {
                MakeMove(validMoves[i]);
                if (piece.GetPieceType() == Piece.PieceType.King)
                {
                    if (!isSqrUnderAttack(new Vector2Int(validMoves[i].toj, validMoves[i].toi), _curState.turn))
                    {   
                        filteredMoves.Add(validMoves[i]);
                    }
                }
                else
                {
                    if (!isSqrUnderAttack(kingsq,  _curState.turn))
                    {
                        filteredMoves.Add(validMoves[i]);
                    }
                }
                UndoMove(validMoves[i]);
            }
            
            return filteredMoves;

        }

        public bool isSqrUnderAttack(Vector2Int sqr, Piece.PieceColor color)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if(sqr.x==j && sqr.y==i) continue;
                    if(_board[i, j]==null) continue;
                    if(_board[i, j].GetPieceColor() != color) continue;
                    var piece = _board[i, j];
                    if (piece.GetPieceType() == Piece.PieceType.Pawn)
                    {
                        var dir = piece.GetPieceColor() == Piece.PieceColor.White ? -1 : 1;
                        if (Math.Abs(sqr.x - j) == 1 && i + dir == sqr.y)
                        {
                            return true;
                        }
                    }
                    if (piece.GetPieceType() == Piece.PieceType.Rook ||
                              piece.GetPieceType() == Piece.PieceType.Queen)
                    {
                        if (sqr.x == j)
                        {
                            bool nothing = true;
                            for (int y = sqr.y; y != i; y += (i > sqr.y) ? 1 : -1)
                            {
                                if (sqr.y == y) continue;
                                if (_board[y, j] != null)
                                {
                                    nothing = false;
                                    break;
                                }
                            }
                            
                            if (nothing)
                            {
                                
                                return true;
                            }
                            
                        }else if (sqr.y == i)
                        {
                            bool nothing = true;
                            for (int x = sqr.x; x != j; x += (j > sqr.x) ? 1 : -1)
                            {
                                if (sqr.x == x) continue;
                                if (_board[i, x] != null)
                                {
                                    nothing = false;
                                    break;
                                }
                            }

                            if (nothing)
                            {
                                return true;
                            }
                        }
                    }
                    if (piece.GetPieceType()==Piece.PieceType.Bishop || piece.GetPieceType()==Piece.PieceType.Queen)
                    {
                        if (i + j == sqr.x + sqr.y || i - j == sqr.y - sqr.x) 
                        {
                            var yDir = (sqr.y > i) ? 1 : -1;
                            var xDir = (sqr.x > j) ? 1 : -1;
                            var nothing = true;
                            for (int x = j + xDir, y = i + yDir; x != sqr.x && y != sqr.y; x += xDir, y += yDir)
                            {
                                if (_board[y, x] != null)
                                {
                                    nothing = false;
                                    break;
                                }
                            }
                            if (nothing)
                            {
                                return true;
                            }
                            
                        }
                    }
                    if (piece.GetPieceType() == Piece.PieceType.Knight)
                    {
                        if (Math.Abs(sqr.x - j) * Math.Abs(sqr.y - i) == 2) 
                        {
                            
                            return true;
                        }
                        
                    }
                    if (piece.GetPieceType() == Piece.PieceType.King)
                    {
                        if (Math.Abs(sqr.x - j) <= 1 && Math.Abs(sqr.y - i) <= 1)
                        {
                            
                            return true;
                        }
                        
                    }
                }
            }

            return false;
        }
        
        
        
        
        public void GenerateFileRowMoves(Vector2Int sqr, List<Move> moves)
        {
            for (int i = sqr.x+1; i < 8; i++)
            {
                if (_board[sqr.y, i] != null)
                {
                    if (_board[sqr.y, i].GetPieceColor() != _board[sqr.y, sqr.x].GetPieceColor())
                    {
                        moves.Add(new Move(sqr.y, sqr.x, sqr.y, i));
                    }
                    break;
                }
                moves.Add(new Move(sqr.y, sqr.x, sqr.y, i));
            }
            for (int i = sqr.x-1; i >=0 ; i--)
            {
                if (_board[sqr.y, i] != null)
                {
                    if (_board[sqr.y, i].GetPieceColor() != _board[sqr.y, sqr.x].GetPieceColor())
                    {
                        moves.Add(new Move(sqr.y, sqr.x, sqr.y, i));
                    }
                    break;
                }
                moves.Add(new Move(sqr.y, sqr.x, sqr.y, i));
            }
            for (int i = sqr.y+1; i < 8; i++)
            {
                if (_board[i, sqr.x] != null) 
                {
                    if (_board[i, sqr.x].GetPieceColor() != _board[sqr.y, sqr.x].GetPieceColor())
                    {
                        moves.Add(new Move(sqr.y, sqr.x, i, sqr.x));
                    }
                    break;
                }
                moves.Add(new Move(sqr.y, sqr.x, i, sqr.x));
            }
            for (int i = sqr.y-1; i >=0 ; i--)
            {
                if (_board[i, sqr.x] != null)
                {
                    if (_board[i, sqr.x].GetPieceColor() != _board[sqr.y, sqr.x].GetPieceColor())
                    {
                        moves.Add(new Move(sqr.y, sqr.x, i, sqr.x));
                    }
                    break;
                }
                moves.Add(new Move(sqr.y, sqr.x, i, sqr.x));
            }
        }

        public void GenerateDiagonalMoves(Vector2Int sqr, List<Move> moves)
        {
            for (int x = sqr.x+1, y=sqr.y+1; x < 8 && y < 8 && x>=0 && y>=0; x++, y++)
            {
                if (_board[y, x] != null)
                {
                    if (_board[y, x].GetPieceColor() != _board[sqr.y, sqr.x].GetPieceColor())
                    {
                        moves.Add(new Move(sqr.y, sqr.x, y, x));
                    }
                    break;
                }
                moves.Add(new Move(sqr.y, sqr.x, y, x));
            }
            for (int x = sqr.x+1, y=sqr.y-1; x < 8 && y < 8 && x>=0 && y>=0; x++, y--)
            {
                if (_board[y, x] != null)
                {
                    if (_board[y, x].GetPieceColor() != _board[sqr.y, sqr.x].GetPieceColor())
                    {
                        moves.Add(new Move(sqr.y, sqr.x, y, x));
                    }
                    break;
                }
                moves.Add(new Move(sqr.y, sqr.x, y, x));
            }
            for (int x = sqr.x-1, y=sqr.y+1; x < 8 && y < 8 && x>=0 && y>=0; x--, y++)
            {
                if (_board[y, x] != null)
                {
                    if (_board[y, x].GetPieceColor() != _board[sqr.y, sqr.x].GetPieceColor())
                    {
                        moves.Add(new Move(sqr.y, sqr.x, y, x));
                    }
                    break;
                }
                moves.Add(new Move(sqr.y, sqr.x, y, x));
            }
            for (int x = sqr.x-1, y=sqr.y-1; x < 8 && y < 8 && x>=0 && y>=0; x--, y--)
            {
                if (_board[y, x] != null)
                {
                    if (_board[y, x].GetPieceColor() != _board[sqr.y, sqr.x].GetPieceColor())
                    {
                        moves.Add(new Move(sqr.y, sqr.x, y, x));
                    }
                    break;
                }
                moves.Add(new Move(sqr.y, sqr.x, y, x));
            }
        }

        public void MakeMove(Move move)
        {
            if(move.toi==move.fromi && move.toj==move.fromj) return;
            var piece = _board[move.fromi, move.fromj];
            if (piece.GetPieceType() == Piece.PieceType.King)
            {
                _curState.canCastleKingSide[_curState.turn] = false;
                _curState.canCastleQueenSide[_curState.turn] = false;
            }

            if (piece.GetPieceType() == Piece.PieceType.Rook)
            {
                int row = _curState.turn == Piece.PieceColor.White ? 7 : 0;
                if (move.fromj == 0 && move.fromi==row)
                {
                    _curState.canCastleQueenSide[_curState.turn] = false;
                }
                else if(move.fromj==7 && move.fromi==row)
                {
                    _curState.canCastleKingSide[_curState.turn] = false;
                }
            }

            if (piece.GetPieceType() == Piece.PieceType.Pawn && Math.Abs(move.fromi - move.toi) == 2)
            {
                _curState.enPassantFile = move.fromj;
            }
            else
            {
                _curState.enPassantFile = -1;
            }
            _curState.capturedPiece = _board[move.toi, move.toj];
            if (_curState.capturedPiece!=null && _curState.capturedPiece.GetPieceType() == Piece.PieceType.Rook)
            {
                int row = _curState.turn == Piece.PieceColor.White ? 0 : 7;
                if (move.toi == row && move.toj == 7)
                {
                    _curState.canCastleKingSide[
                            _curState.turn == Piece.PieceColor.White
                                ? Piece.PieceColor.Black
                                : Piece.PieceColor.White] =
                        false;
                }
                if (move.toi == row && move.toj == 0)
                {
                    _curState.canCastleQueenSide[
                            _curState.turn == Piece.PieceColor.White
                                ? Piece.PieceColor.Black
                                : Piece.PieceColor.White] =
                        false;
                }
            }
            _board[move.toi, move.toj] = piece;
            _board[move.fromi, move.fromj] = null;
            if (move.moveType == Move.MoveType.KingSideCastle)
            {
                _board[move.toi, move.toj - 1] = _board[move.toi, move.toj + 1];
                _board[move.toi, move.toj + 1] = null;
            }else if (move.moveType == Move.MoveType.QueenSideCastle)
            {
                _board[move.toi, move.toj + 1] = _board[move.toi, move.toj - 2];
                _board[move.toi, move.toj - 2] = null;
            }
            else if(move.moveType == Move.MoveType.Enpassant)
            {
                _curState.capturedPiece = _board[move.fromi, move.toj];
                    _board[move.fromi, move.toj] = null;
            }else if (move.moveType == Move.MoveType.PromotetoQueen)
            {
                _board[move.toi, move.toj].SetPieceType(Piece.PieceType.Queen);
            }else if (move.moveType == Move.MoveType.PromotetoBishop)
            {
                _board[move.toi, move.toj].SetPieceType(Piece.PieceType.Bishop);
            }else if (move.moveType == Move.MoveType.PromotetoKnight)
            {
                _board[move.toi, move.toj].SetPieceType(Piece.PieceType.Knight);
            }else if (move.moveType == Move.MoveType.PromotetoRook)
            {
                _board[move.toi, move.toj].SetPieceType(Piece.PieceType.Rook);
            }

            if (piece.GetPieceType() == Piece.PieceType.Pawn ||  _curState.capturedPiece!=null)
            {
                _curState.noOfMoves = 0;
            }
            else
            {
                _curState.noOfMoves++;
            }

            _curState.turn = (_curState.turn == Piece.PieceColor.White) ? Piece.PieceColor.Black : Piece.PieceColor.White;
            
            states.Push(new GameState(_curState));

        }

        public void UndoMove(Move move)
        {
            if(move.toi==move.fromi && move.toj==move.fromj) return;
            _board[move.fromi, move.fromj] = _board[move.toi, move.toj] ;
            _board[move.toi, move.toj] = _curState.capturedPiece;
            
            if (move.moveType == Move.MoveType.KingSideCastle)
            {
                _board[move.toi, move.toj + 1] = _board[move.toi, move.toj - 1];
                _board[move.toi, move.toj - 1] = null;
            }else if (move.moveType == Move.MoveType.QueenSideCastle)
            {
                _board[move.toi, move.toj - 2] = _board[move.toi, move.toj + 1];
                _board[move.toi, move.toj + 1] = null;
            }
            else if(move.moveType == Move.MoveType.Enpassant)
            {
                _board[move.fromi, move.toj] = _curState.capturedPiece;
                _board[move.toi, move.toj] = null;
            }else if (move.moveType == Move.MoveType.PromotetoQueen || move.moveType == Move.MoveType.PromotetoBishop || move.moveType == Move.MoveType.PromotetoKnight || move.moveType == Move.MoveType.PromotetoRook )
            {
                _board[move.fromi, move.fromj].SetPieceType(Piece.PieceType.Pawn);
            }

            states.Pop();
            _curState = new GameState(states.Peek());

        }
        
        
        public Piece[,] GetBoard()
        {
            return _board;
        }
        
        public enum GameResult { Playing, Checkmate, Stalemate, FiftyMoveRule }

    public GameResult CheckGameStatus()
    {
        if (_curState.noOfMoves >= 100) return GameResult.FiftyMoveRule;

        var moves = GenerateAllValidMoves();
        
        if (moves.Count > 0) return GameResult.Playing;

        Vector2Int kingPos = new Vector2Int(-1, -1);
        for (int i = 0; i < 8; i++) {
            for (int j = 0; j < 8; j++) {
                if (_board[i, j] != null && 
                    _board[i, j].GetPieceType() == Piece.PieceType.King && 
                    _board[i, j].GetPieceColor() == _curState.turn) 
                {
                    kingPos = new Vector2Int(j, i);
                    break;
                }
            }
        }

        if (isSqrUnderAttack(kingPos, _curState.turn == Piece.PieceColor.White ? Piece.PieceColor.Black : Piece.PieceColor.White))
        {
            return GameResult.Checkmate;
        }
        
        return GameResult.Stalemate;
    }

    public long Perft(int depth)
    {
        if (depth == 0) return 1;

        var moves = GenerateAllValidMoves();
        long nodes = 0;

        foreach (var move in moves)
        {
            MakeMove(move);
            _curState.noOfMoves = 0;
            nodes += Perft(depth - 1);
            UndoMove(move);
        }

        return nodes;
    }

    public string PerftDivide(int depth)
    {
        var moves = GenerateAllValidMoves();
        long totalNodes = 0;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (var move in moves)
        {
            MakeMove(move);
            long nodes = Perft(depth - 1);
            UndoMove(move);
            
            totalNodes += nodes;
            // Format: e2e4: 20
            sb.AppendLine($"{MoveToString(move)}: {nodes}");
        }
        sb.AppendLine($"Total Nodes: {totalNodes}");
        return sb.ToString();
    }

    private string MoveToString(Move m)
    {
        string files = "abcdefgh";
        return $"{files[m.fromj]}{m.fromi + 1}{files[m.toj]}{m.toi + 1}";
    }
        
    }
}