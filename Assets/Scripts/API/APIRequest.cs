using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public class APIRequest : MonoBehaviour
{
    public async void SendPostRequest(string _json, string _apiPath, string _command, string _pathExtension = "")
    {
        string url = _apiPath + _command + _pathExtension;
        await PostRequest(url, _json);
    }
    public void SendGETRequestTEST()
    {
        print(GetRequest($"http://localhost:5000/readfile?path=E:/Ben/UnityProjekts/GithubProjects/TabGenerator/GTP_Recordings/cool.gp5").Result);
    }
    public async Task<string> SendGetRequest(string _apiPath, string _command, string _pathExtension = "")
    {
        string url = _apiPath + _command + _pathExtension;
        return await GetRequest(url);
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

            if (!response.IsSuccessStatusCode)
            {
                print("Error: " + response.StatusCode);
            }

            var responseString = await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e);
        }
    }
}
