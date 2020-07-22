using System;
using System.Numerics;

namespace CeMSIM_BasicServer
{
    // This class houses all data, behaviors a player have
    public class Player
    {
        public int id;
        public string username;

        public Vector3 position;
        public Quaternion rotation;

        private float moveSpeed = Constants.MOVE_SPEED_PER_SECOND / Constants.TICKS_PER_SECOND;
        private bool[] inputs;


        public Player(int _id, string _username, Vector3 _spawnPosition)
        {
            id = _id;
            username = _username;
            position = _spawnPosition;
            rotation = Quaternion.Identity;

            inputs = new bool[4];

        }

        public void Update()
        {
            Vector2 _inputDirection = Vector2.Zero;
            if (inputs[0]) // W
            {
                _inputDirection.Y += 1;
            }
            if (inputs[1]) // S
            {
                _inputDirection.Y -= 1;
            }
            if (inputs[2]) // A
            {
                _inputDirection.X += 1;
            }
            if (inputs[3]) // D
            {
                _inputDirection.X -= 1;
            }

            Move(_inputDirection);

        }

        private void Move(Vector2 _inputDirection)
        {
            /*
             * Here, we assume perfect network status. So we don't consider quick 
             * movements and quick turning due to the packet loss.
             * **/

            // Transform user's relative coordinating system to absolute coordinating system
            Vector3 _forward = Vector3.Transform(new Vector3(0, 0, 1), rotation);
            Vector3 _right = Vector3.Normalize(Vector3.Cross(_forward, new Vector3(0, 1, 0)));

            Vector3 _moveDirection = _right * _inputDirection.X + _forward * _inputDirection.Y;
            position += _moveDirection * moveSpeed;


            // public the position to every client, but public the facing direction to all but the player
            ServerSend.PlayerPosition(this);
            ServerSend.PlayerRotation(this);

        }

        public void SetInput(bool[] _inputs, Quaternion _rotation)
        {
            inputs = _inputs;
            rotation = _rotation;
        }
    }
}
