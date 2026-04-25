using UnityEngine;

public class PlayerCar : MonoBehaviour
{
    public Rigidbody rigid;
    public Transform rigidBody;
    public WheelCollider wheel1, wheel2, wheel3, wheel4;
    public Transform meshFL, meshFR, meshBL, meshBR;
    public float driveSpeed = 5000f;
    public float steerSpeed = 25f;
    float horizontalInput, verticalInput;
    //public Transform camera;
    public float decelerateTorque = 10000f;
    public float brakeTorque = 10000f;
    Vector3 carPosChange;
    Vector3 CarPrevPos;
    Quaternion carPrevRotation;
    public float cameraDistance = 50f;
    public float cameraSpeed = 10.0f;
    public float cameraHeight = 30f;
    public float cameraRotationSpeed = 5f;
    


    void Start()
    {
        //rigid.centerOfMass = new Vector3(-46.81f, -0.9f, 0.12f);
        rigid.centerOfMass = new Vector3(0, -1.9f, 0f);

    }

    void Update()
    {


        //Debug.Log("Rigid body position: " + rigidBody.position);
        //Debug.Log("Rigid body com: " + rigid.centerOfMass);

        if (carPrevRotation == null)
        {
            carPrevRotation = rigidBody.rotation;
        }
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        //Debug.Log("Horizontal Input: " + horizontalInput);
        //Debug.Log("Vertical Input: " + verticalInput);
        if (CarPrevPos == null)
        {
            CarPrevPos = rigidBody.position;
        }

        Vector3 carPosition = new Vector3(rigidBody.position.x, rigidBody.position.y, rigidBody.position.z);
        carPosChange = CarPrevPos - carPosition;
        //camera.position = camera.position - carPosChange;
        CarPrevPos = carPosition;

        //Debug.Log("car previous rotation: " + carPrevRotation);
        //Debug.Log("car current rotaion: " + rigidBody.rotation);
        carPrevRotation = rigidBody.rotation;
        //camera.rotation = rigidBody.rotation;
        Vector3 carBackwardVector = -rigidBody.forward;
    }
    void FixedUpdate()
    {
        float motor = Input.GetAxis("Vertical");
        if (motor != 0f)
        {
            wheel1.brakeTorque = 0f;
            wheel2.brakeTorque = 0f;
            wheel3.brakeTorque = 0f;
            wheel4.brakeTorque = 0f;

            wheel1.motorTorque = motor * driveSpeed;
            wheel2.motorTorque = motor * driveSpeed;
            wheel3.motorTorque = motor * driveSpeed;
            wheel4.motorTorque = motor * driveSpeed;
        }
        else
        {
            Decelerate();
        }

        Vector3 pos1;
        Quaternion rot1;
        wheel1.GetWorldPose(out pos1, out rot1);
        meshFL.position = pos1;
        meshFL.rotation = rot1;

        Vector3 pos2;
        Quaternion rot2;
        wheel2.GetWorldPose(out pos2, out rot2);
        meshFR.position = pos2;
        meshFR.rotation = rot2;



        Vector3 pos3;
        Quaternion rot3;
        wheel3.GetWorldPose(out pos3, out rot3);
        meshBL.position = pos3;
        meshBL.rotation = rot3;


        Vector3 pos4;
        Quaternion rot4;
        wheel4.GetWorldPose(out pos4, out rot4);
        meshBR.position = pos4;
        meshBR.rotation = rot4;



        float steering = horizontalInput * steerSpeed;
        wheel1.steerAngle = steering;
        wheel2.steerAngle = steering;

        if (Input.GetKey(KeyCode.Space))
        {
            wheel1.motorTorque = 0;
            wheel2.motorTorque = 0;
            wheel3.motorTorque = 0;
            wheel4.motorTorque = 0;
            wheel1.brakeTorque = brakeTorque;
            wheel2.brakeTorque = brakeTorque;
            wheel3.brakeTorque = brakeTorque;
            wheel4.brakeTorque = brakeTorque;

        }
    }


    void Decelerate()
    {
        float rollingResistance = 1000f;
        wheel1.brakeTorque = rollingResistance;
        wheel2.brakeTorque = rollingResistance;
        wheel3.brakeTorque = rollingResistance;
        wheel4.brakeTorque = rollingResistance;

        wheel1.motorTorque = 0;
        wheel2.motorTorque = 0;
        wheel3.motorTorque = 0;
        wheel4.motorTorque = 0;
    }
    //private void LateUpdate()
    //{
    //    Vector3 targetPosition = rigidBody.position + (rigidBody.forward * cameraDistance) + (Vector3.up * cameraHeight);
    //    camera.position = Vector3.Lerp(camera.position, targetPosition, cameraSpeed * Time.deltaTime);
    //    Vector3 lookDirection = rigidBody.position - camera.position;
    //    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
    //    camera.rotation = Quaternion.Slerp(camera.rotation, targetRotation, cameraRotationSpeed * Time.deltaTime);
    //}
}
