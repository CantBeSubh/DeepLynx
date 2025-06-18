using System.Text.Json.Nodes;
using deeplynx.models;

namespace deeplynx.tests;

public class DataSourceTests : IClassFixture<IntegrationTestBase>
{
    private readonly IntegrationTestBase _fixture;

    public DataSourceTests(IntegrationTestBase fixture)
    {
        _fixture = fixture;
    }
    //
    //
    // [Fact]
    // public async Task GetAllDataSources_ReturnsAllDataSources()
    // { 
    //     // Act
    //     var result = await _fixture.DataSourceBusiness.GetAllDataSources(1);
    //     
    //     // Assert
    //     var resultList = result.ToList();
    //     Assert.Equal(2, resultList.Count);
    //     Assert.Equal("DataSource1", resultList[0].Name);
    //     Assert.Equal("DataSource2", resultList[1].Name);
    //     Assert.Equal(1, resultList[0].ProjectId);
    //     Assert.Equal(1, resultList[1].ProjectId);
    // }
    //
    // [Fact]
    // public async Task UpdateDataSource_UpdatesAndReturnsUpdatedDataSource()
    // {
    //     // Arrange
    //     var updateDto = new DataSourceRequestDto()
    //     {
    //         Name = "UpdatedName",
    //         Abbreviation = "UpdAbbr",
    //         Type = "UpdatedType",
    //         BaseUri = "http://updated.uri",
    //         Config = new JsonObject { ["a"] = "b" }
    //     };
    //
    //     // Act
    //     var result = await _fixture.DataSourceBusiness.UpdateDataSource(1, 1,updateDto);
    //
    //     // Assert
    //     Assert.Equal(updateDto.Name, result.Name);
    //     Assert.Equal(updateDto.Abbreviation, result.Abbreviation);
    //     Assert.Equal(updateDto.Type, result.Type);
    //     Assert.Equal(updateDto.BaseUri, result.BaseUri);
    //     Assert.Equal(updateDto.Config.ToString(), result.Config.ToString());
    //     
    // }
}