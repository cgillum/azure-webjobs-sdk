// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Protocols;

namespace Microsoft.Azure.WebJobs.Host.DurableTasks
{
    internal class OrchestrationClientBindingProvider : IBindingProvider
    {
        public Task<IBinding> TryCreateAsync(BindingProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            // Determine whether we should bind to the current parameter
            ParameterInfo parameter = context.Parameter;
            OrchestrationClientAttribute attribute = parameter.GetCustomAttribute<OrchestrationClientAttribute>(inherit: false);
            if (attribute == null)
            {
                return Task.FromResult<IBinding>(null);
            }

            if (parameter.ParameterType != typeof(OrchestrationClientContext))
            {
                return Task.FromResult<IBinding>(null);
            }

            string serviceBusConnectionString = AmbientConnectionStringProvider.Instance.GetConnectionString(ConnectionStringNames.ServiceBus);
            if (string.IsNullOrEmpty(serviceBusConnectionString))
            {
                // Service Bus connection string is required.
                return Task.FromResult<IBinding>(null);
            }

            string tableStorageConnectionString = AmbientConnectionStringProvider.Instance.GetConnectionString(ConnectionStringNames.Storage);

            return Task.FromResult<IBinding>(new OrchestrationClientBinding(
                parameter.Name,
                attribute.TaskHub,
                serviceBusConnectionString,
                tableStorageConnectionString));
        }

        private class OrchestrationClientBinding : IBinding
        {
            private readonly string parameterName;
            private readonly string taskHub;
            private readonly string serviceBusConnectionString;
            private readonly string tableStorageConnectionString;

            public OrchestrationClientBinding(
                string parameterName,
                string taskHub,
                string serviceBusConnectionString,
                string tableStorageConnectionString)
            {
                this.parameterName = parameterName;
                this.taskHub = taskHub;
                this.serviceBusConnectionString = serviceBusConnectionString;
                this.tableStorageConnectionString = tableStorageConnectionString;
            }

            public bool FromAttribute
            {
                get { return true; }
            }

            public Task<IValueProvider> BindAsync(BindingContext context)
            {
                var clientContext = new OrchestrationClientContext(
                    this.taskHub,
                    this.serviceBusConnectionString,
                    this.tableStorageConnectionString);

                var valueProvider = new ObjectValueProvider(clientContext, clientContext.GetType());
                return Task.FromResult<IValueProvider>(valueProvider);
            }

            public Task<IValueProvider> BindAsync(object value, ValueBindingContext context)
            {
                var valueProvider = new ObjectValueProvider(value, value != null ? value.GetType() : typeof(object));
                return Task.FromResult<IValueProvider>(valueProvider);
            }

            public ParameterDescriptor ToParameterDescriptor()
            {
                return new ParameterDescriptor
                {
                    Name = this.parameterName
                };
            }
        }
    }
}
