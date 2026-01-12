namespace Interfazxn.Models
{
    /// <summary>
    /// Segmento Header (H) - Primera línea de la trama ASTM
    /// Contiene información del equipo analizador
    /// </summary>
    public class HeaderSegment
    {
        public string FieldDelimiter { get; set; } = "|";
        public string ComponentDelimiter { get; set; } = "^";
        public string RepeatDelimiter { get; set; } = "\\";
        public string EscapeCharacter { get; set; } = "&";
        
        public string ManufacturerId { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public string VersionId { get; set; } = "";
        public string SoftwareVersion { get; set; } = "";
        public string SerialNumber { get; set; } = "";
        public string BatchNumber { get; set; } = "";
        
        public string MessageType { get; set; } = "";
        public string ProcessingId { get; set; } = "";
        public string VersionNumber { get; set; } = "";
        public string Timestamp { get; set; } = "";

        public override string ToString()
        {
            return $"Equipo: {Manufacturer} {ManufacturerId} | Versión: {SoftwareVersion} | Tipo: {MessageType} | Hora: {Timestamp}";
        }
    }
}