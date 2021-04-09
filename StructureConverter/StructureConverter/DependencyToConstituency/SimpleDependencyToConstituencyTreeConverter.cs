using System;
using System.Collections.Generic;
using AnnotatedSentence;
using AnnotatedTree;
using ParseTree;

namespace StructureConverter.DependencyToConstituency {
    
    public class SimpleDependencyToConstituencyTreeConverter : IDependencyToConstituencyTreeConverter {
        
        private Dictionary<string, int> SetSpecialMap() {
            var map = new Dictionary<string, int>();
            map["COMPOUND"] = 8;
            map["AUX"] =  7;
            map["DET"] = 6;
            map["AMOD"] = 5;
            map["NUMMOD"] = 4;
            map["CASE"] = 3;
            map["CCOMP"] = 2;
            map["NEG"] = 1;
            return map;
        }
        
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
                if (!word.IsDone() && i != j && toWord > -1 && toWord < wordList.Count) {
                    if (wordList[i].Equals(wordList[toWord])) {
                        return false;
                    }
                }
            }
            return true;
        }
        
        private void UpdateUnionCandidateLists(List<WordNodePair> list, WordNodePair wordNodePair) {
            if (list.Count < 2) {
                if (list.Count == 1 && list[0].GetNo() > wordNodePair.GetNo()) {
                    list.Insert(0, wordNodePair);
                } else {
                    list.Add(wordNodePair);
                }
            } else {
                if (list[0].GetNo() > wordNodePair.GetNo()) {
                    list.Insert(0, wordNodePair);
                } else if (list[list.Count - 1].GetNo() < wordNodePair.GetNo()) {
                    list.Add(wordNodePair);
                } else {
                    for (var i = 0; i < list.Count - 1; i++) {
                        if (wordNodePair.GetNo() > list[i].GetNo() && wordNodePair.GetNo() < list[i + 1].GetNo()) {
                            list.Insert(i + 1, wordNodePair);
                            break;
                        }
                    }
                }
            }
        }
        
        private Tuple<List<WordNodePair>, bool> SetOfNodesToBeMergedOntoNode(List<WordNodePair> wordNodePairs, WordNodePair headWord) {
            var list = new List<WordNodePair>();
            var isFinished = false;
            for (var i = 0; i < wordNodePairs.Count; i++) {
                var wordNodePair = wordNodePairs[i];
                var toWord1 = wordNodePair.GetTo() - 1;
                if (!wordNodePair.IsDone()) {
                    if (NoIncomingNodes(wordNodePairs, i) && toWord1 == headWord.GetNo()) {
                        wordNodePair.SetDone();
                        UpdateUnionCandidateLists(list, wordNodePair);
                        if (!isFinished && headWord.GetTo() - 1 < wordNodePairs.Count && headWord.GetTo() - 1 > -1 && !wordNodePairs[headWord.GetTo() - 1].IsDone() && System.Math.Abs(headWord.GetTo() - headWord.GetNo()) == 1 && headWord.GetUniversalDependency().Equals("CONJ") && System.Math.Abs(wordNodePair.GetTo() - wordNodePair.GetNo()) == 2 && wordNodePair.GetUniversalDependency().Equals("CC")) {
                            if (NoIncomingNodes(wordNodePairs, headWord.GetTo() - 1)) {
                                wordNodePairs[headWord.GetTo() - 1].SetDone();
                            }
                            isFinished = true;
                            UpdateUnionCandidateLists(list, wordNodePairs[headWord.GetTo() - 1]);
                        }
                    }
                } else {
                    if (toWord1 > -1 && toWord1 == headWord.GetNo()) {
                        UpdateUnionCandidateLists(list, wordNodePair);
                    }
                }
            }
            return new Tuple<List<WordNodePair>, bool>(list, isFinished);
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

        private void Merge(List<WordNodePair> wordNodePairs, Dictionary<string, int> specialsMap, List<WordNodePair> unionList, int i, ProjectionOracle oracle) { 
            UpdateUnionCandidateLists(unionList, wordNodePairs[i]);
            var index = -1; 
            for (var j = 0; j < unionList.Count; j++) { 
                if (unionList[j].Equals(wordNodePairs[i])) { 
                    index = j; 
                    break; 
                } 
            } 
            var list = oracle.MakeCommands(specialsMap, unionList, index); 
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
        
        private ParseTree.ParseTree ConstructTreeFromWords(List<WordNodePair> wordNodePairs, Dictionary<int, List<int>> dependencyMap, ParserConverterType type) {
            ProjectionOracle oracle;
            if (type.Equals(ParserConverterType.BasicOracle)) {
                oracle = new BasicOracle();
            } else {
                oracle = new ClassifierOracle();
            }
            var specialsMap = SetSpecialMap();
            int total;
            while (true) {
                var j = 0;
                var unionList = new List<WordNodePair>();
                do {
                    if (!wordNodePairs[j].IsFinished()) {
                        var tuple = SetOfNodesToBeMergedOntoNode(wordNodePairs, wordNodePairs[j]);
                        unionList = tuple.Item1;
                        var isFinished = tuple.Item2;
                        j++;
                        total = unionList.Count;
                        if (isFinished || (dependencyMap.ContainsKey(j) && IsThereAll(dependencyMap, j, total) && (unionList.Count != 0))) {
                            break;
                        }
                    } else {
                        j++;
                    }
                    if (j == wordNodePairs.Count) {
                        break;
                    }
                } while (true);
                wordNodePairs[j - 1].SetFinish();
                if (unionList.Count > 0) {
                    Merge(wordNodePairs, specialsMap, unionList, j - 1, oracle);
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
        
        public ParseTree.ParseTree Convert(AnnotatedSentence.AnnotatedSentence annotatedSentence, ParserConverterType type) {
            try {
                var wordNodePairs = ConstructWordPairList(annotatedSentence, annotatedSentence.GetFileName());
                var dependencyMap = SetDependencyMap(wordNodePairs);
                if (wordNodePairs.Count > 1) {
                    return ConstructTreeFromWords(wordNodePairs, dependencyMap, type);
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