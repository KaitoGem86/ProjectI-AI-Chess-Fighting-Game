using System.Collections.Generic;
using AIPackage;
using Chess;
using UnityEngine;

namespace AIRefactoring
{
    public class MoveOrdering : ActionOrdering<Move>
    {
        const int squareControlledByOpponentPawnPenalty = 350;
        const int capturedPieceValueMultiplier = 10;
        public MoveOrdering(ActionGenerator<Move> actionGenerator, TranspositionTable<Move> transpositionTable, Move invalidAction) : base(actionGenerator, transpositionTable, invalidAction)
        {
            maxActionCount = 218;
            actionScores = new int[maxActionCount];
        }

        public override void OrderActions(Enviroment<Move> env, List<Move> actions, bool useTT)
        {
            Move hashMove = invalidAction;
            if (useTT)
            {
                hashMove = transpositionTable.GetStoredAction();
            }
            Debug.Log(actions.Count);
            for (int i = 0; i < actions.Count; i++)
            {
                int score = 0;
                int movePieceType = Piece.PieceType(env.Square[actions[i].StartSquare]);
                Debug.Log("movePieceType : " + movePieceType);
                int capturePieceType = Piece.PieceType(env.Square[actions[i].TargetSquare]);
                Debug.Log("capturePieceType : " + capturePieceType);
                int flag = actions[i].MoveFlag;

                if (capturePieceType != Piece.None)
                {
                    // Order moves to try capturing the most valuable opponent piece with least valuable of own pieces first
                    // The capturedPieceValueMultiplier is used to make even 'bad' captures like QxP rank above non-captures
                    score = capturedPieceValueMultiplier * GetPieceValue(capturePieceType) - GetPieceValue(movePieceType);
                }
                if (movePieceType == Piece.Pawn)
                {

                    if (flag == Move.Flag.PromoteToQueen)
                    {
                        score += Evaluation.queenValue;
                    }
                    else if (flag == Move.Flag.PromoteToKnight)
                    {
                        score += Evaluation.knightValue;
                    }
                    else if (flag == Move.Flag.PromoteToRook)
                    {
                        score += Evaluation.rookValue;
                    }
                    else if (flag == Move.Flag.PromoteToBishop)
                    {
                        score += Evaluation.bishopValue;
                    }
                }
                else
                {
                    Debug.Log("move type: " + movePieceType.ToString());

                    // Penalize moving piece to a square attacked by opponent pawn
                    if (BitBoardUtility.ContainsSquare(((MoveGenerator)actionGenerator).opponentPawnAttackMap, actions[i].TargetSquare))
                    {
                        score -= squareControlledByOpponentPawnPenalty;
                    }
                    Debug.Log("score : " + score);
                }
                if (Action.SameAction(actions[i], hashMove))
                {
                    score += 10000;
                }
                Debug.Log("check Action " + i + " : " + actions[i].ActionValue + " score : " + score + " hashMove : " + hashMove.ActionValue);

                actionScores[i] = score;
            }


            Sort(actions);
        }

        static int GetPieceValue(int pieceType)
        {
            switch (pieceType)
            {
                case Piece.Queen:
                    return Evaluation.queenValue;
                case Piece.Rook:
                    return Evaluation.rookValue;
                case Piece.Knight:
                    return Evaluation.knightValue;
                case Piece.Bishop:
                    return Evaluation.bishopValue;
                case Piece.Pawn:
                    return Evaluation.pawnValue;
                default:
                    return 0;
            }
        }
    }
}