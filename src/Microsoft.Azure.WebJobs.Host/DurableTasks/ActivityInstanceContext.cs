// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using DurableTask;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs
{
    /// <summary>
    /// Parameter data for activity bindings that are scheduled by their parent orchestrations.
    /// </summary>
    public class ActivityInstanceContext : IActivityReturnValue
    {
        private static readonly JsonDataConverter SharedJsonConverter = OrchestrationInstanceContext.SharedJsonConverter;

        private readonly TaskContext innerContext;
        private readonly string rawInput;

        private string returnValue;

        internal ActivityInstanceContext(TaskContext innerContext, string rawInput)
        {
            this.innerContext = innerContext;
            this.rawInput = rawInput;
        }

        /// <summary>
        /// Gets the <see cref="OrchestrationInstance"/> associated with the currently running activity task.
        /// </summary>
        [CLSCompliant(false)]
        public OrchestrationInstance OrchestrationInstance
        {
            get { return this.innerContext.OrchestrationInstance; }
        }

        // Intended for use by internal callers.
        string IActivityReturnValue.ReturnValue
        {
            get { return this.returnValue; }
        }

        // Intended for use by internal callers.
        void IActivityReturnValue.SetReturnValue(object responseValue)
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
        /// Returns the input of the task activity in its raw string value.
        /// </summary>
        public string GetInput()
        {
            return this.rawInput;
        }

        /// <summary>
        /// Gets the input of the task activity as a deserialized value of type <typeparam name="T"/>.
        /// </summary>
        public T GetInput<T>()
        {
            // Copied from DTFx Framework\TaskActivity.cs
            T parameter = default(T);
            JArray array = JArray.Parse(this.rawInput);
            if (array != null)
            {
                int parameterCount = array.Count;
                if (parameterCount > 1)
                {
                    throw new ArgumentException(
                        "Activity implementation cannot be invoked due to more than expected input parameters.  Signature mismatch.");
                }

                if (parameterCount == 1)
                {
                    JToken token = array[0];
                    var value = token as JValue;
                    if (value != null)
                    {
                        parameter = value.ToObject<T>();
                    }
                    else
                    {
                        string serializedValue = token.ToString();
                        parameter = SharedJsonConverter.Deserialize<T>(serializedValue);
                    }
                }
            }

            return parameter;
        }
    }
}
