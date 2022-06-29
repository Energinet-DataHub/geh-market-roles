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

using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace Messaging.Application.SchemaStore;

public class XmlSchemaProvider : SchemaProvider
{
    private readonly ISchema _schema;

    public XmlSchemaProvider()
    {
        _schema = new CimXmlSchemas();
    }

    public override Task<T?> GetSchemaAsync<T>(string businessProcessType, string version)
        where T : default
    {
        var schemaName = _schema.GetSchemaLocation(businessProcessType, version);

        if (schemaName == null)
        {
            return Task.FromResult(default(T));
        }

        return (Task<T?>)(object)LoadSchemaWithDependentSchemasAsync<XmlSchema>(schemaName);
    }

    protected override async Task<T?> LoadSchemaWithDependentSchemasAsync<T>(string location)
        where T : default
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
                await LoadSchemaWithDependentSchemasAsync<XmlSchema>(_schema.SchemaPath + external.SchemaLocation).ConfigureAwait(false);
        }

        return (T)(object)xmlSchema;
    }
}
