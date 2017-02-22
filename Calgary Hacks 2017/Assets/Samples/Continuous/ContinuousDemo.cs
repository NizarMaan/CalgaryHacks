using BarcodeScanner;
using BarcodeScanner.Scanner;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ContinuousDemo : MonoBehaviour {

	private IScanner BarcodeScanner;
	public RawImage Image;
	public AudioSource Audio;
    public Text productNameWalmart, productNameEbay, productPriceWalmart, productPriceEbay;
    public Button continueBtn;
    public RawImage scannerScreen; 
	private float RestartTime;

    private string walMartKey;
    private string eBayKey;
    private string url;
    private string itemUPC;
    private WWW www, ebayURL, walmartURL;
    private bool scan;
    private string walmartPrice, ebayPrice, walmartProductName, ebayProductName;

    // Disable Screen Rotation on that screen
    void Awake()
	{
		Screen.autorotateToPortrait = false;
		Screen.autorotateToPortraitUpsideDown = false;
        SetGUI(false, 0.0f);
        scan = true;
	}

	void Start () {
        walMartKey = "rgdqpvrcz8xg2h9gzzzdjcdh";
        eBayKey = "SyedZaid-CalgaryH-PRD-c2466ad0e-135ec8fd";

        // Create a basic scanner
        BarcodeScanner = new Scanner();
		BarcodeScanner.Camera.Play();

		// Display the camera texture through a RawImage
		BarcodeScanner.OnReady += (sender, arg) => {
			// Set Orientation & Texture
			Image.transform.localEulerAngles = BarcodeScanner.Camera.GetEulerAngles();
			Image.transform.localScale = BarcodeScanner.Camera.GetScale();
			Image.texture = BarcodeScanner.Camera.Texture;

			// Keep Image Aspect Ratio
			var rect = Image.GetComponent<RectTransform>();
			var newHeight = rect.sizeDelta.x * BarcodeScanner.Camera.Height / BarcodeScanner.Camera.Width;
			rect.sizeDelta = new Vector2(rect.sizeDelta.x, newHeight);

			RestartTime = Time.realtimeSinceStartup;
        };
	}

	/// <summary>
	/// Start a scan and wait for the callback (wait 1s after a scan success to avoid scanning multiple time the same element)
	/// </summary>
	private void StartScanner()
	{
		BarcodeScanner.Scan((barCodeType, barCodeValue) => {
			BarcodeScanner.Stop();
			//TextHeader.text += "Found: " + barCodeType + " / " + barCodeValue + "\n";
			RestartTime += Time.realtimeSinceStartup + 1f;

			// Feedback
			Audio.Play();

            itemUPC = barCodeValue;

            //walmart
            url = "http://api.walmartlabs.com/v1/items?apiKey=" + walMartKey + "&upc=" + barCodeValue;
            www = new WWW(url);
            StartCoroutine(WaitForRequest(www, Service.WALMART));

            //ebay
            url = "http://svcs.ebay.com/services/search/FindingService/v1?SECURITY-APPNAME=" + eBayKey + "&OPERATION-NAME=findItemsByProduct&SERVICE-VERSION=1.0.0&RESPONSE-DATA-FORMAT=XML&REST-PAYLOAD&productId.@type=UPC&productId=" + itemUPC + "&paginationInput.entriesPerPage=3";
            www = new WWW(url);
            StartCoroutine(WaitForRequest(www, Service.EBAY));

            #if UNITY_ANDROID || UNITY_IOS
                Handheld.Vibrate();
			#endif
		});
	}

	/// <summary>
	/// The Update method from unity need to be propagated
	/// </summary>
	void Update()
	{
		if (BarcodeScanner != null)
		{
			BarcodeScanner.Update();
		}

		// Check if the Scanner need to be started or restarted
		if (RestartTime != 0 && RestartTime < Time.realtimeSinceStartup && scan)
		{
			StartScanner();
			RestartTime = 0;
		}
	}

	#region UI Buttons

	/// <summary>
	/// This coroutine is used because of a bug with unity (http://forum.unity3d.com/threads/closing-scene-with-active-webcamtexture-crashes-on-android-solved.363566/)
	/// Trying to stop the camera in OnDestroy provoke random crash on Android
	/// </summary>
	/// <param name="callback"></param>
	/// <returns></returns>
	public IEnumerator StopCamera(Action callback)
	{
		// Stop Scanning
		Image = null;
		BarcodeScanner.Destroy();
		BarcodeScanner = null;

		// Wait a bit
		yield return new WaitForSeconds(0.1f);

		callback.Invoke();
	}

    private enum Service
    {
        WALMART,
        EBAY
    }

    IEnumerator WaitForRequest(WWW www, Service service)
    {
        yield return www;
        // check for errors
        if (www.error == null)
        {
            /*Walmart testWal = JsonUtility.FromJson<Walmart>(www.text);
            Debug.Log("Product Name: " + testWal.items[0].name);
            Debug.Log("Price: " + testWal.items[0].msrp);
            Debug.Log("Sale Price: " + testWal.items[0].salePrice);*/

            scan = false; //don't scan until "continue" button is pressed

            if (service == Service.WALMART)
            {
                print(service);
                walmartPrice = ParseWalmartURL(www.text)[0];
                walmartProductName = ParseWalmartURL(www.text)[1];
                walmartURL = www;
            }

            if (service == Service.EBAY){
                print(service);
                ebayPrice = ParseEbayURL(www.text)[0];
                ebayProductName = ParseEbayURL(www.text)[1];
                ebayURL = www;
            }

            //show info
            SetProductInfo(walmartPrice, walmartProductName, ebayPrice, ebayProductName);
            SetGUI(true, 255.0f);
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
        }
    }

    public void Continue()
    {
        scan = true;
        SetGUI(false, 0.0f);
    }

    private void SetGUI(bool enable, float alpha)
    {
        scannerScreen.GetComponent<RawImage>().enabled = !enable;

        continueBtn.enabled = enable;
        productNameWalmart.enabled = enable;
        productNameEbay.enabled = enable;
        productPriceWalmart.enabled = enable;
        productPriceEbay.enabled = enable;

        Color c = continueBtn.GetComponent<Image>().color;
        c.a = alpha;
        continueBtn.GetComponent<Image>().color = c;

        //the text of the continue button
        c = continueBtn.GetComponentInChildren<Text>().color;
        c.a = alpha;
        continueBtn.GetComponentInChildren<Text>().color = c;
    }

    private void SetProductInfo(string walmartPrice, string walmartProductName, string ebayPrice, string ebayProductName)
    {
        productNameWalmart.text = "Walmart Product: " + walmartProductName;
        productPriceWalmart.text = "Walmart Price: " + walmartPrice;

        productNameEbay.text = "Ebay Product: " + ebayProductName;
        productPriceEbay.text = "Ebay Price: " + ebayPrice;
    }

    private string[] ParseWalmartURL(string result)
    {
        walmartPrice = "$0.99";
        walmartProductName = "walmart_test";

        return new string[] { walmartPrice, walmartProductName };
    }

    private string[] ParseEbayURL(string result)
    {       
        ebayPrice = "$1.50";
        ebayProductName = "ebay_test";

        return new string[]{ ebayPrice, ebayProductName};
    }

    public void ToProduct(string service)
    {
        if(service.Equals("walmart"))
        {
            Application.OpenURL(walmartURL.ToString());
        }

        if (service.Equals("ebay"))
        {
            Application.OpenURL(ebayURL.ToString());
        }     
    }
    #endregion
}
