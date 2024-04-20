// =====================================================================================================================
// = LICENSE:       Copyright (c) 2023 Kevin De Coninck
// =
// =                Permission is hereby granted, free of charge, to any person
// =                obtaining a copy of this software and associated documentation
// =                files (the "Software"), to deal in the Software without
// =                restriction, including without limitation the rights to use,
// =                copy, modify, merge, publish, distribute, sublicense, and/or sell
// =                copies of the Software, and to permit persons to whom the
// =                Software is furnished to do so, subject to the following
// =                conditions:
// =
// =                The above copyright notice and this permission notice shall be
// =                included in all copies or substantial portions of the Software.
// =
// =                THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// =                EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// =                OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// =                NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// =                HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// =                WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// =                FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// =                OTHER DEALINGS IN THE SOFTWARE.
// =====================================================================================================================
namespace Kwality.UVault.M2M.QA;

using System.Diagnostics.CodeAnalysis;

using AutoFixture.Xunit2;

using FluentAssertions;

using Kwality.UVault.M2M.Managers;
using Kwality.UVault.M2M.Models;
using Kwality.UVault.M2M.QA.Internal.Factories;
using Kwality.UVault.QA.Common.Xunit.Traits;

using Xunit;

[SuppressMessage("ReSharper", "MemberCanBeFileLocal")]
public sealed class ApplicationTokenManagementDefaultTests
{
    private readonly ApplicationTokenManager<TokenModel> manager
        = new ApplicationTokenManagerFactory().Create<TokenModel>();

    [AutoData]
    [M2MTokenManagement]
    [Theory(DisplayName = "Get access token succeeds.")]
    internal async Task GetToken_Succeeds(string clientId, string clientSecret, string audience, string grantType)
    {
        // ACT.
        TokenModel result = await this.manager.GetAccessTokenAsync(clientId, clientSecret, audience, grantType)
                                      .ConfigureAwait(true);

        // ASSERT.
        result.Token.Should()
              .NotBeNullOrWhiteSpace();

        result.Scope.Should()
              .BeEmpty();

        result.ExpiresIn.Should()
              .Be(86400);

        result.TokenType.Should()
              .Be("Bearer");
    }
}
