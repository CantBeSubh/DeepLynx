using deeplynx.models;
using Microsoft.AspNetCore.Http;

namespace deeplynx.interfaces
{
    public interface ITimeseriesUploadBusiness
    {
        Task<TimeseriesResponseDto> UploadFile(string projectId, string dataSourceId, IFormFile file);
    }
}