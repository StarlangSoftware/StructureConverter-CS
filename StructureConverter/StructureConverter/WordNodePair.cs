using AnnotatedSentence;
using AnnotatedTree;
using ParseTree;

namespace StructureConverter {
    public class WordNodePair {
        
        private readonly AnnotatedWord _annotatedWord; 
        private ParseNodeDrawable _node; 
        private readonly int _no; 
        private bool _doneForConnect; 
        private bool _doneForHead; 
        
        public WordNodePair(AnnotatedWord annotatedWord, int no) { 
            _annotatedWord = annotatedWord; 
            ParseNodeDrawable parent; 
            if (GetUniversalDependency().Equals("ADVMOD")) { 
                parent = new ParseNodeDrawable(new Symbol("ADVP")); 
            } else if (GetUniversalDependency().Equals("ACL")) { 
                parent = new ParseNodeDrawable(new Symbol("ADJP")); 
            } else { 
                parent = new ParseNodeDrawable(new Symbol(annotatedWord.GetParse().GetTreePos())); 
            } 
            _node = new ParseNodeDrawable(parent, annotatedWord.ToString().Replace("\\(", "-LRB-").Replace("\\)", "-RRB-"), true, 0); 
            parent.AddChild(_node); 
            _no = no; 
            _doneForConnect = false; 
            _doneForHead = false; 
        }
        
        public WordNodePair(ParseNodeDrawable parseNodeDrawable, int no) { 
            _node = parseNodeDrawable; 
            _annotatedWord = new AnnotatedWord(parseNodeDrawable.GetLayerData()); 
            _doneForConnect = false; 
            _no = no; 
        } 
        
        public string GetWordName() {
            return _annotatedWord.GetName();
        }
        
        public int GetNo() {
            return _no;
        } 
        
        public ParseNodeDrawable GetNode() { 
            return _node; 
        } 
        
        public AnnotatedWord GetWord() { 
            return _annotatedWord; 
        }
        
        public void UpdateNode() { 
            _node = (ParseNodeDrawable) _node.GetParent(); 
        } 
        
        public int GetTo() { 
            return _annotatedWord.GetUniversalDependency().To(); 
        }
        
        public bool IsDoneForConnect() { 
            return _doneForConnect; 
        }
        
        public bool IsDoneForHead() { 
            return _doneForHead; 
        }
        
        public void DoneForConnect() { 
            _doneForConnect = true; 
        }
        
        public void DoneForHead() { 
            _doneForHead = true; 
        }
        
        public string GetTreePos() { 
            return _annotatedWord.GetParse().GetTreePos();
        }
        public string GetUniversalDependency() { 
            return _annotatedWord.GetUniversalDependency().ToString(); 
        }
        
        public bool Equals(WordNodePair wordNodePair) { 
            return _annotatedWord.Equals(wordNodePair._annotatedWord) && _no == wordNodePair._no && _doneForConnect == wordNodePair._doneForConnect; 
        }
    }
}