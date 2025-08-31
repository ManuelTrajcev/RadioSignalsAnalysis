namespace Domain.Enums;

public enum MeasurementStatus
{
    ExemptFromFee,            // ослободени од р.д. такса
    Covered,                  // Покриени
    PartiallyCovered,         // Делумно покриени
    NotCovered,               // Непокриени
    TheoreticallyCovered,     // ** Покриени (теоретски)
    NoResidents2002,          // Без постојани жители според попис 2002
    RadioSignal,              // (ф.м.) - радио сигнал
    DigitalSignal,            // (д) - Дигитален сигнал
    NoDigitalDevices,         // нема дигитални уреди
    Renamed,                  // преименување
    DeviceIssues,             // проблеми со уреди
    Uninhabited               // ненаселен дел
}