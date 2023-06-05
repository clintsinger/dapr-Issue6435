using Castle.Core.Logging;
using Dapr.Workflow;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ServiceB.Workflows.Common
{
    public class DelayActivity : WorkflowActivity<DelayActivityInput, DelayActivityPayload>
    {
        private readonly ILogger<DelayActivity> logger;

        public DelayActivity(ILogger<DelayActivity> logger)
        {
            this.logger = logger;
        }

        public override async Task<DelayActivityPayload> RunAsync(WorkflowActivityContext context, DelayActivityInput input)
        {
            this.logger.LogInformation("WF [{InstanceId}]: Entering DelayActivity", context.InstanceId);

            await Task.Delay(input.Delay);

            this.logger.LogInformation("WF [{InstanceId}]: Exiting DelayActivity", context.InstanceId);

            return new DelayActivityPayload();
        }
    }

    public record DelayActivityInput(TimeSpan Delay);

    public record DelayActivityPayload();
}
