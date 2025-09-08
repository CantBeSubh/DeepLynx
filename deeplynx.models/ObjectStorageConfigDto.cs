namespace deeplynx.models;

// structure for serializing/deserializing object storage config
public class ObjectStorageConfigDto
{
    public string? MountPath {get; set;}
    public string? AzureConnectionString {get; set;}
    public string? AwsConnectionString {get; set;}
}