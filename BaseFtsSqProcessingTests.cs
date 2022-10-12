using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aggregation.IntegrationTests.Tools.Extensions;
using Aggregation.System.External.Label;
using Aggregation.System.External.LinkedAo;
using Authentication.IntegrationTests.Tools.Extensions;
using Castle.Core.Internal;
using FTSService.Core.Enums;
using Notification.IntegrationTests.Tools.Extensions;
using Notification.IntegrationTests.Tools.Models;
using NUnit.Framework;
using SearchQueries.Common.Core;
using SearchQueries.IntegrationTests.Tools.Helpers.Extensions;
using SearchQueries.IntegrationTests.Tools.Models;
using SearchQueries.Processing.IntegrationTests.Tests.Initialization;
using SearchQueries.Processing.IntegrationTests.Tests.TestBase;
using SearchQueries.Processing.IntegrationTests.Tests.TestDefaultData;
using SearchQueries.Processing.IntegrationTests.Tests.Tests.Data;
using TestCommonData;
using Tests.Extensions;
using Tests.Models;
using TestToolsCommon.Models;
using TestFilterSq = SearchQueries.IntegrationTests.Tools.Models.TestFilter;

namespace SearchQueries.Processing.IntegrationTests.Tests.Tests
{
    public abstract class BaseFtsSqProcessingTests : AuthenticatedTestBase
    {
	    [SetUp]
	    public async Task TwoInit()
	    {
		    string userLogin = MainTestLoginData.TestsUserWithAllPermissionsLogin;
		    string userPassword = MainTestLoginData.TestsUserWithAllPermissionsPassword;
		    var result = await Loader.ContainerService
			    .LoginAsync(
				    userLogin,
				    userPassword)
			    .AssertSuccess()
			    .ConfigureAwait(false);
		    var additionalParameters = new AdditionalParameters(result.Result.SessionIdentifier);
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

		    await Loader.ContainerService
			    .LogoutAsync(additionalParameters.SessionId)
			    .AssertSuccess()
			    .ConfigureAwait(false);
	    }

	    #region FTS

		[TestCaseSource(typeof(BaseFtsSqProcessingDataProviders),
			nameof(BaseFtsSqProcessingDataProviders.C443370ProcessingSqActionNotifyFts1Result))]
		[TestCaseSource(typeof(BaseFtsSqProcessingDataProviders),
			nameof(BaseFtsSqProcessingDataProviders.C1117048ProcessingSqActionNotifyFts1ResultTheme))]
		[TestCaseSource(typeof(BaseFtsSqProcessingDataProviders),
			nameof(BaseFtsSqProcessingDataProviders.C1117049ProcessingSqActionNotifyFts1ResultThemeAndWords))]
		[TestCaseSource(typeof(BaseFtsSqProcessingDataProviders),
			nameof(BaseFtsSqProcessingDataProviders.C1117051ProcessingSqActionNotifyFts1ResultThemeAndWordsWordDoNotMatch))]
		[TestCaseSource(typeof(BaseFtsSqProcessingDataProviders),
			nameof(BaseFtsSqProcessingDataProviders.C1117052ProcessingSqActionNotifyFts1ResultThemeAndWordsThemeDoNotMatch))]
		[TestCaseSource(typeof(BaseFtsSqProcessingDataProviders),
			nameof(BaseFtsSqProcessingDataProviders.C1117053ProcessingSqActionNotifyFts1ResultThemeAndWordsDoNotMatch))]
		[TestCaseSource(typeof(BaseFtsSqProcessingDataProviders),
			nameof(BaseFtsSqProcessingDataProviders.C1113205ProcessingSqAction2NotifyFts1Result))]
		public async Task CommonPositiveTestNotificationFts(
			Guid aggregationEntityFilterGuid,
			TestCreateProcessingSearchQueryParams searchQueryToCreate,
			int count)
		{
			AddUserToNotificationList(searchQueryToCreate);

			var createdAsq = (await Loader.ContainerService.CreateProcessingSearchQueryWaitAsync(
					searchQueryToCreate,
					_additionalParameters,
					CancellationToken.None)
				.AssertSuccess()
				.ConfigureAwait(false)).Result;

			var notice1 = (await Loader.ContainerService
				.GetNotificationsCountAsync(_additionalParameters.SessionId,
					new TestNotificationFilter().SetExternalKeys(new List<string>()
						{createdAsq.ToString()})).AssertSuccess().ConfigureAwait(false)).Result;

			var storageKey = await GetIndexKey();

			var testEntity =
				DefaultData.CreateEntityWithTranscriberResultsStorageKey(aggregationEntityFilterGuid,
					storageKey);

			await Loader.ContainerService
				.SaveEntityAsync(
					testEntity,
					_additionalParameters)
				.AssertSuccess()
				.ConfigureAwait(false);
			AggregationId.Add(testEntity.Id);

			Assert.IsTrue(await Waiter.WaitAsync(async () =>
				{
					var notice2 = (await Loader.ContainerService
						.GetNotificationsCountAsync(_additionalParameters.SessionId,
							new TestNotificationFilter().SetExternalKeys(new List<string>()
								{createdAsq.ToString()})).AssertSuccess().ConfigureAwait(false)).Result;
					return Equals(notice1 + count, notice2);
				},
				TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(Loader.WaitTime)), "No new notifications");
		}

