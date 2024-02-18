using System.Collections.Generic;

namespace AIPackage{
    public class ActionGenerator<T> where T : Action{
        protected List<T> actions;
        protected bool inCheck;
        protected bool inDoubleCheck;
        protected bool genQuiets;
        protected Enviroment<T> env;
        
        public List<T> GenerateAllActions(Enviroment<T> env, bool includeQuietMoves = true)
        {
            Prepare(env, includeQuietMoves);
            if (inDoubleCheck) {
				return actions;
			}
            CalculateAllActions(env, includeQuietMoves);
            return actions;
        }

        public bool IsInCheck()
        {
            return inCheck;
        }

        protected virtual void Prepare(Enviroment<T> env, bool includeQuietMoves = true){
            this.env = env;
            genQuiets = includeQuietMoves;
            Init();
        }

        protected virtual void Init(){}

        protected virtual void CalculateAllActions(Enviroment<T> env, bool includeQuietMoves){}
    }
}