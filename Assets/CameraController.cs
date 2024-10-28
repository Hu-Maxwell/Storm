using UnityEngine;

public class CameraController : MonoBehaviour
{
    // room camera
    [SerializeField] private float speed;
    private float currentPosX;
    private Vector3 velocity = Vector3.zero;

    [SerializeField] private Transform player;
    [SerializeField] private float cameraDeltaY; 

    void Start()
    {
        
    }
    void Update()
    {
        // room camera
        // transform.position = Vector3.SmoothDamp(transform.position, new Vector3(currentPosX, transform.position.y, transform.position.z), ref velocity, speed * Time.deltaTime);

        // follow player
        transform.position = new Vector3(player.position.x, player.position.y - cameraDeltaY, transform.position.z); 
    }

    public void MoveToNewRoom(Transform _newRoom)
    {
        currentPosX = _newRoom.position.x;
    }
}
