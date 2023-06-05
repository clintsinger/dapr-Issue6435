using Castle.Core.Logging;
using Dapr.Workflow;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ServiceB.Workflows.Common
{
    public class LoggerActivity : WorkflowActivity<LoggerActivityInput, object?>
    {
        private readonly ILogger<LoggerActivity> logger;

        public LoggerActivity(ILogger<LoggerActivity> logger)
        {
            this.logger = logger;
        }

        public override async Task<object?> RunAsync(WorkflowActivityContext context, LoggerActivityInput input)
        {
            this.logger.LogInformation("WF [{InstanceId}]: {Input}", context.InstanceId, input.Message);

            await Task.Delay(5);

            return default;
        }
    }

    public record LoggerActivityInput(string Message);
}
