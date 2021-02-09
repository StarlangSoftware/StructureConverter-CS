namespace StructureConverter.DependencyToConstituency {
    
    public interface IDependencyToConstituencyTreeConverter {
        ParseTree.ParseTree Convert(AnnotatedSentence.AnnotatedSentence annotatedSentence, ParserConverterType type);
    }
}