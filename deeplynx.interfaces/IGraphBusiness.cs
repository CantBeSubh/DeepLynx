using System.Linq.Expressions;
using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.interfaces;

public interface IGraphBusiness
{
    Task<List<RelatedRecordsResponseDto>> GetEdgesByRecord(long recordId, bool isOrigin, int page, bool hideArchived, int pageSize);
    Task<GraphResponse> GetGraphDataForRecord(long recordId, long userId, int depth);
}