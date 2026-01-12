using System;
using System.Collections.Generic;

namespace Interfazxn.Models
{
    /// <summary>
    /// Representa una trama ASTM completa del Cobas c 111
    /// Contiene todos los segmentos procesados
    /// </summary>
    public class AstmMessage
    {
        public int MessageId { get; set; }
        public string RawMessage { get; set; } // Trama cruda recibida
        
        // Segmentos
        public HeaderSegment Header { get; set; }
        public PatientSegment Patient { get; set; }
        public OrderSegment Order { get; set; }
        public QuerySegment Query { get; set; }
        public List<ResultSegment> Results { get; set; } = new List<ResultSegment>();
        public List<CommentSegment> Comments { get; set; } = new List<CommentSegment>();
        public TerminatorSegment Terminator { get; set; }
        
        // Metadata
        public DateTime ReceivedDateTime { get; set; }
        public bool IsValid { get; set; }
        public string ValidationError { get; set; }
        public string MessageType { get; set; } // "HOST_QUERY" o "RESULTS"

        public int ResultCount => Results.Count;

        public override string ToString()
        {
            // Para HOST_QUERY, mostrar información de la consulta
            if (MessageType == "HOST_QUERY" && Query != null)
            {
                // Limpiar el SpecimenId (remover el ^ inicial)
                string orderId = Query.SpecimenId?.TrimStart('^') ?? "N/A";
                return $"ASTM [{MessageType}] - Orden: {orderId} | {ReceivedDateTime:yyyy-MM-dd HH:mm:ss}";
            }
            
            // Para RESULTS, mostrar cantidad de resultados
            return $"ASTM [{MessageType}] - {ResultCount} resultados | {ReceivedDateTime:yyyy-MM-dd HH:mm:ss}";
        }
    }
}