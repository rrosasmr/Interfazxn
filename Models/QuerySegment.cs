namespace Interfazxn.Models
{
    /// <summary>
    /// Representa un segmento de consulta (Q) del protocolo ASTM
    /// Utilizado en mensajes HOST-QUERY
    /// </summary>
    public class QuerySegment
    {
        public int SequenceNumber { get; set; }
        public string SpecimenId { get; set; }
        public string PatientId { get; set; }
        public string QueryRange { get; set; }      // ALL, etc.
        public string SampleType { get; set; }
        public string RequestInfo { get; set; }

        public override string ToString()
        {
            return $"Query - Specimen: {SpecimenId}, Range: {QueryRange}";
        }
    }
}