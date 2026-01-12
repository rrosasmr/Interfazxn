using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Interfazxn.Services
{
    /// <summary>
    /// Servicio para comunicación serial con el Cobas c 111
    /// Maneja apertura, lectura y cierre del puerto serial
    /// </summary>
    public class SerialCommunicationService : IDisposable
    {
        private SerialPort _serialPort;
        private StringBuilder _receiveBuffer;
        private bool _isConnected;
        private CancellationTokenSource _cancellationTokenSource;

        // Eventos
        public event EventHandler<string> DataReceived;
        public event EventHandler<string> ConnectionStatusChanged;
        public event EventHandler<string> ErrorOccurred;

        // Constantes ASTM
        private const byte STX = 0x02; // Start of Text
        private const byte ETX = 0x03; // End of Text
        private const byte CR = 0x0D;  // Carriage Return
        private const byte LF = 0x0A;  // Line Feed

        public SerialCommunicationService()
        {
            _receiveBuffer = new StringBuilder();
            _isConnected = false;
        }

        /// <summary>
        /// Abre conexión con el puerto serial
        /// </summary>
        public bool ConnectToPort(string portName, int baudRate = 9600, 
            Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    DisconnectFromPort();
                }

                _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits)
                {
                    Handshake = Handshake.None,
                    ReadTimeout = 1000,
                    WriteTimeout = 1000,
                    RtsEnable = true,
                    DtrEnable = true
                };

                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.ErrorReceived += SerialPort_ErrorReceived;

                _serialPort.Open();
                
                if (_serialPort.IsOpen)
                {
                    _isConnected = true;
                    _cancellationTokenSource = new CancellationTokenSource();
                    ConnectionStatusChanged?.Invoke(this, $"Conectado a {portName}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _isConnected = false;
                ErrorOccurred?.Invoke(this, $"Error al conectar: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cierra la conexión serial
        /// </summary>
        public void DisconnectFromPort()
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.Close();
                    _serialPort.Dispose();
                    _serialPort = null;
                    _isConnected = false;
                    ConnectionStatusChanged?.Invoke(this, "Desconectado");
                }

                _cancellationTokenSource?.Cancel();
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Error al desconectar: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene lista de puertos disponibles en el sistema
        /// </summary>
        public static string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }

        /// <summary>
        /// Verifica si está conectado
        /// </summary>
        public bool IsConnected => _isConnected && _serialPort?.IsOpen == true;

        /// <summary>
        /// Envía datos al puerto serial (para enviar comandos al Cobas)
        /// </summary>
        public bool SendData(string data)
        {
            try
            {
                if (!IsConnected)
                {
                    ErrorOccurred?.Invoke(this, "Puerto no conectado");
                    return false;
                }

                _serialPort.Write(data);
                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Error al enviar: {ex.Message}");
                return false;
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (!_serialPort.IsOpen) return;

                int bytesCount = _serialPort.BytesToRead;
                byte[] buffer = new byte[bytesCount];
                _serialPort.Read(buffer, 0, bytesCount);

                // Procesar bytes recibidos
                foreach (byte b in buffer)
                {
                    if (b == STX) // Inicio de trama
                    {
                        _receiveBuffer.Clear();
                    }
                    else if (b == ETX) // Fin de trama
                    {
                        string completeMessage = _receiveBuffer.ToString();
                        if (!string.IsNullOrWhiteSpace(completeMessage))
                        {
                            DataReceived?.Invoke(this, completeMessage);
                        }
                        _receiveBuffer.Clear();
                    }
                    else
                    {
                        // ✅ CORREGIDO: Mantener TODOS los bytes, incluyendo CR (0x0D) y LF (0x0A)
                        // Los delimitadores CR/LF son esenciales para que AstmParser pueda dividir las líneas
                        _receiveBuffer.Append((char)b);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Error al procesar datos: {ex.Message}");
            }
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            ErrorOccurred?.Invoke(this, $"Error serial: {e.EventType}");
        }

        public void Dispose()
        {
            DisconnectFromPort();
            _serialPort?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }
}