using System;

namespace StructureConverter.DependencyToConstituency {
    
    [Serializable]
    public class UniversalDependencyNotExistsException : Exception {
        
        private string _fileName;

        public UniversalDependencyNotExistsException(string fileName) {
            _fileName = fileName;
        }

        public string ToString() {
            return "Universal Dependency Not Existed Failed: " + _fileName;
        }
    }
}