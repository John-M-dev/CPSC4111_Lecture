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
                CameraController cc = FindObjectOfType<CameraController>();
                cc.player = transform;
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
            //ulong clientId = rpcParams.Receive.SenderClientId;
            //Debug.Log("Client ID: " + rpcParams.Receive.SenderClientId);
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { rpcParams.Receive.SenderClientId }
                }
            };
            SetPlayerColorClientRpc(color, clientRpcParams);
            //Debug.Log("Color: " + color);
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
            Color color = Random.ColorHSV();
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

                if(!Mathf.Approximately(horizontal, 0.0f) || !Mathf.Approximately(vertical, 0.0f))
                {
                    MovePlayerServerRpc(horizontal, vertical);
                }
            }
        }

        [ServerRpc]
        void MovePlayerServerRpc(float horizontal, float vertical, ServerRpcParams rpcParams = default)
        {
            Vector3 oldPosition = transform.position;
            transform.Translate(horizontal, vertical, 0);
            foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (uid != rpcParams.Receive.SenderClientId)
                {
                    Player player = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<Player>();
                    float distance = Vector3.Distance(player.transform.position, transform.position);
                    if(distance < 1f)
                    {
                        Vector3 correctionVector = (1 - distance) * (oldPosition - transform.position).normalized;
                        transform.position += correctionVector;
                        //transform.position = oldPosition;
                        //RequestPlayerColorServerRpc();
                    }
                }
                //else
                //{
                //    Debug.Log("Checking against self not allowed!");
                //}
            }
        }
    }
}