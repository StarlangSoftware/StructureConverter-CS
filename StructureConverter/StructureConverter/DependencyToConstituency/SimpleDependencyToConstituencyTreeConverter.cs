using System;
using System.Collections.Generic;
using AnnotatedSentence;
using AnnotatedTree;
using Classification.Model;
using ParseTree;

namespace StructureConverter.DependencyToConstituency {
    
    public class SimpleDependencyToConstituencyTreeConverter : IDependencyToConstituencyTreeConverter {

        private List<WordNodePair> ConstructWordPairList(AnnotatedSentence.AnnotatedSentence sentence, String fileName) {
            var wordNodePairs = new List<WordNodePair>();
            for (var i = 0; i < sentence.WordCount(); i++) {
                var annotatedWord1 = (AnnotatedWord) sentence.GetWord(i);
                if (annotatedWord1.GetParse() == null) {
                    throw new MorphologicalAnalysisNotExistsException(fileName);
                }
                if (annotatedWord1.GetUniversalDependency() == null) {
                    throw new UniversalDependencyNotExistsException(fileName);
                }
                var toWord1 = annotatedWord1.GetUniversalDependency().To() - 1;
                wordNodePairs.Add(new WordNodePair(annotatedWord1, i));
                for (var j = 0; j < sentence.WordCount(); j++) {
                    if (i == j){
                        continue;
                    }
                    var annotatedWord2 = (AnnotatedWord) sentence.GetWord(j);
                    if (annotatedWord2.GetUniversalDependency() == null) {
                        throw new UniversalDependencyNotExistsException(fileName);
                    }
                    var toWord2 = annotatedWord2.GetUniversalDependency().To() - 1;
                    if (i > j) {
                        if (toWord2 > i && toWord1 > toWord2) {
                            throw new NonProjectiveDependencyException(fileName);
                        }
                    } else {
                        if (toWord1 > j && toWord1 < toWord2) {
                            throw new NonProjectiveDependencyException(fileName);
                        }
                    }
                }
            }
            return wordNodePairs;
        }
        
        private bool  NoIncomingNodes(List<WordNodePair> wordList, int i) {
            for (var j = 0; j < wordList.Count; j++) {
                var word = wordList[j];
                var toWord = word.GetTo() - 1;
                if (!word.IsDoneForConnect() && i != j && toWord > -1 && toWord < wordList.Count) {
                    if (wordList[i].Equals(wordList[toWord])) {
                        return false;
                    }
                }
            }
            return true;
        }
        
        private int UpdateUnionCandidateLists(List<WordNodePair> list, WordNodePair wordNodePair) {
            if (list.Count < 2) {
                if (list.Count == 1 && list[0].GetNo() > wordNodePair.GetNo()) {
                    list.Insert(0, wordNodePair);
                    return 0;
                } else {
                    list.Add(wordNodePair);
                    return list.Count - 1;
                }
            } else {
                if (list[0].GetNo() > wordNodePair.GetNo()) {
                    list.Insert(0, wordNodePair);
                    return 0;
                } else if (list[list.Count - 1].GetNo() < wordNodePair.GetNo()) {
                    list.Add(wordNodePair);
                    return list.Count - 1;
                } else {
                    for (var i = 0; i < list.Count - 1; i++) {
                        if (wordNodePair.GetNo() > list[i].GetNo() && wordNodePair.GetNo() < list[i + 1].GetNo()) {
                            list.Insert(i + 1, wordNodePair);
                            return i + 1;
                        }
                    }
                }
            }
            return -1;
        }
        
        private List<WordNodePair> SetOfNodesToBeMergedOntoNode(List<WordNodePair> wordNodePairs, WordNodePair headWord) {
            var list = new List<WordNodePair>();
            for (var i = 0; i < wordNodePairs.Count; i++) {
                var wordNodePair = wordNodePairs[i];
                var toWordIndex = wordNodePair.GetTo() - 1;
                if (!wordNodePair.IsDoneForConnect()) {
                    if (NoIncomingNodes(wordNodePairs, i) && toWordIndex == headWord.GetNo()) {
                        UpdateUnionCandidateLists(list, wordNodePair);
                    }
                }
            }
            return list;
        }
        
