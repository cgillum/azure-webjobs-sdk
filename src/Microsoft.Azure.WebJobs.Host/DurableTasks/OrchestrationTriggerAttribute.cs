// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Microsoft.Azure.WebJobs
{
    /// <summary>
    /// Attribute used to bind a parameter to a Durable Task Orchestration, causing the method to
    /// run when an orchestration is resumed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    [DebuggerDisplay("{TaskHub,nq}")]
    public sealed class OrchestrationTriggerAttribute : Attribute
    {
        private readonly string taskHub;
        private readonly string orchestration;
        private readonly string version;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrchestrationTriggerAttribute"/> class.
        /// </summary>
        /// <param name="taskHub">
        /// The name of the task hub in which the orchestration is running.
        /// </param>
        /// <param name="orchestration">The name of the orchestration to trigger.</param>
        /// <param name="version">The version of the orchestration to trigger.</param>
        public OrchestrationTriggerAttribute(string taskHub, string orchestration, string version = "")
        {
            if (string.IsNullOrEmpty(taskHub))
            {
                throw new ArgumentNullException("taskHub");
            }

            if (string.IsNullOrEmpty(orchestration))
            {
                throw new ArgumentNullException("orchestration");
            }

            this.taskHub = taskHub;
            this.orchestration = orchestration;
            this.version = version;
        }

        /// <summary>
        /// Gets the name of the task hub in which the orchestration is running.
        /// </summary>
        public string TaskHub
        {
            get { return this.taskHub; }
        }

        /// <summary>
        /// Gets the name of the orchestration.
        /// </summary>
        public string Orchestration
        {
            get { return this.orchestration; }
        }

        /// <summary>
        /// Gets the version of the orchestration.
        /// </summary>
        public string Version
        {
            get { return this.version; }
        }
    }
}