		[TestCaseSource(typeof(BaseFtsSqProcessingDataProviders),
			nameof(BaseFtsSqProcessingDataProviders.C443369ProcessingSqActionMarkFts1Result))]
		public async Task CommonPositiveTestMarkFts(
			Guid aggregationEntityFilterGuid,
			TestCreateProcessingSearchQueryParams searchQueryToCreate,
			Guid expectedId,
			string expectedComment)
		{
			var createdAsq = (await Loader.ContainerService.CreateProcessingSearchQueryWaitAsync(
					searchQueryToCreate,
					_additionalParameters,
					CancellationToken.None)
				.AssertSuccess()
				.ConfigureAwait(false)).Result;
			var storageKey = await GetIndexKey();

			var testEntity =
				DefaultData.CreateEntityWithTranscriberResultsStorageKey(aggregationEntityFilterGuid,
					storageKey);

			await Loader.ContainerService
				.SaveEntityAsync(
					testEntity,
					_additionalParameters)
				.AssertSuccess()
				.ConfigureAwait(false);
			AggregationId.Add(testEntity.Id);

			Assert.IsTrue(await Waiter.WaitAsync(async () =>
				{
					var resultEntity = (await Loader.ContainerService
						.GetEntityAsync(testEntity.Id, _additionalParameters)
						.AssertSuccess()
						.ConfigureAwait(false)).Result;
					return resultEntity.AdditionalInformation.Any(into =>
						into.TypeId.Equals(LabelInformationCoreKeys.TypeId));
				},
				TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(Loader.WaitTime)), "No new addition info in aggregation");

			var result = (await Loader.ContainerService
				.GetEntityAsync(testEntity.Id, _additionalParameters)
				.AssertSuccess()
				.ConfigureAwait(false)).Result;
			var info = result.AdditionalInformation
				.FirstOrDefault(x => x.TypeId == LabelInformationCoreKeys.TypeId)
				?.Informations?.FirstOrDefault()?.ToObject<LabelInformationCore>();

			Assert.AreEqual(expectedId, info?.LabelTypeAccountingObjectId, "Mark id is not equal to expected");
			Assert.AreEqual(expectedComment, info?.Comment, "Mark comment is not equal to expected");
		}
		
