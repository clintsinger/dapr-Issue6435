using Castle.Core.Logging;
using Dapr.Client;
using Dapr.Workflow;
using Microsoft.Extensions.Logging;
using ServiceB.Workflows.Test;
using System;
using System.Threading.Tasks;

namespace ServiceB.Services
{
    public class WorkflowSchedulerService : IWorkflowSchedulerService
    {
        private readonly DaprWorkflowClient workflowClient;
        private readonly DaprClient daprClient;
        private readonly ILogger<WorkflowSchedulerService> logger;

        public WorkflowSchedulerService(
            DaprWorkflowClient workflowClient, 
            DaprClient daprClient,
            ILogger<WorkflowSchedulerService> logger)
        {
            this.workflowClient = workflowClient;
            this.daprClient = daprClient;
            this.logger = logger;
        }

        public async Task ScheduleTestWorkflow(TestWorkflowInput input)
        {
            this.logger.LogInformation("WF {Timestamp} ====================================================", DateTime.Now.ToString("hh: mm:ss"));
            this.logger.LogInformation("WF Entering {Name}", nameof(ScheduleTestWorkflow));

            await this.ScheduleWorkflowAsync<TestWorkflow>(input.Id.ToString(), input, true);

            this.logger.LogInformation("WF Exiting {Name}", nameof(ScheduleTestWorkflow));
        }

        private async Task ScheduleWorkflowAsync<T>(string id, object? input, bool singleton)
        {
            var workflowName = typeof(T).Name;
            var instanceId = CreateInstanceId<T>(id);

            if (singleton)
            {
                try
                {
                    this.logger.LogInformation("WF Getting {WorkflowName} with {Id}", workflowName, id);
                    var wf = await this.daprClient.GetWorkflowAsync(instanceId, "dapr");
                    this.logger.LogInformation("WF Got {WorkflowName} with {Id}", workflowName, id);

                    if (wf != null)
                    {
                        this.logger.LogInformation("WF Terminating {WorkflowName} with {Id}", workflowName, id);
                        await this.daprClient.TerminateWorkflowAsync(instanceId, "dapr").ConfigureAwait(false);
                        this.logger.LogInformation("WF Terminated {WorkflowName} with {Id}", workflowName, id);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            this.logger.LogInformation("WF Scheduling {WorkflowName} with {Id}", workflowName, id);

            await this.workflowClient.ScheduleNewWorkflowAsync(
                name: workflowName,
                instanceId: instanceId,
                input: input
            );
        }

        private static string CreateInstanceId<T>(string id)
        {
            return $"{typeof(T).Name}_{id}";
        }
    }
}
