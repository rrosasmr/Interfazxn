namespace Interfazxn.Models
{
    /// <summary>
    /// Segmento Order (O) - Información de la orden de prueba
    /// </summary>
    public class OrderSegment
    {
        public int SequenceNumber { get; set; } // 1
        public string SpecimenId { get; set; } // 84509300023
        public string TypeOfSample { get; set; } // Tipo de muestra
        public string PriorityOrder { get; set; } // Prioridad (R = Routine)
        public string RequestedDateTime { get; set; } // 20210930123346
        public string SpecimenCollectionDateTime { get; set; }
        public string ReportDateTime { get; set; }
        public string ReportType { get; set; } // F = Final

        public override string ToString()
        {
            return $"Orden: {SpecimenId} | Muestra: {TypeOfSample} | Prioridad: {PriorityOrder}";
        }
    }
}
