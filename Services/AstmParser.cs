using System;
using System.Collections.Generic;
using System.Linq;
using Interfazxn.Models;

namespace Interfazxn.Services
{
    /// <summary>
    /// Parser ASTM para procesar tramas del Cobas c 111
    /// Extrae y valida cada segmento (H, P, O, R, C, L, Q)
    /// </summary>
    public class AstmParser
    {
        private int _messageCounter = 0;

        // Mapeo de códigos de prueba del Cobas c 111 a nombres legibles
        private readonly Dictionary<string, string> _testCodeMap = new()
        {
            { "767", "Glucosa" },
            { "687", "ALT (SGPT)" },
            { "418", "AST (SGOT)" },
            { "685", "Fosfatasa Alcalina" },
            { "690", "Bilirrubina Total" },
            { "712", "Bilirrubina Directa" },
            { "734", "Albúmina" },
            { "780", "Urea" },
            { "790", "Creatinina" },
            { "800", "Colesterol Total" },
            { "810", "Triglicéridos" },
            { "820", "HDL" },
            { "830", "LDL" },
            { "840", "Proteínas Totales" },
            { "850", "Sodio" },
            { "860", "Potasio" },
            { "870", "Cloro" },
            { "880", "Calcio" },
            { "890", "Fósforo" }
        };

        /// <summary>
        /// Parsea una trama ASTM completa
        /// </summary>
        public AstmMessage ParseMessage(string rawMessage)
        {
            _messageCounter++;
            var message = new AstmMessage
            {
                MessageId = _messageCounter,
                RawMessage = rawMessage,
                ReceivedDateTime = DateTime.Now,
                IsValid = true
            };

            try
            {
                // Dividir por saltos de línea
                var lines = rawMessage.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length == 0)
                {
                    throw new Exception("Trama vacía");
                }

                // Procesar cada línea
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var segmentType = line[0].ToString();
                    
                    switch (segmentType)
                    {
                        case "H":
                            message.Header = ParseHeaderSegment(line);
                            message.MessageType = DetermineMessageType(message.Header.MessageType);
                            break;
                        case "P":
                            message.Patient = ParsePatientSegment(line);
                            break;
                        case "O":
                            message.Order = ParseOrderSegment(line);
                            break;
                        case "R":
                            message.Results.Add(ParseResultSegment(line));
                            break;
                        case "C":
                            message.Comments.Add(ParseCommentSegment(line));
                            break;
                        case "L":
                            message.Terminator = ParseTerminatorSegment(line);
                            break;
                        case "Q":
                            message.Query = ParseQuerySegment(line);
                            break;
                    }
                }

                // Validaciones básicas
                if (message.Header == null)
                {
                    throw new Exception("Falta segmento Header (H)");
                }

                if (message.Terminator == null)
                {
                    throw new Exception("Falta segmento Terminator (L)");
                }

                message.IsValid = true;
            }
            catch (Exception ex)
            {
                message.IsValid = false;
                message.ValidationError = ex.Message;
            }

