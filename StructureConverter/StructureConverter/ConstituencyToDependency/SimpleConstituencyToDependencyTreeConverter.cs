using System;
using System.Collections.Generic;
using AnnotatedSentence;
using AnnotatedTree;
using AnnotatedTree.Processor;
using AnnotatedTree.Processor.Condition;

namespace StructureConverter.ConstituencyToDependency {
    
    public class SimpleConstituencyToDependencyTreeConverter : IConstituencyToDependencyTreeConverter {
        
        private int FindEndingNode(int start, List<WordNodePair> wordNodePairList) {
            var i = start + 1;
            while (i < wordNodePairList.Count - 1 && wordNodePairList[i].GetNode().GetParent().Equals(wordNodePairList[i + 1].GetNode().GetParent())) {
                i++;
            }
            return i;
        }
        
        private WordNodePair ConvertParseNodeDrawableToWordNodePair(ParseNodeDrawable parseNodeDrawable, List<WordNodePair> wordNodePairList) {
            foreach (var wordNodePair in wordNodePairList) {
                if (wordNodePair.GetNode().Equals(parseNodeDrawable)) {
                    return wordNodePair;
                }
            }
            return null;
        }
        
        private void AddUniversalDependency(List<ParseNodeDrawable> parseNodeDrawableList, List<WordNodePair> wordNodePairList, IDependencyOracle oracle) { 
            for (var i = 0; i < parseNodeDrawableList.Count - 1; i++) { 
                if (parseNodeDrawableList[i].Equals(parseNodeDrawableList[i + 1])) { 
                    var last = FindEndingNode(i, wordNodePairList); 
                    if (last - i + 1 == parseNodeDrawableList[i].NumberOfChildren()) { 
                        if (parseNodeDrawableList[i].NumberOfChildren() == 3 && parseNodeDrawableList[i].GetChild(1).GetData().GetName().Equals("CONJP")) { 
                            WordNodePair first = ConvertParseNodeDrawableToWordNodePair((ParseNodeDrawable) parseNodeDrawableList[i].GetChild(0), wordNodePairList); 
                            WordNodePair second = ConvertParseNodeDrawableToWordNodePair((ParseNodeDrawable) parseNodeDrawableList[i].GetChild(1), wordNodePairList); 
                            WordNodePair third = ConvertParseNodeDrawableToWordNodePair((ParseNodeDrawable) parseNodeDrawableList[i].GetChild(2), wordNodePairList); 
                            if (first != null && second != null && third != null) { 
                                second.SetDone(); 
                                third.SetDone(); 
                                second.GetWord().SetUniversalDependency(third.GetNo(), "CC"); 
                                third.GetWord().SetUniversalDependency(first.GetNo(), "CONJ"); 
                                if (first.GetNode().GetParent() != null) { 
                                    first.UpdateNode(); 
                                    if (first.GetNode().GetParent() != null && first.GetNode().GetParent().NumberOfChildren() == 1) { 
                                        first.UpdateNode(); 
                                    } 
                                } 
                            } 
                        } else { 
                            var decisions = oracle.MakeDecisions(i, last, wordNodePairList, parseNodeDrawableList[i]); 
                            for (var j = 0; j < decisions.Count; j++) { 
                                var decision = decisions[j]; 
                                if (decision.GetTo() == 0) { 
                                    if (wordNodePairList[i + j].GetNode().GetParent() != null) { 
                                        wordNodePairList[i + j].UpdateNode(); 
                                        if (wordNodePairList[i + j].GetNode().GetParent() != null && wordNodePairList[i + j].GetNode().GetParent().NumberOfChildren() == 1) { 
                                            wordNodePairList[i + j].UpdateNode(); 
                                        } 
                                    } 
                                } else { 
                                    wordNodePairList[i + j].SetDone(); 
                                    wordNodePairList[i + j].GetWord().SetUniversalDependency(wordNodePairList[i + j + decision.GetTo()].GetNo(), decision.GetData()); 
                                } 
                            } 
                        } 
                        break; 
                    } 
                } 
            } 
        }
        
        private void ConstructDependenciesFromTree(List<WordNodePair> wordNodePairList, ParserConverterType type) {
            IDependencyOracle oracle;
            if (type.Equals(ParserConverterType.BasicOracle)) {
                oracle = new BasicDependencyOracle();
            } else {
                oracle = new ClassifierDependencyOracle();
            }
            SetRoot(wordNodePairList);
            var parseNodeDrawableList = new List<ParseNodeDrawable>();
            var wordNodePairs = new List<WordNodePair>(wordNodePairList);
            foreach (WordNodePair wordNodePair in wordNodePairList) {
                parseNodeDrawableList.Add((ParseNodeDrawable) wordNodePair.GetNode().GetParent());
            }
            while (parseNodeDrawableList.Count > 1) {
                AddUniversalDependency(parseNodeDrawableList, wordNodePairs, oracle);
                parseNodeDrawableList.Clear();
                wordNodePairs.Clear();
                foreach (WordNodePair wordNodePair in wordNodePairList) {
                    if (!wordNodePair.IsDone()) {
                        parseNodeDrawableList.Add((ParseNodeDrawable) wordNodePair.GetNode().GetParent());
                        wordNodePairs.Add(wordNodePair);
                    }
                }
            }
        }
        
        private void SetRoot(List<WordNodePair> wordNodePairList) {
            AnnotatedWord last = null;
            for (var i = 0; i < wordNodePairList.Count; i++) {
                WordNodePair wordNodePair = wordNodePairList[wordNodePairList.Count - i - 1];
                if (!wordNodePair.GetWord().IsPunctuation()) {
                    last = wordNodePair.GetWord();
                    break;
                }
            }
            if (last != null) {
                last.SetUniversalDependency(0, "ROOT");
            }
        }
        
        public AnnotatedSentence.AnnotatedSentence Convert(ParseTreeDrawable parseTree, ParserConverterType type) { 
            if (parseTree != null) {
                var annotatedSentence = new AnnotatedSentence.AnnotatedSentence();
                var nodeDrawableCollector = new NodeDrawableCollector((ParseNodeDrawable) parseTree.GetRoot(), new IsLeafNode());
                var leafList = nodeDrawableCollector.Collect();
                var wordNodePairList = new List<WordNodePair>();
                for (var i = 0; i < leafList.Count; i++) {
                    var parseNode = leafList[i];
                    var wordNodePair = new WordNodePair(parseNode, i + 1);
                    wordNodePair.UpdateNode();
                    if (wordNodePair.GetNode().GetParent() != null && wordNodePair.GetNode().GetParent().NumberOfChildren() == 1) {
                        wordNodePair.UpdateNode();
                        Console.WriteLine("check this");
                        return null;
                    }
                    annotatedSentence.AddWord(wordNodePair.GetWord());
                    wordNodePairList.Add(wordNodePair);
                }
                ConstructDependenciesFromTree(wordNodePairList, type);
                return annotatedSentence;
            }
            return null;
        }
    }
}