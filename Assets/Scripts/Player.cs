using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace FirstMultiplayer
{
    public class Player : NetworkBehaviour
    {
        public float speed = 10.0f;
        
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                RandomSpawn();
            }
        }

        public void RandomSpawn()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                var randomPosition = GetRandomPositionOnPlane();
                transform.position = randomPosition;
                Position.Value = randomPosition;
            }
            else
            {
                SubmitPositionRequestServerRpc();
            }
        }

        [ServerRpc]
        void SubmitPositionRequestServerRpc(ServerRpcParams rpcParams = default)
        {
            Position.Value = GetRandomPositionOnPlane();
        }

        static Vector3 GetRandomPositionOnPlane()
        {
            return new Vector3(Random.Range(-3f, 3f),Random.Range(-3f, 3f), 0f);
        }

        void Update()
        {
            if (IsOwner)
            {
                float horizontal = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
                float vertical = Input.GetAxis("Vertical") * speed * Time.deltaTime;
                
                MovePlayerServerRPC(horizontal, vertical);
            }

            
            transform.position = Position.Value;
        }

        [ServerRpc]
        void MovePlayerServerRPC(float horizontal, float vertical)
        {
            transform.Translate(horizontal, vertical, 0);
            Position.Value = transform.position;
        }
    }
}