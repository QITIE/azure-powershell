﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System.Management.Automation;
using Microsoft.Azure.Commands.DataLakeAnalytics.Models;

namespace Microsoft.Azure.Commands.DataLakeAnalytics
{
    [Cmdlet(VerbsCommon.Remove, "AzureRmDataLakeAnalyticsAccount"), OutputType(typeof(bool))]
    public class RemoveAzureDataLakeAnalyticsAccount : DataLakeAnalyticsCmdletBase
    {
        [Parameter(ValueFromPipelineByPropertyName = true, Position = 0, Mandatory = true, HelpMessage = "Name of account to be removed.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, Position = 1, Mandatory = false, HelpMessage = "Name of resource group under which the account exists.")]
        [ValidateNotNullOrEmpty]
        public string ResourceGroupName { get; set; }

        [Parameter(Mandatory = false, Position = 2, HelpMessage = "Do not ask for confirmation.")]
        public SwitchParameter Force { get; set; }

        [Parameter(Mandatory = false,  Position = 3)]
        public SwitchParameter PassThru { get; set; }

        protected override void ProcessRecord()
        {
            if (!Force.IsPresent)
            {
                ConfirmAction(
                Force.IsPresent,
                string.Format(Properties.Resources.RemovingDataLakeAnalyticsAccount, Name),
                string.Format(Properties.Resources.RemoveDataLakeAnalyticsAccount, Name),
                Name,
                () => DataLakeAnalyticsClient.DeleteAccount(ResourceGroupName, Name));
            }
            else
            {
                DataLakeAnalyticsClient.DeleteAccount(ResourceGroupName, Name);
            }

            if (PassThru)
            {
                WriteObject(true);
            }
        }
    }
}