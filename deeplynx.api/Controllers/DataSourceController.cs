using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Mvc;
using deeplynx.business;

namespace deeplynx.api.Controllers
{
    [ApiController]
    public class DataSourceController : ControllerBase
    {
        private readonly IDataSourceBusiness _dataSourceBusiness;

        public DataSourceController(IDataSourceBusiness dataSourceBusiness)
        {
            _dataSourceBusiness = dataSourceBusiness;
        }

        /// <summary>
        /// Get all data sources from the database
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/datasource/getAllDataSources")]
        public async Task<ActionResult<IEnumerable<DataSourceDto>>> Get()
        {
            var dataSources = await _dataSourceBusiness.GetAllDataSources();
            return  Ok(dataSources);
        }

        /// <summary>
        /// Create new data source 
        /// </summary>
        /// <param name="dataSourceDto"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/datasource/createDataSource")]
        public async Task<ActionResult<DataSourceDto>> Post([FromBody] DataSourceDto dataSourceDto)
        {
            if (dataSourceDto == null)
                return BadRequest("Datasource is empty");

            var createdDataSource = await _dataSourceBusiness.CreateDataSource(dataSourceDto);
            return CreatedAtAction(nameof(Get), new { id = createdDataSource.Id }, createdDataSource);
        }

        /// <summary>
        /// Update a data source record
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dataSourceDto"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("api/datasource/updateDatasource/{id:long}")]
        public async Task<ActionResult<DataSourceDto>> Put(long id, [FromBody] DataSourceDto dataSourceDto)
        {
            var updatedDataSource = await _dataSourceBusiness.UpdateDataSource(id, dataSourceDto);
            return Ok(updatedDataSource);
        }

        /// <summary>
        /// Soft delete data source
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("api/datasource/deleteDataSource")]
        public async Task<ActionResult> Delete(long id)
        {
            var success = await _dataSourceBusiness.DeleteDataSource(id);
            if (!success)
                return NotFound($"Data source {id} not found");

            return Ok($"Data source {id} deleted");
        }
    }
}