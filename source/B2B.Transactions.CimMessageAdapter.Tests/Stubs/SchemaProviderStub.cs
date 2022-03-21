﻿// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using B2B.CimMessageAdapter;

namespace MarketRoles.B2B.CimMessageAdapter.IntegrationTests.Stubs
{
    public class SchemaProviderStub : ISchemaProvider
    {
        public Task<XmlSchema?> GetSchemaAsync(string businessProcessType, string version)
        {
            var schemas = new Dictionary<KeyValuePair<string, string>, string>()
            {
                {
                    new KeyValuePair<string, string>("requestchangeofsupplier", "1.0"),
                    "urn-ediel-org-structure-requestchangeofsupplier-0-1.xsd"
                },
            };

            if (schemas.TryGetValue(new KeyValuePair<string, string>(businessProcessType, version), out var schemaName) == false)
            {
                return Task.FromResult(default(XmlSchema));
            }

            return LoadSchemaWithDependentSchemasAsync(schemaName);
        }

        private static async Task<XmlSchema?> LoadSchemaWithDependentSchemasAsync(string location)
        {
            using var reader = new XmlTextReader(location);
            var xmlSchema = XmlSchema.Read(reader, null);
            if (xmlSchema is null)
            {
                throw new XmlSchemaException($"Could not read schema at {location}");
            }

            foreach (XmlSchemaExternal external in xmlSchema.Includes)
            {
                if (external.SchemaLocation == null)
                {
                    continue;
                }

                external.Schema =
                    await LoadSchemaWithDependentSchemasAsync(external.SchemaLocation).ConfigureAwait(false);
            }

            return xmlSchema;
        }
    }
}