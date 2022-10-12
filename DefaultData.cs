using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccountingObjects.Core.Patches;
using Aggregation.Data.Base;
using Aggregation.IntegrationTests.Const.Schemes;
using Aggregation.IntegrationTests.Tools.Helpers;
using Aggregation.IntegrationTests.Tools.Models;
using Aggregation.IntegrationTests.Tools.Models.Builders;
using Aggregation.System.Ident;
using Aggregation.System.Transcriber;
using Newtonsoft.Json.Linq;
using SearchQueries.IntegrationTests.Tools.Models;
using SearchQueries.Registry.Facade.Models.Filters.Default;
using SearchQueries.Registry.Facade.Models.ReferenceModels.Default;
using TestToolsCommon.Helpers;
using Aggregation.IntegrationTests.Tools.Models.Filters;
using Aggregation.System.External;
using Aggregation.System.External.Call;
using Aggregation.System.External.File;
using Aggregation.System.External.Phone;
using Aggregation.System.FullTextIndexing;
using Analysis.IntegrationTests.Const.DefaultData;
using Analysis.IntegrationTests.Extensions.Extensions;
using Analysis.IntegrationTests.Extensions.Helpers.Builders;
using Analysis.IntegrationTests.Extensions.Models;
using Analysis.IntegrationTests.Extensions.Models.Filter;
using Analysis.IntegrationTests.Extensions.Models.VoiceInfoPatch;
using CoreClassDescriptionConst.ObjectFieldKeys;
using DevTools.Core.ContainerService;
using SearchQueries.Common.Core;
using TestToolsCommon.Models;
using TestAction = SearchQueries.IntegrationTests.Tools.Models.TestAction;
using TestFilter = Aggregation.IntegrationTests.Tools.Models.Filters.TestFilter;
using TestFilterSq = SearchQueries.IntegrationTests.Tools.Models.TestFilter;

namespace SearchQueries.Processing.IntegrationTests.Tests.TestDefaultData
{
	public static class DefaultData
	{
		public static TestFilter CreateFilter(Guid aggregationEntityTypeGuid, Guid aggregationEntityFilterGuid)
		{
			var testFieldFilter = new TestFieldFilter()
				.SetMetaDataType(aggregationEntityTypeGuid.ToString())
				.SetTargetValue(aggregationEntityFilterGuid.ToString())
				.SetMain(true)
				.SetPredicate(Predicate.Eq)
				.SetFieldPath("filtertofilterfilters");

			var filter = new AggregationFilterBuilder()
				.SetOperator(Operator.AND)
				.AddFieldFilter(testFieldFilter)
				.Build();
			return filter;
		}
		public static TestFilter CreateFilter(Guid aggregationEntityFilterGuid)
		{
			var testFieldFilter = new TestFieldFilter()
				.SetMetaDataType(AggregatedPacketMainInformationCoreKeys.TypeId.ToString())
				.SetTargetValue(aggregationEntityFilterGuid.ToString())
				.SetMain(true)
				.SetPredicate(Predicate.Eq)
				.SetFieldPath("Id");

			var filter = new AggregationFilterBuilder()
				.SetOperator(Operator.AND)
				.AddFieldFilter(testFieldFilter)
				.Build();
			return filter;
		}
		public static TestFieldFilter CreateMetaFilter(Guid aggregationEntityFilterGuid)
		{
			var testFieldFilter = new TestFieldFilter()
				.SetMetaDataType(AggregatedPacketMainInformationCoreKeys.TypeId.ToString())
				.SetTargetValue(aggregationEntityFilterGuid.ToString())
				.SetMain(true)
				.SetPredicate(Predicate.Eq)
				.SetFieldPath("Id");
			return testFieldFilter;
		}
		public static TestCreateProcessingSearchQueryParams CreateQuery(TestAction[] actions, TestFilterSq[] filters)
		{
			var searchQueryToCreate = new TestCreateProcessingSearchQueryParams()
			{
				Name = TestStringsHelper.RandomLatinString(),
				Mandate = 1,
				Actions = actions,
				FilterCombination = new TestFilterCombination()
				{
					Filters = filters
				}

			};
			return searchQueryToCreate;
		}

		public static TestMarkAction CreateMarkAction(Dictionary<Guid, string> labels)
		{
			return new TestMarkAction
			{
				Labels = labels
			};
		}

		public static TestNotifyAction CreateNotifyAction(string description)
		{
			return new TestNotifyAction()
			{
				To = new long[] { },
				Description = description
			};
		}
		
		public static TestSetAccessAction CreateSetAccess(int? mandate = null, IReadOnlyCollection<Guid> accessGroups = null)
		{
			return new TestSetAccessAction()
			{
				Mandate = mandate,
				AccessGroups = accessGroups
			};
		}
		
