using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Chess.Peices
{

    public enum PSide { White, Black} 
    public enum PType { King, Queen, Rook, Bishop, Knight, Pawn }

    public enum TileTypes { free, blocked, enemy}
    public enum MoveTypes { standard, enpass, castle, checktest}

    public struct Move //Structure representing a Piece's move
    {
        //type-dependant constructors
        public Move(Point movpoint_init, MoveTypes movtype_init)
        { movPoint = movPoint = movpoint_init; moveType = movtype_init; ensidePoint = Point.Empty; rookPoint = Point.Empty; newrookPoint = Point.Empty; } //Standard       
        public Move(Point movpoint_init, MoveTypes movtype_init, Point ensidepoint_init)
        { movPoint = movPoint = movpoint_init; moveType = movtype_init; ensidePoint = ensidepoint_init; rookPoint = Point.Empty; newrookPoint = Point.Empty; } //En Passant
        public Move(Point movpoint_init, MoveTypes movtype_init, Point rookpoint_init, Point newrookpoint_init)
        { movPoint = movPoint = movpoint_init; moveType = movtype_init; ensidePoint = Point.Empty; rookPoint = rookpoint_init; newrookPoint = newrookpoint_init; } //Castle

        //standard move vars
        public Point movPoint;
        public MoveTypes moveType;

        //non-standard move vars
        public Point ensidePoint;
        public Point rookPoint;
        public Point newrookPoint;
    } 

    public class Peice
    {
        private ChessGame myGame;

        //accessible properties
        public PType MyType { get { return myType; } }
        public PSide MySide { get { return mySide; } }
        public Point MyLocation { get { return GetLocation(myGame.Gameboard); } }

        private PType myType;
        private PSide mySide;

        //meta-fields
        private bool firstmove = true;
        private int lastmove = 0;



        //possible moves for piece on current turn
        public List<Move> myMoves = new List<Move>(); 
        internal int UpdateMoves()
        {
            myMoves.Clear();

            List<Move> rawmoves = new List<Move>();
            rawmoves.Add(new Move(MyLocation, MoveTypes.checktest)); //Add probability of no-movement to test checkmate
            rawmoves.AddRange(GetRawMoves(myGame.Gameboard));

            foreach (Move move in rawmoves)
            {
                bool valid = true;
                if (move.moveType == MoveTypes.checktest) { valid = false; }

                //Clone board for tested move
                Peice[,] cloneboard = (Peice[,])myGame.Gameboard.Clone();

                //Peform test move      
                ChessGame.movePeice(cloneboard, MyLocation, move);

                //Ensure the move doesn't place self into check
                Point myKingPoint = ChessGame.getKing(cloneboard, mySide);

                List<Peice> altsideList = ChessGame.getPeices(cloneboard, ChessGame.altSide(mySide));
                //Get moves of all alternate peices should move take place
                foreach (Peice peice in altsideList)
                {
                    List<Move> altMoves = peice.GetRawMoves(cloneboard);
                    if (move.moveType == MoveTypes.checktest) { myGame.inattacksquares[(int)mySide].UnionWith(altMoves); }

                    if (altMoves.Any(mov => mov.movPoint == myKingPoint))
                    {
                        if (move.moveType == MoveTypes.checktest) { myGame.ischecked[(int)mySide] = true; } //Test for check
                        valid = false; break;
                    }
                }

                if (valid == true) { myMoves.Add(move); }
            }

            return myMoves.Count;
        }


        public Peice(PType myType_init, PSide mySide_init, ChessGame myGame_init)
        {
            myType = myType_init; mySide = mySide_init; myGame = myGame_init;
            myGame.PieceList.Add(this);
        }

        internal void ChangeType(PType myType_init)
        { myType = myType_init; }

        internal void AfterMove()
        {
            firstmove = false;
            lastmove = myGame.turn_count;
        }


        private static Point[] knight_offsets = new Point[] { new Point(1, 2), new Point(2, 1), new Point(2, -1), new Point(1, -2), new Point(-1, -2), new Point(-2, -1), new Point(-2, 1), new Point(-1, 2) };
        private static Point[] king_offsets = new Point[] { new Point(0, 1), new Point(1, 0), new Point(0, -1), new Point(-1, 0), new Point(1, 1), new Point(1, -1), new Point(-1, -1), new Point(-1, 1) };
        private static Point[] quadoffsets = new Point[] { new Point(1, 1), new Point(1, -1), new Point(-1, -1), new Point(-1, 1) };

        //Methods returning un-filtered moves for piece types
        private List<Move> GetRawMoves(Peice[,] board)
        {
            switch (myType) //Assign appropriate move function
            {
                case (PType.King): { return GetRawMoves_King(board); }
                case (PType.Queen): { return GetRawMoves_Queen(board); }
                case (PType.Rook): { return GetRawMoves_Rook(board); }
                case (PType.Bishop): { return GetRawMoves_Bishop(board); }
                case (PType.Knight): { return GetRawMoves_Knight(board); }
                case (PType.Pawn): { return GetRawMoves_Pawn(board); }
            }
            return null;
        }

        private List<Move> GetRawMoves_King(Peice[,] board)
        {
            Point location = GetLocation(board); 

            List<Move> validmoves = new List<Move>();
            foreach (Point offset in king_offsets)
            {
                Point newloc = new Point(location.X + offset.X, location.Y + offset.Y);
                TileTypes moveType = GetTileType(board, newloc);

                if (moveType != TileTypes.blocked) { validmoves.Add(new Move(newloc, MoveTypes.standard)); }
            }

            //Castling 
            if(firstmove && !myGame.ischecked[(int)mySide]) //Conditional, ensure king first move and not in check
            {
                Point[] CastlePoints = { new Point(0, (int)mySide * 7), new Point(7, (int)mySide * 7) }; //Get peices at both corners of side
                foreach(Point rookPoint in CastlePoints)
                {
                    Peice rook = board[rookPoint.X, rookPoint.Y];
                    if(rook != null && rook.mySide == mySide && rook.myType == PType.Rook && rook.firstmove == true) //Conditional peice is side rook, no prior movements
                    {
                        bool directpath = true;
                        int dir = Math.Sign(rookPoint.X - location.X);
                        //Ensure all peices between rook and king are clear, and aren't in line of fire/attacked
                        for(int xoffset = dir; board[location.X + xoffset, location.Y] != rook; xoffset += dir)
                        {
                            Point testloc = new Point(location.X + xoffset, location.Y);
                            if(GetTileType(board, testloc) != TileTypes.free || myGame.inattacksquares[(int)mySide].Any(attack => attack.movPoint == testloc))
                            { directpath = false; break; }
                        }
                        Point newloc = new Point(location.X + dir * 2, location.Y);
                        Point newrookloc = new Point(newloc.X + dir * -1,location.Y);
                        if (directpath) { validmoves.Add(new Move(newloc, MoveTypes.castle, rookPoint, newrookloc)); }
                    }              
                }

            }

            return validmoves;
        }

        private List<Move> GetRawMoves_Queen(Peice[,] board)
        {
            List<Move> validmoves = new List<Move>();
            //Queen moves represnted by combination of rook and bishop
            validmoves.AddRange(GetRawMoves_Rook(board)); 
            validmoves.AddRange(GetRawMoves_Bishop(board));

            return validmoves;
        }

        private List<Move> GetRawMoves_Rook(Peice[,] board)
        {
            Point location = GetLocation(board);

            List<Move> validmoves = new List<Move>();
            for (int i = -1; true; i = 1) //Axis direction
            {
                //X-axis movement
                for (int xoffset = i; true; xoffset += i)
                {
                    Point newloc = new Point(location.X + xoffset, location.Y);

                    TileTypes moveType = GetTileType(board, newloc);
                    if (moveType == TileTypes.free) { validmoves.Add(new Move(newloc, MoveTypes.standard)); }
                    else if (moveType == TileTypes.enemy) { validmoves.Add(new Move(newloc, MoveTypes.standard)); break; }
                    else { break; } //blocked
                }

                //Y-axis movement
                for (int yoffset = i; true; yoffset += i)
                {
                    Point newloc = new Point(location.X, location.Y + yoffset);

                    TileTypes moveType = GetTileType(board, newloc);
                    if (moveType == TileTypes.free) { validmoves.Add(new Move(newloc, MoveTypes.standard)); }
                    else if (moveType == TileTypes.enemy) { validmoves.Add(new Move(newloc, MoveTypes.standard)); break; }
                    else { break; } //blocked

                }

                if (i == 1) { break; }
            }

            return validmoves;
        }

        private List<Move> GetRawMoves_Bishop(Peice[,] board)
        {
            Point location = GetLocation(board);

            List<Move> validmoves = new List<Move>();
            foreach (Point quad in quadoffsets) //Iterate through quadrant-movemetns for diagonals
            {
                Point offset = quad;
                while (true)
                {
                    Point newloc = new Point(location.X + offset.X, location.Y + offset.Y);
                    TileTypes moveType = GetTileType(board, newloc);
                    if (moveType == TileTypes.free) { validmoves.Add(new Move(newloc, MoveTypes.standard)); }
                    else if (moveType == TileTypes.enemy) { validmoves.Add(new Move(newloc, MoveTypes.standard)); break; }
                    else { break; } //blocked

                    offset.X += quad.X; offset.Y += quad.Y; //Increment offsets in specific direction
                }
            }

            return validmoves;
        }

        private List<Move> GetRawMoves_Knight(Peice[,] board)
        {
            Point location = GetLocation(board);

            List<Move> validmoves = new List<Move>();
            foreach (Point offset in knight_offsets)
            {
                Point newloc = new Point(location.X + offset.X, location.Y + offset.Y);

                TileTypes moveType = GetTileType(board, newloc);
                if (moveType != TileTypes.blocked) { validmoves.Add(new Move(newloc, MoveTypes.standard)); };
            }

            return validmoves;
        }

        private List<Move> GetRawMoves_Pawn(Peice[,] board)
        {
            Point location = GetLocation(board);

            List<Move> validmoves = new List<Move>();

            int yoffset = 1; if (mySide == PSide.Black) { yoffset = -1; } //Side indicated forward direction

            //Take diaganols
            for (int i = 1; true; i = -1) //X-Axis direction
            {
                Point newloc = new Point(location.X + i, location.Y + yoffset);

                TileTypes moveType = GetTileType(board, newloc);
                if (moveType == TileTypes.enemy)
                { validmoves.Add(new Move(newloc, MoveTypes.standard)); }

                //En Passant
                else if(moveType == TileTypes.free) //diagonal free
                {
                    //Get side piece
                    Point sideloc = new Point(newloc.X, location.Y);
                    Peice sidePeice = board[sideloc.X, sideloc.Y];
                    //Rules: 1)Piece type of pawn 2)Pawn first moved last turn 3)Pawn moved two-spaces
                    if (GetTileType(board, sideloc) == TileTypes.enemy && sidePeice.lastmove == myGame.turn_count - 1 && sideloc.Y == (int)(sidePeice.mySide) * 7 + (yoffset * -3))
                    {
                        validmoves.Add(new Move(newloc, MoveTypes.enpass, sideloc));
                    }
                }

                if (i == -1) { break; }
            }

            //Forward directional movement
            for (int i = 1; i <= 2; i++)
            {
                Point newloc = new Point(location.X, location.Y + (yoffset * i));
                TileTypes moveType = GetTileType(board, newloc);

                if (moveType == TileTypes.free) { validmoves.Add(new Move(newloc, MoveTypes.standard)); }
                if (!firstmove || moveType != TileTypes.free) { break; }
            }

            return validmoves;
        }




        private Point GetLocation(Peice[,] board)
        {
            for(int x = 0; x < 8; x++)
            {
                for(int y = 0; y < 8; y++)
                {
                    if (board[x, y] == this) { return new Point(x, y); }
                }
            }
            throw new Exception();
        } //Gets piece location on board

        private TileTypes GetTileType(Peice[,] board, Point newloc)
        {
            if (ChessGame.inBounds(newloc))
            {
                if (board[newloc.X, newloc.Y] == null) return TileTypes.free;
                if(board[newloc.X, newloc.Y].mySide != mySide) return TileTypes.enemy; 
            }
            return TileTypes.blocked;
        } //Gets tile type of new position for move
    }

}
