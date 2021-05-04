using System;
using System.Collections.Generic;
using System.IO;
using Attribute = Classification.Attribute;
using Classification.Instance;
using Classification.Model;
using MorphologicalAnalysis;

namespace StructureConverter.DependencyToConstituency {
    
    public class ClassifierOracle : ProjectionOracle {
        
        private static List<string[]> _dataList;

        public ClassifierOracle() {
            _dataList = new List<string[]>();
            using (var source = new StreamReader("/Users/oguzkeremyildiz/Dropbox (Starlang)/nlptoolkit-c#/StructureConverter/StructureConverter/Files/DepToCons/dataset.txt")) {
                while (!source.EndOfStream) {
                    var line = source.ReadLine();
                    _dataList.Add(line?.Split(" "));
                }
            }
        }
        
        private List<Attribute.Attribute> SetTestData(List<WordNodePair> unionList, int currentIndex) {
            var array = new Attribute.Attribute[(unionList.Count * 8) + ((unionList.Count - 1) * 3)];
            var iterate = 0;
            for (var i = 0; i < unionList.Count; i++) {
                array[i * 8] = new Attribute.DiscreteAttribute(unionList[i].GetWord().GetParse().GetPos());
                array[(i * 8) + 1] = new Attribute.DiscreteAttribute(unionList[i].GetWord().GetParse().GetRootPos());
                array[(i * 8) + 2] = new Attribute.DiscreteAttribute(unionList[i].GetWord().GetParse().ContainsTag(MorphologicalTag.ABLATIVE).ToString());
                array[(i * 8) + 3] = new Attribute.DiscreteAttribute(unionList[i].GetWord().GetParse().ContainsTag(MorphologicalTag.DATIVE).ToString());
                array[(i * 8) + 4] = new Attribute.DiscreteAttribute(unionList[i].GetWord().GetParse().ContainsTag(MorphologicalTag.GENITIVE).ToString());
                array[(i * 8) + 5] = new Attribute.DiscreteAttribute(unionList[i].GetWord().GetParse().ContainsTag(MorphologicalTag.NOMINATIVE).ToString());
                array[(i * 8) + 6] = new Attribute.DiscreteAttribute(unionList[i].GetWord().GetParse().ContainsTag(MorphologicalTag.ACCUSATIVE).ToString());
                array[(i * 8) + 7] = new Attribute.DiscreteAttribute(unionList[i].GetWord().GetParse().ContainsTag(MorphologicalTag.PROPERNOUN).ToString());
                if (i != currentIndex) {
                    array[(unionList.Count * 8) + (3 * iterate)] = new Attribute.DiscreteAttribute(i.ToString());
                    array[(unionList.Count * 8) + (3 * iterate) + 1] = new Attribute.DiscreteAttribute(currentIndex.ToString());
                    array[(unionList.Count * 8) + (3 * iterate) + 2] = new Attribute.DiscreteAttribute(unionList[i].GetUniversalDependency());
                    iterate++;
                }
            }
            return new List<Attribute.Attribute>(array);
        }

        private List<Tuple<Command, string>> FindList(Dictionary<string, double> classInfo, int headIndex, List<WordNodePair> unionList, string currentPos) {
            var best = new List<Tuple<Command, string>>();
            double bestValue = Int32.MinValue;
            var listMap = new Dictionary<List<Tuple<Command, string>>, double>();
            foreach (var array in _dataList) {
                if (Int32.Parse(array[0]) == unionList.Count && classInfo.ContainsKey(array[1]) && Int32.Parse(array[2]) == headIndex) {
                    var list = new List<Tuple<Command, string>>();
                    for (var j = 3; j < array.Length; j++) {
                        if (array[j].Equals("MERGE")) {
                            list.Add(new Tuple<Command, string>(Command.Merge, SetTreePos(unionList, currentPos)));
                        } else if (array[j].Equals("RIGHT")) {
                            list.Add(new Tuple<Command, string>(Command.Right, null));
                        } else {
                            list.Add(new Tuple<Command, string>(Command.Left, null));
                        }
                    }
                    listMap[list] = classInfo[array[1]];
                }
            }
            foreach (var key in listMap.Keys) {
                if (listMap[key] > bestValue) { 
                    best = key; 
                    bestValue = listMap[key];
                }
            }
            return best;
        }

        public override List<Tuple<Command, string>> MakeCommands(List<WordNodePair> unionList, int currentIndex, List<TreeEnsembleModel> models) {
            var testData = SetTestData(unionList, currentIndex); 
            Dictionary<string, double> classInfo; 
            switch (unionList.Count) {
                case 2: 
                    var list = new List<Tuple<Command, string>>(); 
                    if (currentIndex == 1) { 
                        list.Add(new Tuple<Command, string>(Command.Left, null)); 
                    } else { 
                        list.Add(new Tuple<Command, string>(Command.Right, null)); 
                    } 
                    list.Add(new Tuple<Command, string>(Command.Merge, SetTreePos(unionList, unionList[currentIndex].GetTreePos()))); 
                    return list; 
                case 3: 
                    classInfo = models[0].PredictProbability(new Instance("", testData)); 
                    return FindList(classInfo, currentIndex, unionList, unionList[currentIndex].GetTreePos());  
                case 4: 
                    classInfo = models[1].PredictProbability(new Instance("", testData)); 
                    return FindList(classInfo, currentIndex, unionList, unionList[currentIndex].GetTreePos()); 
                case 5: 
                    classInfo = models[2].PredictProbability(new Instance("", testData)); 
                    return FindList(classInfo, currentIndex, unionList, unionList[currentIndex].GetTreePos()); 
                case 6: 
                    classInfo = models[3].PredictProbability(new Instance("", testData)); 
                    return FindList(classInfo, currentIndex, unionList, unionList[currentIndex].GetTreePos()); 
                case 7: 
                    classInfo = models[4].PredictProbability(new Instance("", testData)); 
                    return FindList(classInfo, currentIndex, unionList, unionList[currentIndex].GetTreePos()); 
                case 8: 
                    classInfo = models[5].PredictProbability(new Instance("", testData)); 
                    return FindList(classInfo, currentIndex, unionList, unionList[currentIndex].GetTreePos()); 
                default: 
                    break; 
            } 
            return null; 
        } 
    }
}