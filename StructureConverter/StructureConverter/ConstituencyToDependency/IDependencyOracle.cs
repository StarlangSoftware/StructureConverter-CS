using System.Collections.Generic;
using AnnotatedTree;
using Classification.Model;

namespace StructureConverter.ConstituencyToDependency {
    
    public interface IDependencyOracle {
        List<Decision> MakeDecisions(int firstIndex, int lastIndex, List<WordNodePair> wordNodePairList, ParseNodeDrawable node, List<TreeEnsembleModel> models);
    }
}