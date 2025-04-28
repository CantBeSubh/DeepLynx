using deeplynx.interfaces;
using deeplynx.models;
using Microsoft.AspNetCore.Mvc;

namespace deeplynx.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataSourceController : ControllerBase
    {
        private readonly IDataSourceBusiness _dataSourceBusiness;

        public DataSourceController(IDataSourceBusiness DataSourceBusiness)
        {
            _dataSourceBusiness = dataSourceBusiness;
        }

        [HttpGet]
        public ActionResult<IEnumerable<DataSource>> Get()
        {
            var dataSources = _dataSourceBusiness.GetDataSources();
            return Ok(dataSources);
        }

        [HttpPost]
        public ActionResult<DataSource> Post([FromBody] DataSource dataSource)
        {
            if (dataSource == null)
                return BadRequest("Datasource is null.");

            var createdDataSource = _dataSourceBusiness.CreateDataSource(dataSource);
            return CreatedAtAction(nameof(Get), new { id = createdDataSource.Id }, createdDataSource);
        }

        [HttpPut("{id}")]
        public ActionResult<DataSource> Put(long id, [FromBody] DataSource dataSource)
        {
            var updatedDataSource = _dataSourceBusiness.UpdateDataSource(id, dataSource);
            return Ok(updatedDataSource);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(long id)
        {
            var success = _dataSourceBusiness.DeleteDataSource(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}