		[TestCaseSource(typeof(BaseFtsSqProcessingDataProviders),
			nameof(BaseFtsSqProcessingDataProviders.C443371ProcessingSqActionLinkFts1Result))]
		public async Task CommonPositiveTestLinkFts(
			Guid aggregationEntityFilterGuid,
			TestCreateProcessingSearchQueryParams searchQueryToCreate,
			Guid expectedId,
			string expectedComment)
		{
			var createdAsq = (await Loader.ContainerService.CreateProcessingSearchQueryWaitAsync(
					searchQueryToCreate,
					_additionalParameters,
					CancellationToken.None)
				.AssertSuccess()
				.ConfigureAwait(false)).Result;
			var storageKey = await GetIndexKey();

			var testEntity =
				DefaultData.CreateEntityWithTranscriberResultsStorageKey(aggregationEntityFilterGuid,
					storageKey);

			await Loader.ContainerService
				.SaveEntityAsync(
					testEntity,
					_additionalParameters)
				.AssertSuccess()
				.ConfigureAwait(false);
			AggregationId.Add(testEntity.Id);

			Assert.IsTrue(await Waiter.WaitAsync(async () =>
				{
					var resultEntity = (await Loader.ContainerService
						.GetEntityAsync(testEntity.Id, _additionalParameters)
						.AssertSuccess()
						.ConfigureAwait(false)).Result;
					return resultEntity.AdditionalInformation.Any(into =>
						into.TypeId.Equals(LinkedAoInformationCoreKeys.TypeId));
				},
				TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(Loader.WaitTime)), "No new addition info in aggregation");

			var result = (await Loader.ContainerService
				.GetEntityAsync(testEntity.Id, _additionalParameters)
				.AssertSuccess()
				.ConfigureAwait(false)).Result;
			var info = result.AdditionalInformation
				.FirstOrDefault(x => x.TypeId == LinkedAoInformationCoreKeys.TypeId)
				?.Informations?.FirstOrDefault()?.ToObject<LinkedAoInformationCore>();

			Assert.AreEqual(expectedId, info?.AccountingObjectId, "Mark id is not equal to expected");
			Assert.AreEqual(expectedComment, info?.ClassDescriptionKey, "Mark comment is not equal to expected");
		}
		
        
		public static TestDocumentInformation Doc =
			new TestDocumentInformation()
				.SetName("trans_results")
				.SetDocumentType(DocType.Doc)
				.SetFileName(Path.Combine("TestDefaultData/trans_results.txt"));
		
		[TestCaseSource(typeof(BaseFtsSqProcessingDataProviders),
			nameof(BaseFtsSqProcessingDataProviders.C1109301ProcessingSqActionMarkLinkNotifyFts1Result))]
		public async Task CommonPositiveTestFts(
			Guid aggregationEntityFilterGuid,
			TestCreateProcessingSearchQueryParams searchQueryToCreate,
			int count,
			Dictionary<Guid, string> expectedMark,
			Dictionary<Guid, string> expectedLink)
		{
			AddUserToNotificationList(searchQueryToCreate);

			var createdAsq = (await Loader.ContainerService.CreateProcessingSearchQueryWaitAsync(
					searchQueryToCreate,
					_additionalParameters,
					CancellationToken.None)
				.AssertSuccess()
				.ConfigureAwait(false)).Result;

			var notice1 = (await Loader.ContainerService
				.GetNotificationsCountAsync(_additionalParameters.SessionId,
					new TestNotificationFilter().SetExternalKeys(new List<string>()
						{createdAsq.ToString()})).AssertSuccess().ConfigureAwait(false)).Result;

			var storageKey = await GetIndexKey().ConfigureAwait(false);
			Doc = Doc.SetStorageKey(storageKey);
			var filter = new TestFindDocumentsFilter()
				.SetSearchText("Раскольников")
				.SetDocumentsToFind(new []{Doc});
			var docs= await Loader.ContainerService
				.FindDocumentsAsync(
					filter,
					_additionalParameters.SessionId)
				.AssertSuccess()
				.ConfigureAwait(false);

			var testEntity =
				DefaultData.CommonTestEntityWithModelAndRecordDateStorage(DateTimeOffset.UtcNow,aggregationEntityFilterGuid,
					storageKey);

			await Loader.ContainerService
				.SaveEntityAsync(
					testEntity,
					_additionalParameters)
				.AssertSuccess()
				.ConfigureAwait(false);
			AggregationId.Add(testEntity.Id);

			Assert.IsTrue(await Waiter.WaitAsync(async () =>
					await WatFoThreeActions(notice1, count, testEntity.Id, createdAsq),
				TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(Loader.WaitTime)), $"Notify or mark or link is absent");
			var result = (await Loader.ContainerService
				.GetEntityAsync(testEntity.Id, _additionalParameters)
				.AssertSuccess()
				.ConfigureAwait(false)).Result;

			var markInfo = result.AdditionalInformation
				.FirstOrDefault(x => x.TypeId == LabelInformationCoreKeys.TypeId)
				?.Informations?.Select(z => z.ToObject<LabelInformationCore>())
				.ToDictionary(y => y?.LabelTypeAccountingObjectId, y => y?.Comment);

			var linkInfo = result.AdditionalInformation
				.FirstOrDefault(x => x.TypeId == LinkedAoInformationCoreKeys.TypeId)
				?.Informations?.Select(z => z.ToObject<LinkedAoInformationCore>())
				.ToDictionary(y => y?.AccountingObjectId, y => y?.ClassDescriptionKey);

			Assert.That(markInfo, Is.EquivalentTo(expectedMark));
			Assert.That(linkInfo, Is.EquivalentTo(expectedLink));
		}
		
