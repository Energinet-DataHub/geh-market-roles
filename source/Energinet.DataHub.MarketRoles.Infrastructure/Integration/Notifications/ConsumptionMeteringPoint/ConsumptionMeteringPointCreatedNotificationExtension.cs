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
using NotificationContracts;
using Processing.Domain.AccountingPoints;

namespace Energinet.DataHub.MarketRoles.Infrastructure.Integration.Notifications.ConsumptionMeteringPoint
{
    internal static class ConsumptionMeteringPointCreatedNotificationExtension
    {
        public static PhysicalState GetConnectionState(
            this NotificationContracts.ConsumptionMeteringPointCreated @notification)
        {
            return @notification.ConnectionState switch
            {
                ConsumptionMeteringPointCreated.Types.ConnectionState.CsNew => PhysicalState.New,
                _ => throw new ArgumentException("Connection state is not recognized."),
            };
        }
    }
}
