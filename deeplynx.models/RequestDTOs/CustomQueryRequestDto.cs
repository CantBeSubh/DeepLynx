
using System.Text.Json;

namespace deeplynx.models;

public class CustomQueryRequestDto

{
        public string? Connector { get; set; } // AND, OR
        public string Filter { get; set; } // properties from historical records model
        public string Operator { get; set; } // =, <, >, LIKE, KEY_VALUE
        public string Value { get; set; } // One selected option from listed values of Filters 
        
        public string? Json { get; set; }
}

