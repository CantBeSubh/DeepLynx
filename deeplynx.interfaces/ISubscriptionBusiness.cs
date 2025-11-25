using deeplynx.models;

namespace deeplynx.interfaces;

public interface ISubscriptionBusiness
{
    Task<List<SubscriptionResponseDto>> GetAllSubscriptions(
        long currentUserId, 
        long projectId, 
        bool hideArchived);
    Task<SubscriptionResponseDto> GetSubscription(
        long currentUserId, 
        long subscriptionId, 
        long projectId,
        bool hideArchived);

    Task<List<SubscriptionResponseDto>> BulkCreateSubscriptions(
        long currentUserId, 
        long organizationId,
        long projectId,
        List<CreateSubscriptionRequestDto>? dtos
        );

    Task<List<SubscriptionResponseDto>> BulkUpdateSubscriptions(
        long currentUserId, 
        long organizationId,
        long projectId,
        List<UpdateSubscriptionRequestDto>? dtos);

    Task<bool> BulkDeleteSubscriptions(
        long currentUserId, 
        long projectId,
        List<long> subscriptionIds);
    Task<bool> BulkArchiveSubscriptions(
        long currentUserId, 
        long projectId,
        List<long> subscriptionIds);

    Task<bool> BulkUnarchiveSubscriptions(
        long currentUserId, 
        long projectId,
        List<long>? subscriptionIds);
}