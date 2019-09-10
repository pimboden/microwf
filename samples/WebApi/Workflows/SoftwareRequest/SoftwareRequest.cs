using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using tomware.Microwf.Engine;

namespace WebApi.Workflows.SoftwareRequest
{
  [Table("SoftwareRequest")]
  public partial class SoftwareRequest : IEntityWorkflow
  {
    [Key]
    public int Id { get; set; }

    [Required]
    public string Type { get; set; }

    [Required]
    public string State { get; set; }

    [Required]
    public string Creator { get; set; }

    [Required]
    public string Assignee { get; set; }

    [Required]
    public string Software { get; set; }

    public string OperatingSystem { get; set; }

    public static SoftwareRequest Create(string creator)
    {
      return new SoftwareRequest
      {
        Type = SoftwareRequestWorkflow.TYPE,
        State = SoftwareRequestWorkflow.OPEN_STATE,
        Creator = creator,
        Assignee = creator
      };
    }
  }
}