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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using B2B.Transactions.Infrastructure.Configuration.Correlation;
using B2B.Transactions.IntegrationTests.Fixtures;
using B2B.Transactions.IntegrationTests.TestDoubles;
using B2B.Transactions.Messages;
using B2B.Transactions.OutgoingMessages;
using B2B.Transactions.Transactions;
using B2B.Transactions.Xml.Outgoing;
using Energinet.DataHub.MarketRoles.Domain.SeedWork;
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.MessageHub.Model.Model;
using Xunit;

namespace B2B.Transactions.IntegrationTests
{
    public class MessagePublishingTests : TestBase
    {
        private readonly IOutgoingMessageStore _outgoingMessageStore;
        private readonly IMessageFactory<IMessage> _messageFactory;

        public MessagePublishingTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
            var systemDateTimeProvider = GetService<ISystemDateTimeProvider>();
            _outgoingMessageStore = new OutgoingMessageStoreSpy();
            _messageFactory = new AcceptMessageFactory(systemDateTimeProvider);
        }

        [Fact]
        public async Task Outgoing_messages_are_published()
        {
            var dataAvailableNotificationSenderSpy = new DataAvailableNotificationSenderSpy();
            var messagePublisher = new MessagePublisher(dataAvailableNotificationSenderSpy, GetService<ICorrelationContext>());
            var transaction = CreateTransaction();
            var outgoingMessage = new OutgoingMessage(_messageFactory.CreateMessage(transaction), transaction.Message.ReceiverId);
            _outgoingMessageStore.Add(outgoingMessage);

            await messagePublisher.PublishAsync(await _outgoingMessageStore.GetUnpublishedAsync().ConfigureAwait(false)).ConfigureAwait(false);
            var unpublishedMessages = await _outgoingMessageStore.GetUnpublishedAsync().ConfigureAwait(false);
            var publishedMessage = dataAvailableNotificationSenderSpy.PublishedMessages.FirstOrDefault();

            Assert.Empty(unpublishedMessages);
            Assert.NotNull(publishedMessage);
            Assert.Equal(outgoingMessage.RecipientId, publishedMessage?.Recipient.Value);
            Assert.Equal(DomainOrigin.MarketRoles, publishedMessage?.Origin);
            Assert.Equal(outgoingMessage.DocumentType, publishedMessage?.DocumentType);
            Assert.Equal(false, publishedMessage?.SupportsBundling);
            Assert.Equal(string.Empty, publishedMessage?.MessageType.Value);
        }

        private static B2BTransaction CreateTransaction()
        {
            return B2BTransaction.Create(
                new MessageHeader("fake", "fake", "fake", "fake", "fake", "somedate", "fake"),
                new MarketActivityRecord()
                {
                    BalanceResponsibleId = "fake",
                    Id = "fake",
                    ConsumerId = "fake",
                    ConsumerName = "fake",
                    EffectiveDate = "fake",
                    EnergySupplierId = "fake",
                    MarketEvaluationPointId = "fake",
                });
        }
    }

    #pragma warning disable
    public class MessagePublisher
    {
        private readonly IDataAvailableNotificationSender _dataAvailableNotificationSender;
        private readonly ICorrelationContext _correlationContext;

        public MessagePublisher(IDataAvailableNotificationSender dataAvailableNotificationSender, ICorrelationContext correlationContext)
        {
            _dataAvailableNotificationSender = dataAvailableNotificationSender ?? throw new ArgumentNullException(nameof(dataAvailableNotificationSender));
            _correlationContext = correlationContext;
        }


        public async Task PublishAsync(ReadOnlyCollection<OutgoingMessage> unpublishedMessages)
        {
            foreach (var message in unpublishedMessages)
            {
                await _dataAvailableNotificationSender.SendAsync(
                    _correlationContext.Id,
                    new DataAvailableNotificationDto(
                        Guid.NewGuid(),
                        new GlobalLocationNumberDto(message.RecipientId),
                        new MessageTypeDto(string.Empty),
                        DomainOrigin.MarketRoles,
                        false,
                        1,
                        message.DocumentType)).ConfigureAwait(false);

                message.Published();
            }
        }
    }
}
