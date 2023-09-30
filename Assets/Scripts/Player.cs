using Unity.Netcode;
using UnityEngine;

namespace FirstMultiplayer
{
    public class Player : NetworkBehaviour
    {
        //public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
        public float speed = 10.0f;
        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                RandomSpawn();
                RequestPlayerColorServerRpc();
                UpdatePlayerColorsServerRpc();
            }
            else
            {
                UpdatePlayerColorsServerRpc();
            }
        }

        public void Start()
        {
            //UpdatePlayerColorsServerRpc();
        }

        [ServerRpc]
        public void UpdatePlayerColorsServerRpc(ServerRpcParams rpcParams = default)
        {
            Color color = gameObject.GetComponent<Renderer>().material.color;
            SetPlayerColorClientRpc(color);
            Debug.Log("Color: " + color);
            /*
            if (NetworkManager.Singleton.IsServer)
            {
                foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    Player player = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<Player>();
                    Color color = player.gameObject.GetComponent<Renderer>().material.color;
                    player.SetPlayerColorClientRpc(color);
                }
            }
            */
        }

            public void RandomSpawn()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                var randomPosition = GetRandomPositionOnPlane();
                transform.position = randomPosition;
                //Position.Value = randomPosition;
                //Renderer r = GetComponent<Renderer>();
                //r.material.color = Random.ColorHSV();
                
            }
            else
            {
                SubmitPositionRequestServerRpc();
            }
        }

        [ServerRpc]
        void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
        {
            //Position.Value = GetRandomPositionOnPlane();
            transform.position = GetRandomPositionOnPlane();
        }
        [ServerRpc]
        void RequestPlayerColorServerRpc(ServerRpcParams rpcParams = default)
        {
            Color color = Random.ColorHSV(0f, 1f, 0f, 1f, 0f, 0.7f);
            gameObject.GetComponent<Renderer>().material.color = color;
            SetPlayerColorClientRpc(color);
        }

        [ClientRpc]
        public void SetPlayerColorClientRpc(Color newColor)
        {
            GetComponent<Renderer>().material.color = newColor;
        }

        static Vector3 GetRandomPositionOnPlane()
        {
            return new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 1f );
        }

        void Update()
        {

            if(IsOwner)
            {
                float horizontal = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
                float vertical = Input.GetAxis("Vertical") * speed * Time.deltaTime;

                //transform.Translate(horizontal, vertical, 0);
                MovePlayerServerRpc(horizontal, vertical);
            }

            //transform.position = Position.Value;
        }

        [ServerRpc]
        void MovePlayerServerRpc(float horizontal, float vertical)
        {
            transform.Translate(horizontal, vertical, 0);
            //Position.Value = transform.position;
        }
    }
}