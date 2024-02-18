using AIPackage;
using Chess;
using UnityEngine;

namespace AIRefactoring {
    public class Search : Search<Move> {
        public Search(Board board, AISettings settings) : base(board, settings, Move.InvalidMove) {
        }

        protected override void SetEvaluation()
        {
            Debug.Log("Setting evaluation");            
            evaluation = new EvaluationBoard();
        }

        protected override void SetActionGenerator()
        {
            Debug.Log("Setting action generator");
            actionGenerator = new MoveGenerator();
        }

        protected override void SetActionordering(ActionGenerator<Move> actionGenerator, TranspositionTable<Move> tt, Move invalidAction)
        {
            Debug.Log("Setting action ordering");
            actionOrdering = new MoveOrdering(actionGenerator, tt, invalidAction);
        }
    }
}