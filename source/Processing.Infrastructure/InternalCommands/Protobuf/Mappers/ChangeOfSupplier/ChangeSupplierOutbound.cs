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
using Energinet.DataHub.MarketRoles.Contracts;
using Google.Protobuf;
using Processing.Infrastructure.Transport.Protobuf;
using ChangeSupplier = Processing.Application.ChangeOfSupplier.Processing.ChangeSupplier;

namespace Processing.Infrastructure.InternalCommands.Protobuf.Mappers.ChangeOfSupplier
{
    public class ChangeSupplierOutbound : ProtobufOutboundMapper<ChangeSupplier>
    {
        protected override IMessage Convert(ChangeSupplier obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return new MarketRolesEnvelope()
            {
                ChangeSupplier = new Energinet.DataHub.MarketRoles.Contracts.ChangeSupplier
                {
                    Id = obj.Id.ToString(),
                    Transaction = obj.Transaction,
                    AccountingPointId = obj.AccountingPointId.ToString(),
                },
            };
        }
    }
}