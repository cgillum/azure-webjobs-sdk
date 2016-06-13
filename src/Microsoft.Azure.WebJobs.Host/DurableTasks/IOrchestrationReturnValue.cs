// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.WebJobs
{
    /// <summary>
    /// Intended for internal use only.
    /// </summary>
    public interface IOrchestrationReturnValue
    {
        /// <summary>
        /// Gets the return value of the orchestration.
        /// </summary>
        string ReturnValue { get; }

        /// <summary>
        /// Sets the return value of the orchestration.
        /// </summary>
        void SetReturnValue(object returnValue);
    }
}
