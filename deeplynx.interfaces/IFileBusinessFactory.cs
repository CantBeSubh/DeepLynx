namespace deeplynx.interfaces;

public interface IFileBusinessFactory
{
    IFileBusiness CreateFileBusiness(string storageType);
}