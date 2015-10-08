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
using System.Text.RegularExpressions;
using Hyak.Common;

namespace Microsoft.Azure.Commands.DataLakeAnalytics.Models
{
    /// <summary>
    /// The object that is passed in and parsed for all dataLake paths.
    /// </summary>
    public class CatalogPathInstance
    {
        public string DatabaseName { get; set; }

        public string SchemaAssemblyOrExternalDataSourceName { get; set; }

        public string TableOrTableValuedFunctionName { get; set; }

        public string TableStatisticsName { get; set; }

        public string FullCatalogItemPath { get; set; }

        public static CatalogPathInstance Parse(string path)
        {
            // Catalog paths must all be in the format "firstPart<.optionalSecondPart><.OptionalThirdPart>"
            // First version does not support '.' within an element name.
            // if there are no '.' in the path, then the entire path is just the database
            if (!path.Contains("."))
            {
                return new CatalogPathInstance
                {
                    FullCatalogItemPath = path,
                    DatabaseName = path
                };
            }

            var regex =
                new Regex(
                    @"^(?<firstPart>\w+|\[[a-zA-Z_0-9.]+\])(\.(?<secondPart>\w+|\[[a-zA-Z_0-9.]+\]))?(\.(?<thirdPart>\w+|\[[a-zA-Z_0-9.]+\]))?\.(?<fourthPart>\w+|\[[a-zA-Z_0-9.]+\])$");

            if (!regex.IsMatch(path))
            {
                throw new CloudException(string.Format(Properties.Resources.InvalidCatalogPath, path));
            }

            var splitPath = regex.Match(path);

            string firstPart = GetSanitizedPath(splitPath.Groups["firstPart"].Value, path);
            string secondPart = GetSanitizedPath(splitPath.Groups["secondPart"].Value, path);
            string thirdPart = GetSanitizedPath(splitPath.Groups["thirdPart"].Value, path);
            string fourthPart = GetSanitizedPath(splitPath.Groups["fourthPart"].Value, path);

            // only two entries
            if (string.IsNullOrEmpty(secondPart) && string.IsNullOrEmpty(thirdPart))
            {
                secondPart = fourthPart;
                fourthPart = null;
            }
            else if (string.IsNullOrEmpty(thirdPart))
            {
                // three entries where the third part is not matched
                thirdPart = fourthPart;
                fourthPart = null;
            }
            else if (string.IsNullOrEmpty(secondPart))
            {
                // three entries where the second part is not matched
                secondPart = thirdPart;
                thirdPart = fourthPart;
                fourthPart = null;
            }

            return new CatalogPathInstance
            {
                DatabaseName = firstPart,
                SchemaAssemblyOrExternalDataSourceName = secondPart,
                TableOrTableValuedFunctionName = thirdPart,
                TableStatisticsName = fourthPart,
                FullCatalogItemPath = path
            };
        }

        private static string GetSanitizedPath(string path, string fullPath)
        {
            // this case is if there is no match, so we just return what we recieved
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }
            
            // in all other cases we will throw if there is '[]'
            if (path.StartsWith("[", StringComparison.InvariantCultureIgnoreCase) &&
                path.EndsWith("]", StringComparison.InvariantCultureIgnoreCase))
            {
                // remove first bracket
                path = path.Substring(1);
                // remove last bracket
                path = path.Substring(0, path.Length - 1);
            }

            // after trimming and removing external braces, if the string is now empty, it was an invalid path
            if (string.IsNullOrEmpty(path))
            {
                throw new CloudException(string.Format(Properties.Resources.InvalidCatalogPath, fullPath));
            }

            return path;
        }
    }
}