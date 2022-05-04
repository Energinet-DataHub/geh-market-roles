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
using Messaging.Application.Configuration.Authentication;
using Messaging.Application.IncomingMessages;
using Messaging.Application.Xml.SchemaStore;
using Messaging.CimMessageAdapter.Errors;
using Messaging.CimMessageAdapter.Messages;

namespace Messaging.CimMessageAdapter
{
    public class MessageReceiver
    {
        private readonly List<ValidationError> _errors = new();
        private readonly IMessageIds _messageIds;
        private readonly IMessageQueueDispatcher _messageQueueDispatcher;
        private readonly ITransactionIds _transactionIds;
        private readonly ISchemaProvider _schemaProvider;
        private readonly IMarketActorAuthenticator _marketActorAuthenticator;

        public MessageReceiver(IMessageIds messageIds, IMessageQueueDispatcher messageQueueDispatcher, ITransactionIds transactionIds, ISchemaProvider schemaProvider, IMarketActorAuthenticator marketActorAuthenticator)
        {
            _messageIds = messageIds ?? throw new ArgumentNullException(nameof(messageIds));
            _messageQueueDispatcher = messageQueueDispatcher ??
                                             throw new ArgumentNullException(nameof(messageQueueDispatcher));
            _transactionIds = transactionIds;
            _schemaProvider = schemaProvider ?? throw new ArgumentNullException(nameof(schemaProvider));
            _marketActorAuthenticator = marketActorAuthenticator ?? throw new ArgumentNullException(nameof(marketActorAuthenticator));
        }

        public async Task<Result> ReceiveAsync(Stream message, string businessProcessType, string version)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var messageParser = new MessageParser(_schemaProvider);
            var messageParserResult =
                 await messageParser.ParseAsync(message, businessProcessType, version).ConfigureAwait(false);

            if (InvalidMessageHeader(messageParserResult))
            {
                return Result.Failure(messageParserResult.Errors.ToArray());
            }

            var messageHeader = messageParserResult.MessageHeader;

            await AuthorizeSenderAsync(messageHeader!).ConfigureAwait(false);
            await VerifyReceiverAsync(messageHeader!).ConfigureAwait(false);
            await CheckMessageIdAsync(messageHeader!.MessageId).ConfigureAwait(false);
            if (_errors.Count > 0)
            {
                return Result.Failure(_errors.ToArray());
            }

            foreach (var marketActivityRecord in messageParserResult.MarketActivityRecords)
            {
                if (await CheckTransactionIdAsync(marketActivityRecord.Id).ConfigureAwait(false) == false)
                {
                    return Result.Failure(new DuplicateTransactionIdDetected(marketActivityRecord.Id));
                }

                await AddToTransactionQueueAsync(CreateTransaction(messageHeader, marketActivityRecord)).ConfigureAwait(false);
            }

            await _messageQueueDispatcher.CommitAsync().ConfigureAwait(false);
            return Result.Succeeded();
        }

        private static bool InvalidMessageHeader(MessageParserResult messageParserResult)
        {
            return messageParserResult.MessageHeader is null || messageParserResult.Success == false;
        }

        private static IncomingMessage CreateTransaction(MessageHeader messageHeader, MarketActivityRecord marketActivityRecord)
        {
            return IncomingMessage.Create(messageHeader, marketActivityRecord);
        }

        private Task<bool> CheckTransactionIdAsync(string transactionId)
        {
            if (transactionId == null) throw new ArgumentNullException(nameof(transactionId));
            return _transactionIds.TryStoreAsync(transactionId);
        }

        private Task AddToTransactionQueueAsync(IncomingMessage transaction)
        {
            return _messageQueueDispatcher.AddAsync(transaction);
        }

        private async Task CheckMessageIdAsync(string messageId)
        {
            if (messageId == null) throw new ArgumentNullException(nameof(messageId));
            if (await _messageIds.TryStoreAsync(messageId).ConfigureAwait(false) == false)
            {
                _errors.Add(new DuplicateMessageIdDetected(messageId));
            }
        }

        private async Task AuthorizeSenderAsync(MessageHeader messageHeader)
        {
            if (messageHeader == null) throw new ArgumentNullException(nameof(messageHeader));
            var authorizer = new SenderAuthorizer(_marketActorAuthenticator);
            var result = await authorizer.AuthorizeAsync(messageHeader.SenderId, messageHeader.SenderRole).ConfigureAwait(false);
            _errors.AddRange(result.Errors);
        }

        private async Task VerifyReceiverAsync(MessageHeader messageHeader)
        {
            if (messageHeader == null) throw new ArgumentNullException(nameof(messageHeader));
            var receiverVerification = await ReceiverVerification.VerifyAsync(messageHeader!.ReceiverId, messageHeader.ReceiverRole).ConfigureAwait(false);
            _errors.AddRange(receiverVerification.Errors);
        }
    }
}