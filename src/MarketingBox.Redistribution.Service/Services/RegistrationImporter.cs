using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketingBox.Auth.Service.Client.Interfaces;
using MarketingBox.Redistribution.Service.Domain.Models;
using MarketingBox.Redistribution.Service.Grpc;
using MarketingBox.Redistribution.Service.Grpc.Models;
using MarketingBox.Redistribution.Service.Storage;
using MarketingBox.Sdk.Common.Extensions;
using MarketingBox.Sdk.Common.Models.Grpc;
using Microsoft.Extensions.Logging;

namespace MarketingBox.Redistribution.Service.Services
{
    public class RegistrationImporter : IRegistrationImporter
    {
        private readonly ILogger<RegistrationImporter> _logger;
        private readonly FileStorage _fileStorage;
        private IUserClient _userClient;

        public RegistrationImporter(ILogger<RegistrationImporter> logger, FileStorage fileStorage, IUserClient userClient)
        {
            _logger = logger;
            _fileStorage = fileStorage;
            _userClient = userClient;
        }

        public async Task<Response<RegistrationsFile>> ImportAsync(ImportRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "RegistrationImporter.ImportAsync receive request {@Request}",request);
                var user = await _userClient.GetUser(request.UserId, request.TenantId, true);
                
                var registrationsFile = new RegistrationsFile()
                {
                    CreatedAt = DateTime.UtcNow,
                    FileName = request.FileName,
                    CreatedByUserId = request.UserId,
                    CreatedByUserName = user.Username,
                    File = request.RegistrationsFile,
                    TenantId = request.TenantId
                };

                await _fileStorage.Save(registrationsFile);

                return new Response<RegistrationsFile>()
                {
                    Status = ResponseStatus.Ok,
                    Data = registrationsFile
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ex.FailedResponse<RegistrationsFile>();
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