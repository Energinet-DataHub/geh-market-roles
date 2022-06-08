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
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Messaging.Application.OutgoingMessages;

namespace Messaging.Application.Common;

public abstract class DocumentWriter<TMarketActivityRecord>
{
    private readonly DocumentDetails _documentDetails;

    protected DocumentWriter(DocumentDetails documentDetails)
    {
        _documentDetails = documentDetails;
    }

    public async Task<Stream> WriteAsync(MessageHeader header, IReadOnlyCollection<TMarketActivityRecord> marketActivityRecords)
    {
        var settings = new XmlWriterSettings { OmitXmlDeclaration = false, Encoding = Encoding.UTF8, Async = true };
        var stream = new MemoryStream();
        using var writer = XmlWriter.Create(stream, settings);
        await WriteHeaderAsync(header, _documentDetails, writer).ConfigureAwait(false);
        await WriteMarketActivityRecordsAsync(marketActivityRecords, writer).ConfigureAwait(false);
        await WriteEndAsync(writer).ConfigureAwait(false);
        stream.Position = 0;
        return stream;
    }

    protected abstract Task WriteMarketActivityRecordsAsync(IReadOnlyCollection<TMarketActivityRecord> marketActivityPayloads, XmlWriter writer);

    private static Task WriteHeaderAsync(MessageHeader header, DocumentDetails documentDetails, XmlWriter writer)
    {
        return HeaderWriter.WriteAsync(writer, header, documentDetails);
    }

    private static async Task WriteEndAsync(XmlWriter writer)
    {
        await writer.WriteEndElementAsync().ConfigureAwait(false);
        writer.Close();
    }
}