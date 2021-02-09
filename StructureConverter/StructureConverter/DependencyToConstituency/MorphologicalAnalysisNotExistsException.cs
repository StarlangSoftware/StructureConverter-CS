using System;

namespace StructureConverter.DependencyToConstituency {
    public class MorphologicalAnalysisNotExistsException : Exception {
        private readonly string _fileName;

        public MorphologicalAnalysisNotExistsException(string fileName) {
            _fileName = fileName;
        }

        public string ToString() {
            return "Morphologic Failed: " + _fileName;
        }
    }
}