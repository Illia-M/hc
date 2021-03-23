using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HC.Domain;
using HC.Domain.Repositories;

namespace HC.ApplicationServices.History
{
    public class StatusWriter : IDisposable, ICheckHistoryRepository
    {
        private readonly object _lock = new object();
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly byte[] _endLineBytes = Encoding.UTF8.GetBytes("\n");
        private FileStream? _file;

        public StatusWriter()
        {
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
            };
        }

        public async Task Store(CheckStatus checkStatus, CancellationToken cancellationToken)
        {
            if (_file is null)
            {
                lock (_lock)
                {
                    _file ??= File.Open("StatusHistory.json", FileMode.Append, FileAccess.Write, FileShare.Read);
                }
            }

            await _file.WriteAsync(JsonSerializer.SerializeToUtf8Bytes(checkStatus, _jsonSerializerOptions).Concat(_endLineBytes).ToArray(), cancellationToken);
        }

        public void Dispose()
        {
            _file?.Dispose();
        }
    }
}