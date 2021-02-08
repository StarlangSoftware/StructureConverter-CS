using System.Collections.Generic;
using AnnotatedTree;

namespace StructureConverter.ConstituencyToDependency {
    
    public class ClassifierDependencyOracle : IDependencyOracle {
        public List<Decision> MakeDecisions(int firstIndex, int lastIndex, List<WordNodePair> wordNodePairList, ParseNodeDrawable node) {
            return null;
        }
    }
}