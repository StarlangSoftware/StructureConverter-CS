using System;
using System.Collections.Generic;
using System.IO;
using AnnotatedTree;
using Classification.Instance;
using Classification.Model;
using MorphologicalAnalysis;
using Attribute = Classification.Attribute;

namespace StructureConverter.ConstituencyToDependency {
    
    public class ClassifierDependencyOracle : IDependencyOracle {

        private static List<string[]> _dataList;

        public ClassifierDependencyOracle() {
            _dataList = new List<string[]>();
            using (var source = new StreamReader("/Users/oguzkeremyildiz/Dropbox (Starlang)/nlptoolkit-c#/StructureConverter/StructureConverter/Files/ConsToDep/dataset.txt")) {
                while (!source.EndOfStream) {
                    var line = source.ReadLine();
                    _dataList.Add(line?.Split(" "));
                }
            }
        }
        
        private List<Tuple<int, int>> FindList(int length, string classInfo) {
            var list = new List<Tuple<int, int>>();
            foreach (var array in _dataList) {
                for (var j = 0; j < array.Length; j++) {
                    if (array[0].Equals(length.ToString()) && array[1].Equals(classInfo)) {
                        for (var k = 2; k < array.Length; k += 2) {
                            list.Add(new Tuple<int, int>(Int32.Parse(array[k]), Int32.Parse(array[k + 1])));
                        }
                        return list;
                    }
                }
            }
            return null;
        }
        
        private bool Contains(int i, List<Tuple<int, int>> list) {
            foreach (var tuple in list) {
                if (tuple.Item1 == i) {
                    return true;
                }
            }
            return false;
        }
        
        private int FindHeadIndex(List<Tuple<int, int>> list, int first, int last) {
            var index = -1;
            for (var i = 0; i <= System.Math.Abs(last - first); i++) {
                if (!Contains(i, list)) {
                    index = i;
                    break;
                }
            }
            return index + first;
        }
        
        private void AddHeadToDecisions(List<Decision> decisions, int index) {
            for (var i = 0; i < decisions.Count; i++) {
                if (i == 0) {
                    if (decisions[i].GetNo() > index) {
                        decisions.Insert(0, new Decision(-1, 0, null));
                        break;
                    }
                }
                if (i + 1 < decisions.Count) {
                    if (decisions[i].GetNo() < index && decisions[i + 1].GetNo() > index) {
                        decisions.Insert(i + 1, new Decision(-1, 0, null));
                        break;
                    }
                }
                if (i + 1 == decisions.Count) {
                    if (decisions[i].GetNo() < index) {
                        decisions.Insert(i + 1, new Decision(-1, 0, null));
                        break;
                    }
                }
            }
        }
        
