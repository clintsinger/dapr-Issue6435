using ServiceB.Workflows.Test;
using System.Threading.Tasks;

namespace ServiceB.Services
{
    public interface IWorkflowSchedulerService
    {
        Task ScheduleTestWorkflow(TestWorkflowInput input);
    }
}
