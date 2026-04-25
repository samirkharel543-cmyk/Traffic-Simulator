using UnityEngine;
using System;
using Unity.Cinemachine;
using System.Collections.Generic;
using Unity.GraphToolkit.Editor; // Required for List

public class GenRoads : MonoBehaviour
{
    public int noOfNodes = 50;
    public float roadUnitSize = 30f;
    public Transform roadBuilder;
    public Transform roadUnits;
    public GameObject roadPrefab;
    Vector3 startPosition;
    Vector3 endPosition;
    Quaternion startRotation;
    public GameObject playerCarPrefab;
    GameObject playerCar;
    public GameObject cineMachineCamera;
    public PhysicsMaterial floorPhyMat;
    Vector3 prevDirection;
    public GameObject trafficManager;
    public trafficAI trafficAiScript;
    public GameObject lane1, lane2;
    GameObject childNodeLane2;
    bool leftTurningPointNode;
    bool rightTurningPointNode;


    private List<Vector3> occupiedPositions = new List<Vector3>();

    enum directions { NE, NW, SE, SW }

    void Start()
    {
        leftTurningPointNode = false;
        rightTurningPointNode = false;

        lane1 = new GameObject("Lane1");
        lane2 = new GameObject("Lane2");
        lane1.transform.SetParent(this.transform);
        lane2.transform.SetParent(this.transform);
        trafficAiScript = trafficManager.GetComponent<trafficAI>();
        prevDirection = Vector3.forward;
        BuildTrack();
        trafficManager.SetActive(true);
        AddRoads();
        AddPlayerCar();

    }
    void AddPlayerCar()
    {
        playerCar = Instantiate(playerCarPrefab, new Vector3(startPosition.x,startPosition.y+2f,startPosition.z),startRotation);
        var vcam = cineMachineCamera.GetComponent<CinemachineCamera>();
        if (vcam != null)
        {
            vcam.Follow = playerCar.transform;
            vcam.LookAt = playerCar.transform;

            var thirdPersonFollow= cineMachineCamera.GetComponent<CinemachineThirdPersonFollow>();
            thirdPersonFollow.ShoulderOffset = new Vector3(0.5f, 5.5999999f, -8.93000031f);
        }
        else
        {
            Debug.LogError("Cinemachine Gameobject missing");
        }
    }
    void AddRoads()
    {
        Transform[] nodes = lane1.GetComponentsInChildren<Transform>();
        startPosition = nodes[1].position;
        startRotation = nodes[2].rotation;
        
        

        for (int i = 1; i < noOfNodes; i++)
        {
            Vector3 shiftVector = (nodes[i].right.normalized) * roadUnitSize / 4;
            Vector3 roadUnitPos = nodes[i].position + shiftVector;

            GameObject road = Instantiate(roadPrefab, nodes[i].position, Quaternion.identity);
            road.name = "plane" + i;
            road.transform.localScale = new Vector3(30f / 10f,1f,30f/10f);
            road.transform.SetParent(roadUnits.transform);
            road.transform.position = nodes[i].position;

            road.transform.LookAt(nodes[i+1].position);
            road.transform.position = roadUnitPos;

            road.AddComponent<BoxCollider>();
            road.GetComponent <BoxCollider>().material = floorPhyMat;
        }
    }
    void BuildTrack()
    {
        occupiedPositions.Add(roadBuilder.position);

        for (int i = 0; i < noOfNodes; i++)
        {
            Vector3 targetPosition = Vector3.zero;
            Quaternion targetRotation = Quaternion.identity;
            bool foundValidSpot = false;
            int attempts = 0;

            /
            while (!foundValidSpot && attempts < 10)
            {
                int randomDirectionIndex = UnityEngine.Random.Range(0, 4);
                roadUnitSize = 30f;
                Vector3 directionVector = roadBuilder.forward;
                prevDirection = Vector3.forward;

                if (UnityEngine.Random.Range(0,10) < 2)
                {
                    //directionVector = GetDirectionVector((directions)randomDirectionIndex);
                    var randomNumber = UnityEngine.Random.Range(0, 10);
                        if(randomNumber < 5)
                    {
                        if(prevDirection != Vector3.right)
                        {
                            directionVector = roadBuilder.right;
                            prevDirection = Vector3.right;

                        }
                        else
                        {
                            directionVector = roadBuilder.forward;
                            prevDirection = Vector3.forward;
                        }
                    }
                    else
                    {
                        if(prevDirection != -Vector3.right)
                        {
                            directionVector = -roadBuilder.right;
                            prevDirection = -Vector3.right;

                        }
                        else
                        {
                            directionVector = roadBuilder.forward;
                            prevDirection = Vector3.forward;

                        }
                    }

                }
                targetRotation = Quaternion.LookRotation(directionVector);

                targetPosition = roadBuilder.position + (directionVector.normalized * roadUnitSize);

                if (!IsPositionOccupied(targetPosition))
                {
                    foundValidSpot = true;
                }
                attempts++;
            }

            if (foundValidSpot)
            {
                roadBuilder.position = targetPosition;
                roadBuilder.rotation = targetRotation;
                
                //Debug.Log("roadBuilderRotation at: " + i + roadBuilder.rotation);
                occupiedPositions.Add(targetPosition);

                GameObject childNode = new GameObject("node" + i);
                childNode.transform.position = roadBuilder.position;
                childNode.transform.rotation = targetRotation;
                childNode.transform.SetParent(lane1.transform);

            }
            else
            {
                Debug.LogWarning("Road got stuck at node " + i + " - no free directions!");
                break;
            }

        }
        trafficAiScript.lane1 = lane1.transform;
        BuildLane2();

    }
    void BuildLane2()
    {
        Vector3 nextLane2NodePos = new Vector3(0,0,0);
        Quaternion nextLane2NodeRot = new Quaternion(0,0,0,0) ;

        var noOfLane1Nodes = lane1.transform.childCount;
        for (int i = 0; i < noOfLane1Nodes ; i++)
        {
            if (i < noOfLane1Nodes - 2)
            {
                float signedAngle = Vector3.SignedAngle(lane1.transform.GetChild(i).transform.rotation * Vector3.forward, lane1.transform.GetChild(i + 1).transform.rotation * Vector3.forward, Vector3.up);
                Debug.Log("SignedAngle at node: " + i + " " + signedAngle);
                if (signedAngle > 85f && signedAngle < 93f)
                {
                    Debug.Log("Turn right at node: " + i);
                    rightTurningPointNode = true;
                    //if(childNodeLane2 != null)
                    //{
                    //    Vector3 lastLane2NodePos = childNodeLane2.transform.position;
                    //    Vector3 specialRightLanePos = lastLane2NodePos + (childNodeLane2.transform.forward * roadUnitSize/2f);

                    //}
                }
                else if (signedAngle < -85f && signedAngle > -95f)
                {
                    Debug.Log("Turn left at node: " + i);
                    leftTurningPointNode = false;
                }
                else
                { 
                    leftTurningPointNode = false;
                    rightTurningPointNode = false;
                }
            }
            if (rightTurningPointNode && lane2.transform.GetChild(i-1)!= null)
            {
                Transform lastLane2Node = lane2.transform.GetChild(i-1);
                Vector3 NextLane2NodePos = lastLane2Node.position + (childNodeLane2.transform.forward * roadUnitSize / 2f);
                Quaternion NextLane2NodeRot = lastLane2Node.rotation;
            }
            //else if (leftTurningPointNode && lane2.transform.GetChild(i - 1) != null)
            //{
            //    Transform lastLane2Node = lane2.transform.GetChild(i - 1);
            //    Vector3 NextLane2NodePos = lastLane2Node.position + (childNodeLane2.transform.forward * roadUnitSize);
            //    Quaternion NextLane2NodeRot = lastLane2Node.rotation;

            //}
            else if(!rightTurningPointNode)
            {
                Transform lane1Node = lane1.transform.GetChild(i);
                Vector3 lane1Position = lane1Node.position;
                Vector3 rightLaneVector = lane1Node.right.normalized * (roadUnitSize / 2);
                nextLane2NodePos = lane1Node.position + rightLaneVector;
                nextLane2NodeRot = lane1Node.rotation;
            }
            childNodeLane2 = new GameObject("node" + i);
            childNodeLane2.transform.position = nextLane2NodePos;
            childNodeLane2.transform.rotation = nextLane2NodeRot;
            childNodeLane2.transform.SetParent(lane2.transform);
            leftTurningPointNode = false;
            rightTurningPointNode = false;

            //Transform lane1Node = lane1.transform.GetChild(i);
            //Vector3 lane1Position = lane1Node.position;
            //Vector3 rightLaneVector = lane1Node.right.normalized * (roadUnitSize / 2);
            //Vector3 rightLanePos = lane1Node.position + rightLaneVector;
            
        }
        trafficAiScript.lane2 = lane2.transform;
    }

