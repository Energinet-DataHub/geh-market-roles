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

using Energinet.DataHub.MarketRoles.Application.Common.Validation;
using FluentValidation;

namespace Energinet.DataHub.MarketRoles.Application.ChangeOfSupplier.Validation
{
    public class RequestChangeOfSupplierRuleSet : AbstractValidator<RequestChangeOfSupplier>
    {
        public RequestChangeOfSupplierRuleSet()
        {
            RuleFor(request => request.AccountingPointGsrnNumber).SetValidator(new GsrnNumberMustBeValidRule());
            RuleFor(request => request.EnergySupplierGlnNumber).SetValidator(new GlnNumberMustBeValidRule());
            RuleFor(request => request.StartDate).SetValidator(new StartOfSupplyMustBeValidRule());
            RuleFor(request => request.TransactionId).SetValidator(new TransactionMustBeValidRule());
        }
    }
}
