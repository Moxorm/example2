using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aggregation.Data.Base;
using Aggregation.IntegrationTests.Tools.Extensions;
using Aggregation.IntegrationTests.Tools.Models.Builders;
using Aggregation.IntegrationTests.Tools.Models.Filters;
using Aggregation.System.External.Label;
using Aggregation.System.External.LinkedAo;
using Aggregation.System.Ident;
using Authentication.IntegrationTests.Tools.Extensions;
using Authentication.IntegrationTests.Tools.Models;
using Castle.Core.Internal;
using FTSService.Core.Enums;
using Notification.IntegrationTests.Tools.Extensions;
using Notification.IntegrationTests.Tools.Models;
using NUnit.Framework;
using Processing.VmCaches.Calculator;
using SearchQueries.ArchiveEngine.IntegrationTests.Tools.Helpers;
using SearchQueries.Common.Core;
using SearchQueries.IntegrationTests.Tools.Helpers.Extensions;
using SearchQueries.IntegrationTests.Tools.Models;
using SearchQueries.Processing.IntegrationTests.Tests.Initialization;
using Storage.ClientService.Http.IntegrationTests.Helpers;
using TestCommonData;
using Tests.Extensions;
using Tests.Models;
using TestToolsCommon.DependencyInjection;
using TestToolsCommon.Models;
using TService.Clients.NamedGalleries.Facade;

namespace SearchQueries.Processing.IntegrationTests.Tests.TestBase
{
    public class AuthenticatedTestBase
    {
        public AdditionalParameters _additionalParameters;
        protected long UserId;
        
        
        protected string VoiceModel;
        protected string VoiceModel2;
        protected string VoiceModel3;
        protected string VoiceModel4;
        protected string VoiceModel5;
        protected List<Guid> AggregationId;
        
        private string UserLogin => MainTestLoginData.TestsUserWithAllPermissionsLogin;
        private string UserPassword => MainTestLoginData.TestsUserWithAllPermissionsPassword;

        protected async Task<string> GetIndexKey()
        {
            (var storageKey, _) =
                await Loader.ContainerService.UploadTestFile("TestDefaultData/trans_results.txt",
                    _additionalParameters);

            await Loader.ContainerService.IndexDocumentAsync(new TestDocumentInformation()
            {
                Name = "Document",
                StorageKey = storageKey,
                DocumentType = DocType.Doc,
            }, _additionalParameters.SessionId).AssertSuccess();
            return storageKey;
        }
        protected async Task<string> GetOtherIndexKey()
        {
            (var storageKey, _) =
                await Loader.ContainerService.UploadTestFile("TestDefaultData/other_trans_results.txt",
                    _additionalParameters);

            await Loader.ContainerService.IndexDocumentAsync(new TestDocumentInformation()
            {
                Name = "Document",
                StorageKey = storageKey,
                DocumentType = DocType.Doc,
            }, _additionalParameters.SessionId).AssertSuccess();
            return storageKey;
        }


        [SetUp]
        public async Task Init()
        {
            var result = await Loader.ContainerService
                .LoginAsync(
                    UserLogin,
                    UserPassword)
                .AssertSuccess()
                .ConfigureAwait(false);
            _additionalParameters = new AdditionalParameters(result.Result.SessionIdentifier);
            UserId = (long)result.Result.BaseUserId;
        }
        
