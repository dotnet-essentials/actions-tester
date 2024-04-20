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

using AutoFixture.Xunit2;

using FluentAssertions;

using JetBrains.Annotations;

using Kwality.UVault.Core.Exceptions;
using Kwality.UVault.Core.Helpers;
using Kwality.UVault.Core.Keys;
using Kwality.UVault.Core.Models;
using Kwality.UVault.M2M.Managers;
using Kwality.UVault.M2M.Models;
using Kwality.UVault.M2M.Operations.Filters.Abstractions;
using Kwality.UVault.M2M.Operations.Mappers;
using Kwality.UVault.M2M.QA.Internal.Factories;
using Kwality.UVault.QA.Common.Xunit.Traits;

using Xunit;

public sealed class ApplicationManagementDefaultIntKeyTests
{
    private const int pageSize = 100;
    private readonly ApplicationManager<Model, IntKey>
        manager = new ApplicationManagerFactory().Create<Model, IntKey>();

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Get all (pageIndex: 0, all data showed) succeeds.")]
    internal async Task GetAll_FirstPageWhenAllDataShowed_Succeeds(Model model)
    {
        // ARRANGE.
        await this.manager.CreateAsync(model, new CreateOperationMapper())
                  .ConfigureAwait(true);

        // ACT.
        PagedResultSet<Model> result = await this.manager.GetAllAsync(0, 10)
                                                 .ConfigureAwait(true);

        // ASSERT.
        result.HasNextPage.Should()
              .BeFalse();

        result.ResultSet.Count()
              .Should()
              .Be(1);

        result.ResultSet.Take(1)
              .First()
              .Should()
              .BeEquivalentTo(model,
                  static options => options.Excluding(static application => application.ClientSecret));
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Get all (pageIndex: 1, all data showed) succeeds.")]
    internal async Task GetAll_SecondPageWhenAllDataShowed_Succeeds(Model model)
    {
        // ARRANGE.
        await this.manager.CreateAsync(model, new CreateOperationMapper())
                  .ConfigureAwait(true);

        // ACT.
        PagedResultSet<Model> result = await this.manager.GetAllAsync(1, 10)
                                                 .ConfigureAwait(true);

        // ASSERT.
        result.HasNextPage.Should()
              .BeFalse();

        result.ResultSet.Count()
              .Should()
              .Be(0);
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Get all (pageIndex: 0, all data NOT showed) succeeds.")]
    internal async Task GetAll_FirstPageWhenNotAllDataShowed_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        await this.manager.CreateAsync(modelOne, new CreateOperationMapper())
                  .ConfigureAwait(true);

        await this.manager.CreateAsync(modelTwo, new CreateOperationMapper())
                  .ConfigureAwait(true);

        // ACT.
        PagedResultSet<Model> result = await this.manager.GetAllAsync(0, 1)
                                                 .ConfigureAwait(true);

        // ASSERT.
        result.HasNextPage.Should()
              .BeTrue();

        result.ResultSet.Count()
              .Should()
              .Be(1);

        result.ResultSet.Take(1)
              .First()
              .Should()
              .BeEquivalentTo(modelOne,
                  static options => options.Excluding(static application => application.ClientSecret));
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Get all (pageIndex: 1, all data NOT showed) succeeds.")]
    internal async Task GetAll_SecondPageWhenNotAllDataShowed_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        await this.manager.CreateAsync(modelOne, new CreateOperationMapper())
                  .ConfigureAwait(true);

        await this.manager.CreateAsync(modelTwo, new CreateOperationMapper())
                  .ConfigureAwait(true);

        // ACT.
        PagedResultSet<Model> result = await this.manager.GetAllAsync(1, 1)
                                                 .ConfigureAwait(true);

        // ASSERT.
        result.HasNextPage.Should()
              .BeFalse();

        result.ResultSet.Count()
              .Should()
              .Be(1);

        result.ResultSet.Take(1)
              .First()
              .Should()
              .BeEquivalentTo(modelTwo,
                  static options => options.Excluding(static application => application.ClientSecret));
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Get all with filter succeeds.")]
    internal async Task GetAll_WithFilter_Succeeds(Model modelOne, Model modelTwo)
    {
        // ARRANGE.
        await this.manager.CreateAsync(modelOne, new CreateOperationMapper())
                  .ConfigureAwait(true);

        await this.manager.CreateAsync(modelTwo, new CreateOperationMapper())
                  .ConfigureAwait(true);

        PagedResultSet<Model> result = await this
                                             .manager.GetAllAsync(0, 10,
                                                 new OperationFilter(modelTwo.Name ?? string.Empty))
                                             .ConfigureAwait(true);

        // ASSERT.
        result.ResultSet.Count()
              .Should()
              .Be(1);

        result.ResultSet.First()
              .Should()
              .BeEquivalentTo(modelTwo,
                  static options => options.Excluding(static application => application.ClientSecret));
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Get by key raises an exception when the key is NOT found.")]
    internal async Task GetByKey_UnknownKey_RaisesException(IntKey key)
    {
        // ACT.
        Func<Task<Model>> act = () => this.manager.GetByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Failed to read application: `{key}`. Not found.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Create succeeds.")]
    internal async Task Create_Succeeds(Model model)
    {
        // ACT.
        IntKey key = await this.manager.CreateAsync(model, new CreateOperationMapper())
                               .ConfigureAwait(true);

        // ASSERT.
        (await this.manager.GetByKeyAsync(key)
                   .ConfigureAwait(true)).Should()
                                         .BeEquivalentTo(model);
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Update succeeds.")]
    internal async Task Update_Succeeds(Model model)
    {
        // ARRANGE.
        IntKey key = await this.manager.CreateAsync(model, new CreateOperationMapper())
                               .ConfigureAwait(true);

        // ACT.
        model.Name = "UVault (Sample application)";

        await this.manager.UpdateAsync(key, model, new ApplicationUpdateOperationMapper())
                  .ConfigureAwait(true);

        // ASSERT.
        (await this.manager.GetAllAsync(0, pageSize)
                   .ConfigureAwait(true)).ResultSet.Count()
                                         .Should()
                                         .Be(1);

        (await this.manager.GetByKeyAsync(key)
                   .ConfigureAwait(true)).Should()
                                         .BeEquivalentTo(model);
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Update raises an exception when the key is not found.")]
    internal async Task Update_UnknownKey_RaisesException(IntKey key, Model model)
    {
        // ACT.
        Func<Task> act = () => this.manager.UpdateAsync(key, model, new ApplicationUpdateOperationMapper());

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UpdateException>()
                 .WithMessage($"Failed to update application: `{key}`. Not found.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Delete succeeds.")]
    internal async Task Delete_Succeeds(Model model)
    {
        // ARRANGE.
        IntKey key = await this.manager.CreateAsync(model, new CreateOperationMapper())
                               .ConfigureAwait(true);

        // ACT.
        await this.manager.DeleteByKeyAsync(key)
                  .ConfigureAwait(true);

        // ASSERT.
        Func<Task<Model>> act = () => this.manager.GetByKeyAsync(key);

        await act.Should()
                 .ThrowAsync<ReadException>()
                 .WithMessage($"Failed to read application: `{key}`. Not found.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Delete succeeds when the key is not found.")]
    internal async Task Delete_UnknownKey_Succeeds(IntKey key)
    {
        // ACT.
        Func<Task> act = () => this.manager.DeleteByKeyAsync(key);

        // ASSERT.
        await act.Should()
                 .NotThrowAsync()
                 .ConfigureAwait(true);
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Rotate client secret raises an exception when the key is NOT found.")]
    internal async Task RotateClientSecret_UnknownKey_RaisesException(IntKey key)
    {
        // ACT.
        Func<Task<Model>> act = () => this.manager.RotateClientSecretAsync(key);

        // ASSERT.
        await act.Should()
                 .ThrowAsync<UpdateException>()
                 .WithMessage($"Failed to update application: `{key}`. Not found.")
                 .ConfigureAwait(true);
    }

    [AutoData]
    [M2MManagement]
    [Theory(DisplayName = "Rotate client secret succeeds.")]
    internal async Task RotateClientSecret_Succeeds(Model model)
    {
        // ARRANGE.
        IntKey key = await this.manager.CreateAsync(model, new CreateOperationMapper())
                               .ConfigureAwait(true);

        string? initialClientSecret = model.ClientSecret;

        // ACT.
        await this.manager.RotateClientSecretAsync(key)
                  .ConfigureAwait(true);

        // ASSERT.
        Model application = await this.manager.GetByKeyAsync(key)
                                      .ConfigureAwait(true);

        initialClientSecret.Should()
                           .NotMatch(application.ClientSecret);
    }

    private sealed class OperationFilter(string name) : IApplicationFilter
    {
        public TDestination Create<TDestination>()
            where TDestination : class
        {
            if (typeof(TDestination) != typeof(Func<Model, bool>))
            {
                throw new ReadException(
                    $"Invalid {nameof(IApplicationFilter)}: Destination is NOT `{typeof(Func<Model, bool>).Name}`.");
            }

            return ((Func<Model, bool>)Filter).UnsafeAs<Func<Model, bool>, TDestination>();

            // The filter which is filters out data in the store.
            bool Filter(Model model)
            {
                return model.Name == name;
            }
        }
    }

    [UsedImplicitly]
#pragma warning disable CA1812
    internal sealed class Model : ApplicationModel<IntKey>
#pragma warning restore CA1812
    {
        public Model(IntKey key, string name)
            : base(key)
        {
            this.Name = name;
        }
    }
}
