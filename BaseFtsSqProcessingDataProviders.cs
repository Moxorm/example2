using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using NUnit.Framework;
using SearchQueries.IntegrationTests.Tools.Models;
using SearchQueries.Processing.IntegrationTests.Tests.TestDefaultData;
using TestCommonData;
using TestToolsCommon.Helpers;
using TestFilterSq = SearchQueries.IntegrationTests.Tools.Models.TestFilter;

namespace SearchQueries.Processing.IntegrationTests.Tests.Tests.Data
{
    public class BaseFtsSqProcessingDataProviders
    {
        private static readonly TestTheme ThemeWithMatch = new TestTheme()
        {
            Key = new TestThemeKey()
            {
                InstanceId = Guid.NewGuid(),
                ThemeId = Guid.NewGuid(),
                UpdatedAt = DateTimeOffset.UtcNow
            },
            KeyWords = new[] {"Раскольников", "Дуня"}
        };

        private static readonly TestTheme ThemeWithoutMatch = new TestTheme()
        {
            Key = new TestThemeKey()
            {
                InstanceId = Guid.NewGuid(),
                ThemeId = Guid.NewGuid(),
                UpdatedAt = DateTimeOffset.UtcNow
            },
            KeyWords = new[] {"adnsakl", "yruwerwhj"}
        };
        public static IEnumerable<TestCaseData> C443370ProcessingSqActionNotifyFts1Result
        {
            get
            {
                var aggregationEntityFilterGuid = Guid.NewGuid();
                var searchQuery = DefaultData.CreateQuery(new TestAction[]
                    {
                        new TestNotifyAction()
                        {
                            To = new long[] {1},
                            Description = TestStringsHelper.RandomLatinString()
                        },
                    },
                    new TestFilterSq[]
                    {
                        DefaultData.CommonFtsFilter(new []{"Раскольников"})
                    });
                
                yield return new TestCaseData(aggregationEntityFilterGuid, searchQuery, 1)
                    .SetName($"{nameof(C443370ProcessingSqActionNotifyFts1Result)}")
                    .SetCategory(nameof(TestCategories.PriorityHigh));
            }
        }
            
        public static IEnumerable<TestCaseData> C1113205ProcessingSqAction2NotifyFts1Result
        {
            get
            {
                var aggregationEntityFilterGuid = Guid.NewGuid();
                var searchQuery = DefaultData.CreateQuery(new TestAction[]
                    {
                        DefaultData.CreateNotifyAction("Раскольников"),
                        DefaultData.CreateNotifyAction("Раскольников")
                    },
                    new TestFilterSq[]
                    {
                        DefaultData.CommonFtsFilter(new []{"Раскольников"})
                    });
                
                yield return new TestCaseData(aggregationEntityFilterGuid, searchQuery, 2)
                    .SetName($"{nameof(C1113205ProcessingSqAction2NotifyFts1Result)}")
                    .SetCategory(nameof(TestCategories.PriorityMedium));
            }
        }
        
        public static IEnumerable<TestCaseData> C1117048ProcessingSqActionNotifyFts1ResultTheme
        {
            get
            {
                var aggregationEntityFilterGuid = Guid.NewGuid();
                var searchQuery = DefaultData.CreateQuery(new TestAction[]
                    {
                        new TestNotifyAction()
                        {
                            To = new long[] {1},
                            Description = TestStringsHelper.RandomLatinString()
                        },
                    },
                    new TestFilterSq[]
                    {
                        DefaultData.CommonFtsFilter(null,new Dictionary<TestThemeKey, TestTheme>()
                        {
                            {
                                ThemeWithMatch.Key,
                                ThemeWithMatch
                            },
                            {
                                ThemeWithoutMatch.Key,
                                ThemeWithoutMatch
                            },
                        })
                    });
                
                yield return new TestCaseData(aggregationEntityFilterGuid, searchQuery, 1)
                    .SetName($"{nameof(C1117048ProcessingSqActionNotifyFts1ResultTheme)}")
                    .SetCategory(nameof(TestCategories.PriorityMedium));
            }
        }
        