        [OneTimeSetUp]
        public async Task OneInit()
        {
            var result = await Loader.ContainerService
                .LoginAsync(
                    UserLogin,
                    UserPassword)
                .AssertSuccess()
                .ConfigureAwait(false);
            var additionalParameters = new AdditionalParameters(result.Result.SessionIdentifier);
            UserId = (long) Loader.ContainerService.GetUserInfoAsync(
                new Service(additionalParameters.SessionId, new string[] { })).Result.Result.UserIdentifier;

            VoiceModel = EnrollDataGeneratorHelper.AsqEngMaleModelAudio4.IsNullOrEmpty()
                ? null
                : EnrollDataGeneratorHelper.GetModelPathByResultName(EnrollDataGeneratorHelper.AsqEngMaleModelAudio4);
            VoiceModel2 = EnrollDataGeneratorHelper.AsqClusterModelName1.IsNullOrEmpty()
                ? null
                : EnrollDataGeneratorHelper.GetModelPathByResultName(EnrollDataGeneratorHelper.AsqClusterModelName1);
            VoiceModel3 = EnrollDataGeneratorHelper.AsqEngMaleModelAudio2.IsNullOrEmpty()   
                ? null
                : EnrollDataGeneratorHelper.GetModelPathByResultName(EnrollDataGeneratorHelper.AsqEngMaleModelAudio2);
            VoiceModel4 = EnrollDataGeneratorHelper.AsqEngMaleModelAudio3.IsNullOrEmpty()
                ? null
                : EnrollDataGeneratorHelper.GetModelPathByResultName(EnrollDataGeneratorHelper.AsqEngMaleModelAudio3);
            VoiceModel5 = EnrollDataGeneratorHelper.AsqEngMaleModelAudio1.IsNullOrEmpty()
                ? null
                : EnrollDataGeneratorHelper.GetModelPathByResultName(EnrollDataGeneratorHelper.AsqEngMaleModelAudio1);

            //clean all entity with model what we need
            var testFieldFilter = new TestFieldFilter()
                .SetFieldPath(new[] {"VoiceModel", "VoiceModelPath"})
                .SetMetaDataType(IdentTypes.VoiceModelTypeId.ToString())
                .SetTargetValue(VoiceModel)
                .SetPredicate(Predicate.Eq)
                .SetMain(false);

            var filter = new AggregationFilterBuilder()
                .SetOperator(Operator.AND)
                .AddFieldFilter(testFieldFilter)
                .Build();

            var entities = await Loader.ContainerService
                .GetEntitiesByFilterAsync(filter, additionalParameters)
                .AssertSuccess()
                .ConfigureAwait(false);
            foreach (var entity in entities.Result)
            {
                await Loader.ContainerService
                    .RemoveEntityAsync(entity.Id, additionalParameters)
                    .AssertSuccess()
                    .ConfigureAwait(false);
            }

            var allSq = (await Loader.ContainerService
                .GetSearchQueriesByFilterAsync(new TestSearchQueryFilter() {QueryType = QueryType.Processing},
                    additionalParameters)
                .AssertSuccess()
                .ConfigureAwait(false)).Result;

            if (!allSq.IsNullOrEmpty())
            {
                foreach (var psq in allSq)
                {
                    await Loader.ContainerService
                        .DeleteSearchQueryAsync(new TestByIdParams() {Id = psq.Id}, additionalParameters)
                        .ConfigureAwait(false);
                }
            }

            var galleryName = Loader.ContainerService.ResolveItem<IGalleryNameCalculator>()
                .Calculate(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            var galleryName2 = Loader.ContainerService.ResolveItem<IGalleryNameCalculator>()
                .Calculate(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            if (VoiceModel != null)
            {
                await Loader.ContainerService.ResolveItem<IVoiceModelsCachesService>()
                    .AddModelsToGallery(galleryName,
                        new[]
                        {
                            VoiceModel
                        }, CancellationToken.None);
            }

            if (VoiceModel2 != null)
            {
                await Loader.ContainerService.ResolveItem<IVoiceModelsCachesService>()
                    .AddModelsToGallery(galleryName2,
                        new[]
                        {
                            VoiceModel2
                        }, CancellationToken.None);
            }

            if (VoiceModel3 != null)
            {
                await Loader.ContainerService.ResolveItem<IVoiceModelsCachesService>()
                    .AddModelsToGallery(galleryName2,
                        new[]
                        {
                            VoiceModel3
                        }, CancellationToken.None);
            }

            if (VoiceModel4 != null)
            {
                await Loader.ContainerService.ResolveItem<IVoiceModelsCachesService>()
                    .AddModelsToGallery(galleryName2,
                        new[]
                        {
                            VoiceModel4
                        }, CancellationToken.None);
            }

            if (VoiceModel5 != null)
            {
                await Loader.ContainerService.ResolveItem<IVoiceModelsCachesService>()
                    .AddModelsToGallery(galleryName2,
                        new[]
                        {
                            VoiceModel4
                        }, CancellationToken.None);
            }

            AggregationId = new List<Guid>();
            
            await Loader.ContainerService
                .LogoutAsync(additionalParameters.SessionId)
                .AssertSuccess()
                .ConfigureAwait(false);
        }

        protected void AddUserToNotificationList(TestCreateProcessingSearchQueryParams searchQueryParams)
        {
            searchQueryParams.Actions
                .Where(a => a.ActionType == ActionType.Notify)
                .Select(a => a as TestNotifyAction)
                .ToList()
                .ForEach(a => a.To = a.To.Append(UserId).ToArray());
        }
        protected void AddUserToNotificationList(TestChangeProcessingQueryParams searchQueryParams)
        {
            searchQueryParams.Actions.Updated
                .Where(a => a.Action.ActionType == ActionType.Notify)
                .Select(a => a.Action as TestNotifyAction)
                .ToList()
                .ForEach(a => a.To = a.To.Append(UserId).ToArray());
        }

        protected async Task<bool> WatFoThreeActions(long notice1, long count, Guid entityId, Guid createdAsq)
        {
            var notice2 = (await Loader.ContainerService
                .GetNotificationsCountAsync(_additionalParameters.SessionId,
                    new TestNotificationFilter().SetExternalKeys(new List<string>()
                        {createdAsq.ToString()})).AssertSuccess().ConfigureAwait(false)).Result;
            var resultEntity = (await Loader.ContainerService
                .GetEntityAsync(entityId, _additionalParameters)
                .AssertSuccess()
                .ConfigureAwait(false)).Result;
            var rightCount = Equals(notice1 + count, notice2);
            var rightMark = resultEntity.AdditionalInformation.Any(into =>
                into.TypeId.Equals(LabelInformationCoreKeys.TypeId));
            var rightLink = resultEntity.AdditionalInformation.Any(into =>
                into.TypeId.Equals(LinkedAoInformationCoreKeys.TypeId));
            Loader.ContainerService.GetTestLogger().Info($"Wait for result: " +
                                                         $"notice1 {notice1}, " +
                                                         $"notice2 {notice2}, " +
                                                         $"count {count} " +
                                                         $"rightMark {rightMark} " +
                                                         $"rightLink {rightLink} ");
            return  rightCount &&
                    rightMark && 
                    rightLink;
        }

        protected async Task<bool> WatFoTwoActions(Guid entityId)
        {
            var resultEntity = (await Loader.ContainerService
                .GetEntityAsync(entityId, _additionalParameters)
                .AssertSuccess()
                .ConfigureAwait(false)).Result;
            var rightMark = resultEntity.AdditionalInformation.Any(into =>
                into.TypeId.Equals(LabelInformationCoreKeys.TypeId));
            var rightLink = resultEntity.AdditionalInformation.Any(into =>
                into.TypeId.Equals(LinkedAoInformationCoreKeys.TypeId));
            
            Loader.ContainerService.GetTestLogger().Info(
                $"Got entity 123456789");
            Loader.ContainerService.GetTestLogger().Info(
                $"{resultEntity}");
            return rightMark && 
                   rightLink;
        }

        protected async Task<bool> WatFoMarkActions(Guid entityId)
        {
            var resultEntity = (await Loader.ContainerService
                .GetEntityAsync(entityId, _additionalParameters)
                .AssertSuccess()
                .ConfigureAwait(false)).Result;
            var rightMark = resultEntity.AdditionalInformation.Any(into =>
                into.TypeId.Equals(LabelInformationCoreKeys.TypeId));
            return rightMark;
        }

        protected async Task<bool> CheckEmpty(Guid entityId)
        {
            var resultEntity = (await Loader.ContainerService
                .GetEntityAsync(entityId, _additionalParameters)
                .AssertSuccess()
                .ConfigureAwait(false)).Result;
            var rightMark = resultEntity.AdditionalInformation.Any(into =>
                into.TypeId.Equals(LabelInformationCoreKeys.TypeId));
            var rightLink = resultEntity.AdditionalInformation.Any(into =>
                into.TypeId.Equals(LinkedAoInformationCoreKeys.TypeId));
            return  !rightMark && 
                    !rightLink;
        }
        
        [TearDown]
        public async Task ShutDown()
        {
            foreach (var entity in AggregationId)
            {
                await Loader.ContainerService
                    .RemoveEntityAsync(entity,_additionalParameters).ConfigureAwait(false);
            }
            AggregationId = new List<Guid>();
            
            await Loader.ContainerService
                .LogoutAsync(_additionalParameters.SessionId)
                .ConfigureAwait(false);
        }
    }
}
