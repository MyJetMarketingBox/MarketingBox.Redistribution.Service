using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MarketingBox.Redistribution.Service.Domain.Models;
using MarketingBox.Redistribution.Service.Grpc;
using MarketingBox.Redistribution.Service.Grpc.Models;
using MarketingBox.Redistribution.Service.Storage;
using MarketingBox.Sdk.Common.Extensions;
using MarketingBox.Sdk.Common.Models.Grpc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MarketingBox.Redistribution.Service.Services
{
    public class RegistrationImporter : IRegistrationImporter
    {
        private readonly ILogger<RegistrationImporter> _logger;
        private readonly FileStorage _fileStorage;

        public RegistrationImporter(ILogger<RegistrationImporter> logger, FileStorage fileStorage)
        {
            _logger = logger;
            _fileStorage = fileStorage;
        }

        public async Task<Response<ImportResponse>> ImportAsync(ImportRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "RegistrationImporter.ImportAsync receive request {@Request}",request);
                
                var registrationsFile = new RegistrationsFile()
                {
                    CreatedAt = DateTime.UtcNow,
                    FileName = request.FileName,
                    CreatedBy = request.UserId,
                    File = request.RegistrationsFile,
                    TenantId = request.TenantId
                };

                await _fileStorage.Save(registrationsFile);

                return new Response<ImportResponse>()
                {
                    Status = ResponseStatus.Ok,
                    Data = new ImportResponse()
                    {
                        FileId = registrationsFile.Id
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ex.FailedResponse<ImportResponse>();
            }
        }

        public async Task<Response<IReadOnlyCollection<RegistrationsFile>>> GetRegistrationFilesAsync(
            GetFilesRequest request)
        {
            try
            {
                var (files, total) = await _fileStorage.Search(request);

                return new Response<IReadOnlyCollection<RegistrationsFile>>()
                {
                    Status = ResponseStatus.Ok,
                    Data = files,
                    Total = total
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ex.FailedResponse<IReadOnlyCollection<RegistrationsFile>>();
            }
        }

        public async Task<Response<IReadOnlyCollection<RegistrationFromFile>>> GetRegistrationsFromFileAsync(
            GetRegistrationsFromFileRequest request)
        {
            try
            {
                var (registrationsFiles,total) = await _fileStorage.ParseFile(request);

                return new Response<IReadOnlyCollection<RegistrationFromFile>>()
                {
                    Status = ResponseStatus.Ok,
                    Data = registrationsFiles,
                    Total = total
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ex.FailedResponse<IReadOnlyCollection<RegistrationFromFile>>();
            }
        }
    }
}