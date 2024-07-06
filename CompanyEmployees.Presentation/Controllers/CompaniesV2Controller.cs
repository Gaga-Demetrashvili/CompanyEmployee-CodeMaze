using CompanyEmployees.Presentation.Extensions;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects;

namespace CompanyEmployees.Presentation.Controllers;

//[ApiVersion("2.0", Deprecated = true)] We do not need this attr, because I used Conventions in ConfigureVersioning Service Extension Method
// One thing to mention, we can’t use the query string pattern (https://localhost:5001/api/companies?api-version=2.0) to call the companies v2 controller anymore. We can use it for version 1.0, though.
//[Route("api/{v:apiversion}/companies")] - This should be used when you want to implement URL versioning
[Route("api/companies")]
[ApiController]
[ApiExplorerSettings(GroupName = "v2")]
public class CompaniesV2Controller : ControllerBase
{
    private readonly IServiceManager _serviceManager;

    public CompaniesV2Controller(IServiceManager serviceManager) => _serviceManager = serviceManager;

    [HttpGet]
    public async Task<IActionResult> GetCompanies()
    {
        var baseResult = await _serviceManager.CompanyService.GetAllCompaniesAsync(trackChanges: false);

        var companies = baseResult.GetResult<IEnumerable<CompanyDto>>();

        var companiesV2 = companies.Select(c => $"{c.Name} V2");

        return Ok(companiesV2);
    }
}