        public static IEnumerable<TestCaseData> C1117049ProcessingSqActionNotifyFts1ResultThemeAndWords
        {
            get
            {
                var aggregationEntityFilterGuid = Guid.NewGuid();
                var searchQuery = DefaultData.CreateQuery(new TestAction[]
                    {
                        new TestNotifyAction()
                        {
                            To = new long[] {1},
                            Description = TestStringsHelper.RandomLatinString()
                        },
                    },
                    new TestFilterSq[]
                    {
                        DefaultData.CommonFtsFilter(new[] {"раскольников"},new Dictionary<TestThemeKey, TestTheme>()
                        {
                            {
                                ThemeWithMatch.Key,
                                ThemeWithMatch
                            },
                            {
                                ThemeWithoutMatch.Key,
                                ThemeWithoutMatch
                            },
                        })
                    });
                
                yield return new TestCaseData(aggregationEntityFilterGuid, searchQuery, 1)
                    .SetName($"{nameof(C1117049ProcessingSqActionNotifyFts1ResultThemeAndWords)}")
                    .SetCategory(nameof(TestCategories.PriorityMedium));
            }
        }
        
        public static IEnumerable<TestCaseData> C1117051ProcessingSqActionNotifyFts1ResultThemeAndWordsWordDoNotMatch
        {
            get
            {
                var aggregationEntityFilterGuid = Guid.NewGuid();
                var searchQuery = DefaultData.CreateQuery(new TestAction[]
                    {
                        new TestNotifyAction()
                        {
                            To = new long[] {1},
                            Description = TestStringsHelper.RandomLatinString()
                        },
                    },
                    new TestFilterSq[]
                    {
                        DefaultData.CommonFtsFilter(new[] {"ads"},new Dictionary<TestThemeKey, TestTheme>()
                        {
                            {
                                ThemeWithMatch.Key,
                                ThemeWithMatch
                            },
                            {
                                ThemeWithoutMatch.Key,
                                ThemeWithoutMatch
                            },
                        })
                    });
                
                yield return new TestCaseData(aggregationEntityFilterGuid, searchQuery, 1)
                    .SetName($"{nameof(C1117051ProcessingSqActionNotifyFts1ResultThemeAndWordsWordDoNotMatch)}")
                    .SetCategory(nameof(TestCategories.PriorityMedium));
            }
        }
        
        public static IEnumerable<TestCaseData> C1117052ProcessingSqActionNotifyFts1ResultThemeAndWordsThemeDoNotMatch
        {
            get
            {
                var aggregationEntityFilterGuid = Guid.NewGuid();
                var searchQuery = DefaultData.CreateQuery(new TestAction[]
                    {
                        new TestNotifyAction()
                        {
                            To = new long[] {1},
                            Description = TestStringsHelper.RandomLatinString()
                        },
                    },
                    new TestFilterSq[]
                    {
                        DefaultData.CommonFtsFilter(new[] {"раскольников"},new Dictionary<TestThemeKey, TestTheme>()
                        {
                            {
                                ThemeWithoutMatch.Key,
                                ThemeWithoutMatch
                            },
                        })
                    });
                
                yield return new TestCaseData(aggregationEntityFilterGuid, searchQuery, 1)
                    .SetName($"{nameof(C1117052ProcessingSqActionNotifyFts1ResultThemeAndWordsThemeDoNotMatch)}")
                    .SetCategory(nameof(TestCategories.PriorityMedium));
            }
        }
        
