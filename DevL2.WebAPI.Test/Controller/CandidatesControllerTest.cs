using DevL2.WebAPI.Common;
using DevL2.WebAPI.Controller;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace DevL2.WebAPI.Test.Controller;

[TestSubject(typeof(CandidatesController))]
public class CandidatesControllerTest
{
    [Fact]
    public void Rearranged_CandidatesCountZero_ThrowsArgumentException()
    {
        // Arrange
        var controller = new CandidatesController();
        int candidatesCount = 0;
        
        // Act
        var exception = Record.Exception(() => controller.Rearranged(candidatesCount));
        
        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
        Assert.Equal("candidatesCount must be greater than 0. (Parameter 'candidatesCount')", exception.Message);
    }
    
    
    [Fact]
    public void Rearranged_ValidInput_ReturnsExpectedResponse()
    {
        // Arrange
        var controller = new CandidatesController();
        int candidatesCount = 10;
        
        // Act
        var response = controller.Rearranged(candidatesCount);
        
        // Assert
        Assert.NotNull(response);
        Assert.Equal(200,response.Code);
        
        var result = Assert.IsType<Response<object>>(response);
        var responseData = JsonConvert.DeserializeObject<RearrangedResponse>(JsonConvert.SerializeObject(result.Data));

        Assert.NotNull(responseData);
        Assert.Equal("L1, L2, L3, L4, L5, L6, L7, L8, L9, L10", responseData.OriginalCandidateList);
        Assert.Equal("L1, L10, L2, L9, L3, L8, L4, L7, L5, L6", responseData.RearrangedCandidateList);
    }
    
    public class RearrangedResponse
    {
        public string OriginalCandidateList { get; set; }
        public string RearrangedCandidateList { get; set; }
    }
}