		public static TestLinkAction CreateLinkAction(Dictionary<Guid, string> infoCardIds)
		{
			return new TestLinkAction()
			{
				InfoCardIds = infoCardIds,
			};
		}
		
		public static string CompareForMarkAndLink(Guid? markId, string markComment, Guid? linkId, string linkComment,
			Guid? actualMarkId, string actualMarkComment, Guid? actualLinkId, string actualLinkComment)
		{
			var compareResult = new StringBuilder();
			
			
			compareResult.Append(TestDataComparer.CompareFields($"{nameof(markId)}", markId,actualMarkId) ?? string.Empty);
			compareResult.Append(TestDataComparer.CompareFields($"{nameof(markComment)}", markComment,actualMarkComment) ?? string.Empty);
			compareResult.Append(TestDataComparer.CompareFields($"{nameof(linkId)}", linkId,actualLinkId) ?? string.Empty);
			compareResult.Append(TestDataComparer.CompareFields($"{nameof(linkComment)}", linkComment,actualLinkComment) ?? string.Empty);

			return compareResult.ToString().Contains("field") ? compareResult.ToString() : null;
		}
		
		public static TestVoiceIdentFilter CommonVoiceFilter(Guid entityId, string voicePath, int threshold = 90)
			=> new TestVoiceIdentFilter()
			{
				FilterTemplateId =  null,
				Threshold = threshold,
				ReferenceModels = new[]
				{
					new AggregationVoiceReferenceModel(voicePath, entityId),
				},
				GroupIds = new Guid[] { },
				AcceptedEmptyRecordStartDate = true,
				RecordStartDateFilter =
					new DateTimeOffsetRange()
					{
						From = DateTimeOffset.Now,
						To = DateTimeOffset.Now
					}
			};
		public static TestVoiceIdentFilter CommonVoiceFilterAcc( string voicePath)
			=> new TestVoiceIdentFilter()
			{
				FilterTemplateId =  null,
				Threshold = 1,
				ReferenceModels = new[]
				{
					
					new AccountingVoiceReferenceModel(voicePath, Guid.NewGuid(), Guid.NewGuid(), new List<Guid>()),
				},
				GroupIds = new Guid[] { },
				AcceptedEmptyRecordStartDate = true,
				RecordStartDateFilter =
					new DateTimeOffsetRange()
					{
						From = DateTimeOffset.Now,
						To = DateTimeOffset.Now
					}
			};
		public static TestVoiceIdentFilter CommonVoiceFilterAccNull( string voicePath)
			=> new TestVoiceIdentFilter()
			{
				FilterTemplateId =  null,
				Threshold = 1,
				ReferenceModels = new[]
				{
					
					new AccountingVoiceReferenceModel(voicePath, Guid.NewGuid(), Guid.NewGuid(), new List<Guid>()),
					new AccountingVoiceReferenceModel(null, Guid.NewGuid(), Guid.NewGuid(), new List<Guid>()),
				},
				GroupIds = new Guid[] { },
				AcceptedEmptyRecordStartDate = true,
				RecordStartDateFilter =
					new DateTimeOffsetRange()
					{
						From = DateTimeOffset.Now,
						To = DateTimeOffset.Now
					}
			};

		public static TestAggregationFilter CommonAggregationFilter(Guid aggregationEntityTypeGuid, Guid aggregationEntityFilterGuid)
			=> new TestAggregationFilter()
			{
				FilterTemplateId = null,
				Content = CreateFilter(aggregationEntityTypeGuid, aggregationEntityFilterGuid).ToJsonString()
			};

		public static TestAggregationFilter CommonAggregationFilter(Guid toFind)
			=> new TestAggregationFilter()
			{
				FilterTemplateId = null,
				Content = CreateFilter(toFind).ToJsonString()
			};

		public static TestAccountingFilter CommonAccountingFilter(Guid toFind)
			=> new TestAccountingFilter()
			{
				FilterTemplateId = null,
				Content = CreateFilter(toFind).ToJsonString()
			};

		public static TestAggregationFilter CommonAggregationFilter(TestFilter filter)
			=> new TestAggregationFilter()
			{
				FilterTemplateId = null,
				Content = filter.ToJsonString()
			};

		public static TestAccountingFilter CommonAccountingFilter(TestFilterDataData filter)
			=> new TestAccountingFilter()
			{
				FilterTemplateId = null,
				Content = filter.ToJsonString()
			};

		

        
		private static int _modelNum = -1;
		public static string GetModelKey()
		{
			_modelNum++;
			return $"file:/storage/gallery/{_modelNum}.vm";
		}
		
