// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Threading.Tasks;
using DurableTask;

namespace Microsoft.Azure.WebJobs
{
    /// <summary>
    /// Parameter data for orchestration bindings that can be used to schedule function-based activities.
    /// </summary>
    public class OrchestrationInstanceContext : IOrchestrationReturnValue
    {
        private static readonly JsonDataConverter SharedJsonConverter = new JsonDataConverter();

        private readonly OrchestrationContext innerContext;
        private readonly string rawInput;

        private string returnValue;

        internal OrchestrationInstanceContext(OrchestrationContext innerContext, string rawInput)
        {
            this.innerContext = innerContext;
            this.rawInput = rawInput;
        }

        // Intended for use by internal callers.
        string IOrchestrationReturnValue.ReturnValue
        {
            get { return this.returnValue; }
        }

        // Intended for use by internal callers.
        void IOrchestrationReturnValue.SetReturnValue(object responseValue)
        {
            string stringResponseValue = responseValue as string;
            if (stringResponseValue != null || responseValue == null)
            {
                this.returnValue = stringResponseValue;
            }
            else
            {
                this.returnValue = SharedJsonConverter.Serialize(responseValue);
            }
        }

        /// <summary>
        /// Returns the input of the orchestration in it's raw string value.
        /// </summary>
        public string GetInput()
        {
            return this.rawInput;
        }

        /// <summary>
        /// Gets the input of the orchestration as a deserialized value of type <typeparam name="T"/>.
        /// </summary>
        public T GetInput<T>()
        {
            return SharedJsonConverter.Deserialize<T>(this.rawInput);
        }

        /// <summary>
        /// Schedules an activity named <paramref name="name"/> for execution.
        /// </summary>
        /// <typeparam name="TResult">The return type of the scheduled activity.</typeparam>
        public Task<TResult> ScheduleTask<TResult>(string name, params object[] parameters)
        {
            return this.innerContext.ScheduleTask<TResult>(name, string.Empty /* version */, parameters);
        }
    }
}
