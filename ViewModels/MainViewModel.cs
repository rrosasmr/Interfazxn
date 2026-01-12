using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using Interfazxn.Models;
using Interfazxn.Services;
using ReactiveUI;

namespace Interfazxn.ViewModels
{
    /// <summary>
    /// ViewModel principal de la aplicación
    /// Maneja comunicación serial, parsing ASTM, logging y actualizaciones de UI
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly SerialCommunicationService _serialService;
        private readonly AstmParser _astmParser;
        private readonly LogService _logService;

        // Propiedades de conexión
        private bool _isConnected;
        private string? _selectedPort;
        private ObservableCollection<string> _availablePorts;
        private string _connectionStatus;

        // Propiedades de datos
        private ObservableCollection<AstmMessage> _receivedMessages;
        private ObservableCollection<ResultSegment> _lastResults;
        private AstmMessage? _selectedMessage;

        // Propiedades de UI
        private string _statusMessage;
        private bool _isLoading;

        public MainViewModel()
        {
            _serialService = new SerialCommunicationService();
            _astmParser = new AstmParser();
            _logService = new LogService();

            _availablePorts = new ObservableCollection<string>(SerialCommunicationService.GetAvailablePorts());
            _receivedMessages = new ObservableCollection<AstmMessage>();
            _lastResults = new ObservableCollection<ResultSegment>();

            _connectionStatus = "Desconectado";
            _statusMessage = "Selecciona un puerto para conectar";

            InitializeCommands();
            InitializeEventHandlers();
        }

        // === PROPIEDADES ===

        public bool IsConnected
        {
            get => _isConnected;
            set => this.RaiseAndSetIfChanged(ref _isConnected, value);
        }

        public string? SelectedPort
        {
            get => _selectedPort;
            set => this.RaiseAndSetIfChanged(ref _selectedPort, value);
        }

        public ObservableCollection<string> AvailablePorts
        {
            get => _availablePorts;
            set => this.RaiseAndSetIfChanged(ref _availablePorts, value);
        }

        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => this.RaiseAndSetIfChanged(ref _connectionStatus, value);
        }

        public ObservableCollection<AstmMessage> ReceivedMessages
        {
            get => _receivedMessages;
            set => this.RaiseAndSetIfChanged(ref _receivedMessages, value);
        }

        public ObservableCollection<ResultSegment> LastResults
        {
            get => _lastResults;
            set => this.RaiseAndSetIfChanged(ref _lastResults, value);
        }

        public AstmMessage? SelectedMessage
        {
            get => _selectedMessage;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedMessage, value);
                if (value != null)
                {
                    UpdateLastResults(value);
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public ObservableCollection<LogEntry> Logs => _logService.Logs;

        // === COMANDOS ===

        public ReactiveCommand<Unit, Unit>? ConnectCommand { get; private set; }
        public ReactiveCommand<Unit, Unit>? DisconnectCommand { get; private set; }
        public ReactiveCommand<Unit, Unit>? RefreshPortsCommand { get; private set; }
        public ReactiveCommand<Unit, Unit>? ClearLogsCommand { get; private set; }

        // === INICIALIZACIÓN ===

        private void InitializeCommands()
        {
            ConnectCommand = ReactiveCommand.Create(() =>
            {
                if (string.IsNullOrEmpty(SelectedPort))
                {
                    StatusMessage = "Selecciona un puerto válido";
                    _logService.LogWarning("No se seleccionó puerto", "CONEXION");
                    return;
                }

                IsLoading = true;
                if (_serialService.ConnectToPort(SelectedPort))
                {
                    IsConnected = true;
                    StatusMessage = $"Conectado a {SelectedPort}";
                    _logService.LogSuccess($"Conectado a {SelectedPort}", "CONEXION");
                }
                else
                {
                    StatusMessage = "Error al conectar al puerto";
                    _logService.LogError($"Error al conectar a {SelectedPort}", "CONEXION");
                }
                IsLoading = false;
            });

            DisconnectCommand = ReactiveCommand.Create(() =>
            {
                _serialService.DisconnectFromPort();
                IsConnected = false;
                StatusMessage = "Desconectado";
                _logService.LogInfo("Desconectado del puerto", "CONEXION");
            });

            RefreshPortsCommand = ReactiveCommand.Create(() =>
            {
                AvailablePorts.Clear();
                foreach (var port in SerialCommunicationService.GetAvailablePorts())
                {
                    AvailablePorts.Add(port);
                }
                _logService.LogInfo("Puertos actualizados", "SISTEMA");
            });

            ClearLogsCommand = ReactiveCommand.Create(() =>
            {
                _logService.ClearLogs();
                StatusMessage = "Logs limpiados";
            });
        }

        private void InitializeEventHandlers()
        {
            _serialService.DataReceived += SerialService_DataReceived;
            _serialService.ConnectionStatusChanged += SerialService_ConnectionStatusChanged;
            _serialService.ErrorOccurred += SerialService_ErrorOccurred;
        }

        // === MANEJADORES DE EVENTOS ===

        private void SerialService_DataReceived(object? sender, string rawData)
        {
            try
            {
                var astmMessage = _astmParser.ParseMessage(rawData);

                if (astmMessage.IsValid)
                {
                    ReceivedMessages.Insert(0, astmMessage);
                    SelectedMessage = astmMessage;

                    string messageInfo = $"{astmMessage.MessageType} - {astmMessage.ResultCount} resultados";
                    _logService.LogSuccess(messageInfo, "ASTM");

                    StatusMessage = $"Último mensaje: {astmMessage.MessageType} ({astmMessage.ResultCount} resultados)";
                }
                else
                {
                    _logService.LogError($"Trama inválida: {astmMessage.ValidationError}", "ASTM");
                    StatusMessage = $"Error: {astmMessage.ValidationError}";
                }

                // Limitar número de mensajes en memoria
                while (ReceivedMessages.Count > 100)
                {
                    ReceivedMessages.RemoveAt(ReceivedMessages.Count - 1);
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error procesando trama: {ex.Message}", "ASTM");
                StatusMessage = "Error procesando trama ASTM";
            }
        }

        private void SerialService_ConnectionStatusChanged(object? sender, string status)
        {
            ConnectionStatus = status;
            _logService.LogInfo(status, "CONEXION");
        }

        private void SerialService_ErrorOccurred(object? sender, string error)
        {
            _logService.LogError(error, "ERROR");
            StatusMessage = error;
            IsConnected = false;
        }

        // === MÉTODOS PRIVADOS ===

        private void UpdateLastResults(AstmMessage message)
        {
            LastResults.Clear();
            foreach (var result in message.Results)
            {
                LastResults.Add(result);
            }
        }
    }
}