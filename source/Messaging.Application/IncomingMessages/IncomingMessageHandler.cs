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
using System.Threading.Tasks;
using Messaging.Application.Common;
using Messaging.Application.Configuration;
using Messaging.Application.Configuration.DataAccess;
using Messaging.Application.OutgoingMessages;
using Messaging.Application.Transactions;

namespace Messaging.Application.IncomingMessages
{
    public class IncomingMessageHandler
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IOutgoingMessageStore _outgoingMessageStore;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICorrelationContext _correlationContext;
        private readonly IMarketActivityRecordParser _marketActivityRecordParser;

        public IncomingMessageHandler(
            ITransactionRepository transactionRepository,
            IOutgoingMessageStore outgoingMessageStore,
            IUnitOfWork unitOfWork,
            ICorrelationContext correlationContext,
            IMarketActivityRecordParser marketActivityRecordParser)
        {
            _transactionRepository = transactionRepository;
            _outgoingMessageStore = outgoingMessageStore;
            _unitOfWork = unitOfWork;
            _correlationContext = correlationContext;
            _marketActivityRecordParser = marketActivityRecordParser;
        }

        public Task HandleAsync(IncomingMessage incomingMessage)
        {
            if (incomingMessage == null) throw new ArgumentNullException(nameof(incomingMessage));

            var acceptedTransaction = new AcceptedTransaction(incomingMessage.MarketActivityRecord.Id);
            _transactionRepository.Add(acceptedTransaction);

            var messageId = Guid.NewGuid();
            var marketActivityRecord = new OutgoingMessages.ConfirmRequestChangeOfSupplier.MarketActivityRecord(
                messageId.ToString(),
                acceptedTransaction.TransactionId,
                incomingMessage.MarketActivityRecord.MarketEvaluationPointId);

            var outgoingMessage = new OutgoingMessage(
                "ConfirmRequestChangeOfSupplier",
                incomingMessage.Message.SenderId,
                _correlationContext.Id,
                incomingMessage.Id,
                incomingMessage.Message.ProcessType,
                incomingMessage.Message.SenderRole,
                incomingMessage.Message.ReceiverId,
                incomingMessage.Message.ReceiverRole,
                _marketActivityRecordParser.From(marketActivityRecord));
            _outgoingMessageStore.Add(outgoingMessage);

            return _unitOfWork.CommitAsync();
        }
    }
}