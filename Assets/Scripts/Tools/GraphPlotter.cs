using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Net;

public class GraphPlotter : MonoBehaviour
{
    public static GraphPlotter Instance;
    [SerializeField] APIRequest apiRequest;
    string apiAddr = "http://localhost:5000";

    private void Awake()
    {
        if (Instance != null) Destroy(this);

        Instance = this;
    }

    public void PlotGraph(Dictionary<string,object> _values)
    {
        try
        { 
            var content = JsonConvert.SerializeObject(_values);
            apiRequest.SendPostRequest(content, apiAddr, "/plot_data");          
        }
        catch(WebException e)
        {
            Debug.LogError(e);
        }
        catch (SocketException sockEx)
        {
        }

    }
}
