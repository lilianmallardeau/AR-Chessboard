using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using Chess.Peices;

namespace Chess
{
    public class ChessGame
    {

        public enum EndGames {Check, Checkmate, Stalemate, Draw} //types of endgame


        #region Settings
        private int MOVECAP = 100; //inactive turns until draw offered
        private bool PAWNTRANFORM = true; //allow pawn transformation 
        #endregion


        #region Internal properties

        internal bool active;

        internal Peice[,] Gameboard;
        internal PSide turn;

        internal bool[] ischecked = new bool[2];
        internal HashSet<Move>[] inattacksquares = new HashSet<Move>[] { new HashSet<Move>(), new HashSet<Move>() };

        internal int turn_count = 1;
        internal int inactive_count = 0;

        #endregion


        #region Internal methods
        internal static void movePeice(Peice[,] board, Point fromPoint, Move toMove)
        {
            Peice movpeice = board[fromPoint.X, fromPoint.Y];
            board[fromPoint.X, fromPoint.Y] = null;
            board[toMove.movPoint.X, toMove.movPoint.Y] = movpeice;

            switch (toMove.moveType)
            {
                case MoveTypes.enpass:
                    {
                        board[toMove.ensidePoint.X, toMove.ensidePoint.Y] = null;
                        break;
                    }
                case MoveTypes.castle:
                    {
                        Peice rookPeice = board[toMove.rookPoint.X, toMove.rookPoint.Y];
                        board[toMove.rookPoint.X, toMove.rookPoint.Y] = null;
                        board[toMove.newrookPoint.X, toMove.newrookPoint.Y] = rookPeice;
                        break;
                    }
            }
        }
        internal static Point getKing(Peice[,] board, PSide side)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (board[x, y] != null && board[x, y].MySide == side && board[x, y].MyType == Peices.PType.King) { return new Point(x, y); }
                }
            }
            throw new Exception("Searched for king on board for " + side.ToString() + " side, not present");
        }
        internal static List<Peice> getPeices(Peice[,] board, PSide side)
        {
            List<Peice> sideList = new List<Peice>();
            foreach (Peice peice in board)
            {
                if (peice != null && peice.MySide == side)
                { sideList.Add(peice); }
            }
            return sideList;
        }
        internal static bool inBounds(Point location)
        {
            if (location.X >= 0 && location.X < 8 && location.Y >= 0 && location.Y < 8) return true; return false;
        }
        internal static PSide altSide(PSide side)
        {
            if (side == PSide.White) { return PSide.Black; } else { return PSide.White; }
        }
        #endregion



        //convert notations
        public static Point? convertLoc(char col, char row)
        {
            Point num_loc = new Point((int)col - (int)'A', (int)row - (int)'1');
            if (inBounds(num_loc)) { return num_loc; } else { return null; }

        }
        public static char[] convertLoc(Point loc)
        {
            if (!inBounds(loc)) { return null; }
            return new char[] { (char)((int)'A' + loc.X), (char)((int)'1' + loc.Y) };
        }  

        public ChessGame(int MOVECAP_INIT, bool PAWNTRANSFORM_INIT)
        {
            MOVECAP = MOVECAP_INIT; PAWNTRANFORM = PAWNTRANSFORM_INIT;
            ResetBoard();
        } //game constructor

        public void ResetBoard() //set/reset board 
        {
            active = true;

            //reset game fields
            selectedpiece = null;
            ischecked = new bool[2];
            turn = PSide.White;
            turn_count = 1;

            //reset board
            PieceList.Clear();
            TakenList.Clear();
            Gameboard = new Peice[8, 8]
            {
                {new Peice(PType.Rook, PSide.White, this), new Peice(PType.Pawn, PSide.White, this),null,null,null,null,new Peice(PType.Pawn, PSide.Black, this),new Peice(PType.Rook, PSide.Black, this),},
                {new Peice(PType.Knight, PSide.White, this), new Peice(PType.Pawn, PSide.White, this),null,null,null,null,new Peice(PType.Pawn, PSide.Black, this),new Peice(PType.Knight, PSide.Black, this),},
                {new Peice(PType.Bishop, PSide.White, this), new Peice(PType.Pawn, PSide.White, this),null,null,null,null,new Peice(PType.Pawn, PSide.Black, this),new Peice(PType.Bishop, PSide.Black, this),},
                {new Peice(PType.Queen, PSide.White, this), new Peice(PType.Pawn, PSide.White, this),null,null,null,null,new Peice(PType.Pawn, PSide.Black, this),new Peice(PType.Queen, PSide.Black, this),},
                {new Peice(PType.King, PSide.White, this), new Peice(PType.Pawn, PSide.White, this),null,null,null,null,new Peice(PType.Pawn, PSide.Black, this),new Peice(PType.King, PSide.Black, this),},
                {new Peice(PType.Bishop, PSide.White, this), new Peice(PType.Pawn, PSide.White, this),null,null,null,null,new Peice(PType.Pawn, PSide.Black, this),new Peice(PType.Bishop, PSide.Black, this),},
                {new Peice(PType.Knight, PSide.White, this), new Peice(PType.Pawn, PSide.White, this),null,null,null,null,new Peice(PType.Pawn, PSide.Black, this),new Peice(PType.Knight, PSide.Black, this),},
                {new Peice(PType.Rook, PSide.White, this), new Peice(PType.Pawn, PSide.White, this),null,null,null,null,new Peice(PType.Pawn, PSide.Black, this),new Peice(PType.Rook, PSide.Black, this),},
            };

            //prepare moves for 1st turn
            foreach (Peice peice in getPeices(Gameboard, PSide.White))
            {
                peice.UpdateMoves();
            }

        } 



        #region Public events
        public delegate void moveevent(Tuple<Peice, Point> moveData);
        public delegate void pieceevent(Peice peice);
        public delegate void endgameevent(PSide side, EndGames endType);

        public delegate PType pawnevent();

        public event pawnevent GetPieceTransform;
        public event pieceevent OnPieceTaken;
        public event moveevent OnPieceMove;

        public event endgameevent OnCheck;
        public event endgameevent OnEndGame;
        #endregion


        #region Public methods+fields

        public Peice GetPeice(Point loc)
        {
            return Gameboard[loc.X, loc.Y];
        }
        public bool IsInCheck(PSide side)
        {
            return ischecked[(int)side];
        }
        public bool CanDraw()
        {
            if (inactive_count >= MOVECAP) return true; return false;
        }


        public List<Peice> PieceList = new List<Peice>();
        public List<Peice> TakenList = new List<Peice>();

        public PSide Turn { get { return turn; } }
        public Peice SelectedPiece { get { return selectedpiece; } }


        #endregion


        #region Public Input methods

        public bool SelectPeice(Point loc)
        {
            if (Gameboard[loc.X, loc.Y] != null && Gameboard[loc.X, loc.Y].MySide == turn) { selectedpiece = Gameboard[loc.X, loc.Y]; return true; } else return false;
        } //Set selected peice
        private Peice selectedpiece;

        public void Draw()
        {
            active = false;
            if (OnEndGame != null) { OnEndGame(turn, EndGames.Draw); }
        } //Draw game

        public bool MakeMove(Point toPoint)
        {
            if (selectedpiece == null) { return false; }

            if (selectedpiece.myMoves.Any(mov => mov.movPoint == toPoint)) //Is selected move for peice valid?
            {
                bool activemove = false;

                //Move from chosen point
                Move mov = GetMove(toPoint).Value;

                //Move piece
                Peice lastoccup; if (mov.moveType == MoveTypes.enpass) { lastoccup = GetPeice(mov.ensidePoint); } else { lastoccup = Gameboard[toPoint.X, toPoint.Y]; }
                movePeice(Gameboard, selectedpiece.MyLocation, mov);

                //Transform pawn
                if (selectedpiece.MyType == PType.Pawn)
                {
                    activemove = true;
                    if (PAWNTRANFORM && (int)altSide(turn) * 7 == toPoint.Y) { selectedpiece.ChangeType(GetPieceTransform()); }
                }

                //Move events
                if (OnPieceMove != null) { OnPieceMove(new Tuple<Peice, Point>(selectedpiece, toPoint)); }

                //Take events
                if (lastoccup != null)
                {
                    PieceList.Remove(lastoccup); TakenList.Add(lastoccup); activemove = true;
                    if (OnPieceTaken != null) { OnPieceTaken(lastoccup); }
                }

                //Aftermove update for piece
                selectedpiece.AfterMove();

                //Update inactive move counter
                if (!activemove) { inactive_count++; } else { inactive_count = 0; }
            }
            else { return false; }



            turn_count++;
            ischecked = new bool[2];
            inattacksquares[(int)turn].Clear();

            //Update for next turn, calculate possible moves
            turn = altSide(turn);
            int movecount = 0;
            foreach (Peice peice in getPeices(Gameboard, turn))
            {
                movecount += peice.UpdateMoves();
            }

            //Display endgame messege
            if (ischecked[(int)turn] && movecount != 0) //Checked
            { if (OnCheck != null) { OnCheck(turn, EndGames.Check); } }
            else if (ischecked[(int)turn] && movecount == 0) //Chekmated
            { if (OnEndGame != null) { OnEndGame(turn, EndGames.Checkmate); }; active = false; }
            else if (!ischecked[(int)turn] && movecount == 0) //Stalemated
            { if (OnEndGame != null) { OnEndGame(turn, EndGames.Stalemate); } active = false; }

            //Game continues for next turn
            selectedpiece = null;
            return true;
        } //Play turn, move to point

        internal Move? GetMove(Point loc)
        {
            foreach (Move move in selectedpiece.myMoves)
            {
                if (loc == move.movPoint) { return move; }
            }
            return null;
        }

        public List<Point> GetSelectedMoves()
        {
            if (selectedpiece != null)
            { return selectedpiece.myMoves.Select(p => p.movPoint).ToList(); }
            return null;
        } //Get selected piece moves

        #endregion



    }
}
