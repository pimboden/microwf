using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using tomware.Microwf.Core;
using tomware.Microwf.Engine;

namespace WebApi.Workflows.SoftwareRequest
{  
    public class SoftwareRequestWorkflow : EntityWorkflowDefinitionBase
  {
    private readonly ILogger<SoftwareRequestWorkflow> _logger;

    public const string TYPE = "SoftwareRequestWorkflow";

    public const string REQUEST_TRIGGER = "request";
    public const string APPROVE_TRIGGER = "approve";
    public const string REJECT_TRIGGER = "reject";
    public const string REQUEST_ADDITIONAL_INFO_TRIGGER = "requestmoreinfo";
    public const string PROVIDE_MORE_INFO_TRIGGER = "providemoreinfo";


    public const string OPEN_STATE = "open";
    public const string REQUESTED_STATE = "requested";
    public const string APPROVED_STATE = "approved";
    public const string REJECTED_STATE = "rejected";
    public const string MORE_INFO_STATE = "moreinfo";
    public override string Type => TYPE;

    public override Type EntityType => typeof(SoftwareRequest);

    public override List<Transition> Transitions
    {
      get
      {
        return new List<Transition>
        {
          new Transition {
            State = OPEN_STATE,
            Trigger = REQUEST_TRIGGER,
            TargetState = REQUESTED_STATE,
            AfterTransition = AssignToAdmin
          },
          new Transition {
            State = REQUESTED_STATE,
            Trigger = APPROVE_TRIGGER,
            TargetState = APPROVED_STATE,
            AfterTransition = AssignToCreator
          },
          new Transition {
            State = REQUESTED_STATE,
            Trigger = REJECT_TRIGGER,
            TargetState = REJECTED_STATE,
            AfterTransition = AssignToCreator
          },
          new Transition {
            State = REQUESTED_STATE,
            Trigger = REQUEST_ADDITIONAL_INFO_TRIGGER,
            TargetState = MORE_INFO_STATE,
            AfterTransition = AssignToCreator
          },
          new Transition {
            State = MORE_INFO_STATE,
            Trigger = REQUEST_TRIGGER,
            TargetState = REQUESTED_STATE,
            AfterTransition = AssignToAdmin
          }
        };
      }
    }

    public SoftwareRequestWorkflow(ILoggerFactory loggerFactory)
    {
      this._logger = loggerFactory.CreateLogger<SoftwareRequestWorkflow>();
    }

    private void AssignToAdmin(TransitionContext context)
    {
      // because admin is the dev ;-)...
      var softwareRequest = context.GetInstance<SoftwareRequest>();

      softwareRequest.Assignee = "admin";

      this._logger.LogInformation($"Assignee: {softwareRequest.Assignee}");
    }

    private void AssignToCreator(TransitionContext context)
    {
      var softwareRequest = context.GetInstance<SoftwareRequest>();

      softwareRequest.Assignee = softwareRequest.Creator;

      this._logger.LogInformation($"Assignee: {softwareRequest.Assignee}");
    }
  }
}