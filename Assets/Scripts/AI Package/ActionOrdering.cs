using System.Collections.Generic;
using Chess;

namespace AIPackage{
    public class ActionOrdering<T> where T : Action{
        protected int[] actionScores;
        protected int maxActionCount;
        protected ActionGenerator<T> actionGenerator;
        protected TranspositionTable<T> transpositionTable;
        protected T invalidAction;
        public ActionOrdering(ActionGenerator<T> actionGenerator, TranspositionTable<T> transpositionTable, T invalidAction){
            this.actionGenerator = actionGenerator;
            this.transpositionTable = transpositionTable;
            this.invalidAction = invalidAction;
        }

        public virtual void OrderActions(Enviroment<T> env, List<T> actions, bool useTT){
            
        }

        protected void Sort (List<T> moves) {
			// Sort the moves list based on scores
			for (int i = 0; i < moves.Count - 1; i++) {
				for (int j = i + 1; j > 0; j--) {
					int swapIndex = j - 1;
					if (actionScores[swapIndex] < actionScores[j]) {
						(moves[j], moves[swapIndex]) = (moves[swapIndex], moves[j]);
						(actionScores[j], actionScores[swapIndex]) = (actionScores[swapIndex], actionScores[j]);
					}
				}
			}
		}
    }
}