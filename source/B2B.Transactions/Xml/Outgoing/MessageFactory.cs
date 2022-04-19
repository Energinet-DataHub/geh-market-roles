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
using B2B.Transactions.OutgoingMessages;
using B2B.Transactions.Transactions;
using Energinet.DataHub.MarketRoles.Domain.SeedWork;

namespace B2B.Transactions.Xml.Outgoing
{
    public abstract class MessageFactory<TMessage> : IMessageFactory<TMessage>
    {
        private readonly ISystemDateTimeProvider _systemDateTimeProvider;

        protected MessageFactory(ISystemDateTimeProvider systemDateTimeProvider)
        {
            _systemDateTimeProvider = systemDateTimeProvider ?? throw new ArgumentNullException(nameof(systemDateTimeProvider));
        }

        public abstract TMessage CreateMessage(B2BTransaction transaction);

        protected string GenerateMessageId()
        {
            return MessageIdGenerator.Generate();
        }

        protected string GenerateTransactionId()
        {
            return TransactionIdGenerator.Generate();
        }

        protected string GetCurrentDateTime()
        {
            return _systemDateTimeProvider.Now().ToString();
        }
    }
}