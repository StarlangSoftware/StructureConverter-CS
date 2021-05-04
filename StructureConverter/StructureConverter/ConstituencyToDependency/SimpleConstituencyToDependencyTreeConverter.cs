using System;
using System.Collections.Generic;
using AnnotatedSentence;
using AnnotatedTree;
using AnnotatedTree.Processor;
using AnnotatedTree.Processor.Condition;
using Classification.Model;

namespace StructureConverter.ConstituencyToDependency {
    
    public class SimpleConstituencyToDependencyTreeConverter : IConstituencyToDependencyTreeConverter {
        
        private int FindEndingNode(int start, List<WordNodePair> wordNodePairList) {
            var i = start + 1;
            while (i < wordNodePairList.Count - 1 && wordNodePairList[i].GetNode().GetParent().Equals(wordNodePairList[i + 1].GetNode().GetParent())) {
                i++;
            }
            return i;
        }

        private void AddUniversalDependency(List<ParseNodeDrawable> parseNodeDrawableList, List<WordNodePair> wordNodePairList, List<TreeEnsembleModel> models) {
            for (var i = 0; i < parseNodeDrawableList.Count - 1; i++) {
                if (parseNodeDrawableList[i].Equals(parseNodeDrawableList[i + 1])) {
                    var last = FindEndingNode(i, wordNodePairList);
                    if (last - i + 1 == parseNodeDrawableList[i].NumberOfChildren()) {
                        IDependencyOracle oracle;
                        if (models == null || last - i + 1 > 7) {
                            oracle = new BasicDependencyOracle();
                        } else {
                            oracle = new ClassifierDependencyOracle();
                        }
                        var decisions = oracle.MakeDecisions(i, last, wordNodePairList, parseNodeDrawableList[i], models);
                        for (var j = 0; j < decisions.Count; j++) {
                            var decision = decisions[j];
                            if (decision.GetNo() < 0) {
                                if (wordNodePairList[i + j].GetNode().GetParent() != null) {
                                    wordNodePairList[i + j].UpdateNode();
                                    if (wordNodePairList[i + j].GetNode().GetParent() != null && wordNodePairList[i + j].GetNode().GetParent().NumberOfChildren() == 1) {
                                        wordNodePairList[i + j].UpdateNode();
                                    }
                                }
                            } else {
                                wordNodePairList[decision.GetNo()].DoneForConnect();
                                wordNodePairList[decision.GetNo()].GetWord().SetUniversalDependency(wordNodePairList[decision.GetNo() + decision.GetTo()].GetNo(), decision.GetData());
                            }
                        }
                        break;
                    }
                }
            } 
        }
        
        private void ConstructDependenciesFromTree(List<WordNodePair> wordNodePairList, List<TreeEnsembleModel> models) {
            SetRoot(wordNodePairList);
            var parseNodeDrawableList = new List<ParseNodeDrawable>();
            var wordNodePairs = new List<WordNodePair>(wordNodePairList);
            foreach (var wordNodePair in wordNodePairList) {
                parseNodeDrawableList.Add((ParseNodeDrawable) wordNodePair.GetNode().GetParent());
            }
            while (parseNodeDrawableList.Count > 1) {
                AddUniversalDependency(parseNodeDrawableList, wordNodePairs, models);
                parseNodeDrawableList.Clear();
                wordNodePairs.Clear();
                foreach (var wordNodePair in wordNodePairList) {
                    if (!wordNodePair.IsDoneForConnect()) {
                        parseNodeDrawableList.Add((ParseNodeDrawable) wordNodePair.GetNode().GetParent());
                        wordNodePairs.Add(wordNodePair);
                    }
                }
            }
        }
        
        private void SetRoot(List<WordNodePair> wordNodePairList) {
            AnnotatedWord last = null;
            for (var i = 0; i < wordNodePairList.Count; i++) {
                var wordNodePair = wordNodePairList[wordNodePairList.Count - i - 1];
                if (!wordNodePair.GetWord().IsPunctuation()) {
                    last = wordNodePair.GetWord();
                    break;
                }
            }
            if (last != null) {
                last.SetUniversalDependency(0, "ROOT");
            }
        }
        
        public AnnotatedSentence.AnnotatedSentence Convert(ParseTreeDrawable parseTree, List<TreeEnsembleModel> models) { 
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
                ConstructDependenciesFromTree(wordNodePairList, models);
                return annotatedSentence;
            }
            return null;
        }
    }
}