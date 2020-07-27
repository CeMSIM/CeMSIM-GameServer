using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;

    // constained in "transform"
    //public Vector3 position;
    //public Quaternion rotation;

    private float moveSpeed = Constants.MOVE_SPEED_PER_SECOND / Constants.TICKS_PER_SECOND;
    private bool[] inputs;


    //public Player(int _id, string _username) // unity monobehavior doesn't allow constructor
    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        //position = _spawnPosition;
        //rotation = Quaternion.identity;


        inputs = new bool[4];

    }
    
    public void FixedUpdate()
    {
        Vector2 _inputDirection = Vector2.zero;
        if (inputs[0]) // W
        {
            _inputDirection.y += 1;
        }
        if (inputs[1]) // S
        {
            _inputDirection.y -= 1;
        }
        if (inputs[2]) // A
        {
            _inputDirection.x -= 1;
        }
        if (inputs[3]) // D
        {
            _inputDirection.x += 1;
        }

        Move(_inputDirection);

    }

    private void Move(Vector2 _inputDirection)
    {
        /*
         * Here, we assume perfect network status. So we don't consider quick 
         * movements and quick turning due to the packet loss.
         * **/

        // In unity, the following two variables are in "transform"
        //// Transform user's relative coordinating system to absolute coordinating system
        //Vector3 _forward = Vector3.Transform(new Vector3(0, 0, 1), rotation);
        //Vector3 _right = Vector3.Normalize(Vector3.Cross(_forward, new Vector3(0, 1, 0)));

        Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        //position += _moveDirection * moveSpeed;
        transform.position += _moveDirection * moveSpeed;


        // public the position to every client, but public the facing direction to all but the player
        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);

    }

    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {
        inputs = _inputs;
        transform.rotation = _rotation;
    }
}
