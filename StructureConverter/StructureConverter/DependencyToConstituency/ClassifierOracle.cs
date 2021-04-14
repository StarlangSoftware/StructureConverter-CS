using System;
using System.Collections.Generic;
using System.IO;
using DataStructure;

namespace StructureConverter.DependencyToConstituency {
    
    public class ClassifierOracle : ProjectionOracle {
        
        private static List<string[]> dataList;

        public ClassifierOracle() {
            dataList = new List<string[]>();
            using (var source = new StreamReader("/Users/oguzkeremyildiz/Dropbox/nlptoolkit-c#/StructureConverter/StructureConverter/Files/dataset.txt")) {
                while (!source.EndOfStream) {
                    var line = source.ReadLine();
                    dataList.Add(line?.Split(" "));
                }
            }
        }
        
        private string TestKnn(string[] testData, int pathName, int length1, int length2) { 
            var counts = new CounterHashMap<string>(); 
            var trainData = new string[length1, length2]; 
            var input = new StreamReader("/Users/oguzkeremyildiz/Dropbox/nlptoolkit-c#/StructureConverter/StructureConverter/Files/" + pathName + ".txt"); 
            for (var i = 0; i < length1; i++) { 
                var items = input.ReadLine()?.Split(" "); 
                for (var j = 0; j < length2; j++) { 
                    trainData[i, j] = items[j]; 
                }
            } 
            input.Close(); 
            var minDistance = length2 - 1; 
            for (var i = 0; i < length1; i++) { 
                var count = 0; 
                for (var j = 0; j < length2 - 1; j++){ 
                    if (!testData[j].Equals(trainData[i, j])) { 
                        count++; 
                    } 
                } 
                if (count < minDistance) { 
                    minDistance = count; 
                } 
            } 
            for (var i = 0; i < length1; i++) { 
                var count = 0; 
                for (var j = 0; j < length2 - 1; j++) { 
                    if (!testData[j].Equals(trainData[i, j])) { 
                        count++; 
                    } 
                } 
                if (count == minDistance){ 
                    counts.Put(trainData[i, length2 - 1]); 
                } 
            } 
            return counts.Max(); 
        }
        
        private List<Tuple<Command, string>> FindList(int unionListSize, int classInfo, int headIndex, List<WordNodePair> unionList, string currentPos) {
            for (var i = 0; i < dataList.Count; i++) {
                var array = dataList[i];
                if (Int32.Parse(array[0]) == unionListSize && Int32.Parse(array[1]) == classInfo && Int32.Parse(array[2]) == headIndex) {
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
                    return list;
                }
            }
            return null;
        }
        
        public override List<Tuple<Command, string>> MakeCommands(Dictionary<string, int> specialsMap, List<WordNodePair> unionList, int currentIndex) {
            var testData = new string[unionList.Count + ((unionList.Count - 1) * 3)];
            int iterate = 0, classInfo;
            for (var i = 0; i < unionList.Count; i++) {
                testData[i] = unionList[i].GetWord().GetParse().GetPos();
                if (i != currentIndex) {
                    testData[unionList.Count + (3 * iterate)] = i.ToString();
                    testData[unionList.Count + (3 * iterate) + 1] = currentIndex.ToString();
                    testData[unionList.Count + (3 * iterate) + 2] = unionList[i].GetUniversalDependency();
                    iterate++;
                }
            }
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
                    classInfo = Int32.Parse(TestKnn(testData, unionList.Count, 11239, 10));
                    return FindList(3, classInfo, currentIndex, unionList, unionList[currentIndex].GetTreePos());
                case 4:
                    classInfo = Int32.Parse(TestKnn(testData, unionList.Count, 6360, 14));
                    return FindList(4, classInfo, currentIndex, unionList, unionList[currentIndex].GetTreePos());
                case 5:
                    classInfo = Int32.Parse(TestKnn(testData, unionList.Count, 4265, 18));
                    return FindList(5, classInfo, currentIndex, unionList, unionList[currentIndex].GetTreePos());
                case 6:
                    classInfo = Int32.Parse(TestKnn(testData, unionList.Count, 2115, 22));
                    return FindList(6, classInfo, currentIndex, unionList, unionList[currentIndex].GetTreePos());
                case 7:
                    classInfo = Int32.Parse(TestKnn(testData, unionList.Count, 814, 26));
                    return FindList(7, classInfo, currentIndex, unionList, unionList[currentIndex].GetTreePos());
                case 8:
                    classInfo = Int32.Parse(TestKnn(testData, unionList.Count, 221, 30));
                    return FindList(8, classInfo, currentIndex, unionList, unionList[currentIndex].GetTreePos());
                default:
                    break;
            }
            return null;
        }
    }
}