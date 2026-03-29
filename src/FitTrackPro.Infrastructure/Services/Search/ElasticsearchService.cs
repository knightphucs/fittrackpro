namespace FitTrackPro.Infrastructure.Services.Search;

using System;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Foods.DTOs;
using FitTrackPro.Application.Features.Foods.Queries.SearchFoods;
using FitTrackPro.Application.Features.Workouts.DTOs;
using FitTrackPro.Application.Features.Workouts.Queries.SearchExercises;
using Microsoft.Extensions.Logging;

public class ElasticsearchService : ISearchService
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticsearchService> _logger;
    private const string IndexName = "foods_index";
    private const string ExerciseIndex = "exercises_index";
    private static readonly string[] fields = [ "name^3", "name.autocomplete^2", "nameVi^2", "nameVi.autocomplete" ];

    public ElasticsearchService(ElasticsearchClient client, ILogger<ElasticsearchService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<PaginatedList<FoodDto>> AdvancedSearchFoodsAsync(
        AdvancedSearchFoodsQuery query, 
        CancellationToken cancellationToken)
    {
        var response = await _client.SearchAsync<FoodDto>(s => s
            .Index(IndexName)
            .From((query.PageNumber - 1) * query.PageSize)
            .Size(query.PageSize)
            .Query(q => q
                .Bool(b =>
                {
                    // Text Search (Fuzzy)
                    if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                    {
                        b.Must(m => m.MultiMatch(mm => mm
                            .Query(query.SearchTerm)
                            .Fields(fields) // Boost name English
                            .Fuzziness(new Fuzziness("AUTO"))
                        ));
                    }

                    // Filters List
                    var filters = new List<Action<QueryDescriptor<FoodDto>>>();

                    // Category
                    if (!string.IsNullOrWhiteSpace(query.Category))
                        filters.Add(f => f.Term(t => t.Field(ff => ff.Category).Value(query.Category)));

                    // Calories Range
                    if (query.MinCalories.HasValue || query.MaxCalories.HasValue)
                    {
                        filters.Add(f => f.Range(r => r.NumberRange(n => n
                            .Field(ff => ff.Calories)
                            .Gte(query.MinCalories)
                            .Lte(query.MaxCalories))));
                    }

                    // Protein Range
                    if (query.MinProtein.HasValue || query.MaxProtein.HasValue)
                    {
                        filters.Add(f => f.Range(r => r.NumberRange(n => n
                            .Field(ff => ff.Protein)
                            .Gte((double?)query.MinProtein)
                            .Lte((double?)query.MaxProtein))));
                    }

                    // Carbs Range
                    if (query.MinCarbs.HasValue || query.MaxCarbs.HasValue)
                    {
                        filters.Add(f => f.Range(r => r.NumberRange(n => n
                            .Field(ff => ff.Carbs)
                            .Gte((double?)query.MinCarbs)
                            .Lte((double?)query.MaxCarbs))));
                    }

                    // Fat Range
                    if (query.MinFat.HasValue || query.MaxFat.HasValue)
                    {
                        filters.Add(f => f.Range(r => r.NumberRange(n => n
                            .Field(ff => ff.Fat)
                            .Gte((double?)query.MinFat)
                            .Lte((double?)query.MaxFat))));
                    }

                    // Apply Filters
                    if (filters.Any())
                        b.Filter(filters.ToArray());
                })
            )
            // 3. Sorting
            .Sort(sort => {
                if (string.IsNullOrWhiteSpace(query.SortBy) || query.SortBy == "name")
                {
                    sort.Field(f => f.Name, new FieldSort { Order = SortOrder.Asc });
                }
                else
                {
                    var order = query.IsDescending ? SortOrder.Desc : SortOrder.Asc;
                    switch (query.SortBy.ToLower())
                    {
                        case "calories": sort.Field(f => f.Calories, new FieldSort { Order = order }); break;
                        case "protein": sort.Field(f => f.Protein, new FieldSort { Order = order }); break;
                        case "carbs": sort.Field(f => f.Carbs, new FieldSort { Order = order }); break;
                        case "fat": sort.Field(f => f.Fat, new FieldSort { Order = order }); break;
                    }
                }
            }), 
            cancellationToken);

        return new PaginatedList<FoodDto>(
            response.Documents.ToList(), 
            (int)response.Total, 
            query.PageNumber, 
            query.PageSize);
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> RebuildFoodsIndexAsync(
        List<FoodDto> foods,
        CancellationToken cancellationToken)
    {
        var exists = await _client.Indices.ExistsAsync(IndexName, cancellationToken);
        if (exists.Exists)
        {
            var deleteResponse = await _client.Indices.DeleteAsync(IndexName, cancellationToken);
            if (!deleteResponse.IsValidResponse)
            {
                return (false, $"Delete Failed: {deleteResponse.DebugInformation}");
            }
        }

        var createResponse = await _client.Indices.CreateAsync(IndexName, c => c
            .Settings(s => s
                .Analysis(a => a
                    .Analyzers(an => an
                        .Custom("vi_analyzer", ca => ca
                            .Tokenizer("standard")
                            .Filter(new[] { "lowercase", "asciifolding" })
                        )
                        .Custom("vi_autocomplete", ca => ca
                            .Tokenizer("standard")
                            .Filter(new[] { "lowercase", "asciifolding", "edge_ngram_filter" })
                        )
                    )
                    .TokenFilters(tf => tf
                        .EdgeNGram("edge_ngram_filter", e => e
                            .MinGram(2)
                            .MaxGram(20)
                        )
                    )
                )
            )
            .Mappings(m => m
                .Properties<FoodDto>(p => p
                    .Text(f => f.Name, t => t
                        .Analyzer("vi_analyzer")
                        .Fields(ff => ff
                            .Text("autocomplete", tt => tt.Analyzer("vi_autocomplete"))
                        )
                    )
                    .Text(f => f.NameVi!, t => t
                        .Analyzer("vi_analyzer")
                        .Fields(ff => ff
                            .Text("autocomplete", tt => tt.Analyzer("vi_autocomplete"))
                        )
                    )
                    
                    .Keyword(f => f.Category!)
                    .Keyword(f => f.ServingUnit)
                    .Keyword(f => f.Id)
                    .Keyword(f => f.ImageUrl!, k => k.IgnoreAbove(256)) 

                    .IntegerNumber(f => f.Calories)
                    .DoubleNumber(f => f.ServingSize)
                    .DoubleNumber(f => f.Protein)
                    .DoubleNumber(f => f.Carbs)
                    .DoubleNumber(f => f.Fat)
                    .DoubleNumber(f => f.Fiber!, d => d.IgnoreMalformed(true))
                    .DoubleNumber(f => f.Sugar!, d => d.IgnoreMalformed(true))
                )
            ),
        cancellationToken);

        if (!createResponse.IsValidResponse)
        {
            return (false, $"Create Index Failed: {createResponse.DebugInformation}");
        }

        if (foods.Count == 0) return (true, null);

        var bulkResponse = await _client.BulkAsync(b => b
            .Index(IndexName)
            .Refresh(Refresh.False)
            .IndexMany(foods, (d, food) =>
                d.Id(food.Id.ToString())
            ),
            cancellationToken
        );

        await _client.Indices.RefreshAsync(IndexName, cancellationToken);

        if (bulkResponse.Errors)
        {
            var firstError = bulkResponse.Items.FirstOrDefault(x => x.Error != null)?.Error?.Reason;
            return (false, $"Bulk Insert Failed. First Error: {firstError} | Debug: {bulkResponse.DebugInformation}");
        }
        
        return (true, null);
    }

    public async Task<List<string>> AutocompleteFoodsAsync(
        string term,
        CancellationToken cancellationToken)
    {
        var response = await _client.SearchAsync<FoodDto>(s => s
            .Index(IndexName)
            .Size(10)
            .SourceIncludes(new[] {"name", "nameVi"})
            .Query(q => q
                .MultiMatch(mm => mm
                    .Query(term)
                    .Fields(new[]
                    {
                        "name.autocomplete^2",
                        "nameVi.autocomplete"
                    })
                )
            ),
            cancellationToken
        );

        return response.Documents
            .Select(d => d.NameVi ?? d.Name)
            .Distinct()
            .ToList();
    }

    public async Task<bool> IndexFoodAsync(FoodDto food, CancellationToken cancellationToken)
    {
        var response = await _client.IndexAsync(food, i => i
            .Index(IndexName)
            .Id(food.Id),
            cancellationToken);
        
        if (!response.IsValidResponse)
        {
            _logger.LogError("ES Error: {DebugInfo}", response.DebugInformation);
            return false;
        }

        return true;
    }

    public async Task<bool> UpdateFoodInIndexAsync(FoodDto food, CancellationToken cancellationToken)
    {
        var response = await _client.UpdateAsync<FoodDto, FoodDto>(food.Id, u => u
            .Index(IndexName)
            .Doc(food), cancellationToken);
        
        if (!response.IsValidResponse)
        {
            _logger.LogError("ES Error: {DebugInfo}", response.DebugInformation);
            return false;
        }
        return true;
    }

    public async Task<bool> RemoveFoodFromIndexAsync(Guid foodId, CancellationToken cancellationToken)
    {
        var response = await _client.DeleteAsync(IndexName, foodId, cancellationToken);
        
        if (!response.IsValidResponse)
        {
            _logger.LogError("ES Error: {DebugInfo}", response.DebugInformation);
            return false;
        }
        return true;
    }

    public async Task<PaginatedList<ExerciseDto>> SearchExercisesAsync(SearchExercisesQuery query, CancellationToken cancellationToken)
    {
        var response = await _client.SearchAsync<ExerciseDto>(s => s
            .Index(ExerciseIndex)
            .From((query.PageNumber - 1) * query.PageSize)
            .Size(query.PageSize)
            .Query(q => q
                .Bool(b =>
                {
                    // 1. Text Search (Name & NameVi)
                    if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                    {
                        b.Must(m => m.MultiMatch(mm => mm
                            .Query(query.SearchTerm)
                            .Fields(new[] { "name^3", "name.autocomplete^2", "nameVi^2", "nameVi.autocomplete" })
                            .Fuzziness(new Fuzziness("AUTO"))
                        ));
                    }

                    // 2. Filters (Enum -> Keyword)
                    var filters = new List<Action<QueryDescriptor<ExerciseDto>>>();

                    if (query.Category.HasValue)
                        filters.Add(f => f.Term(t => t.Field(ff => ff.Category).Value(query.Category.Value.ToString())));

                    if (query.MuscleGroup.HasValue)
                        filters.Add(f => f.Term(t => t.Field(ff => ff.PrimaryMuscle).Value(query.MuscleGroup.Value.ToString())));

                    if (query.Equipment.HasValue)
                        filters.Add(f => f.Term(t => t.Field(ff => ff.Equipment).Value(query.Equipment.Value.ToString())));

                    if (query.Difficulty.HasValue)
                        filters.Add(f => f.Term(t => t.Field(ff => ff.Difficulty).Value(query.Difficulty.Value.ToString())));

                    if (filters.Any())
                        b.Filter(filters.ToArray());
                })
            )
            .Sort(sort => sort.Field(f => f.Name, new FieldSort { Order = SortOrder.Asc })),
            cancellationToken);

        if (!response.IsValidResponse || response.Documents == null)
        {
            _logger.LogError("Elasticsearch search failed: {DebugInfo}", response.DebugInformation);
            throw new Exception($"Elasticsearch query failed: {response.ElasticsearchServerError?.Error?.Reason ?? "Unknown error"}");
        }

        return new PaginatedList<ExerciseDto>(
            response.Documents.ToList(), 
            (int)response.Total, 
            query.PageNumber, 
            query.PageSize);
    }

    public async Task<bool> IndexExerciseAsync(ExerciseDto exercise, CancellationToken cancellationToken)
    {
        var response = await _client.IndexAsync(exercise, i => i
            .Index(ExerciseIndex)
            .Id(exercise.Id),
            cancellationToken);
        
        if (!response.IsValidResponse)
        {
            _logger.LogError("ES Error: {DebugInfo}", response.DebugInformation);
            return false;
        }
        return true;
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> RebuildExercisesIndexAsync(List<ExerciseDto> exercises, CancellationToken cancellationToken)
    {
        var exists = await _client.Indices.ExistsAsync(ExerciseIndex, cancellationToken);
        if (exists.Exists)
        {
            await _client.Indices.DeleteAsync(ExerciseIndex, cancellationToken);
        }

        var createResponse = await _client.Indices.CreateAsync(ExerciseIndex, c => c
            .Settings(s => s
                .Analysis(a => a
                    .Analyzers(an => an
                        .Custom("vi_analyzer", ca => ca
                            .Tokenizer("standard")
                            .Filter(new[] { "lowercase", "asciifolding" }))
                        .Custom("vi_autocomplete", ca => ca
                            .Tokenizer("standard")
                            .Filter(new[] { "lowercase", "asciifolding", "edge_ngram_filter" }))
                    )
                    .TokenFilters(tf => tf
                        .EdgeNGram("edge_ngram_filter", e => e.MinGram(2).MaxGram(20))
                    )
                )
            )
            .Mappings(m => m
                .Properties<ExerciseDto>(p => p
                    .Keyword(f => f.Id)
                    // Text fields
                    .Text(f => f.Name, t => t.Analyzer("vi_analyzer").Fields(ff => ff.Text("autocomplete", tt => tt.Analyzer("vi_autocomplete"))))
                    .Text(f => f.NameVi!, t => t.Analyzer("vi_analyzer").Fields(ff => ff.Text("autocomplete", tt => tt.Analyzer("vi_autocomplete"))))
                    .Text(f => f.Description!)
                    
                    // Filter fields (Keyword - Exact Match)
                    .Keyword(f => f.Category)
                    .Keyword(f => f.PrimaryMuscle)
                    .Keyword(f => f.Equipment)
                    .Keyword(f => f.Difficulty)
                    
                    // Other fields (No index needed for search, just store)
                    .Keyword(f => f.VideoUrl!, k => k.IgnoreAbove(512))
                    .Keyword(f => f.ImageUrl!, k => k.IgnoreAbove(512))
                )
            ),
            cancellationToken);

        if (!createResponse.IsValidResponse)
            return (false, createResponse.DebugInformation);

        if (exercises.Count == 0) return (true, null);

        // Bulk Insert
        var bulkResponse = await _client.BulkAsync(b => b
            .Index(ExerciseIndex)
            .IndexMany(exercises, (d, ex) => d.Id(ex.Id.ToString())),
            cancellationToken
        );

        if (bulkResponse.Errors)
            return (false, "Bulk Insert Error");

        return (true, null);
    }
}