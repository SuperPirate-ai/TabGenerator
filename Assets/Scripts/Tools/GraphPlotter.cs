using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class GraphPlotter : MonoBehaviour
{
    public static GraphPlotter Instance;
    [SerializeField] APIRequest apiRequest;
    string apiAddr = "http://localhost:5000";
    List<object> graphs = new List<object>();
    private void Awake()
    {
        if (Instance != null) Destroy(this);

        Instance = this;
    }

    public void PlotGraph(Dictionary<string, object> _values)
    {
        //Dictionary<string, object> graphData = new Dictionary<string, object>();
        try
        {
            var content = JsonConvert.SerializeObject(_values);
            apiRequest.SendPostRequest(content, apiAddr, "/plot_data");
        }
        catch (WebException e)
        {
            Debug.LogError(e);
        }
        catch (SocketException sockEx)
        {
        }

    }
    //public void AddGraph(List<object> _values,int layerX,int layerY)
    //{
    //    List<object> data = new List<object> {layerX,layerY,_values };
    //    graphs.Add(data);



    //}
}
