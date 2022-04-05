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
using System.Collections.Generic;
using System.Security.Claims;
using B2B.Transactions.Authentication;
using B2B.Transactions.Infrastructure.Authentication.MarketActors;
using Xunit;

namespace B2B.Transactions.Tests.Infrastructure
{
    #pragma warning disable
    public class MarketActorAuthenticatorTests
    {
        [Fact]
        public void Current_user_is_not_authenticated()
        {
            var authenticator = new MarketActorAuthenticator();

            Assert.IsType<NotAuthenticated>(authenticator.CurrentIdentity);
        }

        [Fact]
        public void Can_not_authenticate_if_claims_principal_does_not_contain_expected_claims()
        {
            var authenticator = new MarketActorAuthenticator();
            var claims = new List<Claim>()
            {
                new ("role", "balanceresponsibleparty"),
                new ("role", "electricalsupplier"),
            };
            var claimsPrincipal = CreateIdentity(claims);

            authenticator.Authenticate(claimsPrincipal);

            Assert.IsType<NotAuthenticated>(authenticator.CurrentIdentity);
        }

        [Fact]
        public void Current_user_is_authenticated()
        {
            var authenticator = new MarketActorAuthenticator();
            var claimsPrincipal = CreateIdentity();

            authenticator.Authenticate(claimsPrincipal);

            Assert.IsType<Authenticated>(authenticator.CurrentIdentity);
            Assert.Equal(GetClaimValue(claimsPrincipal,"azp"), authenticator.CurrentIdentity.Id);
            Assert.Equal(GetClaimValue(claimsPrincipal, "actorid"), authenticator.CurrentIdentity.ActorIdentifier);
            Assert.Equal(Enum.Parse<MarketActorIdentity.IdentifierType>(GetClaimValue(claimsPrincipal, "actoridtype"), true), authenticator.CurrentIdentity.ActorIdentifierType);
            Assert.True(authenticator.CurrentIdentity.HasRole("balanceresponsibleparty"));
            Assert.True(authenticator.CurrentIdentity.HasRole("electricalsupplier"));
        }

        private string? GetClaimValue(ClaimsPrincipal claimsPrincipal, string claimName)
        {
            return claimsPrincipal.FindFirst(claim => claim.Type.Equals(claimName, StringComparison.OrdinalIgnoreCase))?.Value;
        }

        private ClaimsPrincipal CreateIdentity(List<Claim>? claims = null)
        {
            var validClaims = new List<Claim>()
            {
                new ("azp", Guid.NewGuid().ToString()),
                new ("actorid", Guid.NewGuid().ToString()),
                new ("actoridtype", "GLN"),
                new ("role", "balanceresponsibleparty"),
                new ("role", "electricalsupplier"),
            };

            var identity = new ClaimsIdentity(claims ?? validClaims);
            return new ClaimsPrincipal(identity);
        }
    }
}