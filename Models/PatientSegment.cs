namespace Interfazxn.Models
{
    /// <summary>
    /// Segmento Patient (P) - Información del paciente
    /// </summary>
    public class PatientSegment
    {
        public int SequenceNumber { get; set; } // 1
        public string PatientId { get; set; } // ID del paciente
        public string PatientName { get; set; } // Nombre del paciente
        public string DateOfBirth { get; set; } // Fecha de nacimiento
        public string PatientGender { get; set; } // Sexo
        public string PatientAge { get; set; } // Edad

        public override string ToString()
        {
            return $"Paciente: {PatientName} (ID: {PatientId})";
        }
    }
}
