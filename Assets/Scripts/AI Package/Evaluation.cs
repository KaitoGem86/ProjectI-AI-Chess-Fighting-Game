namespace AIPackage{
    public class Evaluation<T> where T : Action{
        protected Enviroment<T> env;

        public virtual int Evaluate(Enviroment<T> env){
            return 0;
        }
    }
}