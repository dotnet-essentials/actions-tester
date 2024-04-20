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
namespace Kwality.UVault.E2E.App.Builders;

using Kwality.UVault.Core.Auth0.Configuration;
using Kwality.UVault.Core.Extensions;
using Kwality.UVault.Core.Keys;
using Kwality.UVault.E2E.App.Db.Context;
using Kwality.UVault.E2E.App.Models;
using Kwality.UVault.E2E.App.Models.Mappers;
using Kwality.UVault.E2E.App.Stores;
using Kwality.UVault.E2E.App.Web.Models;
using Kwality.UVault.QA.Common.System;
using Kwality.UVault.Users.Auth0.Extensions;
using Kwality.UVault.Users.Extensions;
using Kwality.UVault.Users.Managers;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;

using UserCreateOperationMapper = Kwality.UVault.E2E.App.Models.Operations.Mappers.UserCreateOperationMapper;

internal static class E2EApplicationBuilder
{
    public static IWebHostBuilder CreateApplication()
    {
        return new WebHostBuilder().UseStartup<Program>()
                                   .ConfigureServices(RegisterServices)
                                   .Configure(ConfigureApp);
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddRouting();

        services.AddUVault(static options =>
        {
            options.UseUserManagement<UserModel, StringKey, UserData>(static userManagementOptions =>
            {
                userManagementOptions.UseAuth0Store<UserModel, UserModelMapper>(GetApiConfiguration());
                userManagementOptions.UseDataStore<UserDataStore, UserData>();
            }, ServiceLifetime.Singleton);
        });

        services.AddDbContext<E2EDbContext>(static dbOptions => dbOptions.UseInMemoryDatabase("Kwality.UVault.E2E"));
    }

    private static void ConfigureApp(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(static builder => { builder.MapPost("/api/v1/users/", HandleCreateUserAsync); });
        app.UseEndpoints(static builder => { builder.MapPut("/api/v1/users/{id}", HandleUpdateUserAsync); });
        app.UseEndpoints(static builder => { builder.MapDelete("/api/v1/users/{id}", HandleDeleteUserAsync); });
    }

    private static async Task<IResult> HandleCreateUserAsync(
        [FromServices] UserManager<UserModel, StringKey, UserData> userManager, [FromBody] UserCreateModel model)
    {
        var userModel = new UserModel(new StringKey(model.Email), model.FirstName, model.LastName, model.Password);
        var userData = new UserData(model.Email);

        StringKey key = await userManager.CreateAsync(userModel, userData, new UserCreateOperationMapper())
                                         .ConfigureAwait(false);

        return Results.Ok(new UserCreatedModel(key.Value, model.Email, model.FirstName, model.LastName));
    }

    private static async Task<IResult> HandleUpdateUserAsync(
        [FromServices] UserManager<UserModel, StringKey, UserData> userManager, [FromRoute] string id,
        [FromBody] UserUpdateModel model)
    {
        var userData = new UserData(model.Email);

        await userManager.UpdateDataByKeyAsync(new StringKey(id), userData)
                         .ConfigureAwait(false);

        return Results.Ok();
    }

    private static async Task<IResult> HandleDeleteUserAsync(
        [FromServices] UserManager<UserModel, StringKey, UserData> userManager, [FromRoute] string id)
    {
        await userManager.DeleteByKeyAsync(new StringKey(id))
                         .ConfigureAwait(false);

        await userManager.DeleteDataByKeyAsync(new StringKey(id))
                         .ConfigureAwait(false);

        return Results.Ok();
    }

    private static ApiConfiguration GetApiConfiguration()
    {
        return new ApiConfiguration(new Uri(Environment.AUTH0_TOKEN_ENDPOINT), Environment.AUTH0_CLIENT_ID,
            Environment.AUTH0_CLIENT_SECRET, Environment.AUTH0_AUDIENCE);
    }
}