        private bool ContainsChild(ParseNodeDrawable parent, ParseNodeDrawable child) {
            for (var i = 0; i < parent.NumberOfChildren(); i++) {
                if (GetParent((ParseNodeDrawable) parent.GetChild(i)).Equals(GetParent(child))) {
                    return true;
                }
            }
            return false;
        }
        
        private bool AllSame(List<WordNodePair> unionList) {
            for (var i = 1; i < unionList.Count; i++) {
                if (!GetParent(unionList[i - 1].GetNode()).Equals(GetParent(unionList[i].GetNode()))) {
                    return false;
                }
            }
            return true;
        }

        private void Merge(List<WordNodePair> wordNodePairs, List<WordNodePair> unionList, int i, List<TreeEnsembleModel> models) { 
            var index = UpdateUnionCandidateLists(unionList, wordNodePairs[i]);
            ProjectionOracle oracle;
            if (models == null || unionList.Count > 8) {
                oracle = new BasicOracle();
            } else {
                oracle = new ClassifierOracle();
            }
            var list = oracle.MakeCommands(unionList, index, models); 
            var currentUnionList = new List<WordNodePair>(); 
            currentUnionList.Add(unionList[index]); 
            int leftIndex = 0, rightIndex = 0, iterate = 0; 
            while (iterate < list.Count) { 
                var command = list[iterate].Item1; 
                switch (command) { 
                    case Command.Merge:
                        var treePos = list[iterate].Item2;
                        MergeNodes(currentUnionList, treePos); 
                        currentUnionList.Clear(); 
                        currentUnionList.Add(unionList[index]); 
                        break; 
                    case Command.Left: 
                        leftIndex++; 
                        UpdateUnionCandidateLists(currentUnionList, unionList[index - leftIndex]); 
                        break; 
                    case Command.Right: 
                        rightIndex++; 
                        UpdateUnionCandidateLists(currentUnionList, unionList[index + rightIndex]); 
                        break; 
                    default: 
                        break; 
                } 
                iterate++; 
            } 
        }
        
        private void MergeNodes(List<WordNodePair> list, string treePos) {
            var parent = new ParseNodeDrawable(new Symbol(treePos));
            if (!AllSame(list)) {
                foreach (var wordNodePair in list) {
                    if (!ContainsChild(parent, wordNodePair.GetNode())) {
                        parent.AddChild(GetParent(wordNodePair.GetNode()));
                    }
                }
            }
        }

        private bool IsThereAll(Dictionary<int, List<int>> map, int current, int total) {
            return map[current].Count == total;
        }
        
        private ParseNodeDrawable GetParent(ParseNodeDrawable node) {
            if (node.GetParent() != null) {
                return GetParent((ParseNodeDrawable) node.GetParent());
            } else {
                return node;
            }
        }
        
        private Dictionary<int, List<int>> SetDependencyMap(List<WordNodePair> wordNodePairs) {
            var map = new Dictionary<int, List<int>>();
            for (var i = 0; i < wordNodePairs.Count; i++) {
                int to;
                if (wordNodePairs[i].GetTo() == 0) {
                    to = wordNodePairs.Count;
                } else {
                    to = wordNodePairs[i].GetTo();
                }
                if (!map.ContainsKey(to)) {
                    map[to] = new List<int>();
                }
                map[to].Add(i);
            }
            return map;
        }
        
