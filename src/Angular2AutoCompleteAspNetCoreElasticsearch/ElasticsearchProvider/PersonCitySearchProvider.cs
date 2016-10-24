﻿using System;
using System.Collections.Generic;
using System.Linq;
using ElasticsearchCRUD;
using ElasticsearchCRUD.ContextAddDeleteUpdate.IndexModel.MappingModel;
using ElasticsearchCRUD.ContextAddDeleteUpdate.IndexModel.SettingsModel;
using ElasticsearchCRUD.ContextAddDeleteUpdate.IndexModel.SettingsModel.Analyzers;
using ElasticsearchCRUD.ContextAddDeleteUpdate.IndexModel.SettingsModel.Filters;
using ElasticsearchCRUD.ContextAddDeleteUpdate.IndexModel.SettingsModel.Tokenizers;
using ElasticsearchCRUD.ContextSearch.SearchModel.AggModel;
using ElasticsearchCRUD.Model;
using ElasticsearchCRUD.Model.SearchModel;
using ElasticsearchCRUD.Model.SearchModel.Aggregations;
using ElasticsearchCRUD.Model.SearchModel.Queries;
using ElasticsearchCRUD.Model.SearchModel.Sorting;
using ElasticsearchCRUD.Tracing;

namespace SearchComponent
{
    public class PersonCitySearchProvider : IPersonCitySearchProvider
    {
        private readonly IElasticsearchMappingResolver _elasticsearchMappingResolver = new ElasticsearchMappingResolver();
        //private const string ConnectionString = "http://localhost.fiddler:9200";
        private const string ConnectionString = "http://localhost:9200";
        private readonly ElasticsearchContext _context;

        public PersonCitySearchProvider()
        {
            _elasticsearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(PersonCityMappingDto), new PersonCityMapping());
            _context = new ElasticsearchContext(ConnectionString, new ElasticsearchSerializerConfiguration(_elasticsearchMappingResolver))
            {
                TraceProvider = new ConsoleTraceProvider()
            };
        }

        public void CreateIndex()
        {			
            _context.IndexCreate<PersonCityMappingDto>(CreateNewIndexDefinition());
        }

        public void CreateMapping()
        {
            _context.IndexCreateTypeMapping<PersonCityMappingDto>(new MappingDefinition() {});
        }

        private IndexDefinition CreateNewIndexDefinition()
        {
            return new IndexDefinition
            {
                IndexSettings =
                {
                    Analysis = new Analysis
                    {
                        Filters =
                        {
                            CustomFilters = new List<AnalysisFilterBase>
                            {
                                new StemmerTokenFilter("stemmer"),
                                new ShingleTokenFilter("autocompletefilter")
                                {
                                    MaxShingleSize = 5,
                                    MinShingleSize = 2
                                },
                                new StopTokenFilter("stopwords"),
                                new EdgeNGramTokenFilter("edge_ngram_filter")
                                {
                                    MaxGram = 20,
                                    MinGram = 2
                                }
                            }
                        },
                        Analyzer =
                        {
                            Analyzers = new List<AnalyzerBase>
                            {
                                new CustomAnalyzer("didyoumean")
                                {
                                    Tokenizer = DefaultTokenizers.Standard,
                                    Filter = new List<string> {DefaultTokenFilters.Lowercase, "edge_ngram_filter"},
                                    CharFilter = new List<string> {DefaultCharFilters.HtmlStrip}
                                },
                                new CustomAnalyzer("autocomplete")
                                {
                                    Tokenizer = DefaultTokenizers.Standard,
                                    Filter = new List<string> {DefaultTokenFilters.Lowercase, "autocompletefilter"},
                                    CharFilter = new List<string> {DefaultCharFilters.HtmlStrip}
                                },
                                new CustomAnalyzer("default")
                                {
                                    Tokenizer = DefaultTokenizers.Standard,
                                    Filter = new List<string> {DefaultTokenFilters.Lowercase, "stopwords", "stemmer"},
                                    CharFilter = new List<string> {DefaultCharFilters.HtmlStrip}
                                }
                                
                               
                            }
                        },
                        Tokenizers =
                        {
                            CustomTokenizers = new List<AnalysisTokenizerBase>
                            {
                                new EdgeNGramTokenizer("ngram_tokenizer")
                                {
                                    MaxGram = 4,
                                    MinGram = 4
                                }
                            }
                        }

                    }
                },
            };

        }

        public IEnumerable<string> AutocompleteSearch(string term)
        {
            var search = new Search
            {
                Size = 0,
                Aggs = new List<IAggs>
                {
                    new TermsBucketAggregation("autocomplete", "autocomplete")
                    {
                        Order= new OrderAgg("_count", OrderEnum.desc),
                        Include = new IncludeExpression(term + ".*")
                    }
                },
                Query = new Query(new PrefixQuery("autocomplete", term))
            };

            var items = _context.Search<PersonCity>(search);
            var aggResult = items.PayloadResult.Aggregations.GetComplexValue<TermsBucketAggregationsResult>("autocomplete");
            IEnumerable<string> results = aggResult.Buckets.Select(t =>  t.Key.ToString());
            return results;
        }

