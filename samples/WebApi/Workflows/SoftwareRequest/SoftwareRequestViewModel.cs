using System.ComponentModel.DataAnnotations;
using tomware.Microwf.Core;

namespace WebApi.Workflows.SoftwareRequest
{
  public class SoftwareRequestViewModel : WorkflowVariableBase
  {
    public int? Id { get; set; }

    [Required]
    public string Trigger { get; set; } = SoftwareRequestWorkflow.REQUEST_TRIGGER;

    public string Assignee { get; set; }

    public string Software { get; set; }

    public string OperatingSystem { get; set; }
  }
}