using UnityEngine;
using Voxelis;
using Voxelis.Utils;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(VoxelEntity))]
public class SimplePlayer : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    [SerializeField] private float playerSpeed = 2.0f;
    [SerializeField] private float jumpHeight = 1.0f;
    private float gravityValue = -9.81f;

    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Vector3Int hand;
    [SerializeField] private VoxelRayCast raycast;
    private VoxelEntity entity;
    
    private void Start()
    {
        controller = gameObject.GetComponent<CharacterController>();
        entity = gameObject.GetComponent<VoxelEntity>();
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    void Update()
    {
        // Update handblock
        Block b;
        b.data = raycast.handblock;
        if (entity.GetBlock(hand.ToInt3()).data != b.data)
        {
            entity.SetBlock(hand.ToInt3(), b);
        }
        
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        var cameraRot = Quaternion.Euler(0, cameraTransform.rotation.eulerAngles.y, 0);
        move = cameraRot * move;
        controller.Move(move * Time.deltaTime * playerSpeed);

        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }

        // Makes the player jump
        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
}