    Vector3 GetDirectionVector(directions dir)
    {
        switch (dir)
        {
            case directions.NE: return roadBuilder.forward + roadBuilder.right;
            case directions.NW: return roadBuilder.forward - roadBuilder.right;
            case directions.SE: return roadBuilder.right - roadBuilder.forward;
            case directions.SW: return -roadBuilder.right - roadBuilder.forward;
            default: return Vector3.zero;
        }
    }

    bool IsPositionOccupied(Vector3 pos)
    {
        foreach (Vector3 existingPos in occupiedPositions)
        {
            if (Vector3.Distance(existingPos, pos) < 1f) return true;
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        var Lane1ChildCount = lane1.transform.childCount;
        var Lane2ChildCount = lane2.transform.childCount;
        //Transform[] nodes = lane1.transform.GetChild();
        for (int i = 0; i < Lane1ChildCount; i++)
        {
            if (i == 0)
            {
                Gizmos.color = Color.red;

            }
            else if (i == Lane1ChildCount - 1)
            {
                Gizmos.color = Color.blue;

            }
            //if(i < Lane1ChildCount - 3)
            //{
            //    //float dotProduct = Vector3.Dot(lane1.transform.GetChild(i).transform.forward.normalized, lane1.transform.GetChild(i + 1).transform.forward.normalized);
            //    float signedAngle = Vector3.SignedAngle(lane1.transform.GetChild(i).transform.rotation * Vector3.forward, lane2.transform.GetChild(i + 1).transform.rotation * Vector3.forward,Vector3.up);
            //    Debug.Log("Node" + i + "." + "node" + (i + 1) + " =" + signedAngle);
            //}

            Gizmos.DrawSphere(lane1.transform.GetChild(i).position, 1f);
            Gizmos.color = Color.green;
            if (i < Lane1ChildCount - 2)
            {
                Gizmos.DrawLine(lane1.transform.GetChild(i).position, lane1.transform.GetChild(i + 1).position);
            }
        }

        for (int i = 0; i < Lane2ChildCount; i++)
        {
            if (i == Lane2ChildCount - 2)
            {
                Gizmos.color = Color.red;
            }
            else if (i == 0)
            {
                Gizmos.color = Color.blue;

            }

            Gizmos.DrawSphere(lane2.transform.GetChild(i).position, 1f);
            Gizmos.color = Color.green;
            if (i < Lane2ChildCount - 2)
            {
                Gizmos.DrawLine(lane2.transform.GetChild(i).position, lane2.transform.GetChild(i + 1).position);
            }

        }
    }
}