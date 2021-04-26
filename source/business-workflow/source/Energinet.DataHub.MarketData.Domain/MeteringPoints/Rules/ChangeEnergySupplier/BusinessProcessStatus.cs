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

using Energinet.DataHub.MarketData.Domain.SeedWork;

namespace Energinet.DataHub.MarketData.Domain.MeteringPoints.Rules.ChangeEnergySupplier
{
    internal class BusinessProcessStatus : EnumerationType
    {
        internal static readonly BusinessProcessStatus Pending = new BusinessProcessStatus(0, nameof(Pending));
        internal static readonly BusinessProcessStatus Cancelled = new BusinessProcessStatus(1, nameof(Cancelled));
        internal static readonly BusinessProcessStatus Completed = new BusinessProcessStatus(2, nameof(Completed));

        private BusinessProcessStatus(int id, string name)
            : base(id, name)
        {
        }
    }
}