		public static async Task<(Guid ObjectId, Guid InstanceId, string VmKey, Guid? attachmentId)> PrepareCard(
			this IContainerService containerService,
			AdditionalParameters additionalParameters,
			bool hasVoiceModel = true)
		{
			TestVoiceInfoPatch voicePatch = null;

			var attachmentId = Guid.NewGuid();
			if (hasVoiceModel)
			{
				var vmKey = GetModelKey();
				voicePatch = VoicePatchBuilder.GenerateDummySingleVoicePatch(attachmentId, modelKey:vmKey);
				voicePatch.AudioFilesPatches = null;
			}

			var toCreate = new TestApplyPatchData
			{
				PatchType = PatchType.Create,
				FieldsPatches = new List<TestFieldPatchData>
				{
					new TestFieldPatchData(FieldPatchType.Set,
						Core_CardClassDescriptionFieldKeys.Name,
						TestStringsHelper.RandomLatinString())
				},
				ClassDescriptionKey = Core_CardClassDescriptionFieldKeys.CdKey,
				VoiceInfoPatch = voicePatch
			};

			if (hasVoiceModel)
				toCreate.AddEmbeddedPatch(DefaultApplyPatches.GetDefaultEmbeddedPatchForVoiceLookupCard(
					attachmentId));

			var accObj = (await containerService.ApplyPatchAsync(
					additionalParameters,
					toCreate)
				.AssertSuccess()
				.ConfigureAwait(false)).Result;

			return (accObj.ObjectId, accObj.InstanceId, voicePatch?.VoiceBioModelPatch?.BioModel?.VoiceModelKey,
				(voicePatch == null)? null : attachmentId);
		}
		/*public static TestAggregationFilter CommonAggregationFilter(Guid toFind)
			=> new TestAggregationFilter()
			{
				FilterTemplateId = null,
				Content = PhoneFilter(toFind.ToString()).ToJsonString()
			};*/

		public static TestKeyWordsFilter CommonFtsFilter(string[] keyWords)
			=> new TestKeyWordsFilter()
			{
				FilterTemplateId = null,
				Themes = ImmutableDictionary<TestThemeKey, TestTheme>.Empty,
				KeyWords = keyWords,
			};
		public static TestKeyWordsFilter CommonFtsFilter(string[] keyWords, Dictionary<TestThemeKey, TestTheme> themes)
			=> new TestKeyWordsFilter()
			{
				FilterTemplateId = null,
				Themes = themes,
				KeyWords = keyWords,
			};
		
		public static TestTypedEntity CommonTestEntityWithModelAndRecordDate(DateTimeOffset date, Guid filterId,
			string voiceModelKey = "12.vm", string storageKey = null)
		{
			var phoneId = Guid.NewGuid();
			return new TestTypedEntity()
				.SetTypeId(AggregatedPacketMainInformationCoreKeys.TypeId)
				.SetMetaInformation(filterId, new JObject())
				.SetAdditionalInformation(new[]
				{
					new TestInformationHolder()
						.SetTypeSchemeId(FileInformationCoreKeys.TypeId)
						.SetInformation(
							new[]
							{
								new JObject(new JProperty("FileName", "some_file.wav"),
									new JProperty("StorageKey", storageKey ?? "someKey"), new JProperty("FileLength", 123456))
							}),
					GetCommonVoiceModelInfo(voiceModelKey),
					GetCommonCallInformation(phoneId),
					GetCommonPhoneInformation(date, phoneId),
					GetCommonIdentInformation(date, phoneId),
					GetCommonTranscriberInfoInfo(storageKey),
					GetCommonIndexingInformation(storageKey),
				});
		}
		
		public static TestTypedEntity CommonTestEntityWithModelAndRecordDateStorage(DateTimeOffset date, Guid filterId, string storageKey)
		{
			var phoneId = Guid.NewGuid();
			return new TestTypedEntity()
				.SetTypeId(AggregatedPacketMainInformationCoreKeys.TypeId)
				.SetMetaInformation(filterId, new JObject())
				.SetAdditionalInformation(new[]
				{
					new TestInformationHolder()
						.SetTypeSchemeId(FileInformationCoreKeys.TypeId)
						.SetInformation(
							new[]
							{
								new JObject(new JProperty("FileName", "some_file.wav"),
									new JProperty("StorageKey", storageKey ?? "someKey"), new JProperty("FileLength", 123456))
							}),
					GetCommonVoiceModelInfo("12.vm"),
					GetCommonCallInformation(phoneId),
					GetCommonPhoneInformation(date, phoneId),
					GetCommonIdentInformation(date, phoneId),
					GetCommonTranscriberInfoInfo(storageKey),
					GetCommonIndexingInformation(storageKey),
				});
		}

