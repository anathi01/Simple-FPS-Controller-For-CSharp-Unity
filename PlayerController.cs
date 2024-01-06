using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public Transform playerCam;
	
	Rigidbody rb;
	float x, y;
	
	Vector3 direction;
	bool isMoving = false;

	public float maxSpeed = 10f;
	public float moveSpeed = 5f;
	[Space]
	
	public float jumpForce = 10f;
	int jumpPress;
	
	public bool isGrounded;
	bool isWall;
	
	public Transform groundCheck;
	public float checkSize = 1f;
	public LayerMask whatIsGround, whatIsWall;
	[Space]
	
	public Vector3 crouchSize;
	Vector3 startSize;
	bool isCrouching = false;
	[Space]
	
	public Transform head;
	public Transform orientation;
	bool isVolting = false;
	bool headContact, feetContact;
	int hasVolted = 0;
	
    void Start(){
        rb = GetComponent<Rigidbody>();
		startSize = transform.localScale;
    }

    void MyInput(){
		x = Input.GetAxisRaw("Horizontal");
		y = Input.GetAxisRaw("Vertical");
		
		jumpPress = Input.GetKeyDown(KeyCode.Space) ? 1 : 0;
		
		direction = transform.forward * y + transform.right * x;
		
		//in the editor set the friction to 0 for the player,this is a better way of having more control over the "friction" and general feel
		if (direction.magnitude == 0){
			rb.drag = 2;
		}else if (direction.magnitude != 0){
			rb.drag = 4;
		}
		
		//performs crouch/slide
		if (Input.GetKeyDown(KeyCode.LeftShift)){
			isCrouching = true;
		}else if (Input.GetKeyUp(KeyCode.LeftShift)){
			isCrouching = false;
		}
	}
	
    void Update(){
        MyInput();
		DetectVolt();

		isGrounded = Physics.CheckSphere(groundCheck.position, checkSize, whatIsGround);
		if (isCrouching){transform.localScale = crouchSize;}else{transform.localScale = startSize;}
		
		//checks for wall
		isWall = Physics.CheckSphere(groundCheck.position, 0.6f, whatIsWall);
    }
	
	void FixedUpdate(){
		if (rb.velocity.magnitude < maxSpeed){Movement();}
		if (isGrounded){Jump(); hasVolted = 0;}
		
		//makes sure that the player can only volt once before having to touch the ground
		if (!isGrounded && !isCrouching && isVolting && hasVolted == 0){
			rb.AddForce(transform.up * 12f, ForceMode.Impulse);
			hasVolted = 1;
		}
		
		WallRun();	
	}
	
	void Movement(){
		rb.AddForce(direction.normalized * moveSpeed * Time.fixedDeltaTime, ForceMode.Acceleration);
	}
	
	void Jump(){
		rb.AddForce(transform.up * jumpForce * jumpPress, ForceMode.Impulse);
	}
	
	
	void DetectVolt(){
		headContact = Physics.CheckSphere(head.position, 0.6f, whatIsGround);
		feetContact = Physics.CheckSphere(orientation.position, 0.6f, whatIsGround);
		
		//checks if head is not contact but feet are, if so then volt
		if (!headContact && feetContact){
			isVolting = true;
		}else{
			isVolting = false;
		}
	}
	
	void WallRun(){
		//checks if the player is next to a wall but isn't crouching/sliding
		if (isWall && !isCrouching){
			moveSpeed = 2400;
			rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
			playerCam.rotation = Quaternion.Euler(playerCam.rotation.x, playerCam.rotation.y, 15 * x);
		}else if (!isWall && isCrouching){
			moveSpeed = 1800;
		}else if (!isWall){
			rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
			moveSpeed = 1200;
		}
	}
	
	
	//plays or stops any particle effect
	void PlayEffect(ParticleSystem efx){
		efx.Play();
	}
	
	void StopEffect(ParticleSystem efx){
		efx.Stop();
	}
	
	
	//draws the groundcheck as a visible red line
	void OnDrawGizmosSelected(){
		if (groundCheck == null)
			return;
		
		Debug.DrawLine(groundCheck.position, new Vector3 (groundCheck.position.x, checkSize * -1, groundCheck.position.z), Color.red);
	}
}
