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
using Google.Protobuf;
using Processing.Infrastructure.Integration.Helpers;
using Processing.Infrastructure.Transport.Protobuf;

namespace Processing.Infrastructure.Integration.IntegrationEvents.FutureEnergySupplierChangeRegistered
{
    public class FutureEnergySupplierChangeRegisteredOutBoundMapper : ProtobufOutboundMapper<FutureEnergySupplierChangeRegisteredIntegrationEvent>
    {
        protected override IMessage Convert(FutureEnergySupplierChangeRegisteredIntegrationEvent obj)
        {
            if (obj == null) throw new ArgumentException(null, nameof(obj));
            return new Energinet.DataHub.MarketRoles.IntegrationEventContracts.FutureEnergySupplierChangeRegistered
            {
                AccountingpointId = obj.AccountingPointId.ToString(),
                EnergySupplierGln = obj.EnergySupplierGln,
                GsrnNumber = obj.GsrnNumber,
                EffectiveDate = obj.EffectiveDate.ToTimestamp(),
            };
        }
    }
}