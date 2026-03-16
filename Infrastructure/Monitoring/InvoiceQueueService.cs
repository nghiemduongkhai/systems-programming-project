using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;
using InventoryKPI.Application.Processing;
using InventoryKPI.Common;
using InventoryKPI.Infrastructure.FileSystem;
using InventoryKPI.Infrastructure.Parsing;

namespace InventoryKPI.Infrastructure.Monitoring
{
    public class InvoiceQueueService
    {
        private readonly Channel<string> _channel;

        private readonly JsonParser _parser;
        private readonly FileService _fileService;
        private readonly InvoiceProcessor _processor;

        private readonly CancellationTokenSource _cts = new();

        private readonly int _workerCount;

        private int _activeWorkers = 0;

        public InvoiceQueueService(
            JsonParser parser,
            FileService fileService,
            InvoiceProcessor processor)
        {
            _parser = parser;
            _fileService = fileService;
            _processor = processor;

            _workerCount = Environment.ProcessorCount;

            _channel = Channel.CreateBounded<string>(
                new BoundedChannelOptions(1000)
                {
                    SingleReader = false,
                    SingleWriter = false,
                    FullMode = BoundedChannelFullMode.Wait
                });

            StartWorkers();
        }

        public async Task Enqueue(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            await _channel.Writer.WriteAsync(filePath);

            Logger.Info($"Queued invoice file: {filePath}");
        }

        private void StartWorkers()
        {
            for (int i = 0; i < _workerCount; i++)
            {
                Task.Run(async () =>
                {
                    Logger.Info($"Worker started (Thread {Thread.CurrentThread.ManagedThreadId})");

                    try
                    {
                        await foreach (var file in _channel.Reader.ReadAllAsync(_cts.Token))
                        {
                            ProcessFile(file);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }

                    Logger.Info($"Worker stopped (Thread {Thread.CurrentThread.ManagedThreadId})");

                }, _cts.Token);
            }
        }

        private void ProcessFile(string file)
        {
            Interlocked.Increment(ref _activeWorkers);

            try
            {
                if (!File.Exists(file))
                {
                    Logger.Warning($"File already processed or missing: {file}");
                    return;
                }

                Logger.Info($"Processing file: {file}");

                var invoices = _parser.ParseInvoices(file);

                if (invoices == null || invoices.Count == 0)
                {
                    Logger.Info($"{file} has no valid ItemCode");
                    _fileService.MoveToProcessed(file);
                    return;
                }

                _processor.Process(invoices);

                _fileService.MoveToProcessed(file);

                Logger.Info($"Completed file: {file}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Processing error for {file}: {ex.Message}");
            }
            finally
            {
                Interlocked.Decrement(ref _activeWorkers);
            }
        }

        public async Task WaitForCompletion()
        {
            while (_channel.Reader.Count > 0 || _activeWorkers > 0)
            {
                await Task.Delay(200);
            }
        }

        public void Stop()
        {
            Logger.Info("Stopping invoice workers...");

            _channel.Writer.Complete();
            _cts.Cancel();
        }
    }
}