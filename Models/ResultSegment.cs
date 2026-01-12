namespace Interfazxn.Models
{
    /// <summary>
    /// Segmento Result (R) - Resultado individual de prueba
    /// Este es el segmento más importante que contiene los valores obtenidos
    /// </summary>
    public class ResultSegment
    {
        public int SequenceNumber { get; set; } // 1, 2, 3...
        public string TestCode { get; set; } // 767, 687, 418, etc. (código interno del equipo)
        public string TestName { get; set; } // Nombre de la prueba (se mapea según código)
        public string Result { get; set; } // Valor del resultado: 50.32, 68.6, 29.19
        public string Units { get; set; } // Unidades: mg/dL, U/L
        public string ReferenceRange { get; set; } // Rango de referencia
        public string ResultStatus { get; set; } // N = Normal, A = Abnormal
        public string NormalizationFactor { get; set; }
        public string ControlId { get; set; } // $SYS$ o cobas
        public string ResultTimestamp { get; set; } // 20210930102739
        public string Operator { get; set; } // Usuario que ejecutó la prueba

        // Propiedades calculadas
        public bool IsNormal => ResultStatus == "N";
        public bool IsCritical => ResultStatus == "C";

        public override string ToString()
        {
            return $"Prueba #{SequenceNumber}: {TestName} ({TestCode}) = {Result} {Units} [{ResultStatus}]";
        }
    }
}