            return message;
        }

        private HeaderSegment ParseHeaderSegment(string line)
        {
            var fields = line.Split('|');
            
            // Parsear componentes del campo de información del equipo
            var equipoInfo = fields.Length > 3 ? fields[3].Split('^') : new string[] { };
            var messageTypeInfo = fields.Length > 10 ? fields[10].Split('^') : new string[] { };

            return new HeaderSegment
            {
                ManufacturerId = equipoInfo.Length > 0 ? equipoInfo[0] : "",
                Manufacturer = equipoInfo.Length > 1 ? equipoInfo[1] : "",
                VersionId = equipoInfo.Length > 2 ? equipoInfo[2] : "",
                SoftwareVersion = equipoInfo.Length > 3 ? equipoInfo[3] : "",
                SerialNumber = equipoInfo.Length > 4 ? equipoInfo[4] : "",
                BatchNumber = equipoInfo.Length > 5 ? equipoInfo[5] : "",
                MessageType = messageTypeInfo.Length > 0 ? messageTypeInfo[0] : "",
                ProcessingId = fields.Length > 11 ? fields[11] : "",
                VersionNumber = fields.Length > 12 ? fields[12] : "",
                Timestamp = fields.Length > 13 ? fields[13] : ""
            };
        }

        private PatientSegment ParsePatientSegment(string line)
        {
            var fields = line.Split('|');
            
            return new PatientSegment
            {
                SequenceNumber = int.TryParse(fields[1], out var seq) ? seq : 0,
                PatientId = fields.Length > 2 ? fields[2] : "",
                PatientName = fields.Length > 5 ? fields[5] : "",
                DateOfBirth = fields.Length > 7 ? fields[7] : "",
                PatientGender = fields.Length > 8 ? fields[8] : "",
                PatientAge = fields.Length > 10 ? fields[10] : ""
            };
        }

        private OrderSegment ParseOrderSegment(string line)
        {
            var fields = line.Split('|');
            
            return new OrderSegment
            {
                SequenceNumber = int.TryParse(fields[1], out var seq) ? seq : 0,
                SpecimenId = fields.Length > 2 ? fields[2] : "",
                TypeOfSample = fields.Length > 5 ? fields[5] : "",
                PriorityOrder = fields.Length > 6 ? fields[6] : "R",
                RequestedDateTime = fields.Length > 6 ? fields[6] : "",
                ReportDateTime = fields.Length > 11 ? fields[11] : "",
                ReportType = fields.Length > 14 ? fields[14] : ""
            };
        }

        private ResultSegment ParseResultSegment(string line)
        {
            var fields = line.Split('|');
            var testCodeComponent = fields.Length > 2 ? fields[2].Split('^') : new string[] { };
            
            // 📝 LOG: Escribir en archivo
            string logPath = "astm_debug.txt";
            string logMessage = $"\n[{DateTime.Now:HH:mm:ss.fff}] Parsing ResultSegment\n";
            logMessage += $"  Línea completa: {line}\n";
            logMessage += $"  fields.Length: {fields.Length}\n";
            if (fields.Length > 2)
                logMessage += $"  fields[2]: '{fields[2]}'\n";
            logMessage += $"  testCodeComponent.Length: {testCodeComponent.Length}\n";
            for (int i = 0; i < testCodeComponent.Length; i++)
            {
                logMessage += $"  testCodeComponent[{i}]: '{testCodeComponent[i]}'\n";
            }
            
            // ✅ CORREGIDO: Usar fields[2] en lugar de fields[3]
            string testCode = "";
            if (testCodeComponent.Length > 3)
            {
                testCode = testCodeComponent[3].Trim();
                logMessage += $"  Antes Regex: '{testCode}'\n";
                // Remover caracteres de control adicionales
                testCode = System.Text.RegularExpressions.Regex.Replace(testCode, @"[^\d]", "");
                logMessage += $"  Después Regex: '{testCode}'\n";
            }
            else
            {
                logMessage += $"  ✓ Usando índice correcto: fields[2] dividido por '^'\n";
                logMessage += $"  ⚠️ testCodeComponent.Length ({testCodeComponent.Length}) <= 3\n";
            }
            
            var testName = GetTestName(testCode);
            logMessage += $"  testCode final: '{testCode}' -> testName: '{testName}'\n";
            
            // Extraer valores
            string result = fields.Length > 3 ? fields[3]?.Trim() ?? "" : "";
            string units = fields.Length > 4 ? fields[4]?.Trim() ?? "" : "";
            string referenceRange = fields.Length > 5 ? fields[5]?.Trim() ?? "" : "";
            string resultStatus = fields.Length > 6 ? fields[6]?.Trim() ?? "N" : "N";
            
            logMessage += $"  Result: '{result}'\n";
            logMessage += $"  Units: '{units}'\n";
            logMessage += $"  ReferenceRange: '{referenceRange}'\n";
            logMessage += $"  ResultStatus: '{resultStatus}'\n";
            
            // Escribir en archivo
            try
            {
                System.IO.File.AppendAllText(logPath, logMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error escribiendo log: {ex.Message}");
            }

            return new ResultSegment
            {
                SequenceNumber = int.TryParse(fields[1], out var seq) ? seq : 0,
                TestCode = testCode,
                TestName = testName,
                Result = result,
                Units = units,
                ReferenceRange = referenceRange,
                ResultStatus = resultStatus,
                NormalizationFactor = fields.Length > 7 ? fields[7]?.Trim() ?? "" : "",
                ControlId = fields.Length > 9 ? fields[9]?.Trim() ?? "" : "",
                ResultTimestamp = fields.Length > 11 ? fields[11]?.Trim() ?? "" : "",
                Operator = fields.Length > 13 ? fields[13]?.Trim() ?? "" : ""
            };
        }

        private CommentSegment ParseCommentSegment(string line)
        {
            var fields = line.Split('|');
            
            return new CommentSegment
            {
                SequenceNumber = int.TryParse(fields[1], out var seq) ? seq : 0,
                CommentType = fields.Length > 2 ? fields[2] : "I",
                CommentText = fields.Length > 3 ? fields[3] : "",
                QCCode = fields.Length > 4 ? fields[4] : ""
            };
        }

        private TerminatorSegment ParseTerminatorSegment(string line)
        {
            var fields = line.Split('|');
            
            return new TerminatorSegment
            {
                SequenceNumber = int.TryParse(fields[1], out var seq) ? seq : 0,
                TerminationCode = fields.Length > 2 ? fields[2] : "N"
            };
        }

        private QuerySegment ParseQuerySegment(string line)
        {
            var fields = line.Split('|');
            var specimenInfo = fields.Length > 2 ? fields[2].Split('^') : new string[] { };
            
            return new QuerySegment
            {
                SequenceNumber = int.TryParse(fields[1], out var seq) ? seq : 0,
                SpecimenId = specimenInfo.Length > 1 ? specimenInfo[1]?.Trim() ?? "" : "",
                PatientId = fields.Length > 3 ? fields[3]?.Trim() ?? "" : "",
                QueryRange = fields.Length > 4 ? fields[4]?.Trim() ?? "" : "",
                SampleType = fields.Length > 5 ? fields[5]?.Trim() ?? "" : "",
                RequestInfo = fields.Length > 6 ? fields[6]?.Trim() ?? "" : ""
            };
        }

        private string GetTestName(string testCode)
        {
            return _testCodeMap.TryGetValue(testCode, out var name) ? name : $"Prueba {testCode}";
        }

        private string DetermineMessageType(string messageTypeField)
        {
            return messageTypeField.Contains("TSREQ") ? "HOST_QUERY" : "RESULTS";
        }
    }
}