using System.Collections.Generic;
using AnnotatedTree;

namespace StructureConverter.ConstituencyToDependency {
    
    public interface IDependencyOracle {
        List<Decision> MakeDecisions(int firstIndex, int lastIndex, List<WordNodePair> wordNodePairList, ParseNodeDrawable node);
    }
}