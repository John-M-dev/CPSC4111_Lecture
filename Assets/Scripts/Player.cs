using Unity.Netcode;
using UnityEngine;

namespace FirstMultiplayer
{
    public class Player : NetworkBehaviour
    {
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
        public float speed = 10.0f;
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