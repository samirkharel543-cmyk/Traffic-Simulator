using UnityEngine;

public class trafficAI : MonoBehaviour
{
    public Transform selfCarsParent;
    Transform[] selfCars;
    public Transform lane1;
    public Transform lane2;
    Transform[] nodesLane2;
    int numberOfCarsForLane2;
    public Transform lane2CarsParent;
    Transform[] selfCarsLane2;

    Transform[] nodes;
    int numberOfNodes;
    int numberOfSelfCars;
    public float driveSpeed=0.2f;
    public float reachDistance = 0.5f;
    public Vector3 meshOffSet;
    public float rotationSpeed=2f;
    int[] currentIndexOfEachCar;
    void Start()
    {
        

        numberOfSelfCars = selfCarsParent.childCount;

        currentIndexOfEachCar = new int[numberOfSelfCars];
        for(int i = 0; i < currentIndexOfEachCar.Length; i++)
        {
            currentIndexOfEachCar[i] = 0;
        }
       
        numberOfNodes = lane1.childCount;
        nodes = new Transform[numberOfNodes];
        for (int i = 0; i < numberOfNodes; i++)
        {
            nodes[i] = lane1.GetChild(i);
            //Debug.Log("Accessing Node child: " + i + ": " + nodes[i].name);

        }
        selfCars = new Transform[numberOfSelfCars];
        for (int i = 0; i < numberOfSelfCars; i++)
        {
            selfCars[i] = selfCarsParent.GetChild(i);
            var selfCarStartNode=i+1;
            Vector3 selfCarStartPosition = nodes[selfCarStartNode].position;
            var y_offsetOfselfCarWithRoadPlane = 1.5f;
            selfCars[i].position = new Vector3(selfCarStartPosition.x, nodes[1].position.y+2f, selfCarStartPosition.z);
            selfCars[i].transform.LookAt(nodes[selfCarStartNode+1].position);
            currentIndexOfEachCar[i] = selfCarStartNode+1;
        }

        var numberOfNodesInLane2 = lane2.childCount;
        nodesLane2 = new Transform[numberOfNodesInLane2];
        for (int i = 0; i < numberOfNodesInLane2; i++)
        {
            nodesLane2[i] = lane2.GetChild(i);
            //Debug.Log("Accessing Node child: " + i + ": " + nodes[i].name);

        }

        numberOfCarsForLane2 = lane2CarsParent.childCount;
        selfCarsLane2 = new Transform[numberOfCarsForLane2];
        for (int i = 0; i < numberOfCarsForLane2; i++)
        {
            selfCarsLane2[i] = lane2CarsParent.GetChild(i);
            var selfCarStartNode = i + 1;
            Vector3 selfCarLane2StartPosition = nodesLane2[selfCarStartNode].position;
            selfCarsLane2[i].position = new Vector3(selfCarLane2StartPosition.x, nodesLane2[1].position.y + 2f, selfCarLane2StartPosition.z);
            //selfCarsLane2[i].transform.LookAt(-nodesLane2[selfCarStartNode + 1].position);
            selfCarsLane2[i].rotation = selfCars[i].rotation * Quaternion.Euler(0, 180, 0);


        }

    }
    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < numberOfSelfCars; i++)
        {
            if(currentIndexOfEachCar[i] == numberOfNodes - 1)
            {
                Debug.Log("target reached by car: " + i);
            }
            else
            {
                var y_offsetOfSelfCarWithRoadPlane = 1.54f;
                Vector3 targetPos = nodes[currentIndexOfEachCar[i]].position;
                targetPos = new Vector3(targetPos.x, selfCars[i].position.y, targetPos.z);
                selfCars[i].position = Vector3.MoveTowards(selfCars[i].position, targetPos, driveSpeed * Time.deltaTime);
                //Debug.Log("Current Node Index of car[" + i + "]: " + currentIndexOfEachCar[i]);
                Vector3 lookDir = (targetPos - selfCars[i].position).normalized;

                if (lookDir != Vector3.zero)
                {
                    Quaternion lookAngle = Quaternion.LookRotation(lookDir);
                    selfCars[i].rotation = Quaternion.Slerp(selfCars[i].rotation, lookAngle, rotationSpeed * Time.deltaTime);

                }
                if (currentIndexOfEachCar[i] == numberOfNodes - 1)
                {
                    Debug.Log("All nodes reached by the car[" + i + "]..");
                    //currentIndexOfEachCar[i] = 0;
                }
                else if (Vector3.Distance(selfCars[i].position, targetPos) < reachDistance)
                {
                    currentIndexOfEachCar[i]++;

                }
            }
           
           
            
        }

       

       
       
    }
    
}