		[TestCaseSource(typeof(BaseFtsSqProcessingDataProviders),
			nameof(BaseFtsSqProcessingDataProviders.C1113211ProcessingSqActionMarkLinkNotifyFts1ResultCreatedBefore))]
		public async Task CommonPositiveTestFtsBefore(
			Guid aggregationEntityFilterGuid,
			TestCreateProcessingSearchQueryParams searchQueryToCreate,
			int count,
			Dictionary<Guid, string> expectedMark,
			Dictionary<Guid, string> expectedLink)
		{
			AddUserToNotificationList(searchQueryToCreate);

			var storageKey = await GetIndexKey();
			var testEntity =
				DefaultData.CommonTestEntityWithModelAndRecordDateStorage(DateTimeOffset.UtcNow,aggregationEntityFilterGuid,
					storageKey);

			await Loader.ContainerService
				.SaveEntityAsync(
					testEntity,
					_additionalParameters)
				.AssertSuccess()
				.ConfigureAwait(false);
			AggregationId.Add(testEntity.Id);
			
			var createdAsq = (await Loader.ContainerService.CreateProcessingSearchQueryWaitAsync(
					searchQueryToCreate,
					_additionalParameters,
					CancellationToken.None)
				.AssertSuccess()
				.ConfigureAwait(false)).Result;
			await Task.Delay(1000);

			var notice1 = (await Loader.ContainerService
				.GetNotificationsCountAsync(_additionalParameters.SessionId,
					new TestNotificationFilter().SetExternalKeys(new List<string>()
						{createdAsq.ToString()})).AssertSuccess().ConfigureAwait(false)).Result;

			await Task.Delay(10000);
			var notice2 = (await Loader.ContainerService
				.GetNotificationsCountAsync(_additionalParameters.SessionId,
					new TestNotificationFilter().SetExternalKeys(new List<string>()
						{createdAsq.ToString()})).AssertSuccess().ConfigureAwait(false)).Result;
			Assert.AreEqual(notice1,notice2);
			
			Assert.IsTrue( await CheckEmpty(testEntity.Id));
		}
		
		[TestCaseSource(typeof(BaseFtsSqProcessingDataProviders),
			nameof(BaseFtsSqProcessingDataProviders.C1113213ProcessingSqActionMarkLinkNotifyFts1ResultCreatedBeforeAndAfter))]
		public async Task CommonPositiveTestFtsBeforeAndAfter(
			Guid aggregationEntityFilterGuid,
			TestCreateProcessingSearchQueryParams searchQueryToCreate,
			int count,
			Dictionary<Guid, string> expectedMark,
			Dictionary<Guid, string> expectedLink)
		{
			AddUserToNotificationList(searchQueryToCreate);

			var storageKey = await GetIndexKey();
			var testEntity =
				DefaultData.CommonTestEntityWithModelAndRecordDateStorage(DateTimeOffset.UtcNow,aggregationEntityFilterGuid,
					storageKey);
			var testEntity2 =
				DefaultData.CommonTestEntityWithModelAndRecordDateStorage(DateTimeOffset.UtcNow,aggregationEntityFilterGuid,
					storageKey);

			await Loader.ContainerService
				.SaveEntityAsync(
					testEntity,
					_additionalParameters)
				.AssertSuccess()
				.ConfigureAwait(false);
			AggregationId.Add(testEntity.Id);
			
			await Task.Delay(1000);
			
			var createdAsq = (await Loader.ContainerService.CreateProcessingSearchQueryWaitAsync(
					searchQueryToCreate,
					_additionalParameters,
					CancellationToken.None)
				.AssertSuccess()
				.ConfigureAwait(false)).Result;
			
			await Loader.ContainerService
				.SaveEntityAsync(
					testEntity2,
					_additionalParameters)
				.AssertSuccess()
				.ConfigureAwait(false);
			AggregationId.Add(testEntity2.Id);
			
			var notice1 = (await Loader.ContainerService
				.GetNotificationsCountAsync(_additionalParameters.SessionId,
					new TestNotificationFilter().SetExternalKeys(new List<string>()
						{createdAsq.ToString()})).AssertSuccess().ConfigureAwait(false)).Result;

			Assert.IsTrue(await Waiter.WaitAsync(async () =>
					await WatFoThreeActions(notice1, count, testEntity2.Id, createdAsq),
				TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(Loader.WaitTime)), $"Notify or mark or link is absent");
			var result = (await Loader.ContainerService
				.GetEntityAsync(testEntity2.Id, _additionalParameters)
				.AssertSuccess()
				.ConfigureAwait(false)).Result;

			var markInfo = result.AdditionalInformation
				.FirstOrDefault(x => x.TypeId == LabelInformationCoreKeys.TypeId)
				?.Informations?.Select(z => z.ToObject<LabelInformationCore>())
				.ToDictionary(y => y?.LabelTypeAccountingObjectId, y => y?.Comment);

			var linkInfo = result.AdditionalInformation
				.FirstOrDefault(x => x.TypeId == LinkedAoInformationCoreKeys.TypeId)
				?.Informations?.Select(z => z.ToObject<LinkedAoInformationCore>())
				.ToDictionary(y => y?.AccountingObjectId, y => y?.ClassDescriptionKey);

			Assert.That(markInfo, Is.EquivalentTo(expectedMark));
			Assert.That(linkInfo, Is.EquivalentTo(expectedLink));
			
			Assert.IsTrue( await CheckEmpty(testEntity.Id));
		}
		
