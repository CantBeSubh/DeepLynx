using deeplynx.models;

namespace deeplynx.interfaces;

public interface ISubscriptionBusiness
{
    Task<List<SubscriptionResponseDto>> GetAllSubscriptions(long userId, long projectId, bool hideArchived);
    Task<SubscriptionResponseDto> GetSubscription(long userId, long projectId, long subscriptionId, bool hideArchived);
    Task<List<SubscriptionResponseDto>> BulkCreateSubscriptions(long userId, long projectId,
        List<CreateSubscriptionRequestDto> dtos);
    Task<List<SubscriptionResponseDto>> BulkUpdateSubscriptions(
        long userId,
        long projectId,
        List<UpdateSubscriptionRequestDto> dtos);
    Task<bool> BulkDeleteSubscriptions(long userId, long projectId, List<long> subscriptionIds);
    Task<bool> BulkArchiveSubscriptions(long userId, long projectId, List<long> subscriptionIds);
    Task<bool> BulkUnarchiveSubscriptions(long userId, long projectId, List<long> subscriptionIds);
}