		public static TestInformationHolder GetCommonCallInformation(Guid phoneId)
			{
				return new TestInformationHolder()
					.SetTypeSchemeId(CallInformationCoreKeys.TypeId)
					.SetInformation(new[]
					{
						new JObject(
							new JProperty("Duration", 1234567),
							new JProperty("PhoneInformationId", phoneId))
					});
			}
			public static TestInformationHolder GetCommonPhoneInformation(DateTimeOffset date, Guid id, string imsi = "Imsi")
			{
				return new TestInformationHolder()
					.SetTypeSchemeId(PhoneInformationCoreKeys.TypeId)
					.SetInformation(id.ToString(), new[]
					{
						new JObject(
							new JProperty("DateTime", date.ToUnixTimeMilliseconds()),
							new JProperty("InputPhone", "1234567"),
							new JProperty("OutputPhone", "1234567"),
							new JProperty("Direction", "Outgoing"))
					});
			}
			public static TestInformationHolder GetCommonIdentInformation(DateTimeOffset date, Guid id)
			{
				return new TestInformationHolder()
					.SetTypeSchemeId(IdentTypes.IdentInformationTypeId)
					.SetInformation(new[]
					{
						new JObject(
							new JProperty("InProgress", false),
							new JProperty("InProgressOpGuid", id))
					});
			}
			public static TestInformationHolder GetCommonIndexingInformation(string docId)
			{
				return new TestInformationHolder
				{
					Id = Guid.NewGuid(),
					TypeId = IndexingTypes.IndexingMetadataId,
					Informations = new[]
					{
						JObject.FromObject(new IndexingMetadata
						{
							ErrorData = null,
							Id = Guid.NewGuid(),
							StorageKey = docId,
							Success = true
						})
					}
				};
			}
			
