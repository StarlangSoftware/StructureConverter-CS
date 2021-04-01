namespace StructureConverter.ConstituencyToDependency {
    
    public class Decision {
        
        private readonly int _no;
        private readonly int _to;
        private readonly string _data;

        public Decision(int no, int to, string data) {
            _no = no;
            _to = to;
            _data = data;
        }

        public int GetNo() {
            return _no;
        }

        public int GetTo() {
            return _to;
        }

        public string GetData() {
            return _data;
        }
    }
}