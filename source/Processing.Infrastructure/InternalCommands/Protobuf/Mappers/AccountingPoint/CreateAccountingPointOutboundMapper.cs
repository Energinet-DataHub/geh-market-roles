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
using CreateAccountingPoint = Processing.Application.AccountingPoint.CreateAccountingPoint;

namespace Processing.Infrastructure.InternalCommands.Protobuf.Mappers.AccountingPoint
{
    public class CreateAccountingPointOutboundMapper : ProtobufOutboundMapper<CreateAccountingPoint>
    {
        protected override IMessage Convert(CreateAccountingPoint obj)
        {
            if (obj == null) throw new ArgumentException(null, nameof(obj));

            return new MarketRolesEnvelope
            {
                CreateAccountingPoint = new Energinet.DataHub.MarketRoles.Contracts.CreateAccountingPoint()
                {
                    AccountingPointId = obj.AccountingPointId,
                    GsrnNumber = obj.GsrnNumber,
                    MeteringPointType = obj.MeteringPointType,
                },
            };
        }
    }
}