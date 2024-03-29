using System;
using System.Collections.Generic;
using Classification.Model;

namespace StructureConverter.DependencyToConstituency {
    
    public class BasicOracle : ProjectionOracle {

        private readonly Dictionary<string, int> _specialsMap;

        public BasicOracle() { 
            _specialsMap = SetSpecialMap();
        }
        
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
        
        private int CompareTo(WordNodePair first, WordNodePair second) {
            var firstUniversalDependency = first.GetUniversalDependency();
            var secondUniversalDependency = second.GetUniversalDependency();
            if (_specialsMap.ContainsKey(firstUniversalDependency) && _specialsMap.ContainsKey(secondUniversalDependency)) {
                return _specialsMap[firstUniversalDependency].CompareTo(_specialsMap[secondUniversalDependency]);
            } else if (_specialsMap.ContainsKey(firstUniversalDependency)) {
                return 1;
            } else if (_specialsMap.ContainsKey(secondUniversalDependency)) {
                return -1;
            }
            return 0;
        }
        
        private int AddCommandForDecreasing(int index, List<WordNodePair> unionList, List<Tuple<Command, string>> commands) {
            var i = 0;
            while (index - (i + 1) > -1) {
                if (CompareTo(unionList[index - i], unionList[index - (i + 1)]) == 0) {
                    commands.Add(new Tuple<Command, string>(Command.Left, null));
                } else {
                    break;
                }
                i++;
            }
            return i + 1;
        }
        
        private int AddCommandsForLeft(int currentIndex, int i, List<WordNodePair> unionList, List<Tuple<Command, string>> commands) {
            if (currentIndex - (i + 1) > -1 && CompareTo(unionList[currentIndex - i], unionList[currentIndex - (i + 1)]) == 0) {
                i += AddCommandForDecreasing(currentIndex - i, unionList, commands);
                commands.Add(new Tuple<Command, string>(Command.Left, null));
            } else {
                commands.Add(new Tuple<Command, string>(Command.Left, null));
                i++;
            }
            commands.Add(new Tuple<Command, string>(Command.Merge, SetTreePos(unionList, unionList[currentIndex].GetTreePos())));
            return i;
        }
        
        private int FindSpecialIndex(List<WordNodePair> unionList, int currentIndex) {
            for (var i = 0; i < unionList.Count; i++) {
                if (currentIndex != i && unionList[i].GetUniversalDependency().Equals("NSUBJ") || unionList[i].GetUniversalDependency().Equals("CSUBJ")) {
                    return i;
                }
            }
            return -1;
        }
        
        private void AddSpecialForLeft(List<WordNodePair> unionList, List<Tuple<Command, string>> commands, int i, int j, int currentIndex) {
            var check = false;
            while (currentIndex - i > -1) {
                if (unionList[currentIndex - i].GetWord().IsPunctuation() || unionList[currentIndex - i].GetUniversalDependency().Equals("NSUBJ") || unionList[currentIndex - i].GetUniversalDependency().Equals("CSUBJ")) {
                    break;
                } else {
                    check = true;
                    commands.Add(new Tuple<Command, string>(Command.Left, null));
                }
                i++;
            }
            while (currentIndex + j < unionList.Count) {
                if (unionList[currentIndex + j].GetWord().IsPunctuation()) {
                    break;
                } else {
                    check = true;
                    commands.Add(new Tuple<Command, string>(Command.Right, null));
                }
                j++;
            }
            if (check) {
                commands.Add(new Tuple<Command, string>(Command.Merge, "VP"));
            }
            while (currentIndex + j < unionList.Count) {
                commands.Add(new Tuple<Command, string>(Command.Right, null));
                j++;
            }
            while (currentIndex - i > -1) {
                commands.Add(new Tuple<Command, string>(Command.Left, null));
                i++;
            }
            commands.Add(new Tuple<Command, string>(Command.Merge, SetTreePos(unionList, unionList[currentIndex].GetTreePos())));
        }
        
        private bool ContainsWordNodePair(List<WordNodePair> unionList, int wordNodePairNo) {
            foreach (var wordNodePair in unionList) {
                if (wordNodePair.GetNo() == wordNodePairNo) {
                    return true;
                }
            }
            return false;
        }
        
        private int FinalCommandsForObjects(List<WordNodePair> unionList, int currentIndex, int i, List<Tuple<Command, string>> commands) {
            if (unionList[currentIndex].GetWord().GetUniversalDependency().ToString().Equals("ROOT")) {
                var bound = -1;
                for (var j = 0; j < unionList.Count; j++) {
                    if (unionList[j].GetTo() - 1 == unionList[currentIndex].GetNo() && unionList[j].GetUniversalDependency().Equals("OBJ") || unionList[j].GetUniversalDependency().Equals("IOBJ") || unionList[j].GetUniversalDependency().Equals("OBL")) {
                        bound = j;
                        break;
                    }
                }
                if (bound > -1) {
                    var check = false;
                    while (currentIndex - i >= bound) {
                        check = true;
                        commands.Add(new Tuple<Command, string>(Command.Left, null));
                        i++;
                    }
                    if (check) {
                        commands.Add(new Tuple<Command, string>(Command.Merge, "VP"));
                    }
                }
            }
            return i;
        }