			public static TestInformationHolder GetCommonVoiceModelInfo(string voiceModelKey)
		{
			return new TestInformationHolder()
				.SetTypeSchemeId(IdentTypes.VoiceModelTypeId)
				.SetInformation(
					new[]
					{
						new JObject(
							new JProperty("VoiceModel",
								new JObject(
									new JProperty("VoiceModelPath", voiceModelKey),
									new JProperty("Gender",
										new JObject(
											new JProperty("AdmissionFalseProbability", 0.0),
											new JProperty("NotAdmissionFalseProbability", 95.27704),
											new JProperty("Probability", 99.758125))),
									new JProperty("Quality", 0.9),
									new JProperty("BioSdkVersion", "3.5.110"),
									new JProperty("VoiceModelState", "Normal"),
									new JProperty("Segmentation",
										new JArray(
											new JObject(
												new JProperty("Snr", 21.174),
												new JProperty("SegmentationDataPath",
													"/seg/e2/b9/42/e25fe337c20cd08e.seg"),
												new JProperty("AudioSourcePath",
													"h_service/00002D?length=1862144"),
												new JProperty("RangesDuration", 113.09),
												new JProperty("MultilevelRanges",
													new JObject(
														new JProperty("VadRanges", 2.4481, 2.667),
														new JProperty("BeepRanges", 2.4481, 2.667),
														new JProperty("MusicRanges", 2.4481, 2.667),
														new JProperty("GlitchRanges", 2.4481, 2.667),
														new JProperty("ClippingRanges", 2.4481, 2.667),
														new JProperty("OverloadRanges"),
														new JProperty("TonalNoiseRanges"))),
												new JProperty("RtRms", 0.0110026579350233),
												new JProperty("Quality", 0.5),
												new JProperty("RtAver", 0.31276860833168),
												new JProperty("RtCount", 265.0),
												new JProperty("Algorithm", "Polylog"),
												new JProperty("Languages",
													new JObject(
														new JProperty("Language", "Arabic"),
														new JProperty("Probability", 0.2977),
														new JProperty("AdmissionFalseProbability", 99.26015),
														new JProperty("NotAdmissionFalseProbability", 0.00109)),
													new JObject(
														new JProperty("Language", "Chinese"),
														new JProperty("Probability", 0.2977),
														new JProperty("AdmissionFalseProbability", 99.26015),
														new JProperty("NotAdmissionFalseProbability", 0.00109)),
													new JObject(
														new JProperty("Language", "English"),
														new JProperty("Probability", 0.2977),
														new JProperty("AdmissionFalseProbability", 99.26015),
														new JProperty("NotAdmissionFalseProbability", 0.00109)),
													new JObject(
														new JProperty("Language", "German"),
														new JProperty("Probability", 0.2977),
														new JProperty("AdmissionFalseProbability", 99.26015),
														new JProperty("NotAdmissionFalseProbability", 0.00109)),
													new JObject(
														new JProperty("Language", "Japanese"),
														new JProperty("Probability", 0.2977),
														new JProperty("AdmissionFalseProbability", 99.26015),
														new JProperty("NotAdmissionFalseProbability", 0.00109)),
													new JObject(
														new JProperty("Language", "Korean"),
														new JProperty("Probability", 0.2977),
														new JProperty("AdmissionFalseProbability", 99.26015),
														new JProperty("NotAdmissionFalseProbability", 0.00109)),
													new JObject(
														new JProperty("Language", "Russian"),
														new JProperty("Probability", 12.2977),
														new JProperty("AdmissionFalseProbability", 99.26015),
														new JProperty("NotAdmissionFalseProbability", 0.00109)),
													new JObject(
														new JProperty("Language", "Czech"),
														new JProperty("Probability", 92.2977),
														new JProperty("AdmissionFalseProbability", 99.26015),
														new JProperty("NotAdmissionFalseProbability", 0.00109)),
													new JObject(
														new JProperty("Language", "Farsi"),
														new JProperty("Probability", 0.2977),
														new JProperty("AdmissionFalseProbability", 99.26015),
														new JProperty("NotAdmissionFalseProbability", 0.00109)),
													new JObject(
														new JProperty("Language", "French"),
														new JProperty("Probability", 0.2977),
														new JProperty("AdmissionFalseProbability", 99.26015),
														new JProperty("NotAdmissionFalseProbability", 0.00109)),
													new JObject(
														new JProperty("Language", "Hindustani"),
														new JProperty("Probability", 0.2977),
														new JProperty("AdmissionFalseProbability", 99.26015),
														new JProperty("NotAdmissionFalseProbability", 0.00109)),
													new JObject(
														new JProperty("Language", "Polish"),
														new JProperty("Probability", 0.2977),
														new JProperty("AdmissionFalseProbability", 99.26015),
														new JProperty("NotAdmissionFalseProbability", 0.00109)),
													new JObject(
														new JProperty("Language", "Spanish"),
														new JProperty("Probability", 0.2977),
														new JProperty("AdmissionFalseProbability", 99.26015),
														new JProperty("NotAdmissionFalseProbability", 0.00109)),
													new JObject(
														new JProperty("Language", "Tamil"),
														new JProperty("Probability", 0.2977),
														new JProperty("AdmissionFalseProbability", 99.26015),
														new JProperty("NotAdmissionFalseProbability", 0.00109)),
													new JObject(
														new JProperty("Language", "Vietnamese"),
														new JProperty("Probability", 0.2977),
														new JProperty("AdmissionFalseProbability", 99.26015),
														new JProperty("NotAdmissionFalseProbability", 0.00109)),
													new JObject(
														new JProperty("Language", "Unknown"),
														new JProperty("Probability", 0.2977),
														new JProperty("AdmissionFalseProbability", 99.26015),
														new JProperty("NotAdmissionFalseProbability", 0.00109))
												),
												new JProperty("SpeakerNumber", (long) 0),
												new JProperty("BioMetricsSdkVersion", "3.5.110"),
												new JProperty("MaxSpeakers", (long) 4),
												new JProperty("MinSpeakers", (long) 2),
												new JProperty("TimeSpeech", 47.57),
												new JProperty("ChannelType", "Phone"),
												new JProperty("TimeOverload", 0.0007),
												new JProperty("ChannelNumber", "Left"),
												new JProperty("TimeTonalNoise", 116.35),
												new JProperty("ProcessedDuration", 116.35),
												new JProperty("ProcessedPartially", false),
												new JProperty("LowestSpeechFreqHz", 78.125),
												new JProperty("HighestSpeechFreqHq", 2968.75),
												new JProperty("MaxUnclippedInterval", 80.3),
												new JProperty("SpeechAmplitudeThresholdDb", 20.8357),
												new JProperty("State", "Normal")))))),
							new JProperty("ErrorData", null))
					});
		}
			
