namespace deeplynx.helpers;

public class AuthHelper
{
    // TODO: check if a user has permissions to perform a given action on a given resource within a given domain
    public async Task<bool> AuthCheck(
        long userId, string resource, string action, long? projectId, long? organizationId, long? labelId)
    {
        // TODO: ensure that either project ID or organization ID is supplied
        // TODO: figure out if labelId should replace resource or how that will work
        return true;
    }
}