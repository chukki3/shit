using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;
using UnityEngine.UI;
using DodoDataModel;
public class GameManager : MonoBehaviour
{
    public bool timer, GameStart = false;
    public Text Text;
    // Start is called before the first frame update
    float time = 5.5f;
    public GameObject StartButtton;
    public GameObject ImageTarget, SearchBoxLabel;
    public GameObject Victorine;
    public GameObject bird1;
    public GameObject bird2;
    private string url = "http://89.223.126.195:80/hello";
    private HubConnection hubConnection = null;
    private UnityMainThreadDispatcher _dispatcher;
    public User user;
    // Start is called before the first frame update

    //void Nachalo(string w)
    //{
    //    StartButtton.SetActive(true);
    //}

    public void StartGame(string w)
    {
        _dispatcher.Enqueue(() =>
        {
            GameStart = true;
            StartButtton.SetActive(false);
            Text.gameObject.SetActive(true);
            Debug.Log("Игра началась");
            timer = true;
        });
    }
    public void LostTarget(string w)
    {
        _dispatcher.Enqueue(() =>
        {
            StartButtton.SetActive(false);
        });
    }
    public void FoundTarget(string w)
    {

        
            StartButtton.SetActive(true);
        
    }
    public async void FoundMessage()
    {
        await hubConnection.InvokeAsync("FoundTarget", Newtonsoft.Json.JsonConvert.SerializeObject(user));
    }
    public async void DeleteTarget()
    {
        _dispatcher.Enqueue(() =>
        {        
            StartButtton.SetActive(false);        
        });
        await hubConnection.InvokeAsync("LostTarget", Newtonsoft.Json.JsonConvert.SerializeObject(user));
    }
    async void Start()
    {
        // create new guid
        user = new User();
        user.guid = Guid.NewGuid();

        // set room 
        user.keyRoom = "lol";

        // set distpacher
        _dispatcher = UnityMainThreadDispatcher.Instance();

        // start client
        await this.StartSignalRAsync();
    }
    
    async Task StartSignalRAsync()
    {
        if (this.hubConnection == null)
        {
            // create hub and settings
            this.hubConnection = new HubConnectionBuilder()
                .WithUrl(url, options => { })
                .Build();

            // subscribe to method on server
            //this.hubConnection.On<string>("OnRoomComlete", Nachalo);
            this.hubConnection.On<string>("StartGame", StartGame);
            this.hubConnection.On<string>("getSpeed", getSpeed);
            this.hubConnection.On<string>("LostTarget", LostTarget);
            this.hubConnection.On<string>("FoundTarget", FoundTarget);
            // start server
            await this.hubConnection.StartAsync();
            // registation client on server 
            await hubConnection.InvokeAsync("ClientRegistration", Newtonsoft.Json.JsonConvert.SerializeObject(user));
            
            //Status: connect
            
                //await hubConnection.InvokeAsync("Hello");
        }
        else
        {
            
        }
    }

    public void getSpeed(string json)
    {
        Speed speed = Newtonsoft.Json.JsonConvert.DeserializeObject<Speed>(json);
        Stats.MovementVelocityFirstBird = speed.speed1;
        Stats.MovementVelocitySecondBird = speed.speed2;
    }
    public async void setSpeed()
    {
        await hubConnection.InvokeAsync("setSpeed", Newtonsoft.Json.JsonConvert.SerializeObject(user));
    }
    public async void ClickStartGame()
    {
        await hubConnection.InvokeAsync("ConfirmStartGame", Newtonsoft.Json.JsonConvert.SerializeObject(user));
    }
    private void OnDestroy()
    {
        hubConnection.StopAsync();
    }

    private void Update()
    {
        if (ImageTarget.activeInHierarchy == true)
        {
            SearchBoxLabel.SetActive(false);
            if (GameStart == false)
            {
                FoundMessage();
            }
        }
        if (ImageTarget.activeInHierarchy == false)
        {
            SearchBoxLabel.SetActive(true);
            if (GameStart == false)
            {
                DeleteTarget();
            }
            
        }
        if (timer == true)
        {
            //GameStart = true;
            StartButtton.SetActive(false);
            time -= Time.deltaTime;
            Text.text = Convert.ToString(Mathf.RoundToInt(time));
            if (time <= 0)
            {
                Text.gameObject.SetActive(false);
                Victorine.SetActive(true);
                Stats.isReady = true;
                
                timer = false;
            }
        }
    }
}
