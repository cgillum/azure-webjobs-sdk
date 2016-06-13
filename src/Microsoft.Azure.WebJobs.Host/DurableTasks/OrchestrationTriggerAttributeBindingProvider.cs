// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Triggers;

namespace Microsoft.Azure.WebJobs.Host.DurableTasks
{
    internal class OrchestrationTriggerAttributeBindingProvider : ITriggerBindingProvider
    {
        public OrchestrationTriggerAttributeBindingProvider()
        {
        }

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ParameterInfo parameter = context.Parameter;
            OrchestrationTriggerAttribute trigger = parameter.GetCustomAttribute<OrchestrationTriggerAttribute>(inherit: false);
            if (trigger == null)
            {
                return Task.FromResult<ITriggerBinding>(null);
            }

            string serviceBusConnectionString = AmbientConnectionStringProvider.Instance.GetConnectionString(ConnectionStringNames.ServiceBus);
            if (string.IsNullOrEmpty(serviceBusConnectionString))
            {
                // Service Bus connection string is required.
                return Task.FromResult<ITriggerBinding>(null);
            }

            string tableStorageConnectionString = AmbientConnectionStringProvider.Instance.GetConnectionString(ConnectionStringNames.Storage);

            var binding = new OrchestrationTriggerBinding(
                parameter,
                serviceBusConnectionString,
                tableStorageConnectionString,
                trigger.TaskHub,
                trigger.Orchestration,
                trigger.Version);
            return Task.FromResult<ITriggerBinding>(binding);
        }
    }
}
