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
using B2B.Transactions.OutgoingMessages;
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.MessageHub.Model.Model;

namespace B2B.Transactions.Infrastructure.OutgoingMessages
{
    public class DataAvailableNotificationPublisher : IDataAvailableNotificationPublisher
    {
        private readonly IDataAvailableNotificationSender _dataAvailableNotificationSender;

        public DataAvailableNotificationPublisher(
            IDataAvailableNotificationSender dataAvailableNotificationSender)
        {
            _dataAvailableNotificationSender = dataAvailableNotificationSender;
        }

        public async Task SendAsync(string correlationId, OutgoingMessage message)
        {
            if (correlationId == null) throw new ArgumentNullException(nameof(correlationId));
            if (message == null) throw new ArgumentNullException(nameof(message));

            await _dataAvailableNotificationSender.SendAsync(
                correlationId,
                CreateDataAvailableNotificationFrom(message)).ConfigureAwait(false);
        }

        private static DataAvailableNotificationDto CreateDataAvailableNotificationFrom(OutgoingMessage message)
        {
            return new DataAvailableNotificationDto(
                message.Id,
                new GlobalLocationNumberDto(message.RecipientId),
                new MessageTypeDto(string.Empty),
                DomainOrigin.MarketRoles,
                false,
                1,
                message.DocumentType);
        }
    }
}