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
using System.Linq;
using Processing.Domain.SeedWork;
using Xunit;

namespace Processing.Tests;

public class TestBase
{
    #pragma warning disable
    public void AssertError<TRuleError>(BusinessRulesValidationResult rulesValidationResult, string? expectedErrorCode = null, bool errorExpected = true)
        where TRuleError : ValidationError
    {
        if (rulesValidationResult == null) throw new ArgumentNullException(nameof(rulesValidationResult));
        var error = rulesValidationResult.Errors.FirstOrDefault(error => error is TRuleError);
        if (errorExpected) Assert.NotNull(error);
        if (expectedErrorCode is not null)
        {
            Assert.Equal(expectedErrorCode, error?.Code);
        }
    }
}
