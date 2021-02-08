using AnnotatedTree;

namespace StructureConverter.ConstituencyToDependency {
    
    public class ConstituencyToDependencyTreeBank {
        
        private TreeBankDrawable _treeBank;
        
        public ConstituencyToDependencyTreeBank(TreeBankDrawable treeBank){
            _treeBank = treeBank;
        }

        public Corpus.Corpus Convert(IConstituencyToDependencyTreeConverter constituencyToDependencyTreeConverter) {
            var annotatedCorpus = new Corpus.Corpus();
            for (var i = 0; i < _treeBank.Size(); i++){
                annotatedCorpus.AddSentence(constituencyToDependencyTreeConverter.Convert(_treeBank.Get(i), ParserConverterType.BasicOracle));
            }
            return annotatedCorpus;
        }
    }
}