        private List<Tuple<Command, string>> SimpleMerge(List<WordNodePair> unionList, string treePos, int index) {
            var list = new List<Tuple<Command, string>>();
            for (var i = 0; i < unionList.Count; i++) {
                if (index != i) {
                    if (i > index) {
                        list.Add(new Tuple<Command, string>(Command.Right, null));
                    } else {
                        list.Add(new Tuple<Command, string>(Command.Left, null));
                    }
                }
            }
            list.Add(new Tuple<Command, string>(Command.Merge, treePos));
            return list;
        }
        
        public override List<Tuple<Command, string>> MakeCommands(List<WordNodePair> unionList, int currentIndex, List<TreeEnsembleModel> models) {
            var treePos = SetTreePos(unionList, unionList[currentIndex].GetTreePos());
            if (unionList.Count > 2) {
                int i = 1, j = 1, specialIndex = -1; 
                var commands = new List<Tuple<Command, string>>(); 
                while (currentIndex - i > -1 || currentIndex + j < unionList.Count) { 
                    if (currentIndex - i > -1 && currentIndex + j < unionList.Count) { 
                        var comparisonResult = CompareTo(unionList[currentIndex - i], unionList[currentIndex + j]); 
                        if (comparisonResult > 0) { 
                            i = AddCommandsForLeft(currentIndex, i, unionList, commands); 
                        } else if (comparisonResult < 0) { 
                            commands.Add(new Tuple<Command, string>(Command.Right, null)); 
                            commands.Add(new Tuple<Command, string>(Command.Merge, treePos)); 
                            j++; 
                        } else { 
                            if (!_specialsMap.ContainsKey(unionList[currentIndex - i].GetUniversalDependency()) && !_specialsMap.ContainsKey(unionList[currentIndex + j].GetUniversalDependency())) { 
                                break; 
                            } else { 
                                commands.Add(new Tuple<Command, string>(Command.Left, null)); 
                                commands.Add(new Tuple<Command, string>(Command.Right, null)); 
                                commands.Add(new Tuple<Command, string>(Command.Merge, treePos)); 
                                i++; 
                                j++; 
                            } 
                        } 
                    } else if (currentIndex - i > -1) { 
                        if (_specialsMap.ContainsKey(unionList[currentIndex - i].GetUniversalDependency())) { 
                            i = AddCommandsForLeft(currentIndex, i, unionList, commands); 
                        } else { 
                            if (unionList[currentIndex - i].GetUniversalDependency().Equals("NSUBJ") || unionList[currentIndex - i].GetUniversalDependency().Equals("CSUBJ")) { 
                                specialIndex = currentIndex - i; 
                            } 
                            break; 
                        } 
                    } else { 
                        if (_specialsMap.ContainsKey(unionList[currentIndex + j].GetUniversalDependency())) { 
                            commands.Add(new Tuple<Command, string>(Command.Right, null)); 
                            commands.Add(new Tuple<Command, string>(Command.Merge, treePos)); 
                            j++; 
                        } else { 
                            break; 
                        } 
                    } 
                } 
                if (specialIndex == -1) { 
                    specialIndex = FindSpecialIndex(unionList, currentIndex); 
                } 
                if (specialIndex > -1 && ContainsWordNodePair(unionList, unionList[specialIndex].GetTo() - 1)) { 
                    if (currentIndex > specialIndex) { 
                        AddSpecialForLeft(unionList, commands, i, j, currentIndex); 
                    } else { 
                        i = FinalCommandsForObjects(unionList, currentIndex, i, commands); 
                        var check = false; 
                        while (currentIndex + j < unionList.Count) { 
                            check = true; 
                            commands.Add(new Tuple<Command, string>(Command.Right, null)); 
                            j++; 
                        } 
                        while (currentIndex - i > -1) { 
                            check = true; 
                            commands.Add(new Tuple<Command, string>(Command.Left, null)); 
                            i++; 
                        } 
                        if (check) { 
                            commands.Add(new Tuple<Command, string>(Command.Merge, treePos)); 
                        } 
                    } 
                } else { 
                    i = FinalCommandsForObjects(unionList, currentIndex, i, commands); 
                    var check = false; 
                    while (currentIndex + j < unionList.Count) { 
                        check = true; 
                        commands.Add(new Tuple<Command, string>(Command.Right, null)); 
                        j++; 
                    } 
                    while (currentIndex - i > -1) { 
                        check = true; 
                        commands.Add(new Tuple<Command, string>(Command.Left, null)); 
                        i++; 
                    } 
                    if (check) { 
                        commands.Add(new Tuple<Command, string>(Command.Merge, treePos)); 
                    } 
                } 
                return commands; 
            }
            return SimpleMerge(unionList, treePos, currentIndex);
        }
    }
}