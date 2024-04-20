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
namespace Kwality.UVault.M2M.Auth0.QA;

using System.Diagnostics.CodeAnalysis;

using FluentAssertions;

using global::Auth0.ManagementApi.Models;

using JetBrains.Annotations;

using Kwality.UVault.Core.Auth0.Behaviour;
using Kwality.UVault.Core.Auth0.Configuration;
using Kwality.UVault.Core.Auth0.Models;
using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.Core.Keys;
using Kwality.UVault.M2M.Auth0.Configuration;
using Kwality.UVault.M2M.Auth0.Extensions;
using Kwality.UVault.M2M.Auth0.Mapping.Abstractions;
using Kwality.UVault.M2M.Auth0.Models;
using Kwality.UVault.M2M.Auth0.Options;
using Kwality.UVault.M2M.Auth0.QA.Internal.Factories;
using Kwality.UVault.M2M.Managers;
using Kwality.UVault.QA.Common.System;
using Kwality.UVault.QA.Common.Xunit.Traits;

using Xunit;

[Collection("Auth0")]
public sealed class ApplicationTokenManagementAuth0Tests
{
    private const int rateLimitMaxRetryCount = 5;
    private const int rateLimitDelaySeconds = 2;
    private readonly ApplicationManager<Model, StringKey> manager;
    private readonly ApplicationTokenManager<TokenModel> tokenManager;

    public ApplicationTokenManagementAuth0Tests()
    {
        ApiConfiguration apiConfiguration = GetApiConfiguration();
        M2MConfiguration configuration = GetM2MConfiguration();

        this.manager = new ApplicationManagerFactory().Create<Model, StringKey>(options =>
            options.UseAuth0Store<Model, ModelMapper>(apiConfiguration,
                static () => new Auth0Options
                {
                    RateLimitBehaviour = RateLimitBehaviour.Retry,
                    RateLimitRetryInterval = TimeSpan.FromSeconds(rateLimitDelaySeconds),
                    RateLimitMaxRetryCount = rateLimitMaxRetryCount,
                }));

        this.tokenManager = new ApplicationTokenManagerFactory().Create<TokenModel>(
            options => options.UseAuth0Store<TokenModel, TokenModelMapper>(configuration,
                static () => new Auth0Options
                {
                    RateLimitBehaviour = RateLimitBehaviour.Retry,
                    RateLimitRetryInterval = TimeSpan.FromSeconds(rateLimitDelaySeconds),
                    RateLimitMaxRetryCount = rateLimitMaxRetryCount,
                }));
    }

    [M2MTokenManagement]
    [Fact(DisplayName = "Get access token (for an application with permissions) succeeds.")]
    internal async Task GetToken_ApplicationWithPermission_Succeeds()
    {
        // ARRANGE.
        Model application = await this.manager.GetByKeyAsync(new StringKey(Environment.AUTH0_CLIENT_ID))
                                      .ConfigureAwait(true);

        // ACT.
        TokenModel result = await this.tokenManager.GetAccessTokenAsync(application.Key.ToString() ?? string.Empty,
                                          application.ClientSecret ?? string.Empty, Environment.AUTH0_AUDIENCE,
                                          "client_credentials")
                                      .ConfigureAwait(true);

        // ASSERT.
        result.Token.Should()
              .NotBeNullOrWhiteSpace();

        result.Scope.Should()
              .NotBeNullOrWhiteSpace();

        result.TokenType.Should()
              .Be("Bearer");

        result.ExpiresIn.Should()
              .Be(86400);
    }

    [M2MTokenManagement]
    [Fact(DisplayName = "Get access token (for an application without permissions) fails.")]
    internal async Task GetToken_ApplicationWithoutPermission_Fails()
    {
        // ARRANGE.
        Model application = await this
                                  .manager.GetByKeyAsync(new StringKey(Environment.AUTH0_TEST_APPLICATION_1_CLIENT_ID))
                                  .ConfigureAwait(true);

        // ACT.
        Func<Task<TokenModel>> act = () => this.tokenManager.GetAccessTokenAsync(
            application.Key.ToString() ?? string.Empty, application.ClientSecret ?? string.Empty,
            Environment.AUTH0_AUDIENCE, "client_credentials");

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage("Failed to retrieve an access token.")
                 .ConfigureAwait(true);
    }

    [M2MTokenManagement]
    [Theory(DisplayName = "Get access token is failed when client secret is null.")]
    [InlineData(null, "clientSecret", "audience", "grantType")]
    [InlineData("clientId", null, "audience", "grantType")]
    [InlineData("clientId", "clientSecret", null, "grantType")]
    [InlineData("clientId", "clientSecret", "audience", null)]
    [SuppressMessage("Usage", "xUnit1012:Null should only be used for nullable parameters")]
    internal async Task GetToken_InvalidArguments_Fails(
        string clientId, string clientSecret, string audience, string grantType)
    {
        // ACT.
        Func<Task<TokenModel>> act = () =>
            this.tokenManager.GetAccessTokenAsync(clientId, clientSecret, audience, grantType);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage("Failed to retrieve an access token.")
                 .WithInnerException(typeof(ArgumentNullException))
                 .ConfigureAwait(true);
    }

    private static ApiConfiguration GetApiConfiguration()
    {
        return new ApiConfiguration(new Uri(Environment.AUTH0_TOKEN_ENDPOINT), Environment.AUTH0_CLIENT_ID,
            Environment.AUTH0_CLIENT_SECRET, Environment.AUTH0_AUDIENCE);
    }

    private static M2MConfiguration GetM2MConfiguration()
    {
        return new M2MConfiguration(new Uri(Environment.AUTH0_TOKEN_ENDPOINT));
    }

    private sealed class Model : ApplicationModel
    {
        public Model(StringKey name)
            : base(name)
        {
            this.Name = name.Value;
        }
    }

    
#pragma warning disable CA1812
    private sealed class ModelMapper : IModelMapper<Model>
#pragma warning restore CA1812
    {
        public Model Map(Client client)
        {
            return new Model(new StringKey(client.ClientId))
            {
                Name = client.Name, ClientSecret = client.ClientSecret,
            };
        }
    }

    
    internal sealed class TokenModel : M2M.Models.TokenModel
    {
        public TokenModel()
        {
        }

        public TokenModel(string? token, string tokenType, int expiresIn, string scope)
        {
            this.Token = token;
            this.TokenType = tokenType;
            this.ExpiresIn = expiresIn;
            this.Scope = scope;
        }
    }

    
#pragma warning disable CA1812
    private sealed class TokenModelMapper : IModelTokenMapper<TokenModel>
#pragma warning restore CA1812
    {
        public TokenModel Map(ApiManagementToken token)
        {
            return new TokenModel(token.AccessToken, token.TokenType, token.ExpiresIn, token.Scope);
        }
    }
}
