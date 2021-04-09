using System;
using System.Collections.Generic;

namespace StructureConverter.DependencyToConstituency {
    
    public class ClassifierOracle : ProjectionOracle {
        public override List<Tuple<Command, string>> MakeCommands(Dictionary<string, int> specialsMap, List<WordNodePair> unionList, int currentIndex) {
            var list = new List<Tuple<Command, string>>();
            switch (unionList.Count) {
                case 2:
                    if (currentIndex == 1) {
                        list.Add(new Tuple<Command, string>(Command.Left, null));
                    } else {
                        list.Add(new Tuple<Command, string>(Command.Right, null));
                    }
                    list.Add(new Tuple<Command, string>(Command.Merge, SetTreePos(unionList, unionList[currentIndex].GetTreePos())));
                    break;
                default:
                    break;
            }
            return list;
        }
    }
}