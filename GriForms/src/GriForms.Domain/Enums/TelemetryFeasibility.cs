namespace GriForms.Domain.Enums;

// Kelayakan otomasi pengisian lewat telemetry: Manual diisi orang, Measured bisa diukur sensor/meter,
// Derived dihitung dari item lain. Wajib ditetapkan untuk setiap pertanyaan numerik sebelum versi form terbit.
public enum TelemetryFeasibility { Manual, Measured, Derived }