		public static TestTypedEntity CommonTestEntity(Guid filterId,
			string voiceModelKey = "1.vm") =>
			new TestTypedEntity()
				.SetTypeId(SchemesGuids.MetaDataTypeIdForMarksFilter)
				.SetMetaInformation(EntitiesForFilterHelper.MetaDataCreator(filterId.ToString()))
				.SetAdditionalInformation(new[]
				{
					new TestInformationHolder()
						.SetTypeSchemeId(Guid.Parse("06014d8f-3ce8-4712-821a-681c6609bf26"))
						.SetInformation(
							new[]
							{
								new JObject(
									new JProperty("VoiceModel",
										new JObject(
											new JProperty("VoiceModelPath", voiceModelKey),
											new JProperty("Gender",
												new JObject(
													new JProperty("AdmissionFalseProbability", 0.0),
													new JProperty("NotAdmissionFalseProbability", 95.27704),
													new JProperty("Probability", 99.758125))),
											new JProperty("Quality", 0.9),
											new JProperty("BioSdkVersion", "3.5.110"),
											new JProperty("VoiceModelState", "Normal"),
											new JProperty("Segmentation",
												new JArray(
													new JObject(
														new JProperty("Snr", 21.174),
														new JProperty("SegmentationDataPath",
															"/seg/e2/b9/42/e25fe337c20cd08e.seg"),
														new JProperty("AudioSourcePath",
															"h_service/00002D?length=1862144"),
														new JProperty("RangesDuration", 113.09),
														new JProperty("MultilevelRanges",
															new JObject(
																new JProperty("VadRanges", 2.4481, 2.667),
																new JProperty("BeepRanges", 2.4481, 2.667),
																new JProperty("MusicRanges", 2.4481, 2.667),
																new JProperty("GlitchRanges", 2.4481, 2.667),
																new JProperty("ClippingRanges", 2.4481, 2.667),
																new JProperty("OverloadRanges"),
																new JProperty("TonalNoiseRanges"))),
														new JProperty("RtRms", 0.0110026579350233),
														new JProperty("Quality", 0.5),
														new JProperty("RtAver", 0.31276860833168),
														new JProperty("RtCount", 265.0),
														new JProperty("Algorithm", "Polylog"),
														new JProperty("Languages",
															new JObject(
																new JProperty("Language", "Arabic"),
																new JProperty("Probability", 0.2977),
																new JProperty("AdmissionFalseProbability", 99.26015),
																new JProperty("NotAdmissionFalseProbability", 0.00109)),
															new JObject(
																new JProperty("Language", "Chinese"),
																new JProperty("Probability", 0.2977),
																new JProperty("AdmissionFalseProbability", 99.26015),
																new JProperty("NotAdmissionFalseProbability", 0.00109)),
															new JObject(
																new JProperty("Language", "English"),
																new JProperty("Probability", 0.2977),
																new JProperty("AdmissionFalseProbability", 99.26015),
																new JProperty("NotAdmissionFalseProbability", 0.00109)),
															new JObject(
																new JProperty("Language", "German"),
																new JProperty("Probability", 0.2977),
																new JProperty("AdmissionFalseProbability", 99.26015),
																new JProperty("NotAdmissionFalseProbability", 0.00109)),
															new JObject(
																new JProperty("Language", "Japanese"),
																new JProperty("Probability", 0.2977),
																new JProperty("AdmissionFalseProbability", 99.26015),
																new JProperty("NotAdmissionFalseProbability", 0.00109)),
															new JObject(
																new JProperty("Language", "Korean"),
																new JProperty("Probability", 0.2977),
																new JProperty("AdmissionFalseProbability", 99.26015),
																new JProperty("NotAdmissionFalseProbability", 0.00109)),
															new JObject(
																new JProperty("Language", "Russian"),
																new JProperty("Probability", 12.2977),
																new JProperty("AdmissionFalseProbability", 99.26015),
																new JProperty("NotAdmissionFalseProbability", 0.00109)),
															new JObject(
																new JProperty("Language", "Czech"),
																new JProperty("Probability", 92.2977),
																new JProperty("AdmissionFalseProbability", 99.26015),
																new JProperty("NotAdmissionFalseProbability", 0.00109)),
															new JObject(
																new JProperty("Language", "Farsi"),
																new JProperty("Probability", 0.2977),
																new JProperty("AdmissionFalseProbability", 99.26015),
																new JProperty("NotAdmissionFalseProbability", 0.00109)),
															new JObject(
																new JProperty("Language", "French"),
																new JProperty("Probability", 0.2977),
																new JProperty("AdmissionFalseProbability", 99.26015),
																new JProperty("NotAdmissionFalseProbability", 0.00109)),
															new JObject(
																new JProperty("Language", "Hindustani"),
																new JProperty("Probability", 0.2977),
																new JProperty("AdmissionFalseProbability", 99.26015),
																new JProperty("NotAdmissionFalseProbability", 0.00109)),
															new JObject(
																new JProperty("Language", "Polish"),
																new JProperty("Probability", 0.2977),
																new JProperty("AdmissionFalseProbability", 99.26015),
																new JProperty("NotAdmissionFalseProbability", 0.00109)),
															new JObject(
																new JProperty("Language", "Spanish"),
																new JProperty("Probability", 0.2977),
																new JProperty("AdmissionFalseProbability", 99.26015),
																new JProperty("NotAdmissionFalseProbability", 0.00109)),
															new JObject(
																new JProperty("Language", "Tamil"),
																new JProperty("Probability", 0.2977),
																new JProperty("AdmissionFalseProbability", 99.26015),
																new JProperty("NotAdmissionFalseProbability", 0.00109)),
															new JObject(
																new JProperty("Language", "Vietnamese"),
																new JProperty("Probability", 0.2977),
																new JProperty("AdmissionFalseProbability", 99.26015),
																new JProperty("NotAdmissionFalseProbability", 0.00109)),
															new JObject(
																new JProperty("Language", "Unknown"),
																new JProperty("Probability", 0.2977),
																new JProperty("AdmissionFalseProbability", 99.26015),
																new JProperty("NotAdmissionFalseProbability", 0.00109))
														),
														new JProperty("SpeakerNumber", (long) 0),
														new JProperty("BioMetricsSdkVersion", "3.5.110"),
														new JProperty("MaxSpeakers", (long) 4),
														new JProperty("MinSpeakers", (long) 2),
														new JProperty("TimeSpeech", 47.57),
														new JProperty("ChannelType", "Phone"),
														new JProperty("TimeOverload", 0.0007),
														new JProperty("ChannelNumber", "Left"),
														new JProperty("TimeTonalNoise", 116.35),
														new JProperty("ProcessedDuration", 116.35),
														new JProperty("ProcessedPartially", false),
														new JProperty("LowestSpeechFreqHz", 78.125),
														new JProperty("HighestSpeechFreqHq", 2968.75),
														new JProperty("MaxUnclippedInterval", 80.3),
														new JProperty("SpeechAmplitudeThresholdDb", 20.8357),
														new JProperty("State", "Normal")))))),
									new JProperty("ErrorData", null))
							}),
				});