        public static IEnumerable<TestCaseData> C1117053ProcessingSqActionNotifyFts1ResultThemeAndWordsDoNotMatch
        {
            get
            {
                var aggregationEntityFilterGuid = Guid.NewGuid();
                var searchQuery = DefaultData.CreateQuery(new TestAction[]
                    {
                        new TestNotifyAction()
                        {
                            To = new long[] {1},
                            Description = TestStringsHelper.RandomLatinString()
                        },
                    },
                    new TestFilterSq[]
                    {
                        DefaultData.CommonFtsFilter(new[] {"asddsaxzd"},new Dictionary<TestThemeKey, TestTheme>()
                        {
                            {
                                ThemeWithoutMatch.Key,
                                ThemeWithoutMatch
                            },
                        })
                    });
                
                yield return new TestCaseData(aggregationEntityFilterGuid, searchQuery, 0)
                    .SetName($"{nameof(C1117053ProcessingSqActionNotifyFts1ResultThemeAndWordsDoNotMatch)}")
                    .SetCategory(nameof(TestCategories.PriorityMedium));
            }
        }
        
        public static IEnumerable<TestCaseData> C443369ProcessingSqActionMarkFts1Result
        {
            get
            {
                var id = Guid.NewGuid();
                var comment = "eebbaa aa bee";
                var aggregationEntityFilterGuid = Guid.NewGuid();
                var searchQuery = DefaultData.CreateQuery(new TestAction[]
                    {
                        new TestMarkAction
                        {
                            Labels = new Dictionary<Guid, string>()
                            {
                                {id, comment},
                            }
                        },
                    },
                    new TestFilterSq[]
                    {
                        DefaultData.CommonFtsFilter(new[] {"Раскольников"})
                    });
                
                yield return new TestCaseData(aggregationEntityFilterGuid, searchQuery, id, comment)
                    .SetName($"{nameof(C443369ProcessingSqActionMarkFts1Result)}")
                    .SetCategory(nameof(TestCategories.PriorityHigh));
            }
        }
        
        public static IEnumerable<TestCaseData> C443371ProcessingSqActionLinkFts1Result
        {
            get
            {
                var id = Guid.NewGuid();
                var comment = "eebbaa aa bee";
                var aggregationEntityFilterGuid = Guid.NewGuid();
                var searchQuery = DefaultData.CreateQuery(new TestAction[]
                    {
                        new TestLinkAction()
                        {
                            InfoCardIds = new Dictionary<Guid, string>()
                            {
                                {id, comment},
                            }
                        },
                    },
                    new TestFilterSq[]
                    {
                        DefaultData.CommonFtsFilter(new[] {"Раскольников"})
                    });
                
                yield return new TestCaseData(aggregationEntityFilterGuid, searchQuery, id, comment)
                    .SetName($"{nameof(C443371ProcessingSqActionLinkFts1Result)}")
                    .SetCategory(nameof(TestCategories.PriorityHigh));
            }
        }
        public static IEnumerable<TestCaseData> C1109301ProcessingSqActionMarkLinkNotifyFts1Result
        {
            get
            { 
                var markId = Guid.NewGuid();
                var markComment = "mark1";
                var linkId = Guid.NewGuid();
                var linkComment = "link1";
                
                var aggregationEntityFilterGuid = Guid.NewGuid();
                var searchQuery = DefaultData.CreateQuery(new TestAction[]
                    {
                        DefaultData.CreateNotifyAction("Раскольников"),
                        DefaultData.CreateMarkAction(new Dictionary<Guid, string>()
                        {
                            {markId, markComment},
                        }),
                        DefaultData.CreateLinkAction(new Dictionary<Guid, string>()
                        {
                            {linkId, linkComment},
                        }),
                    },
                    new TestFilterSq[]
                    {
                        DefaultData.CommonFtsFilter(new []{"Раскольников"})
                    });
                
                yield return new TestCaseData(aggregationEntityFilterGuid, searchQuery, 1,
                        new Dictionary<Guid,string>(){{markId,markComment}}, 
                        new Dictionary<Guid,string>(){{linkId,linkComment}})
                    .SetName($"{nameof(C1109301ProcessingSqActionMarkLinkNotifyFts1Result)}")
                    .SetCategory(nameof(TestCategories.PriorityMedium));
            }
        }
        public static IEnumerable<TestCaseData> C1113211ProcessingSqActionMarkLinkNotifyFts1ResultCreatedBefore
        {
            get
            { 
                var markId = Guid.NewGuid();
                var markComment = "mark1";
                var linkId = Guid.NewGuid();
                var linkComment = "link1";
                
                var aggregationEntityFilterGuid = Guid.NewGuid();
                var searchQuery = DefaultData.CreateQuery(new TestAction[]
                    {
                        DefaultData.CreateNotifyAction("Раскольников"),
                        DefaultData.CreateMarkAction(new Dictionary<Guid, string>()
                        {
                            {markId, markComment},
                        }),
                        DefaultData.CreateLinkAction(new Dictionary<Guid, string>()
                        {
                            {linkId, linkComment},
                        }),
                    },
                    new TestFilterSq[]
                    {
                        DefaultData.CommonFtsFilter(new []{"Раскольников"})
                    });
                
                yield return new TestCaseData(aggregationEntityFilterGuid, searchQuery, 1,
                        new Dictionary<Guid,string>(){{markId,markComment}}, 
                        new Dictionary<Guid,string>(){{linkId,linkComment}})
                    .SetName($"{nameof(C1113211ProcessingSqActionMarkLinkNotifyFts1ResultCreatedBefore)}")
                    .SetCategory(nameof(TestCategories.PriorityMedium));
            }
        }
        