        public List<Decision> MakeDecisions(int firstIndex, int lastIndex, List<WordNodePair> wordNodePairList, ParseNodeDrawable node, List<TreeEnsembleModel> models) { 
            var testData = new List<Attribute.Attribute>(lastIndex + 1 - firstIndex); 
            string classInfo; 
            var list = new List<Tuple<int, int>>(); 
            var decisions = new List<Decision>(); 
            for (var i = 0; i < lastIndex + 1 - firstIndex; i++) { 
                testData.Add(new Attribute.DiscreteAttribute(wordNodePairList[firstIndex + i].GetWord().GetParse().GetPos())); 
                testData.Add(new Attribute.DiscreteAttribute(wordNodePairList[firstIndex + i].GetWord().GetParse().GetRootPos())); 
                testData.Add(new Attribute.DiscreteAttribute(wordNodePairList[firstIndex + i].GetWord().GetParse().ContainsTag(MorphologicalTag.ABLATIVE).ToString())); 
                testData.Add(new Attribute.DiscreteAttribute(wordNodePairList[firstIndex + i].GetWord().GetParse().ContainsTag(MorphologicalTag.DATIVE).ToString()));
                testData.Add(new Attribute.DiscreteAttribute(wordNodePairList[firstIndex + i].GetWord().GetParse().ContainsTag(MorphologicalTag.GENITIVE).ToString()));
                testData.Add(new Attribute.DiscreteAttribute(wordNodePairList[firstIndex + i].GetWord().GetParse().ContainsTag(MorphologicalTag.NOMINATIVE).ToString()));
                testData.Add(new Attribute.DiscreteAttribute(wordNodePairList[firstIndex + i].GetWord().GetParse().ContainsTag(MorphologicalTag.ACCUSATIVE).ToString()));
                testData.Add(new Attribute.DiscreteAttribute(wordNodePairList[firstIndex + i].GetWord().GetParse().ContainsTag(MorphologicalTag.PROPERNOUN).ToString()));
            } 
            switch (lastIndex + 1 - firstIndex) { 
                case 2: 
                    classInfo = models[1].Predict(new Instance("", testData)); 
                    list = FindList(2, classInfo); 
                    break; 
                case 3: 
                    classInfo = models[2].Predict(new Instance("", testData)); 
                    list = FindList(3, classInfo); 
                    break; 
                case 4: 
                    classInfo = models[3].Predict(new Instance("", testData)); 
                    list = FindList(4, classInfo); 
                    break; 
                case 5: 
                    classInfo = models[4].Predict(new Instance("", testData)); 
                    list = FindList(5, classInfo); 
                    break; 
                case 6: 
                    classInfo = models[5].Predict(new Instance("", testData));
                    list = FindList(6, classInfo); 
                    break; 
                case 7: 
                    classInfo = models[6].Predict(new Instance("", testData)); 
                    list = FindList(7, classInfo); 
                    break; 
                default: 
                    break; 
            } 
            var headIndex = FindHeadIndex(list, firstIndex, lastIndex); 
            for (var i = 0; i < list.Count; i++) { 
                var fromWord = wordNodePairList[firstIndex + list[i].Item1].GetWord(); 
                var toWord = wordNodePairList[firstIndex + list[i].Item2].GetWord(); 
                var headWord = wordNodePairList[headIndex].GetWord(); 
                var attributes = new List<Attribute.Attribute>(29); 
                attributes.Add(new Attribute.DiscreteAttribute(fromWord.GetParse().GetPos())); 
                attributes.Add(new Attribute.DiscreteAttribute(fromWord.GetParse().GetRootPos())); 
                attributes.Add(new Attribute.DiscreteAttribute(fromWord.GetParse().ContainsTag(MorphologicalTag.ABLATIVE).ToString())); 
                attributes.Add(new Attribute.DiscreteAttribute(fromWord.GetParse().ContainsTag(MorphologicalTag.DATIVE).ToString()));
                attributes.Add(new Attribute.DiscreteAttribute(fromWord.GetParse().ContainsTag(MorphologicalTag.GENITIVE).ToString()));
                attributes.Add(new Attribute.DiscreteAttribute(fromWord.GetParse().ContainsTag(MorphologicalTag.NOMINATIVE).ToString()));
                attributes.Add(new Attribute.DiscreteAttribute(fromWord.GetParse().ContainsTag(MorphologicalTag.ACCUSATIVE).ToString()));
                attributes.Add(new Attribute.DiscreteAttribute(fromWord.GetParse().ContainsTag(MorphologicalTag.PROPERNOUN).ToString()));
                attributes.Add(new Attribute.DiscreteAttribute(toWord.GetParse().GetPos())); 
                attributes.Add(new Attribute.DiscreteAttribute(toWord.GetParse().GetRootPos())); 
                attributes.Add(new Attribute.DiscreteAttribute(toWord.GetParse().ContainsTag(MorphologicalTag.ABLATIVE).ToString())); 
                attributes.Add(new Attribute.DiscreteAttribute(toWord.GetParse().ContainsTag(MorphologicalTag.DATIVE).ToString()));
                attributes.Add(new Attribute.DiscreteAttribute(toWord.GetParse().ContainsTag(MorphologicalTag.GENITIVE).ToString()));
                attributes.Add(new Attribute.DiscreteAttribute(toWord.GetParse().ContainsTag(MorphologicalTag.NOMINATIVE).ToString()));
                attributes.Add(new Attribute.DiscreteAttribute(toWord.GetParse().ContainsTag(MorphologicalTag.ACCUSATIVE).ToString()));
                attributes.Add(new Attribute.DiscreteAttribute(toWord.GetParse().ContainsTag(MorphologicalTag.PROPERNOUN).ToString())); 
                attributes.Add(new Attribute.DiscreteAttribute(headWord.GetParse().GetPos())); 
                attributes.Add(new Attribute.DiscreteAttribute(headWord.GetParse().GetRootPos())); 
                attributes.Add(new Attribute.DiscreteAttribute(headWord.GetParse().ContainsTag(MorphologicalTag.ABLATIVE).ToString())); 
                attributes.Add(new Attribute.DiscreteAttribute(headWord.GetParse().ContainsTag(MorphologicalTag.DATIVE).ToString()));
                attributes.Add(new Attribute.DiscreteAttribute(headWord.GetParse().ContainsTag(MorphologicalTag.GENITIVE).ToString()));
                attributes.Add(new Attribute.DiscreteAttribute(headWord.GetParse().ContainsTag(MorphologicalTag.NOMINATIVE).ToString()));
                attributes.Add(new Attribute.DiscreteAttribute(headWord.GetParse().ContainsTag(MorphologicalTag.ACCUSATIVE).ToString()));
                attributes.Add(new Attribute.DiscreteAttribute(headWord.GetParse().ContainsTag(MorphologicalTag.PROPERNOUN).ToString()));
                if (fromWord.GetSemantic() == null || headWord.GetSemantic() == null) { 
                    attributes.Add(new Attribute.DiscreteAttribute("null")); 
                } else { 
                    attributes.Add(new Attribute.DiscreteAttribute(fromWord.GetSemantic().Equals(headWord.GetSemantic()).ToString())); 
                } 
                attributes.Add(new Attribute.DiscreteAttribute(node.GetData().GetName())); 
                var firstChild = "null";
                var secondChild = "null";
                var thirdChild = "null";
                if (node.NumberOfChildren() > 0) { 
                    firstChild = node.GetChild(0).GetData().GetName(); 
                } 
                if (node.NumberOfChildren() > 1) { 
                    secondChild = node.GetChild(1).GetData().GetName(); 
                } 
                if (node.NumberOfChildren() > 2) { 
                    thirdChild = node.GetChild(2).GetData().GetName(); 
                } 
                attributes.Add(new Attribute.DiscreteAttribute(firstChild)); 
                attributes.Add(new Attribute.DiscreteAttribute(secondChild)); 
                attributes.Add(new Attribute.DiscreteAttribute(thirdChild)); 
                decisions.Add(new Decision(firstIndex + list[i].Item1, list[i].Item2 - list[i].Item1, models[0].Predict(new Instance("", attributes)))); 
            } 
            AddHeadToDecisions(decisions, headIndex); 
            return decisions; 
        } 
    }
}