		[TestCaseSource(typeof(BaseFtsSqProcessingDataProviders),
			nameof(BaseFtsSqProcessingDataProviders.C1109302ProcessingSqActionMarkLinkNotifyFts3Result))]
		public async Task CommonPositiveTestFtsThree(
			Guid aggregationEntityFilterGuid,
			TestCreateProcessingSearchQueryParams searchQueryToCreate,
			int count,
			Dictionary<Guid, string> expectedMark,
			Dictionary<Guid, string> expectedLink)
		{
			AddUserToNotificationList(searchQueryToCreate);

			var storageKey = await GetIndexKey().ConfigureAwait(false);
			
			var createdAsq = (await Loader.ContainerService.CreateProcessingSearchQueryWaitAsync(
					searchQueryToCreate,
					_additionalParameters,
					CancellationToken.None)
				.AssertSuccess()
				.ConfigureAwait(false)).Result;

			var entities = new[]
			{
				DefaultData.CommonTestEntityWithModelAndRecordDateStorage(DateTimeOffset.UtcNow, aggregationEntityFilterGuid,
					storageKey),
				DefaultData.CommonTestEntityWithModelAndRecordDateStorage(DateTimeOffset.UtcNow,aggregationEntityFilterGuid,
					storageKey),
				DefaultData.CommonTestEntityWithModelAndRecordDateStorage(DateTimeOffset.UtcNow,aggregationEntityFilterGuid,
					storageKey),
			};

			foreach (var testEntity in entities)
			{
				await Loader.ContainerService
					.SaveEntityAsync(
						testEntity,
						_additionalParameters)
					.AssertSuccess()
					.ConfigureAwait(false);
				AggregationId.Add(testEntity.Id);
			}
			
			var notice1 = (await Loader.ContainerService
				.GetNotificationsCountAsync(_additionalParameters.SessionId,
					new TestNotificationFilter().SetExternalKeys(new List<string>()
						{createdAsq.ToString()})).AssertSuccess().ConfigureAwait(false)).Result;

			foreach (var testEntity in entities)
			{
			
				Assert.IsTrue(await Waiter.WaitAsync(async () =>
						await WatFoThreeActions(notice1, count, testEntity.Id, createdAsq),
					TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(Loader.WaitTime)), $"Notify or mark or link is absent");
				var result = (await Loader.ContainerService
					.GetEntityAsync(testEntity.Id, _additionalParameters)
					.AssertSuccess()
					.ConfigureAwait(false)).Result;

				var markInfo = result.AdditionalInformation
					.FirstOrDefault(x => x.TypeId == LabelInformationCoreKeys.TypeId)
					?.Informations?.Select(z => z.ToObject<LabelInformationCore>())
					.ToDictionary(y => y?.LabelTypeAccountingObjectId, y => y?.Comment);

				var linkInfo = result.AdditionalInformation
					.FirstOrDefault(x => x.TypeId == LinkedAoInformationCoreKeys.TypeId)
					?.Informations?.Select(z => z.ToObject<LinkedAoInformationCore>())
					.ToDictionary(y => y?.AccountingObjectId, y => y?.ClassDescriptionKey);

				Assert.That(markInfo, Is.EquivalentTo(expectedMark));
				Assert.That(linkInfo, Is.EquivalentTo(expectedLink));
			}
		}
		