        public static IEnumerable<TestCaseData> C1113213ProcessingSqActionMarkLinkNotifyFts1ResultCreatedBeforeAndAfter
        {
            get
            { 
                var markId = Guid.NewGuid();
                var markComment = "mark1";
                var linkId = Guid.NewGuid();
                var linkComment = "link1";
                
                var aggregationEntityFilterGuid = Guid.NewGuid();
                var searchQuery = DefaultData.CreateQuery(new TestAction[]
                    {
                        DefaultData.CreateNotifyAction("Раскольников"),
                        DefaultData.CreateMarkAction(new Dictionary<Guid, string>()
                        {
                            {markId, markComment},
                        }),
                        DefaultData.CreateLinkAction(new Dictionary<Guid, string>()
                        {
                            {linkId, linkComment},
                        }),
                    },
                    new TestFilterSq[]
                    {
                        DefaultData.CommonFtsFilter(new []{"Раскольников"})
                    });
                
                yield return new TestCaseData(aggregationEntityFilterGuid, searchQuery, 1,
                        new Dictionary<Guid,string>(){{markId,markComment}}, 
                        new Dictionary<Guid,string>(){{linkId,linkComment}})
                    .SetName($"{nameof(C1113213ProcessingSqActionMarkLinkNotifyFts1ResultCreatedBeforeAndAfter)}")
                    .SetCategory(nameof(TestCategories.PriorityMedium));
            }
        }
        
        public static IEnumerable<TestCaseData> C1109302ProcessingSqActionMarkLinkNotifyFts3Result
        {
            get
            { 
                var markId = Guid.NewGuid();
                var markComment = "mark1";
                var linkId = Guid.NewGuid();
                var linkComment = "link1";
                
                var aggregationEntityFilterGuid = Guid.NewGuid();
                var searchQuery = DefaultData.CreateQuery(new TestAction[]
                    {
                        DefaultData.CreateNotifyAction("Раскольников"),
                        DefaultData.CreateMarkAction(new Dictionary<Guid, string>()
                        {
                            {markId, markComment},
                        }),
                        DefaultData.CreateLinkAction(new Dictionary<Guid, string>()
                        {
                            {linkId, linkComment},
                        }),
                    },
                    new TestFilterSq[]
                    {
                        DefaultData.CommonFtsFilter(new []{"Раскольников"})
                    });
                
                yield return new TestCaseData(aggregationEntityFilterGuid, searchQuery, 3,
                        new Dictionary<Guid,string>(){{markId,markComment}}, 
                        new Dictionary<Guid,string>(){{linkId,linkComment}})
                    .SetName($"{nameof(C1109302ProcessingSqActionMarkLinkNotifyFts3Result)}")
                    .SetCategory(nameof(TestCategories.PriorityMedium));
            }
        }
        
