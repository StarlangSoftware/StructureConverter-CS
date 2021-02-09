using System.Collections.Generic;
using AnnotatedSentence;
using AnnotatedTree;

namespace StructureConverter.DependencyToConstituency {
    
    public class DependencyToConstituencyTreeBank {
        private readonly AnnotatedCorpus _annotatedCorpus;

        public DependencyToConstituencyTreeBank(AnnotatedCorpus annotatedCorpus) {
            _annotatedCorpus = annotatedCorpus;
        }

        public TreeBankDrawable Convert(IDependencyToConstituencyTreeConverter dependencyToConstituencyTreeConverter) {
            var parseTrees = new List<ParseTree.ParseTree>();
            for (var i = 0; i < _annotatedCorpus.SentenceCount(); i++){
                parseTrees.Add(dependencyToConstituencyTreeConverter.Convert((AnnotatedSentence.AnnotatedSentence) _annotatedCorpus.GetSentence(i), ParserConverterType.BasicOracle));
            }
            return new TreeBankDrawable(parseTrees);
        }
    }
}