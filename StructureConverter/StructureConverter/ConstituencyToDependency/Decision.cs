namespace StructureConverter.ConstituencyToDependency {
    
    public class Decision {
        
        private readonly int _to;
        private readonly string _data;

        public Decision(int to, string data) {
            _to = to;
            _data = data;
        }

        public int GetTo() {
            return _to;
        }

        public string GetData() {
            return _data;
        }
    }
}