		[TestCaseSource(typeof(BaseFtsSqProcessingDataProviders),
			nameof(BaseFtsSqProcessingDataProviders.C1109318ProcessingSqActionMarkLinkNotifyFts2Result1Other))]
		public async Task CommonPositiveTestFtsThree1Other(
			Guid aggregationEntityFilterGuid,
			TestCreateProcessingSearchQueryParams searchQueryToCreate,
			int count,
			Dictionary<Guid, string> expectedMark,
			Dictionary<Guid, string> expectedLink)
		{
			AddUserToNotificationList(searchQueryToCreate);

			var storageKey = await GetIndexKey();
			var otherStorageKey = await GetOtherIndexKey();
			
			var createdAsq = (await Loader.ContainerService.CreateProcessingSearchQueryWaitAsync(
					searchQueryToCreate,
					_additionalParameters,
					CancellationToken.None)
				.AssertSuccess()
				.ConfigureAwait(false)).Result;

			var entities = new[]
			{
				DefaultData.CommonTestEntityWithModelAndRecordDateStorage(DateTimeOffset.UtcNow, aggregationEntityFilterGuid,
					storageKey),
				DefaultData.CommonTestEntityWithModelAndRecordDateStorage(DateTimeOffset.UtcNow,aggregationEntityFilterGuid,
					storageKey),
			};

			var entity = DefaultData.CommonTestEntityWithModelAndRecordDateStorage(DateTimeOffset.UtcNow, aggregationEntityFilterGuid,
				otherStorageKey);
			foreach (var testEntity in entities)
			{
				await Loader.ContainerService
					.SaveEntityAsync(
						testEntity,
						_additionalParameters)
					.AssertSuccess()
					.ConfigureAwait(false);
				AggregationId.Add(testEntity.Id);
			}
			await Loader.ContainerService
				.SaveEntityAsync(
					entity,
					_additionalParameters)
				.AssertSuccess()
				.ConfigureAwait(false);
			AggregationId.Add(entity.Id);
			
			var notice1 = (await Loader.ContainerService
				.GetNotificationsCountAsync(_additionalParameters.SessionId,
					new TestNotificationFilter().SetExternalKeys(new List<string>()
						{createdAsq.ToString()})).AssertSuccess().ConfigureAwait(false)).Result;

			foreach (var testEntity in entities)
			{
			
				Assert.IsTrue(await Waiter.WaitAsync(async () =>
						await WatFoThreeActions(notice1, count, testEntity.Id, createdAsq),
					TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(Loader.WaitTime)), $"Notify or mark or link is absent");
				var result = (await Loader.ContainerService
					.GetEntityAsync(testEntity.Id, _additionalParameters)
					.AssertSuccess()
					.ConfigureAwait(false)).Result;

				var markInfo = result.AdditionalInformation
					.FirstOrDefault(x => x.TypeId == LabelInformationCoreKeys.TypeId)
					?.Informations?.Select(z => z.ToObject<LabelInformationCore>())
					.ToDictionary(y => y?.LabelTypeAccountingObjectId, y => y?.Comment);

				var linkInfo = result.AdditionalInformation
					.FirstOrDefault(x => x.TypeId == LinkedAoInformationCoreKeys.TypeId)
					?.Informations?.Select(z => z.ToObject<LinkedAoInformationCore>())
					.ToDictionary(y => y?.AccountingObjectId, y => y?.ClassDescriptionKey);

				Assert.That(markInfo, Is.EquivalentTo(expectedMark));
				Assert.That(linkInfo, Is.EquivalentTo(expectedLink));
			}
			Assert.IsTrue(await CheckEmpty(entity.Id),"Other empty is also changed");
		}

