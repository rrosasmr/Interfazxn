namespace Interfazxn.Models
{
    /// <summary>
    /// Segmento Terminator (L) - Marca el final de la trama ASTM
    /// </summary>
    public class TerminatorSegment
    {
        public int SequenceNumber { get; set; } // 1
        public string TerminationCode { get; set; } = "N"; // N = Normal, L = Last

        public override string ToString()
        {
            return $"Fin de trama [{TerminationCode}]";
        }
    }
}