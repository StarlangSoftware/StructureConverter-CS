using System.Collections.Generic;
using AnnotatedTree;
using Classification.Model;

namespace StructureConverter.ConstituencyToDependency {
    
    public interface IConstituencyToDependencyTreeConverter {
        AnnotatedSentence.AnnotatedSentence Convert(ParseTreeDrawable parseTree, List<TreeEnsembleModel> models);
    }
}