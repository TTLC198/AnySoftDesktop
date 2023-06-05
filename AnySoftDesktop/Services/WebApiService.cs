using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AnySoftDesktop.Utils;

namespace AnySoftDesktop.Services;

public class WebApiService
{
    public static async Task<HttpResponseMessage> GetCall(string url, string authorizatonToken = "")  
    {  
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;  
        var apiUrl = VersionManager.Instance.ApiUrl + url;
        try
        {
            using var client = new HttpClient();
            if (authorizatonToken != string.Empty)
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authorizatonToken}");
            client.BaseAddress = new Uri(apiUrl);  
            client.Timeout = TimeSpan.FromSeconds(900);  
            client.DefaultRequestHeaders.Accept.Clear();  
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));  
            var response = await client.GetAsync(apiUrl);   
            return response;
        }
        catch
        {
            throw;
        }
    } 
    
    public static async Task<HttpResponseMessage> PostCall<T>(
        string url,
        T model,
        string? authorizationToken = null,
        string? contentType = null) where T : class
    {
        var apiUrl = VersionManager.Instance.ApiUrl + url;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        try
        {
            using var client = new HttpClient();
            if (authorizationToken is not null)
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authorizationToken}");
            client.BaseAddress = new Uri(apiUrl);  
            client.Timeout = TimeSpan.FromSeconds(900);  
            client.DefaultRequestHeaders.Accept.Clear();  
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType ??= "application/json"));  
            var response = await client.PostAsJsonAsync(apiUrl, model);  
            return response;
        }
        catch
        {
            throw;
        }
    }  
    
    public static async Task<HttpResponseMessage> PostCall(
        string url,
        MultipartFormDataContent content,
        string? authorizationToken = null,
        string? contentType = null)
    {
        var apiUrl = VersionManager.Instance.ApiUrl + url;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        try
        {
            using var client = new HttpClient();
            if (authorizationToken is not null)
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authorizationToken}");
            client.BaseAddress = new Uri(apiUrl);  
            client.Timeout = TimeSpan.FromSeconds(900);  
            client.DefaultRequestHeaders.Accept.Clear();  
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType ??= "application/json"));  
            var response = await client.PostAsync(apiUrl, content);  
            return response;
        }
        catch
        {
            throw;
        }
    }  
    
    public static async Task<HttpResponseMessage> PutCall<T>(
        string url,
        T model,
        string? authorizationToken = null) where T : class  
    {  
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;  
        var apiUrl = VersionManager.Instance.ApiUrl + url;
        try
        {
            using var client = new HttpClient();
            if (authorizationToken is not null)
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authorizationToken}");
            client.BaseAddress = new Uri(apiUrl);  
            client.Timeout = TimeSpan.FromSeconds(900);  
            client.DefaultRequestHeaders.Accept.Clear();  
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));  
            var response = await client.PutAsJsonAsync(apiUrl, model); 
            return response;
        }
        catch
        {
            throw;
        }
    }  
    
    public static async Task<HttpResponseMessage> DeleteCall(
        string url,
        string? authorizationToken = null)   
    {  
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;  
        var apiUrl = VersionManager.Instance.ApiUrl + url;
        try
        {
            using var client = new HttpClient();
            if (authorizationToken is not null)
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authorizationToken}");
            client.BaseAddress = new Uri(apiUrl);  
            client.Timeout = TimeSpan.FromSeconds(900);  
            client.DefaultRequestHeaders.Accept.Clear();  
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));  
            var response = await client.DeleteAsync(apiUrl);
            return response;
        }
        catch
        {
            throw;
        }
    } 
}