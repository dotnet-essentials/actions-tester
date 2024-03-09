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
namespace Kwality.UVault.Core.Auth0.QA.API;

using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;

using FluentAssertions;

using global::System.Diagnostics.CodeAnalysis;
using global::System.Net;

using Kwality.UVault.Core.Auth0.API.Clients;
using Kwality.UVault.Core.Auth0.Configuration;
using Kwality.UVault.Core.Auth0.Exceptions;
using Kwality.UVault.Core.Auth0.Models;
using Kwality.UVault.Core.Auth0.QA.Internal.Extensions;
using Kwality.UVault.Core.System.Abstractions;
using Kwality.UVault.QA.Common.Xunit.Traits;

using Moq;

using Xunit;

[Collection("Auth0")]
[SuppressMessage("ReSharper", "MemberCanBeFileLocal")]
public sealed class Auth0ManagementClientTests
{
    private const string testPrefix = "Request an API management token";

    private readonly ApiConfiguration apiConfiguration
        = new(new Uri("http://localhost/"), string.Empty, string.Empty, string.Empty);

    [Auth0]
    [AutoDomainData]
    [Theory(DisplayName = $"{testPrefix} fails when the REST endpoint can't be requested.")]
    internal async Task RequestToken_RequestFails_RaisesException(
        [Frozen] Mock<HttpMessageHandler> messageHandler, ManagementClient managementClient)
    {
        // MOCK SETUP.
        messageHandler.SetupSendAsyncException();

        // ACT.
        Func<Task> act = async () => await managementClient.GetTokenAsync(this.apiConfiguration)
                                                           .ConfigureAwait(true);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ManagementApiException>()
                 .WithMessage("Failed to retrieve an Auth0 token.")
                 .ConfigureAwait(true);
    }

    [Auth0]
    [AutoDomainData]
    [Theory(DisplayName = "Request token fails when the DateTimeProvider is null.")]
    internal async Task RequestToken_NullDateTimeProvider_RaisesException(
        [Frozen] Mock<HttpMessageHandler> messageHandler, HttpClient httpClient, string apiToken)
    {
        // MOCK SETUP.
        using var managementApiTokenHttpResponseMessage = new HttpResponseMessage();
        managementApiTokenHttpResponseMessage.StatusCode = HttpStatusCode.OK;
        managementApiTokenHttpResponseMessage.Content = new StringContent("{\"access_token\":\"" + apiToken + "\"}");
        messageHandler.SetupSendAsyncResponse(managementApiTokenHttpResponseMessage);
#pragma warning disable CS8625
        var managementClient = new ManagementClient(httpClient, null);
#pragma warning restore CS8625

        await managementClient.GetTokenAsync(this.apiConfiguration)
                              .ConfigureAwait(true);

        Func<Task> act = async () => await managementClient.GetTokenAsync(this.apiConfiguration)
                                                           .ConfigureAwait(true);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ArgumentNullException>()
                 .ConfigureAwait(true);
    }

    [Auth0]
    [AutoDomainData]
    [Theory(DisplayName = $"{testPrefix} fails when the result differs from HTTP OK (NO Content).")]
    internal async Task RequestToken_ResponseNoOkWithoutContent_RaisesException(
        [Frozen] Mock<HttpMessageHandler> messageHandler, ManagementClient managementClient)
    {
        // MOCK SETUP.
        const HttpStatusCode statusCode = HttpStatusCode.BadRequest;
        var content = string.Empty;
        using var managementApiTokenHttpResponseMessage = new HttpResponseMessage();
        managementApiTokenHttpResponseMessage.StatusCode = statusCode;
        managementApiTokenHttpResponseMessage.Content = new StringContent(content);
        messageHandler.SetupSendAsyncResponse(managementApiTokenHttpResponseMessage);

        // ACT.
        Func<Task> act = async () => await managementClient.GetTokenAsync(this.apiConfiguration)
                                                           .ConfigureAwait(true);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ManagementApiException>()
                 .WithMessage("Failed to retrieve an Auth0 token.")
                 .WithInnerException<ManagementApiException, TokenRequestException>()
                 .WithMessage("BadRequest")
                 .Where(ex => ex.StatusCode == statusCode && ex.ResponseMessage == content)
                 .ConfigureAwait(true);
    }

    [Auth0]
    [AutoDomainData]
    [Theory(DisplayName = $"{testPrefix} fails when the result differs from HTTP OK.")]
    internal async Task RequestToken_ResponseNoOk_RaisesException(
        [Frozen] Mock<HttpMessageHandler> messageHandler, ManagementClient managementClient, string response)
    {
        // MOCK SETUP.
        const HttpStatusCode statusCode = HttpStatusCode.Unauthorized;
        using var managementApiTokenHttpResponseMessage = new HttpResponseMessage();
        managementApiTokenHttpResponseMessage.StatusCode = statusCode;
        managementApiTokenHttpResponseMessage.Content = new StringContent(response);
        messageHandler.SetupSendAsyncResponse(managementApiTokenHttpResponseMessage);

        // ACT.
        Func<Task> act = async () => await managementClient.GetTokenAsync(this.apiConfiguration)
                                                           .ConfigureAwait(true);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ManagementApiException>()
                 .WithMessage("Failed to retrieve an Auth0 token.")
                 .WithInnerException<ManagementApiException, TokenRequestException>()
                 .WithMessage($"Unauthorized: {response}")
                 .Where(ex => ex.StatusCode == statusCode && ex.ResponseMessage == response)
                 .ConfigureAwait(true);
    }

