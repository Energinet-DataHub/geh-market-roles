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

namespace B2B.Transactions.Transactions
{
    public class MessageHubMessageAvailable
    {
        public MessageHubMessageAvailable(string id, string recipient, string messageType, string domainOrigin, bool supportsBundling, int relativeWeight, string documentType)
        {
            Id = id;
            Recipient = recipient;
            MessageType = messageType;
            DomainOrigin = domainOrigin;
            SupportsBundling = supportsBundling;
            RelativeWeight = relativeWeight;
            DocumentType = documentType;
        }

        public string Id { get; }

        public string Recipient { get; }

        public string MessageType { get; }

        public string DomainOrigin { get; }

        public bool SupportsBundling { get; }

        public int RelativeWeight { get; }

        public string DocumentType { get; }
    }
}