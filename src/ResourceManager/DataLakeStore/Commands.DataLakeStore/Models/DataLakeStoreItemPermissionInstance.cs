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
using System.Collections.Generic;
using System.Linq;
using Hyak.Common;

namespace Microsoft.Azure.Commands.DataLakeStore.Models
{
    /// <summary>
    /// The object that is used to manage permissions for files and folders.
    /// </summary>
    public class DataLakeStoreItemPermissionInstance
    {
        public Dictionary<DataLakeStoreEnums.PermissionScope, DataLakeStoreEnums.Permission> Permissions { get; set; }

        public string PermissionsOctal { get; set; }

        public static DataLakeStoreItemPermissionInstance Parse(string permissions)
        {
            try
            {
                var convertedPermissions = string.Empty;
                
                if (permissions.Length == 3)
                {
                    // assume user passed in the octal
                    convertedPermissions = permissions;
                }
                else if (permissions.Length == 9)
                {
                    // confirm the string is valid
                    if (!permissions.ToLowerInvariant().All(characters => "rwx-".Contains(characters)))
                    {
                        throw new CloudException(string.Format(Properties.Resources.InvalidPermissionString, permissions));
                    }

                    // convert rwxrwxrwx into octal
                    int charsRead = 0;
                    int eachPermission = 0;
                    foreach (char character in permissions)
                    {
                        switch (character)
                        {
                            case 'r':
                                eachPermission += (int) DataLakeStoreEnums.Permission.Read;
                                break;
                            case 'w':
                                eachPermission += (int) DataLakeStoreEnums.Permission.Write;
                                break;
                            case 'x':
                                eachPermission += (int)DataLakeStoreEnums.Permission.Execute;
                                break;
                        }

                        charsRead++;

                        if (charsRead%3 == 0)
                        {
                            convertedPermissions += eachPermission;
                            eachPermission = 0;
                        }
                    }
                }
                else
                {
                    throw new CloudException(string.Format(Properties.Resources.InvalidPermissionString, permissions));
                }

                // Now do the conversion into a short
                var parsedShort = short.Parse(convertedPermissions);

                if (parsedShort < 0 || parsedShort > 777)
                {
                    throw new CloudException(string.Format(Properties.Resources.InvalidPermissionString, permissions));
                }

                // Create the friendly permissions list
                var friendlyPermissions = new Dictionary<DataLakeStoreEnums.PermissionScope, DataLakeStoreEnums.Permission>
                {
                    {DataLakeStoreEnums.PermissionScope.User, (DataLakeStoreEnums.Permission) int.Parse(convertedPermissions[0].ToString())},
                    {DataLakeStoreEnums.PermissionScope.Group, (DataLakeStoreEnums.Permission) int.Parse(convertedPermissions[1].ToString())},
                    {DataLakeStoreEnums.PermissionScope.Other, (DataLakeStoreEnums.Permission) int.Parse(convertedPermissions[2].ToString())}
                };

                return new DataLakeStoreItemPermissionInstance
                {
                    PermissionsOctal = convertedPermissions,
                    Permissions = friendlyPermissions
                };
            }
            catch (Exception)
            {

                throw new CloudException(string.Format(Properties.Resources.InvalidPermissionString, permissions));
            }
            
        }

        internal static string GetPermissionString(DataLakeStoreEnums.Permission permission)
        {
            switch (permission)
            {
                case DataLakeStoreEnums.Permission.All:
                    return "rwx";
                case DataLakeStoreEnums.Permission.Execute:
                    return "--x";
                case DataLakeStoreEnums.Permission.None:
                    return "---";
                case DataLakeStoreEnums.Permission.Read:
                    return "r--";
                case DataLakeStoreEnums.Permission.ReadExecute:
                    return "r-x";
                case DataLakeStoreEnums.Permission.ReadWrite:
                    return "rw-";
                case DataLakeStoreEnums.Permission.Write:
                    return "-w-";
                case DataLakeStoreEnums.Permission.WriteExecute:
                    return "-wx";
                default:
                    throw new CloudException(string.Format(Properties.Resources.InvalidPermissionType, permission));
            }
        }
    }
}