    [Auth0]
    [AutoDomainData]
    [Theory(DisplayName = $"{testPrefix} fails when the result is NOT valid JSON.")]
    internal async Task RequestToken_ResponseNoValidJson_RaisesException(
        [Frozen] Mock<HttpMessageHandler> messageHandler, ManagementClient managementClient)
    {
        // MOCK SETUP.
        using var managementApiTokenHttpResponseMessage = new HttpResponseMessage();
        managementApiTokenHttpResponseMessage.StatusCode = HttpStatusCode.OK;
        managementApiTokenHttpResponseMessage.Content = new StringContent("Invalid data.");
        messageHandler.SetupSendAsyncResponse(managementApiTokenHttpResponseMessage);

        // ACT.
        Func<Task> act = async () => await managementClient.GetTokenAsync(this.apiConfiguration)
                                                           .ConfigureAwait(true);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ManagementApiException>()
                 .WithMessage("Failed to retrieve an Auth0 token. Reason: Invalid HTTP response.")
                 .ConfigureAwait(true);
    }

    [Auth0]
    [AutoDomainData]
    [Theory(DisplayName = $"{testPrefix} fails when the result does NOT contain an access token.")]
    internal async Task RequestToken_ResponseNoAccessToken_RaisesException(
        [Frozen] Mock<HttpMessageHandler> messageHandler, ManagementClient managementClient)
    {
        // MOCK SETUP.
        using var managementApiTokenHttpResponseMessage = new HttpResponseMessage();
        managementApiTokenHttpResponseMessage.StatusCode = HttpStatusCode.OK;
        managementApiTokenHttpResponseMessage.Content = new StringContent("{}");
        messageHandler.SetupSendAsyncResponse(managementApiTokenHttpResponseMessage);

        // ACT.
        Func<Task> act = async () => await managementClient.GetTokenAsync(this.apiConfiguration)
                                                           .ConfigureAwait(true);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ManagementApiException>()
                 .WithMessage("The `API Management Token / Access Token` is `null`.")
                 .ConfigureAwait(true);
    }

    [Auth0]
    [AutoDomainData]
    [Theory(DisplayName = $"{testPrefix} fails when the result does contain an empty access token.")]
    internal async Task RequestToken_ResponseEmptyAccessToken_RaisesException(
        [Frozen] Mock<HttpMessageHandler> messageHandler, ManagementClient managementClient)
    {
        // MOCK SETUP.
        using var managementApiTokenHttpResponseMessage = new HttpResponseMessage();
        managementApiTokenHttpResponseMessage.StatusCode = HttpStatusCode.OK;
        managementApiTokenHttpResponseMessage.Content = new StringContent("{\"access_token\":\"\"}");
        messageHandler.SetupSendAsyncResponse(managementApiTokenHttpResponseMessage);

        // ACT.
        Func<Task> act = async () => await managementClient.GetTokenAsync(this.apiConfiguration)
                                                           .ConfigureAwait(true);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ManagementApiException>()
                 .WithMessage("The `API Management Token / Access Token` is `null`.")
                 .ConfigureAwait(true);
    }

    [Auth0]
    [AutoDomainData]
    [Theory(DisplayName = $"{testPrefix} returns the access token.")]
    internal async Task RequestToken_ReturnsAccessToken(
        [Frozen] Mock<HttpMessageHandler> messageHandler, ManagementClient managementClient, string apiToken)
    {
        // MOCK SETUP.
        using var managementApiTokenHttpResponseMessage = new HttpResponseMessage();
        managementApiTokenHttpResponseMessage.StatusCode = HttpStatusCode.OK;
        managementApiTokenHttpResponseMessage.Content = new StringContent("{\"access_token\":\"" + apiToken + "\"}");
        messageHandler.SetupSendAsyncResponse(managementApiTokenHttpResponseMessage);

        // ACT.
        string result = await managementClient.GetTokenAsync(this.apiConfiguration)
                                              .ConfigureAwait(true);

        // ASSERT.
        result.Should()
              .Be(apiToken);
    }

