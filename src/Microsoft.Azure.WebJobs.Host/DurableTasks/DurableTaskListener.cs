// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DurableTask;
using Microsoft.Azure.WebJobs.Host.Listeners;

namespace Microsoft.Azure.WebJobs.Host.DurableTasks
{
    internal sealed class DurableTaskListener : IListener
    {
        private static readonly ConcurrentDictionary<string, DurableTaskListener> SharedListeners =
            new ConcurrentDictionary<string, DurableTaskListener>(StringComparer.OrdinalIgnoreCase);

        private readonly TaskHubWorker worker;
        private bool isStarted;

        private DurableTaskListener(DurableTaskWorkerContext workerContext)
        {
            this.worker = new TaskHubWorker(
                workerContext.HubName,
                workerContext.ServiceBusConnectionString,
                workerContext.TableStorageConnectionString);
        }

        public static DurableTaskListener GetOrAddSharedListener(DurableTaskWorkerContext workerContext)
        {
            // One worker per task hub
            DurableTaskListener listener = SharedListeners.GetOrAdd(workerContext.HubName, hubName => new DurableTaskListener(workerContext));
            listener.worker.AddTaskOrchestrations(workerContext.Orchestrations.ToArray());
            listener.worker.AddTaskActivities(workerContext.Activities.ToArray());
            return listener;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            lock (this.worker)
            {
                if (!this.isStarted)
                {
                    this.worker.CreateHubIfNotExists();
                    this.worker.Start();
                    this.isStarted = true;
                }
            }

            return Task.FromResult(0);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // REVIEW: When does StopAsync get called? Is it possible that a shared listener will be stopped
            //         by one function's lifecycle event but needs to remain active for others?
            lock (this.worker)
            {
                if (this.isStarted)
                {
                    this.worker.Stop();
                    this.isStarted = false;
                }
            }

            return Task.FromResult(0);
        }

        public void Cancel()
        {
            // REVIEW: When does StopAsync get called? Is it possible that a shared listener will be stopped
            //         by one function's lifecycle event but needs to remain active for others?
            lock (this.worker)
            {
                if (this.isStarted)
                {
                    this.worker.Stop(isForced: true);
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
