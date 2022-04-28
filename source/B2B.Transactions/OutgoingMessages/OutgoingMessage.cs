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

namespace B2B.Transactions.OutgoingMessages
{
    public class OutgoingMessage
    {
        public OutgoingMessage(Guid id, string documentType, string correlationId, string originalMessageId, string processType, string transactionId, string marketActivityRecord, string senderId, string senderRole, string receiverRole, string receiverId)
        {
            DocumentType = documentType;
            CorrelationId = correlationId;
            OriginalMessageId = originalMessageId;
            ProcessType = processType;
            TransactionId = transactionId;
            MarketActivityRecord = marketActivityRecord;
            SenderId = senderId;
            SenderRole = senderRole;
            ReceiverRole = receiverRole;
            ReceiverId = receiverId;
            Id = id;
        }

        public Guid Id { get; }

        public bool IsPublished { get; private set; }

        public string DocumentType { get; }

        public string CorrelationId { get; }

        public string OriginalMessageId { get; }

        public string ProcessType { get; }

        public string TransactionId { get; }

        public string MarketActivityRecord { get; }

        public string SenderId { get; }

        public string SenderRole { get; }

        public string ReceiverRole { get; }

        public string ReceiverId { get; }

        public void Published()
        {
            IsPublished = true;
        }
    }
}
