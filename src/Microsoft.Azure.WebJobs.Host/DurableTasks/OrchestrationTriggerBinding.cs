// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using DurableTask;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;

namespace Microsoft.Azure.WebJobs.Host.DurableTasks
{
    internal class OrchestrationTriggerBinding : ITriggerBinding
    {
        private readonly ParameterInfo parameterInfo;
        private readonly string serviceBusConnectionString;
        private readonly string tableStorageConnectionString;
        private readonly string taskHub;
        private readonly string orchestrationName;
        private readonly string version;

        public OrchestrationTriggerBinding(
            ParameterInfo parameterInfo,
            string serviceBusConnectionString,
            string tableStorageConnectionString,
            string taskHub,
            string orchestrationName,
            string version)
        {
            this.parameterInfo = parameterInfo;
            this.serviceBusConnectionString = serviceBusConnectionString;
            this.tableStorageConnectionString = tableStorageConnectionString;
            this.taskHub = taskHub;
            this.orchestrationName = orchestrationName;
            this.version = version;
        }

        public IReadOnlyDictionary<string, Type> BindingDataContract
        {
            get { return null; }
        }

        public Type TriggerValueType
        {
            get { return typeof(OrchestrationInstanceContext); }
        }

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            // No conversions
            return Task.FromResult<ITriggerData>(new TriggerData(new ObjectValueProvider(value, this.TriggerValueType), null));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            var durableTaskContext = new DurableTaskWorkerContext(
                this.taskHub,
                this.serviceBusConnectionString,
                this.tableStorageConnectionString);

            var orchestrationCreator = new FunctionOrchestrationCreator(
                this.orchestrationName,
                this.version,
                context.Executor);
            durableTaskContext.Orchestrations.Add(orchestrationCreator);

            var listener = new DurableTaskListener(durableTaskContext);
            return Task.FromResult<IListener>(listener);
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new ParameterDescriptor { Name = this.parameterInfo.Name };
        }

        private class FunctionOrchestrationCreator : ObjectCreator<TaskOrchestration>
        {
            private readonly ITriggeredFunctionExecutor executor;

            public FunctionOrchestrationCreator(
                string orchestrationName,
                string version,
                ITriggeredFunctionExecutor executor)
            {
                this.executor = executor;
                this.Name = orchestrationName;
                this.Version = version;
            }

            public override TaskOrchestration Create()
            {
                return new FunctionShimTaskOrchestration(this.executor);
            }

            private class FunctionShimTaskOrchestration : TaskOrchestration
            {
                private readonly ITriggeredFunctionExecutor executor;

                public FunctionShimTaskOrchestration(ITriggeredFunctionExecutor executor)
                {
                    this.executor = executor;
                }

                public override async Task<string> Execute(OrchestrationContext context, string rawInput)
                {
                    var contextWrapper = new OrchestrationInstanceContext(context, rawInput);
                    var triggerInput = new TriggeredFunctionData { TriggerValue = contextWrapper };

                    FunctionResult result = await this.executor.TryExecuteAsync(triggerInput, CancellationToken.None);
                    if (!result.Succeeded && result.Exception != null)
                    {
                        // Preserve the original exception context so that the durable task
                        // framework can report useful failure information.
                        ExceptionDispatchInfo.Capture(result.Exception).Throw();
                    }

                    return ((IOrchestrationReturnValue)contextWrapper).ReturnValue;
                }

                public override void RaiseEvent(OrchestrationContext context, string name, string input)
                {
                    // TODO, cgillum: Figure out how RaiseEvent maps to functions
                }

                public override string GetStatus()
                {
                    // TODO, cgillum: Figure out what GetStatus is used for and how/whether it maps to functions.
                    return null;
                }
            }
        }
    }
}
