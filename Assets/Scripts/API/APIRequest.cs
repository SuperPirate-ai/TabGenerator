using Codice.CM.Client.Differences.Merge;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public class APIRequest : MonoBehaviour
{
    public void SendPostRequest(string _json, string _apiPath,string _command, string _pathExtension = "")
    {  
        string url = _apiPath + _command + _pathExtension;
        PostRequest(url, _json).Wait();
    }
    public void SendGETRequestTEST()
    {
       print(GetRequest($"http://localhost:5000/readfile?path=E:/Ben/UnityProjekts/GithubProjects/TabGenerator/GTP_Recordings/cool.gp5").Result);
    }
    public string SendGetRequest(string _apiPath, string _command, string _pathExtension = "")
    {
        string url = _apiPath + _command + _pathExtension;
        return GetRequest(url).Result;
    }
    public void printer(string _message)
    {
        print(_message);
    }
    private async Task<string> GetRequest(string _url)
    {
        HttpClient httpClient = new HttpClient();
        try
        {
            httpClient.BaseAddress = new Uri("http://0.0.0.0:5000");
            var response = await httpClient.GetStringAsync(_url);
            //var responseString =  await response.Content.ReadAsStringAsync();
            return response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
    private async Task PostRequest(string _url, string _json)
    {
        HttpClient httpClient = new HttpClient();
        try
        {
            var httpContent = new StringContent(_json, System.Text.Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(_url, httpContent);
            var responseString = await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
