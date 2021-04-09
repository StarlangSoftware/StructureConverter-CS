using System;
using System.Collections.Generic;

namespace StructureConverter.DependencyToConstituency {
    
    public abstract class ProjectionOracle {

        public ProjectionOracle() {
        }
        
        protected string SetTreePos(List<WordNodePair> list, string currentPos) {
            var treePos = currentPos;
            foreach (var current in list) {
                if (current != null && current.GetTreePos().Equals("PP")) {
                    treePos = current.GetTreePos();
                }
            }
            return treePos;
        }
        
        public abstract List<Tuple<Command, string>> MakeCommands(Dictionary<string, int> specialsMap, List<WordNodePair> unionList, int currentIndex);
    }
}