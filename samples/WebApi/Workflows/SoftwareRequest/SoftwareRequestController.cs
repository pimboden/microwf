using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApi.Common;

namespace WebApi.Workflows.SoftwareRequest
{
 [Authorize]
  [Route("api/softwareRequest")]
  public class SoftwareRequestController : Controller
  {
    private readonly ISoftwareRequestService _service;

    public SoftwareRequestController(ISoftwareRequestService service)
    {
      this._service = service;
    }

    [HttpGet("assignees")]
    [ProducesResponseType(typeof(IEnumerable<string>), 200)]
    public async Task<IActionResult> GetAssignees()
    {
      var result = await this._service.GetAssigneesAsync();

      return Ok(result);
    }

    [HttpPost("new")]
    [ProducesResponseType(typeof(IWorkflowResult<SoftwareRequestViewModel>), 200)]
    public async Task<IActionResult> New()
    {
      var result = await this._service.NewAsync();

      return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(IWorkflowResult<SoftwareRequestViewModel>), 200)]
    public async Task<IActionResult> Get(int id)
    {
      var result = await this._service.GetAsync(id);

      return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SoftwareRequest), 201)]
    public async Task<IActionResult> Post([FromBody]SoftwareRequestViewModel model)
    {
      if (model == null) return BadRequest();
      if (!ModelState.IsValid) return BadRequest(ModelState);

      var result = await _service.CreateAsync(model);

      return Created($"api/softwareRequest/{result}", result);
    }

    [HttpPost("process")]
    [ProducesResponseType(typeof(IWorkflowResult<AssigneeWorkflowResult>), 200)]
    public async Task<IActionResult> Process([FromBody]SoftwareRequestViewModel model)
    {
      if (model == null) return BadRequest();
      if (!this.ModelState.IsValid) return BadRequest(this.ModelState);

      var result = await this._service.ProcessAsync(model);

      return Ok(result);
    }

    [HttpGet("mywork")]
    [ProducesResponseType(typeof(IEnumerable<SoftwareRequest>), 200)]
    public async Task<IActionResult> MyWork()
    {
      var result = await this._service.MyWorkAsync();

      return Ok(result);
    }
  }
}