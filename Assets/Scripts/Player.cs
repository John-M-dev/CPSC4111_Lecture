using System;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;

namespace FirstMultiplayer
{
    public class Token
    {
        public Token()
        {
            
        }
    }
    public class Player : NetworkBehaviour
    {
        public float speed = 10.0f;
        public TMPro.TextMeshProUGUI gamerTag;
        private Token _token;
        private Color _originalColor;
        private Token Token
        {
            set
            {
                _token = value;
                if (_token == null)
                {
                    SetPlayerColorClientRpc(_originalColor);
                }
                else
                {
                    
                    _originalColor = GetComponent<Renderer>().material.color;
                    SetPlayerColorClientRpc(Color.white);
                }
            }
        }
        public bool Tagged
        {
            get { return _token != null; }
        }

        public void Receive(Token token)
        {
            // _token = token;
            Token = token;
        }

        public Token Remove()
        {
            Token oldToken = _token;
            // _token = null;
            Token = null;
            return oldToken;
        }

        public void TransferTokenTo(Player otherPlayer)
        {
            if (_token != null)
            {
                otherPlayer.Receive(Remove());
            }
        }
        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                //RandomSpawn();
                SubmitPositionRequestServerRpc();
                RequestPlayerColorServerRpc();
                CameraController cc = FindObjectOfType<CameraController>();
                cc.player = transform;
                // gamerTag.text = GameManager.inputGamerTag.text;
                SetPlayerGamerTagServerRpc(GameManager.inputGamerTag.text);
            }
            else
            {
                UpdatePlayerColorsServerRpc();
                UpdatePlayerNameServerRpc();
            }
        }

        [ServerRpc]
        public void RequestTokenServerRpc(ServerRpcParams rpcParams = default)
        {
            Token token = GameManager.RequestToken();
            
            Receive(token);
        }
        
        public void Start()
        {
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdatePlayerColorsServerRpc(ServerRpcParams rpcParams = default)
        {
            Color color = gameObject.GetComponent<Renderer>().material.color;
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { rpcParams.Receive.SenderClientId }
                }
            };
            SetPlayerColorClientRpc(color, clientRpcParams);
        }

        public void RandomSpawn()
        {
            SubmitPositionRequestServerRpc();
            /*if (NetworkManager.Singleton.IsServer)
            {
                var randomPosition = GetRandomPositionOnPlane();
                transform.position = randomPosition;
            }
            else
            {
                SubmitPositionRequestServerRpc();
            }*/
        }

        [ServerRpc]
        void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
        {
            transform.position = GetRandomPositionOnPlane();
        }

        [ServerRpc]
        void SetPlayerGamerTagServerRpc(string gamertag, ServerRpcParams rpcParams = default)
        {
            
            SetPlayerGamerTagClientRpc(gamertag);
        }

        [ClientRpc]
        public void SetPlayerGamerTagClientRpc(string gamertag, ClientRpcParams rpcParams = default)
        {
            gamerTag.text = gamertag;
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void UpdatePlayerNameServerRpc(ServerRpcParams rpcParams = default)
        {
            string oldGamerTag = gamerTag.text;
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { rpcParams.Receive.SenderClientId }
                }
            };
            SetPlayerGamerTagClientRpc(oldGamerTag, clientRpcParams);
        }
        
        [ServerRpc]
        void RequestPlayerColorServerRpc(ServerRpcParams rpcParams = default)
        {
            Color color = GameManager.RequestNextColor();//Random.ColorHSV();
            gameObject.GetComponent<Renderer>().material.color = color;
            RequestTokenServerRpc();
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
                        if (Tagged)
                        {
                            TransferTokenTo(player);
                        }
                        else
                        {
                            if (player.Tagged)
                            {
                                player.TransferTokenTo(this);
                            }
                        }
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