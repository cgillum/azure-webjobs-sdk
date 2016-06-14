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
    public sealed class ActivityTriggerAttribute : Attribute
    {
        private readonly string taskHub;
        private readonly string activity;
        private readonly string version;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityTriggerAttribute"/> class.
        /// </summary>
        /// <param name="taskHub">
        /// The name of the task hub in which the activity is running.
        /// </param>
        /// <param name="activity">The name of the activity trigger.</param>
        /// <param name="version">The version of the activity trigger.</param>
        public ActivityTriggerAttribute(string taskHub, string activity, string version = "")
        {
            if (string.IsNullOrEmpty(taskHub))
            {
                throw new ArgumentNullException("taskHub");
            }

            if (string.IsNullOrEmpty(activity))
            {
                throw new ArgumentNullException("activity");
            }

            this.taskHub = taskHub;
            this.activity = activity;
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
        /// Gets the name of the activity.
        /// </summary>
        public string Activity
        {
            get { return this.activity; }
        }

        /// <summary>
        /// Gets the version of the activity.
        /// </summary>
        public string Version
        {
            get { return this.version; }
        }
    }
}
