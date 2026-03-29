namespace FitTrackPro.API.IntegrationTests.Controllers;

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;
using FitTrackPro.Application.Features.Workouts.Commands;
using FitTrackPro.Application.Features.Workouts.DTOs;
using FitTrackPro.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using FitTrackPro.Application.Features.Workouts.Commands.StartWorkout;
using FitTrackPro.Application.Features.Workouts.Commands.LogExercise;
using FitTrackPro.Application.Features.Workouts.Commands.CompleteWorkout;

public class WorkoutsControllerTests : IntegrationTestBase
{
    public WorkoutsControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task StartWorkout_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        var command = new StartWorkoutCommand
        {
            Title = "Morning Chest Workout",
            Notes = "Focus on progressive overload"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/workouts/start", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<WorkoutSessionDto>();
        result.Should().NotBeNull();
        result!.Title.Should().Be("Morning Chest Workout");
        result.Status.Should().Be("InProgress");
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task StartWorkout_WhenAlreadyHaveActiveWorkout_ShouldReturnBadRequest()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        // Start first workout
        var command1 = new StartWorkoutCommand { Title = "Workout 1" };
        await Client.PostAsJsonAsync("/api/workouts/start", command1);

        // Try to start second workout
        var command2 = new StartWorkoutCommand { Title = "Workout 2" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/workouts/start", command2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CompleteWorkoutFlow_ShouldWorkEndToEnd()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        // Seed an exercise first
        var exercise = await SeedExerciseAsync("Bench Press");

        // Step 1: Start workout
        var startCommand = new StartWorkoutCommand
        {
            Title = "Full Chest Day",
            Notes = "Heavy day"
        };

        var startResponse = await Client.PostAsJsonAsync("/api/workouts/start", startCommand);
        startResponse.EnsureSuccessStatusCode();
        var workout = await startResponse.Content.ReadFromJsonAsync<WorkoutSessionDto>();
        workout.Should().NotBeNull();

        // Step 2: Log exercise with sets
        var logExerciseCommand = new LogExerciseCommand
        {
            ExerciseId = exercise.Id,
            Notes = "Felt strong today",
            Sets = new List<SetInput>
            {
                new SetInput { Weight = 100m, Reps = 10 },
                new SetInput { Weight = 100m, Reps = 8 },
                new SetInput { Weight = 100m, Reps = 6 }
            }
        };

        var logResponse = await Client.PostAsJsonAsync(
            $"/api/workouts/{workout!.Id}/exercises",
            logExerciseCommand);
        
        logResponse.EnsureSuccessStatusCode();
        var workoutExercise = await logResponse.Content.ReadFromJsonAsync<WorkoutExerciseDto>();
        workoutExercise.Should().NotBeNull();
        workoutExercise!.Sets.Should().HaveCount(3);

        // Step 3: Complete workout
        var completeCommand = new CompleteWorkoutCommand
        {
            CaloriesBurned = 350
        };

        var completeResponse = await Client.PostAsJsonAsync(
            $"/api/workouts/{workout.Id}/complete",
            completeCommand);

        completeResponse.EnsureSuccessStatusCode();
        var completedWorkout = await completeResponse.Content.ReadFromJsonAsync<WorkoutSessionDto>();
        completedWorkout.Should().NotBeNull();
        completedWorkout!.Status.Should().Be("Completed");
        completedWorkout.TotalCaloriesBurned.Should().Be(350);
        completedWorkout.DurationMinutes.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetActiveWorkout_WhenHasActiveWorkout_ShouldReturnWorkout()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        // Start a workout
        var command = new StartWorkoutCommand { Title = "Test Workout" };
        await Client.PostAsJsonAsync("/api/workouts/start", command);

        // Act
        var response = await Client.GetAsync("/api/workouts/active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var workout = await response.Content.ReadFromJsonAsync<WorkoutSessionDto>();
        workout.Should().NotBeNull();
        workout!.Title.Should().Be("Test Workout");
    }

    [Fact]
    public async Task GetActiveWorkout_WhenNoActiveWorkout_ShouldReturnNull()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        // Act
        var response = await Client.GetAsync("/api/workouts/active");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("null");
    }

    [Fact]
    public async Task GetWorkoutHistory_ShouldReturnPaginatedResults()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        // Create and complete 3 workouts
        for (int i = 1; i <= 3; i++)
        {
            var startCmd = new StartWorkoutCommand { Title = $"Workout {i}" };
            var startRes = await Client.PostAsJsonAsync("/api/workouts/start", startCmd);
            var workout = await startRes.Content.ReadFromJsonAsync<WorkoutSessionDto>();
            
            var completeCmd = new CompleteWorkoutCommand();
            await Client.PostAsJsonAsync($"/api/workouts/{workout!.Id}/complete", completeCmd);
        }

        // Act
        var response = await Client.GetAsync("/api/workouts/history?pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<
            FitTrackPro.Application.Common.Models.PaginatedList<WorkoutSessionDto>>();
        
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetWorkoutSummary_ShouldReturnStatistics()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        var exercise = await SeedExerciseAsync("Squat");

        // Create workout with exercise
        var startCmd = new StartWorkoutCommand { Title = "Leg Day" };
        var startRes = await Client.PostAsJsonAsync("/api/workouts/start", startCmd);
        var workout = await startRes.Content.ReadFromJsonAsync<WorkoutSessionDto>();

        var logCmd = new LogExerciseCommand
        {
            ExerciseId = exercise.Id,
            Sets = new List<SetInput>
            {
                new SetInput { Weight = 140m, Reps = 5 }
            }
        };
        await Client.PostAsJsonAsync($"/api/workouts/{workout!.Id}/exercises", logCmd);

        var completeCmd = new CompleteWorkoutCommand { CaloriesBurned = 400 };
        await Client.PostAsJsonAsync($"/api/workouts/{workout.Id}/complete", completeCmd);

        // Act
        var response = await Client.GetAsync("/api/workouts/summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var summary = await response.Content.ReadFromJsonAsync<WorkoutSummaryDto>();
        
        summary.Should().NotBeNull();
        summary!.TotalWorkouts.Should().Be(1);
        summary.TotalCaloriesBurned.Should().Be(400);
        summary.TotalSets.Should().Be(1);
    }

    [Fact]
    public async Task GetPersonalRecords_AfterLoggingExercise_ShouldReturnPR()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        var exercise = await SeedExerciseAsync("Deadlift");

        // Create workout and log a PR
        var startCmd = new StartWorkoutCommand { Title = "PR Day" };
        var startRes = await Client.PostAsJsonAsync("/api/workouts/start", startCmd);
        var workout = await startRes.Content.ReadFromJsonAsync<WorkoutSessionDto>();

        var logCmd = new LogExerciseCommand
        {
            ExerciseId = exercise.Id,
            Sets = new List<SetInput>
            {
                new SetInput { Weight = 200m, Reps = 1 }
            }
        };
        await Client.PostAsJsonAsync($"/api/workouts/{workout!.Id}/exercises", logCmd);

        // Act
        var response = await Client.GetAsync($"/api/workouts/records?exerciseId={exercise.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var records = await response.Content.ReadFromJsonAsync<List<PersonalRecordDto>>();
        
        records.Should().NotBeNull();
        records.Should().NotBeEmpty();
        records!.Should().Contain(r => r.ExerciseId == exercise.Id);
    }

    [Fact]
    public async Task DeleteWorkout_ShouldRemoveWorkout()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        SetAuthorizationHeader(authResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        // Create workout
        var startCmd = new StartWorkoutCommand { Title = "To Delete" };
        var startRes = await Client.PostAsJsonAsync("/api/workouts/start", startCmd);
        var workout = await startRes.Content.ReadFromJsonAsync<WorkoutSessionDto>();

        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/workouts/{workout!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's deleted
        var getResponse = await Client.GetAsync("/api/workouts/active");
        var content = await getResponse.Content.ReadAsStringAsync();
        content.Should().Contain("null");
    }

    // Helper method to seed an exercise
    private async Task<Domain.Entities.Exercise> SeedExerciseAsync(string name)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider
            .GetRequiredService<Infrastructure.Persistence.ApplicationDbContext>();

        var exercise = Domain.Entities.Exercise.Create(
            name,
            null,
            ExerciseCategory.Strength,
            MuscleGroup.Chest,
            EquipmentType.Barbell,
            DifficultyLevel.Intermediate);

        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        return exercise;
    }
}
