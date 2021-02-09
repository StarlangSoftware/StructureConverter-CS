using System;

namespace StructureConverter.DependencyToConstituency {
    
    public class NonProjectiveDependencyException : Exception {
        private readonly string _fileName;

        public NonProjectiveDependencyException(string fileName) {
            _fileName = fileName;
        }

        public string ToString() {
            return "Non-Projective Dependency Failed: " + _fileName;
        }
    }
}