    [Auth0]
    [AutoDomainData]
    [Theory(DisplayName = $"{testPrefix} returns the cached access token.")]
    internal async Task RequestToken_LastTokenNotExpired_ReturnsCachedAccessToken(
        [Frozen] Mock<HttpMessageHandler> messageHandler, [Frozen] Mock<IDateTimeProvider> dateTimeProvider,
        ManagementClient managementClient)
    {
        // MOCK SETUP.
        dateTimeProvider.Setup(static x => x.Now)
                        .Returns(DateTime.Now);

        using var managementApiTokenHttpResponseMessageOne = new HttpResponseMessage();
        managementApiTokenHttpResponseMessageOne.StatusCode = HttpStatusCode.OK;

        managementApiTokenHttpResponseMessageOne.Content
            = new StringContent("{\"access_token\":\"Token 1\",\"expires_in\": 86400}");

        messageHandler.SetupSendAsyncResponse(managementApiTokenHttpResponseMessageOne);

        // ACT.
        string resultOne = await managementClient.GetTokenAsync(this.apiConfiguration)
                                                 .ConfigureAwait(true);

        // MOCK SETUP.
        using var managementApiTokenHttpResponseMessageTwo = new HttpResponseMessage();
        managementApiTokenHttpResponseMessageTwo.StatusCode = HttpStatusCode.OK;

        managementApiTokenHttpResponseMessageTwo.Content
            = new StringContent("{\"access_token\":\"Token 2\",\"expires_in\": 86400}");

        messageHandler.SetupSendAsyncResponse(managementApiTokenHttpResponseMessageTwo);

        // ACT.
        string resultTwo = await managementClient.GetTokenAsync(this.apiConfiguration)
                                                 .ConfigureAwait(true);

        // ASSERT.
        resultOne.Should()
                 .Be(resultTwo);
    }

    [Auth0]
    [AutoDomainData]
    [Theory(DisplayName = $"{testPrefix} returns a new access token.")]
    internal async Task RequestToken_LastTokenExpired_ReturnsNewAccessToken(
        [Frozen] Mock<HttpMessageHandler> messageHandler, [Frozen] Mock<IDateTimeProvider> dateTimeProvider,
        ManagementClient managementClient)
    {
        // MOCK SETUP.
        dateTimeProvider.Setup(static x => x.Now)
                        .Returns(DateTime.Now.AddHours(24));

        using var managementApiTokenHttpResponseMessageOne = new HttpResponseMessage();
        managementApiTokenHttpResponseMessageOne.StatusCode = HttpStatusCode.OK;

        managementApiTokenHttpResponseMessageOne.Content
            = new StringContent("{\"access_token\":\"Token 1\",\"expires_in\": 86400}");

        messageHandler.SetupSendAsyncResponse(managementApiTokenHttpResponseMessageOne);

        // ACT.
        string resultOne = await managementClient.GetTokenAsync(this.apiConfiguration)
                                                 .ConfigureAwait(true);

        // MOCK SETUP.
        using var managementApiTokenHttpResponseMessageTwo = new HttpResponseMessage();
        managementApiTokenHttpResponseMessageTwo.StatusCode = HttpStatusCode.OK;

        managementApiTokenHttpResponseMessageTwo.Content
            = new StringContent("{\"access_token\":\"Token 2\",\"expires_in\": 86400}");

        messageHandler.SetupSendAsyncResponse(managementApiTokenHttpResponseMessageTwo);

        // ACT.
        string resultTwo = await managementClient.GetTokenAsync(this.apiConfiguration)
                                                 .ConfigureAwait(true);

        // ASSERT.
        resultOne.Should()
                 .NotBe(resultTwo);
    }

    [Auth0]
    [AutoDomainData]
    [Theory(DisplayName = $"{testPrefix} returns a new access token.")]
    internal async Task RequestM2MToken_ReturnsNewAccessToken(
        [Frozen] Mock<HttpMessageHandler> messageHandler, [Frozen] Mock<IDateTimeProvider> dateTimeProvider,
        ManagementClient managementClient)
    {
        // MOCK SETUP.
        dateTimeProvider.Setup(static x => x.Now)
                        .Returns(DateTime.Now);

        using var managementApiTokenHttpResponseMessageOne = new HttpResponseMessage();
        managementApiTokenHttpResponseMessageOne.StatusCode = HttpStatusCode.OK;

        managementApiTokenHttpResponseMessageOne.Content
            = new StringContent("{\"access_token\":\"Token 1\",\"expires_in\": 86400}");

        messageHandler.SetupSendAsyncResponse(managementApiTokenHttpResponseMessageOne);

        // ACT.
        ApiManagementToken result = await managementClient.GetM2MTokenAsync(this.apiConfiguration.TokenEndpoint,
                                                              this.apiConfiguration.ClientId,
                                                              this.apiConfiguration.ClientSecret,
                                                              this.apiConfiguration.Audience, "client_credentials")
                                                          .ConfigureAwait(true);

        // ASSERT.
        result.AccessToken.Should()
              .Be("Token 1");

        result.ExpiresIn.Should()
              .Be(86400);

        result.TokenType.Should()
              .BeEmpty();

        result.IsExpired(dateTimeProvider.Object)
              .Should()
              .BeFalse();
    }

    [AttributeUsage(AttributeTargets.Method)]
    private sealed class AutoDomainDataAttribute() : AutoDataAttribute(static () =>
    {
        var fixture = new Fixture();
        fixture.Customize(new AutoMoqCustomization());

        fixture.Customize<HttpClient>(static composer => composer
                                                         .FromFactory(static (HttpMessageHandler handler) =>
                                                             new HttpClient(handler))
                                                         .OmitAutoProperties());

        return fixture;
    });
}
