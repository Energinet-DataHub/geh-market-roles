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
using System.Security.Claims;
using B2B.Transactions.Infrastructure.Authentication.Errors;

namespace B2B.Transactions.Infrastructure.Authentication
{
    public class Result
    {
        private Result(AuthenticationError error)
        {
            Success = false;
            Error = error ?? throw new ArgumentNullException(nameof(error));
        }

        private Result(ClaimsPrincipal claimsPrincipal)
        {
            Success = true;
            ClaimsPrincipal = claimsPrincipal;
        }

        public bool Success { get; }

        public ClaimsPrincipal? ClaimsPrincipal { get; }

        public AuthenticationError? Error { get; }

        public static Result Failed(AuthenticationError error)
        {
            return new Result(error);
        }

        public static Result Succeeded(ClaimsPrincipal principal)
        {
            return new Result(principal);
        }
    }
}
