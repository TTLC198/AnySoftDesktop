using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AnySoftDesktop.Services;

public class WebApiService
{
    public static async Task<HttpResponseMessage> GetCall(string url)  
    {  
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;  
        var apiUrl = App.ApiUrl + url;
        using var client = new HttpClient();
        client.BaseAddress = new Uri(apiUrl);  
        client.Timeout = TimeSpan.FromSeconds(900);  
        client.DefaultRequestHeaders.Accept.Clear();  
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));  
        var response = await client.GetAsync(apiUrl);   
        return response;
    } 
    
    public static async Task<HttpResponseMessage> PostCall<T>(string url, T model) where T : class
    {
        var apiUrl = App.ApiUrl + url;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        using var client = new HttpClient();
        client.BaseAddress = new Uri(apiUrl);  
        client.Timeout = TimeSpan.FromSeconds(900);  
        client.DefaultRequestHeaders.Accept.Clear();  
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));  
        var response = await client.PostAsJsonAsync(apiUrl, model);  
        return response;
    }  
    
    public static async Task<HttpResponseMessage> PutCall<T>(string url, T model) where T : class  
    {  
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;  
        var apiUrl = App.ApiUrl + url;
        using var client = new HttpClient();
        client.BaseAddress = new Uri(apiUrl);  
        client.Timeout = TimeSpan.FromSeconds(900);  
        client.DefaultRequestHeaders.Accept.Clear();  
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));  
        var response = await client.PutAsJsonAsync(apiUrl, model); 
        return response;
    }  
    
    public static async Task<HttpResponseMessage> DeleteCall(string url)   
    {  
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;  
        var apiUrl = App.ApiUrl + url;
        using var client = new HttpClient();
        client.BaseAddress = new Uri(apiUrl);  
        client.Timeout = TimeSpan.FromSeconds(900);  
        client.DefaultRequestHeaders.Accept.Clear();  
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));  
        var response = await client.DeleteAsync(apiUrl);
        return response;
    } 
}