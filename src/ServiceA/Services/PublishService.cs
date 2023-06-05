using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using Services.Core.Tags.Interface.Events;

namespace ServiceA.Services
{
    internal class PublishService : BackgroundService
    {
        private readonly DaprClient daprClient;
        private readonly ILogger<PublishService> logger;

        public PublishService(
            DaprClient daprClient,
            ILogger<PublishService> logger)
        {
            this.daprClient = daprClient;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // Wait a minute before starting synchronization to let services have
            // time to start up.
            await Task.Delay(10 * 1000, cancellationToken).ConfigureAwait(false);

            var ids = new List<Guid>()
            {
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()
            };

            while (!cancellationToken.IsCancellationRequested)
            {
                this.logger.LogInformation("Starting Publish");

                for (int i = 0; i < ids.Count; i++)
                {
                    var index = Random.Shared.Next(0, ids.Count - 1);
                    var id = ids[index];

                    this.logger.LogInformation("Publishing {Id}", id);
                    await this.PublishEventAsync(new ContentChangeEvent(
                        Timestamp: DateTime.UtcNow,
                        Id: id
                    ));

                    await Task.Delay(TimeSpan.FromSeconds(2));
                }

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken).ConfigureAwait(false);
            }
        }

        public Task PublishEventAsync<TData>(TData data, CancellationToken cancellationToken = default)
        {
            _ = data ?? throw new ArgumentNullException(nameof(data));

            var topicName = data.GetType().Name;
            //this.logger.LogInformation("Publishing to {TopicName}", topicName);
            return this.daprClient.PublishEventAsync("pubsub", topicName, data, cancellationToken);
        }
    }
}
