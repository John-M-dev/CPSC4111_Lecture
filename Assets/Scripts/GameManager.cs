
using Unity.Netcode;
using UnityEngine;
using TMPro;

namespace FirstMultiplayer
{
    public class GameManager : MonoBehaviour
    {
        public Color[] colors;
        public float[] hues;
        public static IColorStrategy colorStrategy = new AstreaChoices();
        public static TMPro.TextMeshProUGUI inputGamerTag;
        public TMPro.TextMeshProUGUI inputLocalGamerTag;
        public static Token theToken = new Token();

        private void Awake()
        {
            AstreaChoices ac = new AstreaChoices();
            colors = new Color[20];
            hues = new float[20];
            for(int i = 0; i < 20; i++)
            {
                colors[i] = ac.SelectColor();
                hues[i] = ac.Hue(i+1);
            }
            inputGamerTag = inputLocalGamerTag;
        }
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                StartButtons();
            }
            else
            {
                StatusLabels();

                SubmitNewPosition();
            }

            GUILayout.EndArea();
        }

        public static Token RequestToken()
        {
            Token oldToken = theToken;
            theToken = null;
            return oldToken;
        }
        
        static void StartButtons()
        {
            if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
            if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
            if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
        }

        static void StatusLabels()
        {
            var mode = NetworkManager.Singleton.IsHost ?
                "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

            GUILayout.Label("Transport: " +
                NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label("Mode: " + mode);
        }

        static void SubmitNewPosition()
        {
            if (GUILayout.Button(NetworkManager.Singleton.IsServer ? "Move?" : "Request Position Change?"))
            {
                if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
                {
                    foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
                    {
                        Player player = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<Player>();
                        //NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<Player>().RandomSpawn();
                        player.RandomSpawn();
                        Color color = player.gameObject.GetComponent<Renderer>().material.color;
                        player.SetPlayerColorClientRpc(color);
                    }
                }
                else
                {
                    var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                    var player = playerObject.GetComponent<Player>();
                    player.RandomSpawn();
                }
            }
        }

        public static Color RequestNextColor()
        {
            Color assignedColor = colorStrategy.SelectColor();

            return assignedColor;
        }
    }
}