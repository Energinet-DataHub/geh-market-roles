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

using System.Threading.Tasks;
using Processing.Application.ChangeOfSupplier.Processing.EndOfSupplyNotification;
using Processing.Domain.MeteringPoints;

namespace Processing.Infrastructure.EDI.ChangeOfSupplier.EndOfSupplyNotification
{
    public class EndOfSupplyNotifier : IEndOfSupplyNotifier
    {
        public Task NotifyAsync(AccountingPointId accountingPointId)
        {
            //TODO: Add logic for generating actor notification message
            return Task.CompletedTask;
        }
    }
}