        private int IsSpecialState(List<WordNodePair> unionList, List<WordNodePair> wordNodePairs, int headIndex) {
            var head = wordNodePairs[headIndex];
            if (head.GetTo() > 0 && head.GetTo() < wordNodePairs.Count && headIndex - 1 == head.GetTo()) {
                var first = wordNodePairs[head.GetTo() - 1];
                var second = wordNodePairs[head.GetTo()];
                if (!first.IsDoneForConnect() && head.GetUniversalDependency().Equals("CONJ") && second.GetUniversalDependency().Equals("CC") && second.GetTo() - 1 == headIndex) {
                    var index = UpdateUnionCandidateLists(unionList, first);
                    if (NoIncomingNodes(wordNodePairs, head.GetTo() - 1)) {
                        first.DoneForConnect();
                    }
                    return index;
                }
            }
            return -1;
        }
        
        private ParseTree.ParseTree ConstructTreeFromWords(List<WordNodePair> wordNodePairs, Dictionary<int, List<int>> dependencyMap, List<TreeEnsembleModel> models) {
            int total;
            while (true) {
                var j = 0;
                var index = -1;
                var unionList = new List<WordNodePair>();
                do {
                    var head = wordNodePairs[j];
                    if (!head.IsDoneForHead()) {
                        unionList = SetOfNodesToBeMergedOntoNode(wordNodePairs, head);
                        index = IsSpecialState(unionList, wordNodePairs, j);
                        j++;
                        if (index > -1) {
                            break;
                        } else {
                            total = unionList.Count;
                            if (dependencyMap.ContainsKey(j) && IsThereAll(dependencyMap, j, total) && (unionList.Count != 0)) {
                                break;
                            }
                        }
                    } else {
                        j++;
                    }
                } while (j < wordNodePairs.Count);
                for (var i = 0; i < unionList.Count; i++) {
                    if (i != index) {
                        unionList[i].DoneForConnect();
                    }
                }
                wordNodePairs[j - 1].DoneForHead();
                if (unionList.Count > 0) {
                    Merge(wordNodePairs, unionList, j - 1, models);
                } else {
                    break;
                }
            }
            var root = GetParent(wordNodePairs[0].GetNode());
            if (!root.GetData().Equals(new Symbol("S"))) {
                root.SetData(new Symbol("S"));
            }
            var parseNodeDrawables = FindNodes(root);
            SetTree(parseNodeDrawables);
            return new ParseTree.ParseTree(root);
        }
        
        private void SetTree(List<ParseNodeDrawable> parseNodeDrawables) {
            foreach (var p in parseNodeDrawables) {
                var child = (ParseNodeDrawable) p.GetChild(0);
                p.RemoveChild(child);
                for (var i = 0; i < child.NumberOfChildren(); i++) {
                    p.AddChild(child.GetChild(i));
                }
            }
        }
        
        private List<ParseNodeDrawable> FindNodes(ParseNodeDrawable node) {
            var list = new List<ParseNodeDrawable>();
            for (var i = 0; i < node.NumberOfChildren(); i++) {
                var child = (ParseNodeDrawable) node.GetChild(i);
                if (node.GetLayerInfo() == null) {
                    if (node.NumberOfChildren() == 1 && ((ParseNodeDrawable) node.GetChild(0)).GetLayerInfo() == null) {
                        list.Add(node);
                    }
                    list.AddRange(FindNodes(child));
                }
            }
            return list;
        }
        
        public ParseTree.ParseTree Convert(AnnotatedSentence.AnnotatedSentence annotatedSentence, List<TreeEnsembleModel> models) {
            try {
                var wordNodePairs = ConstructWordPairList(annotatedSentence, annotatedSentence.GetFileName());
                var dependencyMap = SetDependencyMap(wordNodePairs);
                if (wordNodePairs.Count > 1) {
                    return ConstructTreeFromWords(wordNodePairs, dependencyMap, models);
                } else {
                    var parent = new ParseNodeDrawable(new Symbol("S"));
                    parent.AddChild(wordNodePairs[0].GetNode());
                    return new ParseTree.ParseTree(parent);
                }
            } catch (Exception e) {
                if (e is UniversalDependencyNotExistsException || e is NonProjectiveDependencyException || e is MorphologicalAnalysisNotExistsException) {
                    Console.WriteLine(e.ToString());
                }
            }
            return null;
        }
    }
}