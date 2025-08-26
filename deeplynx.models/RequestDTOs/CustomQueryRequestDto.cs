
namespace deeplynx.models;

public class CustomQueryRequestDto

{
        public string? Connector { get; set; } // AND, OR, NOT
        public string Filter { get; set; } // properties from historical records model
        public string Operator { get; set; } // =, <, >, LIKE
        public string Value { get; set; } // One selected option from listed values of Filters 
        
}

