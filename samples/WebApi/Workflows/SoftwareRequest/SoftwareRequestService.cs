using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using tomware.Microwf.Core;
using tomware.Microwf.Engine;
using WebApi.Domain;
using WebApi.Common;

namespace WebApi.Workflows.SoftwareRequest
{
  public interface ISoftwareRequestService
  {
    Task<IEnumerable<string>> GetAssigneesAsync();

    Task<IWorkflowResult<SoftwareRequestViewModel>> NewAsync();

    Task<int> CreateAsync(SoftwareRequestViewModel model);

    Task<IWorkflowResult<SoftwareRequestViewModel>> GetAsync(int id);

    Task<IWorkflowResult<AssigneeWorkflowResult>> ProcessAsync(SoftwareRequestViewModel model);

    Task<IEnumerable<SoftwareRequest>> MyWorkAsync();
  }

  public class SoftwareRequestService : ISoftwareRequestService
  {
    private readonly DomainContext _context;
    private readonly IWorkflowEngine _workflowEngine;
    private readonly IUserContextService _userContext;

    public SoftwareRequestService(
      DomainContext context,
      IWorkflowEngine workflowEngine,
      IUserContextService userContext
    )
    {
      this._context = context;
      this._workflowEngine = workflowEngine;
      this._userContext = userContext;
    }

    public Task<IEnumerable<string>> GetAssigneesAsync()
    {
      var assignees = WebApi.Identity.Config.GetUsers()
        .Where(u => u.Username != "bob")
        .Select(u => u.Username);

      return Task.FromResult(assignees);
    }

    public async Task<IWorkflowResult<SoftwareRequestViewModel>> NewAsync()
    {
      var softwareRequest = SoftwareRequest.Create(_userContext.UserName);
      var triggerParam = new TriggerParam(SoftwareRequestWorkflow.REQUEST_TRIGGER, softwareRequest);

      var triggerResult = await this._workflowEngine.CanTriggerAsync(triggerParam);

      var info = await this._workflowEngine.ToWorkflowTriggerInfo(softwareRequest, triggerResult);
      var viewModel = new SoftwareRequestViewModel();
      viewModel.Assignee = _userContext.UserName;

      var result = new WorkflowResult<SoftwareRequest, SoftwareRequestViewModel>(info, softwareRequest, viewModel);

      return await Task.FromResult<IWorkflowResult<SoftwareRequestViewModel>>(result);
    }

    public async Task<int> CreateAsync(SoftwareRequestViewModel model)
    {
      if (model == null) throw new ArgumentNullException("model");

      var softwareRequest = SoftwareRequest.Create(_userContext.UserName);
      softwareRequest.Software = model.Software;
      softwareRequest.OperatingSystem = model.OperatingSystem;
      if (!string.IsNullOrWhiteSpace(model.Assignee)) softwareRequest.Assignee = model.Assignee;

      this._context.SoftwareRequests.Add(softwareRequest);

      await this._context.SaveChangesAsync();

      // await this._messageBus.PublishAsync(WorkItemMessage.Create(
      //   SoftwareRequestWorkflow.ASSIGN_TRIGGER,
      //   softwareRequest.Id,
      //   softwareRequest.Type
      // ));

      return softwareRequest.Id;
    }

    public async Task<IWorkflowResult<SoftwareRequestViewModel>> GetAsync(int id)
    {
      var softwareRequest = await this.FindOrCreate(id);

      return await ToResult(softwareRequest);
    }

    public async Task<IWorkflowResult<AssigneeWorkflowResult>> ProcessAsync(SoftwareRequestViewModel model)
    {
      var softwareRequest = await FindOrCreate(model.Id);

      var triggerParam = new TriggerParam(model.Trigger, softwareRequest)
       .AddVariableWithKey<SoftwareRequestViewModel>(model);

      var triggerResult = await this._workflowEngine.TriggerAsync(triggerParam);

      var info = await this._workflowEngine.ToWorkflowTriggerInfo(softwareRequest, triggerResult);
      var viewModel = new AssigneeWorkflowResult(softwareRequest.Assignee);

      return new WorkflowResult<SoftwareRequest, AssigneeWorkflowResult>(info, softwareRequest, viewModel);
    }

    public async Task<IEnumerable<SoftwareRequest>> MyWorkAsync()
    {
      var softwareRequests = await this._context.SoftwareRequests
        .Where(h => h.Assignee == _userContext.UserName)
        .OrderByDescending(h => h.Id)
        .ToListAsync();

      return softwareRequests;
    }

    private async Task<SoftwareRequest> FindOrCreate(int? id)
    {
      SoftwareRequest softwareRequest;

      if (id.HasValue)
      {
        softwareRequest = await this._context.SoftwareRequests
          .SingleAsync(i => i.Id == id.Value);
      }
      else
      {
        softwareRequest = SoftwareRequest.Create(_userContext.UserName);
        this._context.SoftwareRequests.Add(softwareRequest);
      }

      return softwareRequest;
    }

    private async Task<IWorkflowResult<SoftwareRequestViewModel>> ToResult(SoftwareRequest softwareRequest)
    {
      var info = await this._workflowEngine.ToWorkflowTriggerInfo(softwareRequest);
      var viewModel = this.ToViewModel(softwareRequest);

      return new WorkflowResult<SoftwareRequest, SoftwareRequestViewModel>(info, softwareRequest, viewModel);
    }

    private SoftwareRequestViewModel ToViewModel(SoftwareRequest softwareRequest)
    {
      var viewModel = new SoftwareRequestViewModel
      {
        Id = softwareRequest.Id,
        Trigger = string.Empty,
        Assignee = softwareRequest.Assignee,
        Software = softwareRequest.Software,
        OperatingSystem = softwareRequest.OperatingSystem
      };

      return viewModel;
    }
  }
}
