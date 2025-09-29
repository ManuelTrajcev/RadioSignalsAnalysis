using System.Collections.Generic;

using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Domain_Models;
using Domain.DTO;
using Domain.Enums;
using Domain.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repository.Interface;
using Services.Interface;

namespace Services.Implementation;

public class PredictionService : IPredictionService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IRepository<Settlement> _settlements;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonOptions;

    public PredictionService(IHttpClientFactory httpClientFactory, IRepository<Settlement> settlements, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _settlements = settlements;
        _configuration = configuration;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<PredictionResponseDto> PredictAsync(PredictionRequestDto dto)
    {
        if (dto.Technology == Technology.DIGITAL_TV && dto.ChannelNumber is null)
        {
            throw new ArgumentException("ChannelNumber is required for DIGITAL_TV predictions", nameof(dto.ChannelNumber));
        }
        if (dto.Technology == Technology.FM && dto.FrequencyMHz is null)
        {
            throw new ArgumentException("FrequencyMHz is required for FM predictions", nameof(dto.FrequencyMHz));
        }

        var settlement = await _settlements.GetAsync(
            selector: s => s,
            predicate: s => s.Id == dto.SettlementId,
            include: q => q.Include(x => x.Municipality))
            ?? throw new ArgumentException("Settlement not found", nameof(dto.SettlementId));

        var client = _httpClientFactory.CreateClient("PredictionService");
        var baseUrl = _configuration["PredictionService:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("PredictionService:BaseUrl configuration is missing");
        }
        client.BaseAddress ??= new Uri(baseUrl, UriKind.Absolute);

        // Compose payload with canonical feature names expected by the Python pipeline.

        var payload = new
        {
            technology = dto.Technology.ToString(),
            date = dto.Date,
            latitude = CoordinateHelper.ToDecimal(dto.LatitudeDegrees, dto.LatitudeMinutes, dto.LatitudeSeconds),
            longitude = CoordinateHelper.ToDecimal(dto.LongitudeDegrees, dto.LongitudeMinutes, dto.LongitudeSeconds),
            elevation_m = dto.AltitudeMeters,
            population = settlement.Population ?? 0,
            households = settlement.Households ?? 0,
            registry_number = settlement.RegistryNumber,
            municipality = settlement.Municipality?.Name,
            settlement = settlement.Name,
            program_identifier = dto.ProgramIdentifier,
            transmitter_location = dto.TransmitterLocation,
            channel_number = dto.Technology == Technology.DIGITAL_TV ? dto.ChannelNumber : null,
            frequency_mhz = dto.Technology == Technology.FM ? dto.FrequencyMHz : null
        };

        var content = new StringContent(JsonSerializer.Serialize(payload, _jsonOptions), Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/predict", content);
        Console.WriteLine(response);
        var body = await response.Content.ReadAsStringAsync();
        Console.WriteLine(body);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Prediction service error {(int)response.StatusCode}: {body}");
        }

        var serviceResponse = JsonSerializer.Deserialize<PythonPredictionResponse>(body, _jsonOptions)
            ?? throw new InvalidOperationException("Prediction service returned an empty response");

        Console.WriteLine(serviceResponse.FieldDbuvM);
        // model_version allows UI to surface pipeline provenance during roll-outs.
        return new PredictionResponseDto
        {
            Technology = dto.Technology,
            FieldDbuvPerM = serviceResponse.FieldDbuvM,
            ModelVersion = serviceResponse.ModelVersion,
            Features = serviceResponse.Features ?? new Dictionary<string, object>()
        };    }

    private sealed class PythonPredictionResponse
    {
        public string Technology { get; set; } = string.Empty;

        [JsonPropertyName("field_dbuv_m")]
        public double FieldDbuvM { get; set; }

        [JsonPropertyName("model_version")]
        public string ModelVersion { get; set; } = string.Empty;

        public Dictionary<string, object>? Features { get; set; }
    }
}



