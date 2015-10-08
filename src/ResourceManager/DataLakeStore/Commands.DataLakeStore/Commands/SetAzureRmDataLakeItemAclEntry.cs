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

using System;
using System.Management.Automation;
using Microsoft.Azure.Commands.DataLakeStore.Models;

namespace Microsoft.Azure.Commands.DataLakeStore
{
    [Cmdlet(VerbsCommon.Set, "AzureRmDataLakeStoreItemAclEntry"), OutputType(typeof(bool))]
    public class SetAzureDataLakeStoreItemAclEntry : DataLakeStoreFileSystemCmdletBase
    {
        internal const string BaseParameterSetName = "Set ACL Entries using ACL object";
        internal const string SpecificAceParameterSetName = "Set specific ACE";

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = BaseParameterSetName, Position = 0, Mandatory = true, HelpMessage = "The DataLakeStore account to execute the filesystem operation in")]
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = SpecificAceParameterSetName, Position = 0, Mandatory = true, HelpMessage = "The DataLakeStore account to execute the filesystem operation in")]
        [ValidateNotNullOrEmpty]
        public string AccountName { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = BaseParameterSetName, Position = 1, Mandatory = true, HelpMessage = "The path in the specified dataLake account that should have ACL entries set. Can be a file or folder " +
                                                                                           "In the format 'webhdfs://<accountName>.dataLakeaccountdogfood.net/folder/file.txt', " +
                                                                                           "where the first '/' after the DNS indicates the root of the file system.")]
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = SpecificAceParameterSetName, Position = 1, Mandatory = true, HelpMessage = "The path in the specified dataLake account that should have ACL entries set. Can be a file or folder " +
                                                                                           "In the format 'webhdfs://<accountName>.dataLakeaccountdogfood.net/folder/file.txt', " +
                                                                                           "where the first '/' after the DNS indicates the root of the file system.")]
        [ValidateNotNull]
        public DataLakeStorePathInstance Path { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = BaseParameterSetName, Position = 2, Mandatory = true, HelpMessage = "The ACL spec containing the entries to set. These entries MUST exist in the ACL spec for the file already. This can be a modified ACL from Get-AzureDataLakeStoreItemAcl or it can be the string " +
                                                                                           " representation of an ACL as defined in the apache webhdfs specification. Note that this is only supported for named ACEs." +
                                                                                           "This cmdlet is not to be used for setting the owner or owning group.")]
        public DataLakeStoreItemAcl Acl { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = SpecificAceParameterSetName, Position = 2, Mandatory = true, HelpMessage = "Indicates the type of ACE to set (user, group, mask, other)")]
        public DataLakeStoreEnums.AceType AceType { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = SpecificAceParameterSetName, Position = 3, Mandatory = false, HelpMessage = "The identity of the user or group to set. Optional. If none is passed this will attempt to set an unamed ACE, which is necessary for both mask and other ACEs")]
        [ValidateNotNullOrEmpty]
        public Guid Id { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = SpecificAceParameterSetName, Position = 4, Mandatory = true, HelpMessage = "The permissions to set for the ACE")]
        [ValidateNotNull]
        
        public DataLakeStoreEnums.Permission Permissions { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = SpecificAceParameterSetName, Position = 4, Mandatory = false, HelpMessage = "Indicates that the ACL entry is a default ACE to be set.")]
        public SwitchParameter Default { get; set; }

        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = BaseParameterSetName, Position = 3, Mandatory = false, HelpMessage = "Indicates that the ACL entries should be set on the file with the specified ACL without prompting.")]
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = SpecificAceParameterSetName, Position = 5, Mandatory = false, HelpMessage = "Indicates that the ACL entries should be set on the file with the specified ACL without prompting.")]
        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            var aclSpec = this.ParameterSetName.Equals(BaseParameterSetName)
                ? this.Acl.GetAclSpec()
                : string.Format("{0}{1}:{2}:{3}", this.Default ? "default:" : string.Empty, AceType, Id,
                    DataLakeStoreItemPermissionInstance.GetPermissionString(this.Permissions)).ToLowerInvariant();

            if (!Force.IsPresent)
            {
                ConfirmAction(
                    Force.IsPresent,
                    string.Format(Properties.Resources.SettingDataLakeStoreItemAcl, Path.FullyQualifiedPath),
                    string.Format(Properties.Resources.SetDataLakeStoreItemAcl, Path.FullyQualifiedPath),
                    Path.FullyQualifiedPath,
                    () =>
                        DataLakeStoreFileSystemClient.ModifyAcl(Path.Path, AccountName,
                            aclSpec));
            }
            else
            {
                DataLakeStoreFileSystemClient.ModifyAcl(Path.Path, AccountName,
                            aclSpec);
            }
        }
    }
}