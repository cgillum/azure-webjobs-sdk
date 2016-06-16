// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Microsoft.Azure.WebJobs.Host.DurableTasks
{
    /// <summary>
    /// Attribute used to bind a parameter to a Durable Task Orchestration, causing the method to
    /// run when an orchestration is resumed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    [DebuggerDisplay("{TaskHub,nq}")]
    public sealed class OrchestrationClientAttribute : Attribute
    {
        private readonly string taskHub;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrchestrationClientAttribute"/> class.
        /// </summary>
        /// <param name="taskHub">The name of the task hub to be operated on.</param>
        public OrchestrationClientAttribute(string taskHub)
        {
            if (string.IsNullOrEmpty(taskHub))
            {
                throw new ArgumentNullException("taskHub");
            }

            this.taskHub = taskHub;
        }

        /// <summary>
        /// Gets the name of the task hub to be operated on.
        /// </summary>
        public string TaskHub
        {
            get { return this.taskHub; }
        }
    }
}
