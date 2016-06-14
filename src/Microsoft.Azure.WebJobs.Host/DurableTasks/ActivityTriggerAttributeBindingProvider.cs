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
    internal class ActivityTriggerAttributeBindingProvider : ITriggerBindingProvider
    {
        public ActivityTriggerAttributeBindingProvider()
        {
        }

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ParameterInfo parameter = context.Parameter;
            ActivityTriggerAttribute trigger = parameter.GetCustomAttribute<ActivityTriggerAttribute>(inherit: false);
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

            var binding = new ActivityTriggerBinding(
                parameter,
                serviceBusConnectionString,
                tableStorageConnectionString,
                trigger.TaskHub,
                trigger.Activity,
                trigger.Version);
            return Task.FromResult<ITriggerBinding>(binding);
        }

        private class ActivityTriggerBinding : ITriggerBinding
        {
            private readonly ParameterInfo parameterInfo;
            private readonly string serviceBusConnectionString;
            private readonly string tableStorageConnectionString;
            private readonly string taskHub;
            private readonly string activityName;
            private readonly string version;

            public ActivityTriggerBinding(
                ParameterInfo parameterInfo,
                string serviceBusConnectionString,
                string tableStorageConnectionString,
                string taskHub,
                string activityName,
                string version)
            {
                this.parameterInfo = parameterInfo;
                this.serviceBusConnectionString = serviceBusConnectionString;
                this.tableStorageConnectionString = tableStorageConnectionString;
                this.taskHub = taskHub;
                this.activityName = activityName;
                this.version = version;
            }

            public IReadOnlyDictionary<string, Type> BindingDataContract
            {
                get { return null; }
            }

            public Type TriggerValueType
            {
                get { return typeof(ActivityInstanceContext); }
            }

            public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
            {
                // No conversions
                return Task.FromResult<ITriggerData>(new TriggerData(new ObjectValueProvider(value, this.TriggerValueType), null));
            }

            public ParameterDescriptor ToParameterDescriptor()
            {
                return new ParameterDescriptor { Name = this.parameterInfo.Name };
            }

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

                var activityCreator = new FunctionActivityCreator(
                    this.activityName,
                    this.version,
                    context.Executor);
                durableTaskContext.Activities.Add(activityCreator);

                DurableTaskListener listener = DurableTaskListener.GetOrAddSharedListener(durableTaskContext);
                return Task.FromResult<IListener>(listener);
            }

            private class FunctionActivityCreator : ObjectCreator<TaskActivity>
            {
                private readonly ITriggeredFunctionExecutor executor;

                public FunctionActivityCreator(
                    string activityName,
                    string version,
                    ITriggeredFunctionExecutor executor)
                {
                    this.executor = executor;
                    this.Name = activityName;
                    this.Version = version;
                }

                public override TaskActivity Create()
                {
                    return new FunctionShimTaskActivity(this.executor);
                }

                private class FunctionShimTaskActivity : TaskActivity
                {
                    private readonly ITriggeredFunctionExecutor executor;

                    public FunctionShimTaskActivity(ITriggeredFunctionExecutor executor)
                    {
                        this.executor = executor;
                    }

                    public override async Task<string> RunAsync(TaskContext context, string rawInput)
                    {
                        var contextWrapper = new ActivityInstanceContext(context, rawInput);
                        var triggerInput = new TriggeredFunctionData { TriggerValue = contextWrapper };

                        FunctionResult result = await this.executor.TryExecuteAsync(triggerInput, CancellationToken.None);
                        if (!result.Succeeded && result.Exception != null)
                        {
                            // Preserve the original exception context so that the durable task
                            // framework can report useful failure information.
                            ExceptionDispatchInfo.Capture(result.Exception).Throw();
                        }

                        return ((IActivityReturnValue)contextWrapper).ReturnValue;
                    }

                    public override string Run(TaskContext context, string input)
                    {
                        // This won't get called as long as we've implemented RunAsync.
                        throw new NotImplementedException();
                    }
                }
            }
        }
    }
}
