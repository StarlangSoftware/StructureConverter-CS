using System;
using System.Collections.Generic;

namespace StructureConverter.DependencyToConstituency {
    
    public class ClassifierOracle : IProjectionOracle {
        public List<Tuple<Command, string>> MakeCommands(Dictionary<string, int> specialsMap, List<WordNodePair> unionList, int currentIndex) {
            return null;
        }
    }
}