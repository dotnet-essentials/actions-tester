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
namespace Kwality.UVault.Users.QA;

using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.Xunit2;

using FluentAssertions;

using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.Core.Keys;
using Kwality.UVault.QA.Common.Xunit.Traits;
using Kwality.UVault.Users.Managers;
using Kwality.UVault.Users.Models;
using Kwality.UVault.Users.Operations.Mappers;
using Kwality.UVault.Users.QA.Internal.Factories;

using Xunit;

public sealed class UserManagementDefaultIntKeyTests
{
    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Get by key raises an exception when the key is NOT found.")]
    internal async Task GetByKey_UnknownKey_RaisesException(IntKey key)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager = new UserManagerFactory().Create<Model, IntKey>();

        // ACT.
        Func<Task<Model>> act = () => manager.GetByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Failed to read user: `{key}`. Not found.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Get by email returns NO users when NO users are found.")]
    internal async Task GetByEmail_UnknownEmail_ReturnsEmptyCollection(Model model)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager = new UserManagerFactory().Create<Model, IntKey>();

        await manager.CreateAsync(model, new UserCreateOperationMapper())
                     .ConfigureAwait(true);

        // ACT.
        IEnumerable<Model> result = await manager.GetByEmailAsync("email")
                                                 .ConfigureAwait(true);

        // ASSERT.
        result.Should()
              .BeEmpty();
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Get by email returns the matches.")]
    internal async Task GetByEmail_SingleMatch_ReturnsMatches(List<Model> models)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager = new UserManagerFactory().Create<Model, IntKey>();

        foreach (Model model in models)
        {
            await manager.CreateAsync(model, new UserCreateOperationMapper())
                         .ConfigureAwait(true);
        }

        // ACT.
        Model expected = models.Skip(1)
                               .First();

        IEnumerable<Model> result = await manager.GetByEmailAsync(expected.Email)
                                                 .ConfigureAwait(true);

        // ASSERT.
        result.Should()
              .BeEquivalentTo(new[] { expected });
    }

    [FixedEmail]
    [UserManagement]
    [Theory(DisplayName = "Get by email returns the matches.")]
    internal async Task GetByEmail_MultipleMatches_ReturnsMatches(List<Model> models)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager = new UserManagerFactory().Create<Model, IntKey>();

        foreach (Model model in models)
        {
            await manager.CreateAsync(model, new UserCreateOperationMapper())
                         .ConfigureAwait(true);
        }

        // ACT.
        Model expected = models.Skip(1)
                               .First();

        IEnumerable<Model> result = await manager.GetByEmailAsync(expected.Email)
                                                 .ConfigureAwait(true);

        // ASSERT.
        result.Should()
              .BeEquivalentTo(models);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Create succeeds.")]
    internal async Task Create_Succeeds(Model model)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager = new UserManagerFactory().Create<Model, IntKey>();

        // ACT.
        IntKey key = await manager.CreateAsync(model, new UserCreateOperationMapper())
                                  .ConfigureAwait(true);

        // ASSERT.
        (await manager.GetByKeyAsync(key)
                      .ConfigureAwait(true)).Should()
                                            .BeEquivalentTo(model);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Create raises an exception when another user with the same key already exist.")]
    internal async Task Create_KeyExists_RaisesException(Model model)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager = new UserManagerFactory().Create<Model, IntKey>();

        await manager.CreateAsync(model, new UserCreateOperationMapper())
                     .ConfigureAwait(true);

        // ACT.
        Func<Task<IntKey>> act = () => manager.CreateAsync(model, new UserCreateOperationMapper());

        // ASSERT.
        await act.Should()
                 .ThrowAsync<CreateException>()
                 .WithMessage($"Failed to create user: `{model.Key}`. Duplicate key.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Update succeeds.")]
    internal async Task Update_Succeeds(Model model)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager = new UserManagerFactory().Create<Model, IntKey>();

        IntKey key = await manager.CreateAsync(model, new UserCreateOperationMapper())
                                  .ConfigureAwait(true);

        // ACT.
        model.Email = "kwality.uvault@github.com";

        await manager.UpdateAsync(key, model, new UserUpdateOperationMapper())
                     .ConfigureAwait(true);

        // ASSERT.
        (await manager.GetByKeyAsync(key)
                      .ConfigureAwait(true)).Should()
                                            .BeEquivalentTo(model);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Update raises an exception when the key is not found.")]
    internal async Task Update_UnknownKey_RaisesException(IntKey key, Model model)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager = new UserManagerFactory().Create<Model, IntKey>();

        // ACT.
        Func<Task> act = () => manager.UpdateAsync(key, model, new UserUpdateOperationMapper());

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UpdateException>()
                 .WithMessage($"Failed to update user: `{key}`. Not found.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Delete succeeds.")]
    internal async Task Delete_Succeeds(Model model)
    {
        // ARRANGE.
        UserManager<Model, IntKey> manager = new UserManagerFactory().Create<Model, IntKey>();

        IntKey key = await manager.CreateAsync(model, new UserCreateOperationMapper())
                                  .ConfigureAwait(true);

        // ACT.
        await manager.DeleteByKeyAsync(key)
                     .ConfigureAwait(true);

        // ASSERT.
        Func<Task<Model>> act = () => manager.GetByKeyAsync(key);

        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Failed to read user: `{key}`. Not found.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [UserManagement]
    [Theory(DisplayName = "Delete succeeds when the key is not found.")]
    internal async Task Delete_UnknownKey_Succeeds(IntKey key)
    {
        // ARRANGE.
        UserManager<Model, IntKey> userManager = new UserManagerFactory().Create<Model, IntKey>();

        // ACT.
        Func<Task> act = () => userManager.DeleteByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .NotThrowAsync()
                 .ConfigureAwait(true);
    }

    internal sealed class Model(IntKey key, string email) : UserModel<IntKey>(key, email);

    [AttributeUsage(AttributeTargets.Method)]
    private sealed class FixedEmailAttribute() : AutoDataAttribute(static () =>
    {
        var fixture = new Fixture();
        var email = $"{fixture.Create<string>()}@acme.com";
        fixture.Customizations.Add(new FixedEmailSpecimenBuilder(email));

        return fixture;
    })
    {
        private sealed class FixedEmailSpecimenBuilder(string email) : ISpecimenBuilder
        {
            public object Create(object request, ISpecimenContext context)
            {
                if (request is Type type && type == typeof(Model))
                {
                    return new Model(context.Create<IntKey>(), email);
                }

                return new NoSpecimen();
            }
        }
    }
}
