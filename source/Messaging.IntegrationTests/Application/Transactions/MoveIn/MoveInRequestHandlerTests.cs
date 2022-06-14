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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Messaging.Application.Common;
using Messaging.Application.OutgoingMessages;
using Messaging.Application.Transactions.MoveIn;
using Messaging.Application.Xml;
using Messaging.Application.Xml.SchemaStore;
using Messaging.Infrastructure.Configuration.DataAccess;
using Messaging.Infrastructure.Transactions;
using Messaging.IntegrationTests.Application.IncomingMessages;
using Messaging.IntegrationTests.Fixtures;
using Messaging.IntegrationTests.TestDoubles;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Categories;

namespace Messaging.IntegrationTests.Application.Transactions.MoveIn
{
    [IntegrationTest]
    public class MoveInRequestHandlerTests : TestBase
    {
        private readonly MarketEvaluationPointProviderStub _marketEvaluationPointProvider;
        private readonly IOutgoingMessageStore _outgoingMessageStore;
        private readonly MoveInRequestHandler _moveInRequestHandler;

        public MoveInRequestHandlerTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
            _outgoingMessageStore = GetService<IOutgoingMessageStore>();
            _moveInRequestHandler = GetService<MoveInRequestHandler>();
            _marketEvaluationPointProvider = (MarketEvaluationPointProviderStub)GetService<IMarketEvaluationPointProvider>();
        }

        [Fact]
        public async Task Transaction_is_started()
        {
            var incomingMessage = MessageBuilder()
                    .Build();

            await _moveInRequestHandler.HandleAsync(incomingMessage).ConfigureAwait(false);

            AssertTransactionIsStarted(incomingMessage.MarketActivityRecord.Id);
        }

        [Fact]
        public async Task Confirm_if_business_request_is_valid()
        {
            var incomingMessage = MessageBuilder()
                .WithProcessType(ProcessType.MoveIn.Code)
                .WithReceiver("5790001330552")
                .WithSenderId("123456")
                .WithConsumerName("John Doe")
                .Build();

            await _moveInRequestHandler.HandleAsync(incomingMessage).ConfigureAwait(false);
            var confirmMessage = _outgoingMessageStore.GetByOriginalMessageId(incomingMessage.Id)!;
            await RequestMessage(confirmMessage.Id.ToString()).ConfigureAwait(false);

            await AsserConfirmMessage(confirmMessage).ConfigureAwait(false);
        }

        [Fact]
        public async Task Reject_if_business_request_is_invalid()
        {
            var httpClientMock = GetHttpClientMock();
            httpClientMock.RespondWithValidationErrors(new List<string> { "InvalidConsumer" });

            var incomingMessage = MessageBuilder()
                .WithProcessType(ProcessType.MoveIn.Code)
                .WithReceiver("5790001330552")
                .WithSenderId("123456")
                .WithConsumerName(null)
                .Build();

            await _moveInRequestHandler.HandleAsync(incomingMessage).ConfigureAwait(false);
            var rejectMessage = _outgoingMessageStore.GetByOriginalMessageId(incomingMessage.Id)!;
            await RequestMessage(rejectMessage.Id.ToString()).ConfigureAwait(false);

            await AssertRejectMessage(rejectMessage).ConfigureAwait(false);
        }

        private static void AssertHeader(XDocument document, OutgoingMessage message, string expectedReasonCode)
        {
            Assert.NotEmpty(AssertXmlMessage.GetMessageHeaderValue(document, "mRID")!);
            AssertXmlMessage.AssertHasHeaderValue(document, "type", "414");
            AssertXmlMessage.AssertHasHeaderValue(document, "process.processType", message.ProcessType);
            AssertXmlMessage.AssertHasHeaderValue(document, "businessSector.type", "23");
            AssertXmlMessage.AssertHasHeaderValue(document, "sender_MarketParticipant.mRID", message.SenderId);
            AssertXmlMessage.AssertHasHeaderValue(document, "sender_MarketParticipant.marketRole.type", message.SenderRole);
            AssertXmlMessage.AssertHasHeaderValue(document, "receiver_MarketParticipant.mRID", message.ReceiverId);
            AssertXmlMessage.AssertHasHeaderValue(document, "receiver_MarketParticipant.marketRole.type", message.ReceiverRole);
            AssertXmlMessage.AssertHasHeaderValue(document, "reason.code", expectedReasonCode);
        }

        private IncomingMessageBuilder MessageBuilder()
        {
            return new IncomingMessageBuilder()
                .WithMarketEvaluationPointId(_marketEvaluationPointProvider.MarketEvaluationPoints.First().GsrnNumber);
        }

        private async Task AssertRejectMessage(OutgoingMessage rejectMessage)
        {
            var dispatchedDocument = GetDispatchedDocument();
            await ValidateDocument(dispatchedDocument, "rejectrequestchangeofsupplier", "0.1").ConfigureAwait(false);

            var document = XDocument.Load(dispatchedDocument);
            AssertHeader(document, rejectMessage, "A02");
        }

        private async Task AsserConfirmMessage(OutgoingMessage message)
        {
            var dispatchedDocument = GetDispatchedDocument();

            await ValidateDocument(dispatchedDocument, "confirmrequestchangeofsupplier", "0.1").ConfigureAwait(false);

            var document = XDocument.Load(dispatchedDocument);
            AssertHeader(document, message, "A01");
        }

        private async Task RequestMessage(string id)
        {
            await GetService<MessageRequestHandler>().HandleAsync(new[] { id }).ConfigureAwait(false);
        }

        private Stream GetDispatchedDocument()
        {
            var messageDispatcher = GetService<IMessageDispatcher>() as MessageDispatcherSpy;
            return messageDispatcher!.DispatchedMessage!;
        }

        private async Task ValidateDocument(Stream dispatchedDocument, string schemaName, string schemaVersion)
        {
            var schema = await GetService<ISchemaProvider>().GetSchemaAsync(schemaName, schemaVersion)
                .ConfigureAwait(false);

            var validationResult = await MessageValidator.ValidateAsync(dispatchedDocument, schema!);
            Assert.True(validationResult.IsValid);
        }

        private void AssertTransactionIsStarted(string transactionId)
        {
            var checkStatement =
                $"SELECT * FROM b2b.transactions WHERE TransactionId = '{transactionId}' AND Started = 1";
            var context = GetService<B2BContext>();
            var transaction = context.Transactions.FromSqlRaw(checkStatement).FirstOrDefault();
            Assert.NotNull(transaction);
        }

        private HttpClientSpy GetHttpClientMock()
        {
            var adapter = GetService<IHttpClientAdapter>();
            return adapter as HttpClientSpy ?? throw new InvalidCastException();
        }
    }
}