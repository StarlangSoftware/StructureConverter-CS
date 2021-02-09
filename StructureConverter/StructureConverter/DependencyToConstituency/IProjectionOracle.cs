using System;
using System.Collections.Generic;

namespace StructureConverter.DependencyToConstituency {
    
    public interface IProjectionOracle {
        List<Tuple<Command, string>> MakeCommands(Dictionary<string, int> specialsMap, List<WordNodePair> unionList, int currentIndex);
    }
}