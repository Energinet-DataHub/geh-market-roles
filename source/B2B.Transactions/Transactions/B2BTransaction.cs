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
using B2B.Transactions.Messages;

namespace B2B.Transactions.Transactions
{
    public class B2BTransaction
    {
        public B2BTransaction(MessageHeader message, MarketActivityRecord marketActivityRecord)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            Id = message.MessageId;
            Message = message;
            MarketActivityRecord = marketActivityRecord;
        }

        public MessageHeader Message { get; }

        public MarketActivityRecord MarketActivityRecord { get; }

        public string Id { get; }

        public static B2BTransaction Create(MessageHeader messageHeader, MarketActivityRecord marketActivityRecord)
        {
            if (messageHeader == null) throw new ArgumentNullException(nameof(messageHeader));
            return new B2BTransaction(messageHeader, marketActivityRecord);
        }
    }
}