		[TestCaseSource(typeof(BaseFtsSqProcessingDataProviders),
			nameof(BaseFtsSqProcessingDataProviders.C1109339Processing2SqActionMarkLinkNotifyFts2Result))]
		public async Task CommonPositiveTestFtsThree2Sq(
			Guid aggregationEntityFilterGuid,
			TestCreateProcessingSearchQueryParams searchQueryToCreate,
			TestCreateProcessingSearchQueryParams searchQueryToCreate2,
			int count,
			int count2,
			Dictionary<Guid, string> expectedMark,
			Dictionary<Guid, string> expectedLink,
			Dictionary<Guid, string> expectedMark2,
			Dictionary<Guid, string> expectedLink2)
		{
			AddUserToNotificationList(searchQueryToCreate);
			AddUserToNotificationList(searchQueryToCreate2);

			var storageKey = await GetIndexKey();
			var otherStorageKey = await GetOtherIndexKey();

			var createdAsq = (await Loader.ContainerService.CreateProcessingSearchQueryWaitAsync(
					searchQueryToCreate,
					_additionalParameters,
					CancellationToken.None)
				.AssertSuccess()
				.ConfigureAwait(false)).Result;
			var createdAsq2 = (await Loader.ContainerService.CreateProcessingSearchQueryWaitAsync(
					searchQueryToCreate2,
					_additionalParameters,
					CancellationToken.None)
				.AssertSuccess()
				.ConfigureAwait(false)).Result;

			var entity = DefaultData.CommonTestEntityWithModelAndRecordDateStorage(DateTimeOffset.UtcNow,
				aggregationEntityFilterGuid,
				storageKey);
			var entity2 = DefaultData.CommonTestEntityWithModelAndRecordDateStorage(DateTimeOffset.UtcNow,
				aggregationEntityFilterGuid,
				otherStorageKey);
			foreach (var testEntity in new[] {entity, entity2})
			{
				await Loader.ContainerService
					.SaveEntityAsync(
						testEntity,
						_additionalParameters)
					.AssertSuccess()
					.ConfigureAwait(false);
				AggregationId.Add(testEntity.Id);
			}

			var notice1 = (await Loader.ContainerService
				.GetNotificationsCountAsync(_additionalParameters.SessionId,
					new TestNotificationFilter().SetExternalKeys(new List<string>()
						{createdAsq.ToString()})).AssertSuccess().ConfigureAwait(false)).Result;
			Assert.IsTrue(await Waiter.WaitAsync(async () =>
					await WatFoThreeActions(notice1, count, entity.Id, createdAsq),
				TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(Loader.WaitTime)), $"Notify or mark or link is absent");

			var notice2 = (await Loader.ContainerService
				.GetNotificationsCountAsync(_additionalParameters.SessionId,
					new TestNotificationFilter().SetExternalKeys(new List<string>()
						{createdAsq2.ToString()})).AssertSuccess().ConfigureAwait(false)).Result;

			Assert.AreEqual(count2, notice2);

			var result = (await Loader.ContainerService
				.GetEntityAsync(entity.Id, _additionalParameters)
				.AssertSuccess()
				.ConfigureAwait(false)).Result;

			var markInfo = result.AdditionalInformation
				.FirstOrDefault(x => x.TypeId == LabelInformationCoreKeys.TypeId)
				?.Informations?.Select(z => z.ToObject<LabelInformationCore>())
				.ToDictionary(y => y?.LabelTypeAccountingObjectId, y => y?.Comment);

			var linkInfo = result.AdditionalInformation
				.FirstOrDefault(x => x.TypeId == LinkedAoInformationCoreKeys.TypeId)
				?.Informations?.Select(z => z.ToObject<LinkedAoInformationCore>())
				.ToDictionary(y => y?.AccountingObjectId, y => y?.ClassDescriptionKey);

			Assert.That(markInfo, Is.EquivalentTo(expectedMark));
			Assert.That(linkInfo, Is.EquivalentTo(expectedLink));

			var result2 = (await Loader.ContainerService
				.GetEntityAsync(entity2.Id, _additionalParameters)
				.AssertSuccess()
				.ConfigureAwait(false)).Result;

			var markInfo2 = result2.AdditionalInformation
				.FirstOrDefault(x => x.TypeId == LabelInformationCoreKeys.TypeId)
				?.Informations?.Select(z => z.ToObject<LabelInformationCore>())
				.ToDictionary(y => y?.LabelTypeAccountingObjectId, y => y?.Comment);

			var linkInfo2 = result2.AdditionalInformation
				.FirstOrDefault(x => x.TypeId == LinkedAoInformationCoreKeys.TypeId)
				?.Informations?.Select(z => z.ToObject<LinkedAoInformationCore>())
				.ToDictionary(y => y?.AccountingObjectId, y => y?.ClassDescriptionKey);

			Assert.That(markInfo2, Is.EquivalentTo(expectedMark2));
			Assert.That(linkInfo2, Is.EquivalentTo(expectedLink2));

		}

		#endregion
    }
}
