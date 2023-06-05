
using Dapr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceB.Services;
using ServiceB.Workflows.Test;
using Services.Core.Tags.Interface.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceB.Controllers
{
    [ApiController]
    public class DaprSubscriptionsController : ControllerBase
    {
        private readonly IWorkflowSchedulerService workflowScheduler;

        private readonly ILogger<DaprSubscriptionsController> logger;

        public DaprSubscriptionsController(
            IWorkflowSchedulerService workflowScheduler,
            ILogger<DaprSubscriptionsController> logger)
        {
            this.workflowScheduler = workflowScheduler;
            this.logger = logger;            
        }

        [Topic("pubsub", nameof(ContentChangeEvent))]
        [HttpPost(nameof(ContentChanged))]
        public async Task ContentChanged(ContentChangeEvent evt, CancellationToken cancellationToken)
        {
            _ = evt ?? throw new ArgumentNullException(nameof(evt), "Payload was not supplied for ZonesChangedEvent topic.");
            
            await this.workflowScheduler.ScheduleTestWorkflow(
                new TestWorkflowInput(
                    Id: evt.Id
                )
            ).ConfigureAwait(false);     
        }
    }
}
