using deeplynx.models;

namespace deeplynx.interfaces;

public interface ISubscriptionBusiness
{
    Task<List<SubscriptionResponseDto>> GetAllSubscriptions(long userId, long organizationId, bool hideArchived,
        long? projectId);
    Task<SubscriptionResponseDto> GetSubscription(long userId, long subscriptionId, bool hideArchived, long organizationId, long? projectId);
    Task<List<SubscriptionResponseDto>> BulkCreateSubscriptions(long userId, long organizationId, long? projectId, List<CreateSubscriptionRequestDto> dtos);
    Task<List<SubscriptionResponseDto>> BulkUpdateSubscriptions(long userId, long organizationId, long? projectId, List<UpdateSubscriptionRequestDto> dtos);
    Task<bool> BulkDeleteSubscriptions(long userId, long organizationId, long? projectId, List<long> subscriptionIds);
    Task<bool> BulkArchiveSubscriptions(long userId, long organizationId, long? projectId, List<long> subscriptionIds);
    Task<bool> BulkUnarchiveSubscriptions(long userId, long organizationId, long? projectId, List<long> subscriptionIds);
}