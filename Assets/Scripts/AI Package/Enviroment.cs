using System.Collections.Generic;

namespace AIPackage
{
    public abstract class Enviroment<T> where T : Action
    {
        public ulong ZobristKey { get; protected set; }
        public Stack<ulong> RepetitionPositionHistory;


        //game doi khang co 2 doi thu danh voi nhau => 2 mau 
        public const int WhiteIndex = 0;
        public const int BlackIndex = 1;

        public int[] Square;

        public bool WhiteToMove;
        public int ColourToMove;
        public int OpponentColour;
        public int ColourToMoveIndex;

        public uint currentGameState;
        public int plyCount;

        public abstract void MakeAction(T action, bool inSearch = false);
        public abstract void UnmakeAction(T action, bool inSearch = false);
        public abstract void LoadStartState();
        public abstract void Initialize();
    }
}