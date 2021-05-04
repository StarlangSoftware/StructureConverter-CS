using System.Collections.Generic;
using Classification.Model;

namespace StructureConverter.DependencyToConstituency {
    
    public interface IDependencyToConstituencyTreeConverter {
        ParseTree.ParseTree Convert(AnnotatedSentence.AnnotatedSentence annotatedSentence, List<TreeEnsembleModel> models);
    }
}