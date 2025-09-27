using System.Net.Http.Json;
using Domain.DTO;
using Domain.Domain_Models;
using Microsoft.EntityFrameworkCore;
using Repository.Interface;
using Services.Interface;
using Microsoft.Extensions.Configuration;

namespace Services.Implementation;

// Bridges backend DTOs to the Python FastAPI service.
// - Maps DMS -> decimal coordinates
// - Enriches with settlement + municipality names and demography
// - Sends normalized feature payload expected by Python /predict
public class PythonPredictionService : IPredictionService
{
    private readonly HttpClient _http;
    private readonly IRepository<Settlement> _settlements;

    public PythonPredictionService(HttpClient http, IRepository<Settlement> settlements, IConfiguration cfg)
    {
        _http = http;
        _settlements = settlements;
        // Configure base URL from env/config. Example: http://localhost:8000
        var baseUrl = Environment.GetEnvironmentVariable("PYTHON_SERVICE_URL")
                      ?? cfg.GetValue<string>("PythonService:BaseUrl")
                      ?? "http://localhost:8000";
        if (_http.BaseAddress == null)
        {
            _http.BaseAddress = new Uri(baseUrl);
        }
    }

    public async Task<PredictionResponseDto> PredictAsync(PredictionDto dto)
    {
        var s = await _settlements.GetAsync(
            x => x,
            x => x.Id == dto.SettlementId,
            include: q => q.Include(se => se.Municipality));

        if (s == null) throw new ArgumentException("Settlement not found", nameof(dto.SettlementId));

        var lat = ToDecimal(dto.LatitudeDegrees, dto.LatitudeMinutes, dto.LatitudeSeconds);
        var lon = ToDecimal(dto.LongitudeDegrees, dto.LongitudeMinutes, dto.LongitudeSeconds);

        var tech = dto.Technology == Domain.Enums.Technology.DIGITAL_TV ? "DIGITAL_TV" : "FM";

        var payload = new
        {
            technology = tech,
            municipality = s.Municipality?.Name ?? string.Empty,
            settlement = s.Name,
            program_id = dto.ProgramIdentifier,
            emitter = dto.TransmitterLocation,
            latitude = lat,
            longitude = lon,
            elevation_m = dto.AltitudeMeters,
            year = dto.Date.Year,
            month = dto.Date.Month,
            population = s.Population,
            households = s.Households,
            tv_channel = dto.Technology == Domain.Enums.Technology.DIGITAL_TV ? dto.ChannelNumber : null,
            fm_freq_mhz = dto.Technology == Domain.Enums.Technology.FM ? dto.FrequencyMHz : null
        };

        using var res = await _http.PostAsJsonAsync("/predict", payload);
        if (!res.IsSuccessStatusCode)
        {
            var err = await res.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Python service error: {(int)res.StatusCode} {res.ReasonPhrase} - {err}");
        }

        var doc = await res.Content.ReadFromJsonAsync<PythonPredictResponse>();
        if (doc == null) throw new InvalidOperationException("Invalid response from Python service.");

        return new PredictionResponseDto
        {
            ElectricFieldDbuvPerM = (float)doc.field_dbuv_m,
            Technology = tech,
            Model = doc.used_model
        };
    }

    private static double ToDecimal(int deg, int min, float sec)
        => deg + (min / 60.0) + (sec / 3600.0);

    private class PythonPredictResponse
    {
        public double field_dbuv_m { get; set; }
        public string tech { get; set; }
        public string used_model { get; set; }
    }
}
