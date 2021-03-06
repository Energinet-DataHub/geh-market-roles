// Copyright 2020 Energinet DataHub A/S
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Schema;
using Messaging.Application.Common;
using Messaging.Application.Configuration;
using Messaging.Application.OutgoingMessages.AccountingPointCharacteristics;
using Messaging.Application.SchemaStore;
using Messaging.Domain.OutgoingMessages;
using Messaging.Infrastructure.Common;
using Messaging.Infrastructure.Configuration;
using Messaging.Infrastructure.Configuration.Serialization;
using Messaging.Tests.OutgoingMessages.Asserts;
using Xunit;
using MarketActivityRecord = Messaging.Application.OutgoingMessages.AccountingPointCharacteristics.MarketActivityRecord;

namespace Messaging.Tests.OutgoingMessages.AccountingPointCharacteristics;

public class AccountingPointCharacteristicsDocumentWriterTests
{
    private readonly AccountingPointCharacteristicsDocumentWriter _documentWriter;
    private readonly ISystemDateTimeProvider _systemDateTimeProvider;
    private readonly IMarketActivityRecordParser _marketActivityRecordParser;
    private readonly SampleData _sampleData;
    private ISchemaProvider? _schemaProvider;

    public AccountingPointCharacteristicsDocumentWriterTests()
    {
        _systemDateTimeProvider = new SystemDateTimeProvider();
        _marketActivityRecordParser = new MarketActivityRecordParser(new Serializer());
        _documentWriter = new AccountingPointCharacteristicsDocumentWriter(_marketActivityRecordParser);
        _sampleData = new SampleData();
    }

    [Fact]
    public async Task Document_is_valid()
    {
        var header = new MessageHeader("E03", "SenderId", "DDZ", "ReceiverId", "DDQ", Guid.NewGuid().ToString(), _systemDateTimeProvider.Now(), null);
        var marketActivityRecord = _sampleData.CreateMarketActivityRecord();
        var marketActivityRecords = new List<MarketActivityRecord>()
        {
            marketActivityRecord,
        };
        var message = await _documentWriter.WriteAsync(header, marketActivityRecords.Select(record => _marketActivityRecordParser.From(record)).ToList()).ConfigureAwait(false);
        await AssertMessage(message, header, marketActivityRecords).ConfigureAwait(false);
    }

    private static void AssertMarketActivityRecords(List<MarketActivityRecord> marketActivityRecords, XDocument document)
    {
        AssertXmlMessage.AssertMarketActivityRecordCount(document, 1);
        foreach (var activityRecord in marketActivityRecords)
        {
            var marketActivityRecord = AssertXmlMessage.GetMarketActivityRecordById(document, activityRecord.Id)!;

            Assert.NotNull(marketActivityRecord);
          //  AssertXmlMessage.AssertMarketActivityRecordValue(marketActivityRecord, "originalTransactionIDReference_MktActivityRecord.mRID", activityRecord.OriginalTransactionId);
            AssertXmlMessage.AssertMarketActivityRecordValue(marketActivityRecord, "mRID", activityRecord.Id);
            AssertXmlMessage.AssertMarketActivityRecordValue(marketActivityRecord, "validityStart_DateAndOrTime.dateTime", activityRecord.ValidityStartDate.ToString());
        }
    }

    private async Task AssertMessage(Stream message, MessageHeader header, List<MarketActivityRecord> marketActivityRecords)
    {
        var document = XDocument.Load(message);
        AssertXmlMessage.AssertHeader(header, document);
        AssertXmlMessage.AssertHasHeaderValue(document, "type", "E07");

        AssertMarketActivityRecords(marketActivityRecords, document);

        await AssertConformsToSchema(message).ConfigureAwait(false);
    }

    private async Task AssertConformsToSchema(Stream message)
    {
        _schemaProvider = new XmlSchemaProvider();
        var schema = await _schemaProvider.GetSchemaAsync<XmlSchema>("accountingpointcharacteristics", "0.1")
            .ConfigureAwait(false);
        await AssertXmlMessage.AssertConformsToSchemaAsync(message, schema!).ConfigureAwait(false);
    }
}
