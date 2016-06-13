// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using DurableTask;

namespace Microsoft.Azure.WebJobs.Host.DurableTasks
{
    internal class DurableTaskWorkerContext
    {
        private readonly string hubName;
        private readonly string serviceBusConnectionString;
        private readonly string tableStorageConnectionString;
        private readonly List<ObjectCreator<TaskOrchestration>> orchestrations;

        public DurableTaskWorkerContext(
            string hubName,
            string serviceBusConnectionString,
            string tableStorageConnectionString)
        {
            this.hubName = hubName;
            this.serviceBusConnectionString = serviceBusConnectionString;
            this.tableStorageConnectionString = tableStorageConnectionString;
            this.orchestrations = new List<ObjectCreator<TaskOrchestration>>();
        }

        public string HubName
        {
            get { return this.hubName; }
        }

        public string ServiceBusConnectionString
        {
            get { return this.serviceBusConnectionString; }
        }

        public string TableStorageConnectionString
        {
            get { return this.tableStorageConnectionString; }
        }

        public IList<ObjectCreator<TaskOrchestration>> Orchestrations
        {
            get { return this.orchestrations; }
        }
    }
}
