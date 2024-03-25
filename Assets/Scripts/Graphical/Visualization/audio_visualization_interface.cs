using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;


public class audio_visualization_interface : MonoBehaviour
{
    public static audio_visualization_interface Instance { get; private set; }
    HttpClient httpClient;
    // Start is called before the first frame update

    private void Awake()
    {
        if (Instance != null) Destroy(this);
        Instance = this;
    }

    void Start()
    {
        httpClient = new HttpClient();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public async void CallVisualisation(Dictionary<string, object> values)
    {
        var content = JsonConvert.SerializeObject(values);
        var httpContent = new StringContent(content, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("http://localhost:5001/plot_data", httpContent);

        var responseString = await response.Content.ReadAsStringAsync();
    }
}
