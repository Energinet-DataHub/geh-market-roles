// Copyright 2020 Energinet DataHub A/S
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
using Processing.Infrastructure.Configuration.Correlation;
using Processing.Infrastructure.Configuration.Outbox;
using Processing.Infrastructure.Configuration.Serialization;

namespace Processing.Infrastructure.EDI
{
    public class MessageHubDispatcher : IMessageHubDispatcher
    {
        private readonly ICorrelationContext _correlationContext;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly OutboxMessageFactory _outboxMessageFactory;
        private readonly OutboxProvider _outboxProvider;

        public MessageHubDispatcher(
            ICorrelationContext correlationContext,
            IJsonSerializer jsonSerializer,
            OutboxMessageFactory outboxMessageFactory,
            OutboxProvider outboxProvider)
        {
            _correlationContext = correlationContext ?? throw new ArgumentNullException(nameof(correlationContext));
            _jsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            _outboxMessageFactory = outboxMessageFactory ?? throw new ArgumentNullException(nameof(outboxMessageFactory));
            _outboxProvider = outboxProvider ?? throw new ArgumentNullException(nameof(outboxProvider));
        }

        public Task DispatchAsync<TMessage>(TMessage message, DocumentType documentType, string recipient, string gsrnNumber)
        {
            var envelope = CreateMessageHubEnvelope(
                recipient: recipient,
                cimContent: _jsonSerializer.Serialize(message),
                messageType: documentType,
                gsrnNumber: gsrnNumber);

            AddToOutbox(envelope);

            return Task.CompletedTask;
        }

        private MessageHubEnvelope CreateMessageHubEnvelope(string recipient, string cimContent, DocumentType messageType, string gsrnNumber)
        {
            return new(
                Recipient: recipient,
                Content: cimContent,
                MessageType: messageType,
                Correlation: _correlationContext.AsTraceContext(),
                gsrnNumber); // TODO: add correlation when Telemetry is added
        }

        private void AddToOutbox<TEdiMessage>(TEdiMessage ediMessage)
        {
            var outboxMessage = _outboxMessageFactory.CreateFrom(ediMessage, OutboxMessageCategory.ActorMessage);
            _outboxProvider.Add(outboxMessage);
        }
    }
}
