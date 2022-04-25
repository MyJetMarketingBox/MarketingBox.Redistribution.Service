using System;
using System.Threading.Tasks;
using Flurl.Http;

namespace MarketingBox.Redistribution.Service.Logic
{
    public class AutoLoginClicker
    {
        public static async Task<AutoLoginResult> Click(string url)
        {
            try
            {
                var response = await url.GetAsync();
                return new AutoLoginResult()
                {
                    Success = true,
                    StatusCode = response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new AutoLoginResult()
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }

    public class AutoLoginResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int? StatusCode { get; set; }
    }
}