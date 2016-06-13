// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DurableTask;
using Microsoft.Azure.WebJobs.Host.Listeners;

namespace Microsoft.Azure.WebJobs.Host.DurableTasks
{
    internal sealed class DurableTaskListener : IListener
    {
        private readonly TaskHubWorker worker;

        public DurableTaskListener(DurableTaskWorkerContext workerContext)
        {
            this.worker = new TaskHubWorker(
                workerContext.HubName,
                workerContext.ServiceBusConnectionString,
                workerContext.TableStorageConnectionString);
            this.worker.AddTaskOrchestrations(workerContext.Orchestrations.ToArray());
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.worker.CreateHubIfNotExists();
            this.worker.Start();
            return Task.FromResult(0);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.worker.Stop();
            return Task.FromResult(0);
        }

        public void Cancel()
        {
            this.worker.Stop(isForced: true);
        }

        public void Dispose()
        {
        }
    }
}
