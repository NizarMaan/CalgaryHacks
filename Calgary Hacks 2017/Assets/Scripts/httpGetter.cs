using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class httpGetter : MonoBehaviour {
    private string walMartKey;
    private string url;
    public static string itemUPC;

    void Start()
    {
        walMartKey = "rgdqpvrcz8xg2h9gzzzdjcdh";
        url = "http://api.walmartlabs.com/v1/items?apiKey=" + walMartKey + "&upc=035000521019";
        WWW www = new WWW(url);
        StartCoroutine(WaitForRequest(www));
    }

    IEnumerator WaitForRequest(WWW www)
    {
        yield return www;
        Debug.Log(itemUPC);
        // check for errors
        if (www.error == null)
        {
            Debug.Log("WWW Ok!: " + www.text);
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
        }
    }
}