        public static IEnumerable<TestCaseData> C1109318ProcessingSqActionMarkLinkNotifyFts2Result1Other
        {
            get
            { 
                var markId = Guid.NewGuid();
                var markComment = "mark1";
                var linkId = Guid.NewGuid();
                var linkComment = "link1";
                
                var aggregationEntityFilterGuid = Guid.NewGuid();
                var searchQuery = DefaultData.CreateQuery(new TestAction[]
                    {
                        DefaultData.CreateNotifyAction("Раскольников"),
                        DefaultData.CreateMarkAction(new Dictionary<Guid, string>()
                        {
                            {markId, markComment},
                        }),
                        DefaultData.CreateLinkAction(new Dictionary<Guid, string>()
                        {
                            {linkId, linkComment},
                        }),
                    },
                    new TestFilterSq[]
                    {
                        DefaultData.CommonFtsFilter(new []{"Раскольников"})
                    });
                
                yield return new TestCaseData(aggregationEntityFilterGuid, searchQuery, 2,
                        new Dictionary<Guid,string>(){{markId,markComment}}, 
                        new Dictionary<Guid,string>(){{linkId,linkComment}})
                    .SetName($"{nameof(C1109318ProcessingSqActionMarkLinkNotifyFts2Result1Other)}")
                    .SetCategory(nameof(TestCategories.PriorityMedium));
            }
        }
        
        public static IEnumerable<TestCaseData> C1109339Processing2SqActionMarkLinkNotifyFts2Result
        {
            get
            { 
                var markId = Guid.NewGuid();
                var markComment = "mark1";
                var linkId = Guid.NewGuid();
                var linkComment = "link1";
                var markId2 = Guid.NewGuid();
                var markComment2 = "mark2";
                var linkId2 = Guid.NewGuid();
                var linkComment2 = "link2";
                
                var aggregationEntityFilterGuid = Guid.NewGuid();
                var searchQuery = DefaultData.CreateQuery(new TestAction[]
                    {
                        DefaultData.CreateNotifyAction("Раскольников"),
                        DefaultData.CreateMarkAction(new Dictionary<Guid, string>()
                        {
                            {markId, markComment},
                        }),
                        DefaultData.CreateLinkAction(new Dictionary<Guid, string>()
                        {
                            {linkId, linkComment},
                        }),
                    },
                    new TestFilterSq[]
                    {
                        DefaultData.CommonFtsFilter(new []{"Ноутбук"})
                    });
                var searchQuery2 = DefaultData.CreateQuery(new TestAction[]
                    {
                        DefaultData.CreateNotifyAction("Раскольников"),
                        DefaultData.CreateMarkAction(new Dictionary<Guid, string>()
                        {
                            {markId2, markComment2},
                        }),
                        DefaultData.CreateLinkAction(new Dictionary<Guid, string>()
                        {
                            {linkId2, linkComment2},
                        }),
                    },
                    new TestFilterSq[]
                    {
                        DefaultData.CommonFtsFilter(new []{"Раскольников"})
                    });
                
                yield return new TestCaseData(aggregationEntityFilterGuid, searchQuery, searchQuery2,2, 1,
                        new Dictionary<Guid,string>(){{markId,markComment},{markId2,markComment2}}, 
                        new Dictionary<Guid,string>(){{linkId,linkComment},{linkId2,linkComment2}},
                        new Dictionary<Guid,string>(){{markId,markComment}}, 
                        new Dictionary<Guid,string>(){{linkId,linkComment}})
                    .SetName($"{nameof(C1109339Processing2SqActionMarkLinkNotifyFts2Result)}")
                    .SetCategory(nameof(TestCategories.PriorityMedium));
            }
        }
    }
}