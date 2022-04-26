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
using B2B.Transactions.Configuration;
using B2B.Transactions.IncomingMessages;

namespace B2B.Transactions.IntegrationTests.Transactions
{
    internal class IncomingMessageBuilder
    {
        public IncomingMessageBuilder()
        {
        }

        internal static IncomingMessage CreateMessage()
        {
            return IncomingMessage.Create(
                new MessageHeader(Guid.NewGuid().ToString(), "E03", "senderIdfake", "DDZ", DataHubDetails.IdentificationNumber, "DDQ", "fake"),
                new MarketActivityRecord()
                {
                    BalanceResponsibleId = "fake",
                    Id = Guid.NewGuid().ToString(),
                    ConsumerId = "fake",
                    ConsumerName = "fake",
                    EffectiveDate = "fake",
                    EnergySupplierId = "fake",
                    MarketEvaluationPointId = "fake",
                });
        }
    }
}