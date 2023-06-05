using Castle.Core.Logging;
using Dapr.Workflow;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using ServiceB.Workflows.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceB.Workflows.Test
{
    public class TestWorkflow : Workflow<TestWorkflowInput, object?>
    {
        public TestWorkflow()
        {            
        }

        public override async Task<object?> RunAsync(WorkflowContext context, TestWorkflowInput input)
        {
            if (!context.IsReplaying) Console.WriteLine($"WF [{context.InstanceId}]: Entering {nameof(TestWorkflow)}");

            var retryPolicy = new WorkflowTaskOptions(new WorkflowRetryPolicy(
                maxNumberOfAttempts: 10,
                firstRetryInterval: TimeSpan.FromMinutes(1),
                backoffCoefficient: 2.0,
                maxRetryInterval: TimeSpan.FromHours(1)));

            try
            {
                await context.CallActivityAsync<object?>(
                    name: nameof(LoggerActivity),
                    input: new LoggerActivityInput(
                        Message: $"Doing some work"),
                    options: retryPolicy
                );

                await context.CallActivityAsync<object?>(
                    name: nameof(DelayActivity),
                    input: new DelayActivityInput(
                        Delay: TimeSpan.FromSeconds(30)),
                    options: retryPolicy
                );

                await context.CallActivityAsync<object?>(
                    name: nameof(LoggerActivity),
                    input: new LoggerActivityInput(
                        Message: $"Doing some more work"),
                    options: retryPolicy
                );
            }
            catch (TaskFailedException)
            {
                if (!context.IsReplaying) Console.WriteLine($"WF [{context.InstanceId}]: Exiting {nameof(TestWorkflow)} due to {nameof(TaskFailedException)}");
                return null;
            }

            if (!context.IsReplaying) Console.WriteLine($"WF [{context.InstanceId}]: Exiting {nameof(TestWorkflow)}");

            return default;
        }
    }

    public record TestWorkflowInput(
        Guid Id
    );
}
