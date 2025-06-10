using deeplynx.datalayer.Models;
using deeplynx.models;

namespace deeplynx.interfaces
{
    public interface ITimeseriesBusiness
    {
        // todo: get interface methods defined here
        Task<List<List<dynamic>>> GetAllTableRecords(TimeseriesDataDto timeSeriesDataDTO);
        Task ProcessTimeSeriesDataAsync(TimeseriesDataDto timeSeriesDataDTO);
    }
}