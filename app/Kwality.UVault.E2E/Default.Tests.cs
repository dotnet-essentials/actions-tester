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
namespace Kwality.UVault.E2E;

using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;

using Kwality.UVault.E2E.App.Builders;
using Kwality.UVault.E2E.App.Web.Models;
using Kwality.UVault.QA.Common.Xunit.Traits;

using Microsoft.AspNetCore.TestHost;

using Xunit;

[E2E]
[Auth0]
[Collection("Auth0")]
[SuppressMessage("ReSharper", "MemberCanBeFileLocal")]
public sealed class DefaultTests
{
    [Fact]
    public async Task RunAsync()
    {
        // ARRANGE.
        using var server = new TestServer(E2EApplicationBuilder.CreateApplication());
        using HttpClient httpClient = server.CreateClient();

        // ACT.
        string userId = await CreateUserAsync(httpClient)
            .ConfigureAwait(true);

        await UpdateUserAsync(httpClient, userId)
            .ConfigureAwait(true);

        await DeleteUserAsync(httpClient, userId)
            .ConfigureAwait(true);

        Assert.True(true);
    }

    private static async Task<string> CreateUserAsync(HttpClient httpClient)
    {
        var userModel = new UserCreateModel("kevin.dconinck@gmail.com", "Kevin", "De Coninck", "MySecur3Passw0rd!!!");
        using var json = JsonContent.Create(userModel);

        HttpResponseMessage response = await httpClient.PostAsync(new Uri("/api/v1/users", UriKind.Relative), json)
                                                       .ConfigureAwait(true);

        UserCreatedModel? responseModel = await response.Content.ReadFromJsonAsync<UserCreatedModel>()
                                                        .ConfigureAwait(true);

        return responseModel?.Id ?? string.Empty;
    }

    private static async Task UpdateUserAsync(HttpClient httpClient, string userId)
    {
        var userModel = new UserUpdateModel("kevin.dconinck+updated@gmail.com");
        using var json = JsonContent.Create(userModel);

        await httpClient.PutAsync(new Uri($"/api/v1/users/{userId}", UriKind.Relative), json)
                        .ConfigureAwait(true);
    }

    private static async Task DeleteUserAsync(HttpClient httpClient, string userId)
    {
        await httpClient.DeleteAsync(new Uri($"/api/v1/users/{userId}", UriKind.Relative))
                        .ConfigureAwait(true);
    }
}
