using DevL2.WebAPI.Common;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DevL2.WebAPI.Controller;

[ApiController]
[Route("api/[controller]/[action]")]
public class CandidatesController : ControllerBase
{
    [HttpGet]
    public Response<object> Rearranged(int candidatesCount)
    {
        if (candidatesCount == 0)
        {
            throw new ArgumentException("candidatesCount must be greater than 0.", nameof(candidatesCount));
        }
        
        var candidates = new List<string>();
        for (int i = 1; i <= candidatesCount; i++)
        {
            candidates.Add($"L{i}");
        }
        
        var result = new List<string>();    
        int left = 0, right = candidates.Count - 1;
        while (left <= right)
        {
            if (left == right)
            {
                result.Add(candidates[left]);
            }
            else
            {
                result.Add(candidates[left]);
                result.Add(candidates[right]);
            }
            left++;
            right--;
        }

        var obj = new 
        {
            OriginalCandidateList = string.Join(", ", candidates),
            RearrangedCandidateList = string.Join(", ", result)
        };

        return new Response<object>(obj, "success");
    }
}