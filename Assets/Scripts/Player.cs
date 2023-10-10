using Unity.Netcode;
using UnityEngine;

namespace FirstMultiplayer
{
    public class Player : NetworkBehaviour
    {
        public float speed = 10.0f;
        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                RandomSpawn();
                RequestPlayerColorServerRpc();
            }
            else
            {
                UpdatePlayerColorsServerRpc();
            }
        }

        public void Start()
        {
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdatePlayerColorsServerRpc(ServerRpcParams rpcParams = default)
        {
            Color color = gameObject.GetComponent<Renderer>().material.color;
           
            // Debug.Log("Client ID: [0] " + clientId);
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { rpcParams.Receive.SenderClientId }
                }
            };
            
            SetPlayerColorClientRpc(color, clientRpcParams);
            // Debug.Log("Color: " + color);
        }

            public void RandomSpawn()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                var randomPosition = GetRandomPositionOnPlane();
                transform.position = randomPosition;
            }
            else
            {
                SubmitPositionRequestServerRpc();
            }
        }

        [ServerRpc]
        void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
        {
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
        public void SetPlayerColorClientRpc(Color newColor, ClientRpcParams clientRpcParams = default)
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
                
                MovePlayerServerRpc(horizontal, vertical);
            }
        }

        [ServerRpc]
        void MovePlayerServerRpc(float horizontal, float vertical)
        {
            transform.Translate(horizontal, vertical, 0);
        }
    }
}