		public static TestTypedEntity CreateEntityWithTranscriberResultsStorageKey(Guid filterId, string storageKey) =>
			new TestTypedEntity()
				.SetMetaInformation(filterId, new JObject())
				.SetAdditionalInformation(
					new[]
					{
						new TestInformationHolder()
							.SetTypeSchemeId(FileInformationCoreKeys.TypeId)
							.SetInformation(
								new[]
								{
									new JObject(new JProperty("FileName", "some_file.wav"),
										new JProperty("StorageKey", storageKey ?? "someKey"), 
										new JProperty("FileLength", 123456))
								}),
						GetCommonTranscriberInfoInfo(storageKey),
						GetCommonVoiceModelInfo("12.vm"),
						GetCommonCallInformation(Guid.NewGuid()),
						GetCommonPhoneInformation(DateTimeOffset.Now, Guid.NewGuid()),
						GetCommonIdentInformation(DateTimeOffset.Now, Guid.NewGuid()),
						GetCommonIndexingInformation(storageKey),
					})
				.SetTypeId(AggregatedPacketMainInformationCoreKeys.TypeId);
		
		public static TestInformationHolder GetCommonTranscriberInfoInfo(string storageKey)
		{
			return new TestInformationHolder()
				.SetTypeSchemeId(TranscriberTypes.TranscriberInformationTypeId)
				.SetInformation(
					new[]
					{
						new JObject(
							new JProperty("ErrorData", null),
							new JProperty("PreprocessedResultsStorageKey", storageKey),
							new JProperty("TranscriberResultsStorageKey",
								"7362279D0012390700000000000026E4"),
							// new JProperty("BookmarksStorageKey", "DABD84330020935F0000000000005BC6"),
							new JProperty("InTranscribing", false),
							new JProperty("InTranscribingDate", null),
							new JProperty("InTranscribingOpGuid", null))
					});
		}
		
		public static TestTypedEntity EntityX() => 
			new TestTypedEntity(
					Aggregation.IntegrationTests.Tools.Models.TestAction.PredicateFilterMainTrue)
				.SetAdditionalInformation(
					EntityXHolders);

		public static TestTypedEntity EntityY() =>
			new TestTypedEntity(
					Aggregation.IntegrationTests.Tools.Models.TestAction.PredicateFilterMainTrue)
				.SetAdditionalInformation(
					EntityYHolders);

		public static TestTypedEntity EntityZ() =>
			new TestTypedEntity(Aggregation.IntegrationTests.Tools.Models.TestAction.PredicateFilterMainTrue)
				.SetAdditionalInformation(
					EntityZHolders);
		