        public IEnumerable<PersonCity> Search(string term)
        {
            var search = new Search
            {
                Size = 10,
                Query = new Query(new MatchQuery("did_you_mean", term))
            };

            var results = _context.Search<PersonCity>(search);

            return results.PayloadResult.Hits.HitsResult.Select(t => t.Source);
        }

        public bool GetStatus()
        {
            return _context.IndexExists<PersonCity>();
        }

        /// <summary>
        /// Used for basic auto complete
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public IEnumerable<PersonCity> QueryString(string term)
        {
            var results = _context.Search<PersonCity>(BuildQueryStringSearch(term));

            return results.PayloadResult.Hits.HitsResult.Select(t => t.Source);
        }

        public void CreateTestData()
        {
            var jm = new PersonCity { Id = 1, FamilyName = "Moore", Info = "Muenich", Name = "John" };
            _context.AddUpdateDocument(jm, jm.Id);
            var jj = new PersonCity { Id = 2, FamilyName = "Jones", Info = "Münich, have not heard much about this guy, willig to do more", Name = "Johny" };
            _context.AddUpdateDocument(jj, jj.Id);
            var pm = new PersonCity { Id = 3, FamilyName = "Murphy", Info = "Munich, this guy is special, wants to win everything", Name = "Paul" };
            _context.AddUpdateDocument(pm, pm.Id);
            var sm = new PersonCity { Id = 4, FamilyName = "McGurk", Info = "munich, peanut jelly type", Name = "Séan" };
            _context.AddUpdateDocument(sm, sm.Id);
            var sob = new PersonCity { Id = 5, FamilyName = "O'Brien", Info = "Not a much use, bit of a problem", Name = "Sean" };
            _context.AddUpdateDocument(sob, sob.Id);
            var tmc = new PersonCity { Id = 6, FamilyName = "McCauley", Info = "Couldn't a ask for anyone better", Name = "Tadhg" };
            _context.AddUpdateDocument(tmc, tmc.Id);
            var id7 = new PersonCity { Id = 7, FamilyName = "Martini", Info = "Köniz, one of a million, looking forward to gtting this person on board", Name = "Christian" };
            _context.AddUpdateDocument(id7, id7.Id);
            var id8 = new PersonCity { Id = 8, FamilyName = "Lee", Info = "Basel Stadt, always sing, happy as larry", Name = "Phil" };
            _context.AddUpdateDocument(id8, id8.Id);
            var id9 = new PersonCity { Id = 9, FamilyName = "Wil", Info = "Basel Stadt, another from the sticks, a lot to learn here", Name = "Nicole" };
            _context.AddUpdateDocument(id9, id9.Id);
            var id10 = new PersonCity { Id = 10, FamilyName = "Mario", Info = "Basel in some small town", Name = "Tim" };
            _context.AddUpdateDocument(id10, id10.Id);
            var id11 = new PersonCity { Id = 11, FamilyName = "Martin", Info = "Biel, too much cloud in the head", Name = "Scott" };
            _context.AddUpdateDocument(id11, id11.Id);
            var id12 = new PersonCity { Id = 12, FamilyName = "Newman", Info = "Lyss, what a way to go, very impressive", Name = "Tim" };
            _context.AddUpdateDocument(id12, id12.Id);
            var id13 = new PersonCity { Id = 13, FamilyName = "Lamb", Info = "Thun, never stops dancing and singing", Name = "Carla" };
            _context.AddUpdateDocument(id13, id13.Id);
            var id14 = new PersonCity { Id = 14, FamilyName = "Goldi", Info = "Zug, looper loads to handle here", Name = "Ida" };
            _context.AddUpdateDocument(id14, id14.Id);
            var id15 = new PersonCity { Id = 15, FamilyName = "tim", Info = "Brig, some how this address and data might be incorrect", Name = "Lou" };
            _context.AddUpdateDocument(id15, id15.Id);
            var id16 = new PersonCity { Id = 16, FamilyName = "Mat", Info = "Bulle, chill out sessions expert, always can help a small bit", Name = "Wil" };
            _context.AddUpdateDocument(id16, id16.Id);
            var id17 = new PersonCity { Id = 17, FamilyName = "Jones", Info = "Rheinfelden, having a ball, won't come home, will be there this XMas", Name = "Mick" };
            _context.AddUpdateDocument(id17, id17.Id);
            var id18 = new PersonCity { Id = 18, FamilyName = "Last", Info = "Rheinfelden,new kid on the block, expert, novice in everything", Name = "James" };
            _context.AddUpdateDocument(id18, id18.Id);
            _context.SaveChanges();
        }
        /// <summary>
        /// TODO protect against injection!
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        private Search BuildQueryStringSearch(string term)
        {
            var names = "";
            if (term != null)
            {
                names = term.Replace("+", " OR *");
            }

            var search = new Search
            {
                Query = new Query(new QueryStringQuery(names + "*"))
            };

            return search;
        }
    }
}