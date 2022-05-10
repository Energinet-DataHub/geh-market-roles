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

using System.Diagnostics.CodeAnalysis;
using Processing.Application.EDI;
using Processing.Domain.MeteringPoints.Rules.ChangeEnergySupplier;

namespace Processing.Infrastructure.EDI.Errors.Converters
{
    public class EffectiveDateCannotBeInThePastRuleErrorConverter : ErrorConverter<EffectiveDateCannotBeInThePastRuleError>
    {
        protected override ErrorMessage Convert([NotNull] EffectiveDateCannotBeInThePastRuleError validationError)
        {
            return new("E17", $"Effective date {validationError.EffectiveDate.ToString()} incorrect: The information is not received within the correct time period.");
        }
    }
}