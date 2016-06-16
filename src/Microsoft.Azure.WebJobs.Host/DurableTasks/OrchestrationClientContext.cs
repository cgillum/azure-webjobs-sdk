// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using DurableTask;

namespace Microsoft.Azure.WebJobs
{
    /// <summary>
    /// Context object for interacting with orchestration instances.
    /// </summary>
    public class OrchestrationClientContext
    {
        private readonly TaskHubClient client;

        internal OrchestrationClientContext(
            string taskHub,
            string serviceBusConnectionString,
            string tableStorageConnectionString)
        {
            this.client = new TaskHubClient(
                taskHub,
                serviceBusConnectionString,
                tableStorageConnectionString,
                new TaskHubClientSettings
                {
                    DataConverter = OrchestrationInstanceContext.SharedJsonConverter
                });
        }

        /// <summary>
        /// Creates a new orchestration instance of the specified name and version.
        /// </summary>
        /// <param name="name">The name of the orchestration to create.</param>
        /// <param name="version">The version of the named orchestration to create.</param>
        /// <param name="input">Input data for the created orchestration.</param>
        public async Task<string> CreateOrchestrationInstanceAsync(string name, string version, object input)
        {
            OrchestrationInstance instance = await this.client.CreateOrchestrationInstanceAsync(name, version, input);
            return instance.InstanceId;
        }

        /// <summary>
        /// Sends an event to a running orchestration instance.
        /// </summary>
        /// <param name="instanceId">The ID of the orchestration instance to receive the event.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="eventData">The data associated with the event.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        public async Task RaiseEventAsync(string instanceId, string eventName, object eventData)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                throw new ArgumentNullException("eventName");
            }

            OrchestrationInstance instance = await this.GetOrchestrationInstanceAsync(instanceId);
            await this.client.RaiseEventAsync(instance, eventName, eventData);
        }

        /// <summary>
        /// Terminates a running orchestration instance.
        /// </summary>
        /// <param name="instanceId">The ID of the orchestration instance to terminate.</param>
        /// <param name="reason">The reason for terminating the orchestration instance.</param>
        public async Task TerminateInstanceAsync(string instanceId, string reason)
        {
            OrchestrationInstance instance = await this.GetOrchestrationInstanceAsync(instanceId);
            await this.client.TerminateInstanceAsync(instance, reason);
        }

        private async Task<OrchestrationInstance> GetOrchestrationInstanceAsync(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId))
            {
                throw new ArgumentNullException("instanceId");
            }

            OrchestrationState state = await this.client.GetOrchestrationStateAsync(instanceId);
            if (state == null || state.OrchestrationInstance == null)
            {
                throw new ArgumentException(string.Format("No instance with ID '{0}' was found.", instanceId), "instanceId");
            }

            return state.OrchestrationInstance;
        }
    }
}
