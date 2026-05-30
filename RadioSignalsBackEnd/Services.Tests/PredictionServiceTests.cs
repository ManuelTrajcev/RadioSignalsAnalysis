using System.Net;
using Domain.Domain_Models;
using Domain.DTO;
using Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Repository.Interface;
using Services.Implementation;
using Services.Tests.TestHelpers;
using Xunit;

namespace Services.Tests;

public class PredictionServiceTests
{
    private const string BaseUrl = "http://prediction-service:8000";

    private readonly Mock<IHttpClientFactory> _httpClientFactory = new();
    private readonly Mock<IRepository<Settlement>> _settlements = new();
    private readonly Mock<IConfiguration> _configuration = new();

    private StubHttpMessageHandler? _handler;

    private PredictionService CreateSut() =>
        new(_httpClientFactory.Object, _settlements.Object, _configuration.Object);

    private void ConfigureHttp(HttpStatusCode status, string body)
    {
        _handler = new StubHttpMessageHandler(status, body);
        _httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(_handler!));
    }

    private void ConfigureBaseUrl(string? url) =>
        _configuration.Setup(c => c["PredictionService:BaseUrl"]).Returns(url);

    private void ConfigureSettlement() =>
        _settlements.SetupGet<Settlement, Settlement>(new Settlement
        {
            Id = Guid.NewGuid(),
            Name = "Centar",
            Population = 5000,
            Households = 1800,
            RegistryNumber = "RN-9",
            Municipality = new Municipality { Name = "Skopje" }
        });

    private static PredictionRequestDto FmRequest() => new()
    {
        SettlementId = Guid.NewGuid(),
        Date = new DateTime(2026, 5, 1),
        LatitudeDegrees = 41,
        LatitudeMinutes = 59,
        LatitudeSeconds = 30f,
        LongitudeDegrees = 21,
        LongitudeMinutes = 25,
        LongitudeSeconds = 45f,
        AltitudeMeters = 300,
        FrequencyMHz = 99.5f,
        Technology = Technology.FM,
        TransmitterLocation = "Vodno"
    };

    [Fact]
    public async Task PredictAsync_DigitalTvWithoutChannel_Throws()
    {
        var dto = FmRequest();
        dto.Technology = Technology.DIGITAL_TV;
        dto.ChannelNumber = null;

        var act = () => CreateSut().PredictAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*ChannelNumber*");
    }

    [Fact]
    public async Task PredictAsync_FmWithoutFrequency_Throws()
    {
        var dto = FmRequest();
        dto.FrequencyMHz = null;

        var act = () => CreateSut().PredictAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*FrequencyMHz*");
    }

    [Fact]
    public async Task PredictAsync_WhenSettlementNotFound_Throws()
    {
        _settlements.SetupGet<Settlement, Settlement>(null!);

        var act = () => CreateSut().PredictAsync(FmRequest());

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Settlement not found*");
    }

    [Fact]
    public async Task PredictAsync_WhenBaseUrlMissing_Throws()
    {
        ConfigureSettlement();
        ConfigureHttp(HttpStatusCode.OK, "{}");
        ConfigureBaseUrl(null);

        var act = () => CreateSut().PredictAsync(FmRequest());

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*BaseUrl*");
    }

    [Fact]
    public async Task PredictAsync_WhenServiceReturnsError_Throws()
    {
        ConfigureSettlement();
        ConfigureBaseUrl(BaseUrl);
        ConfigureHttp(HttpStatusCode.InternalServerError, "boom");

        var act = () => CreateSut().PredictAsync(FmRequest());

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*500*");
    }

    [Fact]
    public async Task PredictAsync_HappyPath_MapsPythonResponseIntoDto()
    {
        ConfigureSettlement();
        ConfigureBaseUrl(BaseUrl);
        ConfigureHttp(HttpStatusCode.OK,
            "{\"technology\":\"FM\",\"field_dbuv_m\":42.5,\"model_version\":\"v1.2\",\"features\":{\"a\":1}}");

        var dto = FmRequest();
        var result = await CreateSut().PredictAsync(dto);

        result.Technology.Should().Be(Technology.FM);
        result.FieldDbuvPerM.Should().Be(42.5);
        result.ModelVersion.Should().Be("v1.2");
        result.Features.Should().ContainKey("a");
        _handler!.CallCount.Should().Be(1);
        _handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
        _handler.LastRequest.RequestUri!.AbsolutePath.Should().Be("/predict");
    }

    [Fact]
    public async Task PredictAsync_Fm_SendsFrequencyAndOmitsChannel()
    {
        ConfigureSettlement();
        ConfigureBaseUrl(BaseUrl);
        ConfigureHttp(HttpStatusCode.OK,
            "{\"technology\":\"FM\",\"field_dbuv_m\":10,\"model_version\":\"v1\",\"features\":{}}");

        var dto = FmRequest();
        dto.ChannelNumber = 30; // should be ignored for FM
        await CreateSut().PredictAsync(dto);

        _handler!.LastRequestBody.Should().Contain("\"frequency_mhz\":99.5");
        _handler.LastRequestBody.Should().Contain("\"channel_number\":null");
    }

    [Fact]
    public async Task PredictAsync_DigitalTv_SendsChannelAndOmitsFrequency()
    {
        ConfigureSettlement();
        ConfigureBaseUrl(BaseUrl);
        ConfigureHttp(HttpStatusCode.OK,
            "{\"technology\":\"DIGITAL_TV\",\"field_dbuv_m\":10,\"model_version\":\"v1\",\"features\":{}}");

        var dto = FmRequest();
        dto.Technology = Technology.DIGITAL_TV;
        dto.ChannelNumber = 30;
        dto.FrequencyMHz = 99.5f; // should be ignored for DIGITAL_TV
        await CreateSut().PredictAsync(dto);

        _handler!.LastRequestBody.Should().Contain("\"channel_number\":30");
        _handler.LastRequestBody.Should().Contain("\"frequency_mhz\":null");
    }
}
