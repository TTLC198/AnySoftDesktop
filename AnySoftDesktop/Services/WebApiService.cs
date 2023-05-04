using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AnySoftDesktop.Services;

public class WebApiService
{
    public static async Task<HttpResponseMessage> GetCall(string url, string authorizatonToken = "")  
    {  
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;  
        var apiUrl = App.ApiUrl + url;
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
    
    public static async Task<HttpResponseMessage> PostCall<T>(string url, T model) where T : class
    {
        var apiUrl = App.ApiUrl + url;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        try
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri(apiUrl);  
            client.Timeout = TimeSpan.FromSeconds(900);  
            client.DefaultRequestHeaders.Accept.Clear();  
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));  
            var response = await client.PostAsJsonAsync(apiUrl, model);  
            return response;
        }
        catch
        {
            throw;
        }
    }  
    
    public static async Task<HttpResponseMessage> PutCall<T>(string url, T model) where T : class  
    {  
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;  
        var apiUrl = App.ApiUrl + url;
        try
        {
            using var client = new HttpClient();
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
    
    public static async Task<HttpResponseMessage> DeleteCall(string url)   
    {  
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;  
        var apiUrl = App.ApiUrl + url;
        try
        {
            using var client = new HttpClient();
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