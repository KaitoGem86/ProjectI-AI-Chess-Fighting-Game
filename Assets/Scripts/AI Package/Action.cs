namespace AIPackage{
    public interface Action { //action được mã hóa bằng 
        public ushort ActionValue { get; }
        public int StartSquare { get; }
        public int TargetSquare { get; }
        public static Action InvalidAction { get; }

        public static bool SameAction(Action a, Action b) {
            return a.ActionValue == b.ActionValue;
        }

        public bool IsInvalid {
            get {
                return ActionValue == 0;
            }
        }        
    }
}