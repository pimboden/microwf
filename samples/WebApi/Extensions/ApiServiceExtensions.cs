using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using tomware.Microwf.Core;
using tomware.Microwf.Engine;
using WebApi.Domain;
using WebApi.Workflows.Holiday;
using WebApi.Workflows.Issue;
using WebApi.Workflows.SoftwareRequest;
using WebApi.Workflows.Stepper;

namespace WebApi.Extensions
{
  public static class ApiServiceExtensions
  {
    public static IServiceCollection AddApiServices<TContext>(
      this IServiceCollection services,
      IConfiguration configuration
    ) where TContext : EngineDbContext
    {
      var workflowConf = CreateWorkflowConfiguration(); // GetWorkflowConfiguration(services);
      IOptions<ProcessorConfiguration> processorConf = GetProcessorConfiguration(configuration, services);

      services
       .AddWorkflowEngineServices<TContext>(workflowConf)
       .AddJobQueueServices<TContext>(processorConf.Value)
       .AddTestUserWorkflowMappings(CreateSampleUserWorkflowMappings());

      services.AddTransient<IUserContextService, IdentityUserContextService>();

      services.AddTransient<IWorkflowDefinition, HolidayApprovalWorkflow>();
      services.AddTransient<IWorkflowDefinition, IssueTrackingWorkflow>();
      services.AddTransient<IWorkflowDefinition, SoftwareRequestWorkflow>();
      services.AddTransient<IWorkflowDefinition, StepperWorkflow>();

      services.AddTransient<IHolidayService, HolidayService>();
      services.AddTransient<IIssueService, IssueService>();
      services.AddTransient<ISoftwareRequestService, SoftwareRequestService>();
      services.AddTransient<IStepperService, StepperService>();

      services.AddScoped<IMigrationService, MigrationService>();

      return services;
    }

    private static WorkflowConfiguration CreateWorkflowConfiguration()
    {
      return new WorkflowConfiguration
      {
        Types = new List<WorkflowType> {
          new WorkflowType {
            Type = "HolidayApprovalWorkflow",
            Title = "Holiday",
            Description = "Simple holiday approval process.",
            Route = "holiday"
          },
          new WorkflowType {
            Type = "IssueTrackingWorkflow",
            Title = "Issue",
            Description = "Simple issue tracking process.",
            Route = "issue"
          },
          new WorkflowType {
            Type = "SoftwareRequestWorkflow",
            Title = "SoftwareRequest",
            Description = "Simple software request process.",
            Route = "softwareRequest"
          },
          new WorkflowType {
            Type = "StepperWorkflow",
            Title = "Stepper",
            Description = "Dummy workflow to test workflow processor.",
            Route = ""
          }
        }
      };
    }

    private static List<UserWorkflowMapping> CreateSampleUserWorkflowMappings()
    {
      return new List<UserWorkflowMapping> {
        new UserWorkflowMapping {
          UserName = "admin",
          WorkflowDefinitions = new List<string> {
            HolidayApprovalWorkflow.TYPE,
            IssueTrackingWorkflow.TYPE,
            StepperWorkflow.TYPE
          }
        },
        new UserWorkflowMapping {
          UserName = "alice",
          WorkflowDefinitions = new List<string> {
            HolidayApprovalWorkflow.TYPE,
            IssueTrackingWorkflow.TYPE,
            StepperWorkflow.TYPE
          }
        },
        new UserWorkflowMapping {
          UserName = "bob",
          WorkflowDefinitions = new List<string> {
            HolidayApprovalWorkflow.TYPE,
            StepperWorkflow.TYPE
          }
        }
      };
    }

    private static IOptions<WorkflowConfiguration> GetWorkflowConfiguration(
      IConfiguration configuration,
      IServiceCollection services
    )
    {
      var workflows = configuration.GetSection("Workflows");
      services.Configure<WorkflowConfiguration>(workflows);

      return services
      .BuildServiceProvider()
      .GetRequiredService<IOptions<WorkflowConfiguration>>();
    }

    private static IOptions<ProcessorConfiguration> GetProcessorConfiguration(
      IConfiguration configuration,
      IServiceCollection services
    )
    {
      var worker = configuration.GetSection("Worker");
      services.Configure<ProcessorConfiguration>(worker);

      return services
      .BuildServiceProvider()
      .GetRequiredService<IOptions<ProcessorConfiguration>>();
    }
  }
}
