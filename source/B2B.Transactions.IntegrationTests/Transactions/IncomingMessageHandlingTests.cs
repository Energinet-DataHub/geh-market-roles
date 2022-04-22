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
using System.Xml.Linq;
using B2B.Transactions.Configuration;
using B2B.Transactions.DataAccess;
using B2B.Transactions.IncomingMessages;
using B2B.Transactions.IntegrationTests.Fixtures;
using B2B.Transactions.IntegrationTests.TestDoubles;
using B2B.Transactions.OutgoingMessages;
using B2B.Transactions.Transactions;
using B2B.Transactions.Xml.Incoming;
using B2B.Transactions.Xml.Outgoing;
using Xunit;
using Xunit.Categories;

namespace B2B.Transactions.IntegrationTests.Transactions
{
    [IntegrationTest]
    public class IncomingMessageHandlingTests : TestBase
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IncomingMessageStore _incomingMessageStore;
        private readonly IncomingMessageHandler _incomingMessageHandler;

        public IncomingMessageHandlingTests(DatabaseFixture databaseFixture)
            : base(databaseFixture)
        {
            _transactionRepository =
                GetService<ITransactionRepository>();
            _incomingMessageStore = GetService<IncomingMessageStore>();
            _incomingMessageHandler = GetService<IncomingMessageHandler>();
        }

        [Fact]
        public async Task Transaction_is_registered()
        {
            var incomingMessage = IncomingMessageBuilder.CreateMessage();

            await _incomingMessageHandler.HandleAsync(incomingMessage).ConfigureAwait(false);

            var savedTransaction = _transactionRepository.GetById(incomingMessage.Message.MessageId);
            Assert.NotNull(savedTransaction);
        }

        [Fact]
        public async Task Incoming_message_is_stored()
        {
            var incomingMessage = IncomingMessageBuilder.CreateMessage();

            await _incomingMessageHandler.HandleAsync(incomingMessage).ConfigureAwait(false);

            Assert.Equal(incomingMessage, _incomingMessageStore.GetById(incomingMessage.Id));
        }
    }
}
