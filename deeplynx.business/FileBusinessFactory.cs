using deeplynx.interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace deeplynx.business;

public class FileBusinessFactory: IFileBusinessFactory
{
    
    private readonly IServiceProvider _serviceProvider;

    public FileBusinessFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IFileBusiness CreateFileBusiness(string storageType)
    {
        return storageType switch
        {
            "filesystem" => _serviceProvider.GetRequiredService<FileFilesystemBusiness>(),
            "azure_object" => _serviceProvider.GetRequiredService<FileAzureBusiness>(),
            "aws_s3" => _serviceProvider.GetRequiredService<FileS3Business>(),
            _ => throw new ArgumentException($"Unsupported storage type: {storageType}")
        };
    }
}