using deeplynx.models;

namespace deeplynx.interfaces;

public interface ISubscriptionBusiness
{
    Task<List<SubscriptionResponseDto>> GetAllSubscriptions(long currentUserId, long organizationId, bool hideArchived,
        long? projectId);
    Task<SubscriptionResponseDto> GetSubscription(long currentUserId, long subscriptionId, long organizationId, bool hideArchived, long? projectId);
    Task<List<SubscriptionResponseDto>> BulkCreateSubscriptions(long currentUserId, long organizationId, List<CreateSubscriptionRequestDto> dtos, long? projectId);
    Task<List<SubscriptionResponseDto>> BulkUpdateSubscriptions(long currentUserId, long organizationId, List<UpdateSubscriptionRequestDto> dtos, long? projectId);
    Task<bool> BulkDeleteSubscriptions(long currentUserId, long organizationId, List<long> subscriptionIds, long? projectId);
    Task<bool> BulkArchiveSubscriptions(long currentUserId, long organizationId, List<long> subscriptionIds, long? projectId);
    Task<bool> BulkUnarchiveSubscriptions(long currentUserId, long organizationId, List<long> subscriptionIds, long? projectId);
}