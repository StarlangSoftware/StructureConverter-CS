using AnnotatedTree;

namespace StructureConverter.ConstituencyToDependency {
    
    public interface IConstituencyToDependencyTreeConverter {
        AnnotatedSentence.AnnotatedSentence Convert(ParseTreeDrawable parseTree, ParserConverterType type);
    }
}