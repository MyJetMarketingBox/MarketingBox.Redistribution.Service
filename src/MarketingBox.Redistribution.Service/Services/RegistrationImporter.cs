using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketingBox.Redistribution.Service.Domain.Models;
using MarketingBox.Redistribution.Service.Grpc;
using MarketingBox.Redistribution.Service.Grpc.Models;
using MarketingBox.Redistribution.Service.Storage;
using MarketingBox.Sdk.Common.Models;
using MarketingBox.Sdk.Common.Models.Grpc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MarketingBox.Redistribution.Service.Services
{
    public class RegistrationImporter: IRegistrationImporter
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
                _logger.LogInformation($"RegistrationImporter.ImportAsync receive request {JsonConvert.SerializeObject(request)}");
                
                var registrationsFile = new RegistrationsFile()
                {
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.UserId,
                    File = request.RegistrationsFile
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
                return new Response<ImportResponse>()
                {
                    Status = ResponseStatus.InternalError,
                    Error = new Error()
                    {
                        ErrorMessage = ex.Message
                    }
                };
            }
        }

        public async Task<Response<GetRegistrationFilesResponse>> GetRegistrationFilesAsync()
        {
            try
            {
                var files = await _fileStorage.Get();
                
                return new Response<GetRegistrationFilesResponse>()
                {
                    Status = ResponseStatus.Ok,
                    Data = new GetRegistrationFilesResponse()
                    {
                        Files = files
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response<GetRegistrationFilesResponse>()
                {
                    Status = ResponseStatus.InternalError,
                    Error = new Error()
                    {
                        ErrorMessage = ex.Message
                    }
                };
            }
        }

        public async Task<Response<List<RegistrationFromFile>>> GetRegistrationsFromFileAsync(GetRegistrationsFromFileRequest request)
        {
            try
            {
                var registrationsFiles = await _fileStorage.ParseFile(request.FileId);
                
                if (registrationsFiles == null)
                    return new Response<List<RegistrationFromFile>>()
                    {
                        Status = ResponseStatus.NotFound,
                        Error = new Error()
                        {
                            ErrorMessage = $"Cant find file with id {request.FileId}."
                        }
                    };

                return new Response<List<RegistrationFromFile>>()
                {
                    Status = ResponseStatus.Ok,
                    Data = registrationsFiles
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new Response<List<RegistrationFromFile>>()
                {
                    Status = ResponseStatus.InternalError,
                    Error = new Error()
                    {
                        ErrorMessage = ex.Message
                    }
                };
            }
        }
    }
}
