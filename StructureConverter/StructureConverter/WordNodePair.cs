using AnnotatedSentence;
using AnnotatedTree;
using ParseTree;

namespace StructureConverter {
    public class WordNodePair {
        
        private readonly AnnotatedWord _annotatedWord; 
        private ParseNodeDrawable _node; 
        private readonly int _no; 
        private bool _done; 
        private bool _finished; 
        
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
            _done = false; 
            _finished = false; 
        }
        
        public WordNodePair(ParseNodeDrawable parseNodeDrawable, int no) { 
            _node = parseNodeDrawable; 
            _annotatedWord = new AnnotatedWord(parseNodeDrawable.GetLayerData()); 
            _done = false; 
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
        
        public bool IsDone() { 
            return _done; 
        }
        
        public bool IsFinished() { 
            return _finished; 
        }
        
        public void SetDone() { 
            _done = true; 
        }
        
        public void SetFinish() { 
            _finished = true; 
        }
        
        public string GetTreePos() { 
            return _annotatedWord.GetParse().GetTreePos();
        }
        public string GetUniversalDependency() { 
            return _annotatedWord.GetUniversalDependency().ToString(); 
        }
        
        public bool Equals(WordNodePair wordNodePair) { 
            return _annotatedWord.Equals(wordNodePair._annotatedWord) && _no == wordNodePair._no && _done == wordNodePair._done; 
        }
    }
}