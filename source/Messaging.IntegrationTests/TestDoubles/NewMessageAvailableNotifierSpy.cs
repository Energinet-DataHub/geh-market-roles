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
using System.Collections.Generic;
using System.Threading.Tasks;
using Messaging.Application.OutgoingMessages;
using Messaging.Domain.OutgoingMessages;
using Messaging.Infrastructure.OutgoingMessages;

namespace Messaging.IntegrationTests.TestDoubles
{
    public class NewMessageAvailableNotifierSpy : INewMessageAvailableNotifier
    {
        private readonly Dictionary<string, OutgoingMessage> _publishedNotifications = new();

        public Task NotifyAsync(OutgoingMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            _publishedNotifications.Add(message.CorrelationId, message);
            return Task.CompletedTask;
        }

        public OutgoingMessage? GetMessageFrom(string correlationId)
        {
            _publishedNotifications.TryGetValue(correlationId, out var notification);
            return notification;
        }
    }
}
