namespace FitTrackPro.API.IntegrationTests.Controllers;

using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using FitTrackPro.API.IntegrationTests;
using FitTrackPro.Application.Common.Utils;
using Xunit;

public class FoodsRecognitionTests : IntegrationTestBase
{
    public FoodsRecognitionTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    // ─────────────────────────────────────────────────────
    // POST /api/foods/recognize
    // ─────────────────────────────────────────────────────

    [Fact]
    public async Task RecognizeFood_WithValidImage_ReturnsOkOrServiceUnavailable()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        // Tạo 1x1 white JPEG test image bytes
        var imageBytes = CreateMinimalJpegBytes();
        var content = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(imageBytes);
        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(imageContent, "image", "test_food.jpg");

        // Act
        var response = await Client.PostAsync("/api/foods/recognize", content);

        // Assert: Model có thể chưa được load trong test environment → 503 OK
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task RecognizeFood_WithoutAuth_Returns401()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        Client.DefaultRequestHeaders.Remove("X-Test-UserId");
        var imageBytes = CreateMinimalJpegBytes();
        var content = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(imageBytes);
        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(imageContent, "image", "test.jpg");

        // Act
        var response = await Client.PostAsync("/api/foods/recognize", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RecognizeFood_WithoutImage_Returns400()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        var content = new MultipartFormDataContent(); // empty — no image

        // Act
        var response = await Client.PostAsync("/api/foods/recognize", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RecognizeFood_WithInvalidFileType_Returns400()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        var content = new MultipartFormDataContent();
        var pdfContent = new ByteArrayContent(new byte[] { 0x25, 0x50, 0x44, 0x46 }); // PDF magic bytes
        pdfContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
        content.Add(pdfContent, "image", "document.pdf");

        // Act
        var response = await Client.PostAsync("/api/foods/recognize", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RecognizeFood_WithFileTooLarge_Returns400()
    {
        // Arrange
        var authResponse = await RegisterAndLoginUserAsync();
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        // 11MB file (vượt limit 10MB)
        var oversizedBytes = new byte[11 * 1024 * 1024];
        var content = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(oversizedBytes);
        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        content.Add(imageContent, "image", "oversized.jpg");

        // Act
        var response = await Client.PostAsync("/api/foods/recognize", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ─────────────────────────────────────────────────────
    // POST /api/foods/train-model (Admin only)
    // ─────────────────────────────────────────────────────

    [Fact]
    public async Task TrainModel_WithNonAdminUser_Returns403()
    {
        // Arrange — regular user token (no admin role)
        var authResponse = await RegisterAndLoginUserAsync();
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);
        Client.DefaultRequestHeaders.Add("X-Test-UserId", authResponse.UserId.ToString());

        // Act
        var response = await Client.PostAsync("/api/foods/train-model", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task TrainModel_WithoutAuth_Returns401()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        Client.DefaultRequestHeaders.Remove("X-Test-UserId");
        var response = await Client.PostAsync("/api/foods/train-model", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────
    // UNIT TESTS: FoodLabelMapper
    // ─────────────────────────────────────────────────────

    [Theory]
    [InlineData("pho_bo",   "Phở Bò")]
    [InlineData("com_tam",  "Cơm Tấm")]
    [InlineData("banh_mi",  "Bánh Mì")]
    [InlineData("goi_cuon", "Gỏi Cuốn")]
    [InlineData("unknown",  "unknown")] // fallback = chính label
    public void FoodLabelMapper_GetDisplayName_ReturnsExpected(
        string label, string expectedDisplayName)
    {
        // Act
        var result = FoodLabelMapper.GetDisplayName(label);

        // Assert
        result.Should().Be(expectedDisplayName);
    }

    // ─────────────────────────────────────────────────────
    // UNIT TESTS: RecognizeFoodCommandValidator
    // ─────────────────────────────────────────────────────

    [Fact]
    public async Task Validator_ValidJpegImage_PassesValidation()
    {
        // Arrange
        var validator = new Application.Features.Foods.Commands.RecognizeFood
            .RecognizeFoodCommandValidator();

        var mockFile = CreateMockFormFile("pho.jpg", "image/jpeg", 1024);
        var command  = new Application.Features.Foods.Commands.RecognizeFood
            .RecognizeFoodCommand(mockFile);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validator_PdfFile_FailsValidation()
    {
        var validator = new Application.Features.Foods.Commands.RecognizeFood
            .RecognizeFoodCommandValidator();

        var mockFile = CreateMockFormFile("doc.pdf", "application/pdf", 1024);
        var command  = new Application.Features.Foods.Commands.RecognizeFood
            .RecognizeFoodCommand(mockFile);

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "Image.FileName");
    }

    // ─────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────

    /// <summary>Tạo JPEG bytes tối giản hợp lệ (1x1 pixel)</summary>
    private static byte[] CreateMinimalJpegBytes()
    {
        // Minimal valid JPEG (1x1 white pixel)
        return Convert.FromBase64String(
            "/9j/4AAQSkZJRgABAQEASABIAAD/2wBDAAMCAgMCAgMDAwMEAwMEBQgFBQQEBQoH" +
            "BwYIDAoMCwsKCwsNCxAQDQ4RDgsLEBYQERMUFRUVDA8XGBYUGBIUFRT/2wBDAQME" +
            "BAUEBQkFBQkUDQsNFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQUFBQU" +
            "FBQUFBQUFBT/wAARCAABAAEDASIAAhEBAxEB/8QAFgABAQEAAAAAAAAAAAAAAAAAAAAI" +
            "/8QAFBABAAAAAAAAAAAAAAAAAAAAkP/EABQBAQAAAAAAAAAAAAAAAAAAAAD/xAAUEQEA" +
            "AAAAAAAAAAAAAAAAAAAA/9oADAMBAAIRAxEAPwCwABmX/9k=");
    }

    private static Microsoft.AspNetCore.Http.IFormFile CreateMockFormFile(
        string fileName, string contentType, long length)
    {
        var mockFile = new Moq.Mock<Microsoft.AspNetCore.Http.IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.ContentType).Returns(contentType);
        mockFile.Setup(f => f.Length).Returns(length);
        mockFile.Setup(f => f.OpenReadStream())
                .Returns(new System.IO.MemoryStream(new byte[length]));
        return mockFile.Object;
    }
}