        public static TestInformationHolder[] EntityXHolders =
       {
            new TestInformationHolder()
                .SetTypeSchemeId(SchemesGuids.FilterEntitySchemaGuidX)
                .SetInformation(
                    new[]
                    {
                        FilterPredicateJSONs.GetJObjectFieldIntEq(5),
                        FilterPredicateJSONs.GetJObjectFieldIntEq(3),
                    }),

            new TestInformationHolder()
                .SetTypeSchemeId(SchemesGuids.FilterEntitySchemaGuidY)
                .SetInformation(
                    new[]
                    {
                        FilterPredicateJSONs.GetJObjectFieldIntEq(8),
                        FilterPredicateJSONs.GetJObjectFieldIntEq(9),
                    }),
            new TestInformationHolder()
	            .SetTypeSchemeId(FileInformationCoreKeys.TypeId)
	            .SetInformation(
		            new[]
		            {
			            new JObject(new JProperty("FileName", "some_file.wav"),
				            new JProperty("StorageKey", "someKey"), new JProperty("FileLength", 123456))
		            }),
            GetCommonVoiceModelInfo("12.vm"),
            GetCommonCallInformation(Guid.NewGuid()),
            GetCommonPhoneInformation(DateTimeOffset.Now, Guid.NewGuid()),
            GetCommonIdentInformation(DateTimeOffset.Now, Guid.NewGuid()),
            GetCommonTranscriberInfoInfo(null),
            GetCommonIndexingInformation(null),
        };

        public static TestInformationHolder[] EntityYHolders =
        {
            new TestInformationHolder()
                .SetTypeSchemeId(SchemesGuids.FilterEntitySchemaGuidX)
                .SetInformation(
                    new[]
                    {
                        FilterPredicateJSONs.GetJObjectFieldIntEq(3),
                        FilterPredicateJSONs.GetJObjectFieldIntEq(5),
                    }),

            new TestInformationHolder()
                .SetTypeSchemeId(SchemesGuids.FilterEntitySchemaGuidY)
                .SetInformation(
                    new[]
                    {
                        FilterPredicateJSONs.GetJObjectFieldIntEq(8),
                        FilterPredicateJSONs.GetJObjectFieldIntEq(9),
                    }),
            new TestInformationHolder()
	            .SetTypeSchemeId(FileInformationCoreKeys.TypeId)
	            .SetInformation(
		            new[]
		            {
			            new JObject(new JProperty("FileName", "some_file.wav"),
				            new JProperty("StorageKey", "someKey"), new JProperty("FileLength", 123456))
		            }),
            GetCommonVoiceModelInfo("12.vm"),
            GetCommonCallInformation(Guid.NewGuid()),
            GetCommonPhoneInformation(DateTimeOffset.Now, Guid.NewGuid()),
            GetCommonIdentInformation(DateTimeOffset.Now, Guid.NewGuid()),
            GetCommonTranscriberInfoInfo(null),
            GetCommonIndexingInformation(null),
        };

        public static TestInformationHolder[] EntityZHolders =
        {
            new TestInformationHolder()
                .SetTypeSchemeId(SchemesGuids.FilterEntitySchemaGuidX)
                .SetInformation(
                    new[]
                    {
                        FilterPredicateJSONs.GetJObjectFieldIntEq(8),
                        FilterPredicateJSONs.GetJObjectFieldIntEq(3),
                    }),

            new TestInformationHolder()
                .SetTypeSchemeId(SchemesGuids.FilterEntitySchemaGuidY)
                .SetInformation(
                    new[]
                    {
                        FilterPredicateJSONs.GetJObjectFieldIntEq(5),
                        FilterPredicateJSONs.GetJObjectFieldIntEq(9),
                    }),
            new TestInformationHolder()
	            .SetTypeSchemeId(FileInformationCoreKeys.TypeId)
	            .SetInformation(
		            new[]
		            {
			            new JObject(new JProperty("FileName", "some_file.wav"),
				            new JProperty("StorageKey", "someKey"), new JProperty("FileLength", 123456))
		            }),
            GetCommonVoiceModelInfo("12.vm"),
            GetCommonCallInformation(Guid.NewGuid()),
            GetCommonPhoneInformation(DateTimeOffset.Now, Guid.NewGuid()),
            GetCommonIdentInformation(DateTimeOffset.Now, Guid.NewGuid()),
            GetCommonTranscriberInfoInfo(null),
        };
        
        public static void AddUserToNotificationList(TestCreateProcessingSearchQueryParams searchQueryParams, long userId = 0)
        {
	        searchQueryParams.Actions
		        .Where(a => a.ActionType == ActionType.Notify)
		        .Select(a => a as TestNotifyAction)
		        .ToList()
		        .ForEach(a => a.To = a.To.Append(userId).ToArray());
        }
	}
}
