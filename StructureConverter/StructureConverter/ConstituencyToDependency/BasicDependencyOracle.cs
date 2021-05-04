using System.Collections.Generic;
using AnnotatedSentence;
using AnnotatedTree;
using Classification.Model;
using Dictionary.Dictionary;
using MorphologicalAnalysis;

namespace StructureConverter.ConstituencyToDependency {
    
    public class BasicDependencyOracle : IDependencyOracle {
        private string FindData(string dependent, string head, bool condition1, bool condition2, AnnotatedWord dependentWord, AnnotatedWord headWord) { 
            if (condition1 || condition2) { 
                return "PUNCT"; 
            } 
            switch (dependent) { 
                case "ADVP": 
                    if (dependentWord.GetParse().GetRootPos().Equals("VERB")) { 
                        return "ADVCL"; 
                    } 
                    if (dependentWord.GetParse().GetRootPos().Equals("NOUN")) { 
                        return "NMOD"; 
                    } 
                    return "ADVMOD"; 
                case "ADJP": 
                    switch (head) { 
                        case "NP": 
                            if (dependentWord.GetParse().GetRootPos().Equals("VERB")) { 
                                return "ACL"; 
                            } 
                            return "AMOD"; 
                    } 
                    return "ADVMOD"; 
                case "PP": 
                    switch (head) { 
                        case "NP": 
                            return "CASE"; 
                        default: 
                            if (dependentWord.GetParse() != null && dependentWord.GetParse().GetRootPos().Equals("NOUN")) { 
                                return "NMOD"; 
                            } 
                            return "ADVMOD"; 
                    } 
                case "DP": 
                    return "DET"; 
                case "NP": 
                    switch (head) { 
                        case "NP": 
                            if (dependentWord.GetParse().ContainsTag(MorphologicalTag.PROPERNOUN) && headWord.GetParse().ContainsTag(MorphologicalTag.PROPERNOUN)) { 
                                return "FLAT"; 
                            } 
                            if (dependentWord.GetSemantic() != null && headWord.GetSemantic() != null && dependentWord.GetSemantic().Equals(headWord.GetSemantic())) { 
                                return "COMPOUND"; 
                            } 
                            return "NMOD"; 
                        case "VP": 
                            if (dependentWord.GetSemantic() != null && headWord.GetSemantic() != null && dependentWord.GetSemantic().Equals(headWord.GetSemantic())) { 
                                return "COMPOUND"; 
                            } 
                            if (dependentWord.GetParse().ContainsTag(MorphologicalTag.NOMINATIVE) || dependentWord.GetParse().ContainsTag(MorphologicalTag.ACCUSATIVE)) { 
                                return "OBJ"; 
                            } 
                            return "OBL"; 
                    } 
                    return "NMOD"; 
                case "S": 
                    switch (head) { 
                        case "VP": 
                            return "CCOMP"; 
                        default: 
                            return "DEP"; 
                    } 
                case "NUM": 
                    return "NUMMOD"; 
                case "INTJ": 
                    return "DISCOURSE"; 
                case "NEG": 
                    return "NEG"; 
                case "CONJP": 
                    return "CC"; 
                default: 
                    return "DEP"; 
            } 
        }
        
        private Dictionary<string, int> SetMap() { 
            var set = new Dictionary<string, int>(); 
            set["PUNCT"] = 0;
            set["VP"] = 1;
            set["NOMP"] = 1;
            set["S"] = 2;
            set["NP"] = 2;
            set["ADJP"] = 2;
            set["ADVP"] = 2;
            set["PP"] = 3;
            set["DP"] = 4;
            set["NUM"] = 4;
            set["QP"] = 5;
            set["NEG"] = 5;
            set["CONJP"] = 5;
            set["INTJ"] = 5;
            set["WP"] = 5;
            return set;
        }

        private int GetPriority(Dictionary<string, int> map, string key) { 
            if (map.ContainsKey(key)) {
                return map[key];
            }
            return 6;
        }
        
        private int FindHeadIndex(int start, int last, List<WordNodePair> wordNodePairList) {
            var map = SetMap();
            var bestPriority = GetPriority(map, wordNodePairList[last].GetNode().GetData().GetName());
            var currentIndex = last;
            for (var i = last - 1; i >= start; i--) {
                var priority = GetPriority(map, wordNodePairList[i].GetNode().GetData().GetName());
                if (priority < bestPriority) {
                    bestPriority = priority;
                    currentIndex = i;
                }
            }
            return currentIndex;
        }
        private List<Decision> SetToAndAddUniversalDependency(int startIndex, int headIndex, List<WordNodePair> wordNodePairList, int finishIndex, ParseNodeDrawable parent) { 
            var decisions = new List<Decision>(); 
            for (var i = startIndex; i <= finishIndex; i++) { 
                if (i != headIndex) { 
                    var parentData = parent.GetData().GetName(); 
                    var firstChild = parent.GetChild(0).GetData().GetName(); 
                    string secondChild = null, thirdChild = null; 
                    if (parent.NumberOfChildren() > 1) { 
                        secondChild = parent.GetChild(1).GetData().GetName(); 
                    } 
                    if (parent.NumberOfChildren() > 2){ 
                        thirdChild = parent.GetChild(2).GetData().GetName(); 
                    } 
                    if (parent.NumberOfChildren() == 2 && parentData.Equals("S") && firstChild.Equals("NP")) { 
                        decisions.Add(new Decision(startIndex + decisions.Count, headIndex - i, "NSUBJ")); 
                    } else if (parent.NumberOfChildren() == 3 && parentData.Equals("S") && firstChild.Equals("NP") && secondChild.Equals("VP") && Word.IsPunctuation(thirdChild)) { 
                        if (!wordNodePairList[i].GetWord().IsPunctuation()) { 
                            decisions.Add(new Decision(startIndex + decisions.Count, headIndex - i, "NSUBJ")); 
                        } else { 
                            decisions.Add(new Decision(startIndex + decisions.Count, headIndex - i, "PUNCT")); 
                        } 
                    } else { 
                        var dependent = wordNodePairList[i].GetNode().GetData().GetName(); 
                        var head = wordNodePairList[headIndex].GetNode().GetData().GetName(); 
                        var condition1 = wordNodePairList[i].GetNode().GetData().IsPunctuation(); 
                        var condition2 = wordNodePairList[headIndex].GetNode().GetData().IsPunctuation(); 
                        decisions.Add(new Decision(startIndex + decisions.Count, headIndex - i, FindData(dependent, head, condition1, condition2, wordNodePairList[i].GetWord(), wordNodePairList[headIndex].GetWord()))); 
                    } 
                } else { 
                    decisions.Add(new Decision(-1, 0, null)); 
                } 
            } 
            return decisions; 
        }
        
        public List<Decision> MakeDecisions(int firstIndex, int lastIndex, List<WordNodePair> wordNodePairList, ParseNodeDrawable node, List<TreeEnsembleModel> models) {
            if (node.NumberOfChildren() == 3 && node.GetChild(1).GetData().GetName().Equals("CONJP")) {
                var decisions = new List<Decision>();
                decisions.Add(new Decision(-1, 0, null));
                decisions.Add(new Decision((lastIndex + firstIndex) / 2, lastIndex - ((lastIndex + firstIndex) / 2), "CC"));
                decisions.Add(new Decision(lastIndex, firstIndex - lastIndex, "CONJ"));
                return decisions;
            }
            var headIndex = FindHeadIndex(firstIndex, lastIndex, wordNodePairList);
            return SetToAndAddUniversalDependency(firstIndex, headIndex, wordNodePairList, lastIndex, node);
        }
    }
}