namespace Interfazxn.Models
{
    /// <summary>
    /// Segmento Comment (C) - Información adicional o notas sobre un resultado
    /// </summary>
    public class CommentSegment
    {
        public int SequenceNumber { get; set; } // Secuencia del resultado al que pertenece
        public string CommentType { get; set; } // I = Information, W = Warning, E = Error
        public string CommentText { get; set; } // Texto del comentario
        public string QCCode { get; set; } // 111^? QC (información QC si aplica)

        public override string ToString()
        {
            return $"Comentario [{CommentType}]: {CommentText}";
        }
    }
}
