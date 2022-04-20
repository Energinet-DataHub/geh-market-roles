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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using B2B.Transactions.OutgoingMessages;
using B2B.Transactions.Transactions;

namespace B2B.Transactions.IntegrationTests.TestDoubles
{
    public class OutgoingMessageStoreSpy : IOutgoingMessageStore
    {
        private readonly List<OutgoingMessage> _messages = new();

        public IReadOnlyCollection<OutgoingMessage> Messages => _messages.AsReadOnly();

        #pragma warning disable
        public ReadOnlyCollection<OutgoingMessage> GetUnpublished()
        {
            return _messages.Where(message => message.IsPublished == false).ToList().AsReadOnly();
        }
        #pragma warning restore

        public void Add(OutgoingMessage message)
        {
            _messages.Add(message);
        }
    }
}