using Microsoft.EntityFrameworkCore;
using tomware.Microwf.Engine;
using WebApi.Workflows.Holiday;
using WebApi.Workflows.Issue;
using WebApi.Workflows.SoftwareRequest;
using WebApi.Workflows.Stepper;

namespace WebApi.Domain
{
  public class DomainContext : EngineDbContext
  {
    public DomainContext(DbContextOptions<DomainContext> options) : base(options)
    { }

    public DbSet<Issue> Issues { get; set; }
    public DbSet<Holiday> Holidays { get; set; }
    public DbSet<SoftwareRequest> SoftwareRequests { get; set; }
    public DbSet<Stepper> Steppers { get; set; }
  }
}
