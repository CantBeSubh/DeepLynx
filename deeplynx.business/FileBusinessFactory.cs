using deeplynx.interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace deeplynx.business;

public class FileBusinessFactory: IFileBusinessFactory
{
    public IFileBusiness CreateFileBusiness(string storageType)
    {
        return storageType switch
        {
            "filesystem" => new FileFilesystemBusiness(),
            "azure_object" => new FileAzureBusiness(),
            "aws_s3" => new FileS3Business(),
            _ => throw new ArgumentException($"Unsupported storage type: {storageType}")
        };
    }
}