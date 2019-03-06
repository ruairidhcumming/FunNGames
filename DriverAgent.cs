using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
public class DriverAgent : Agent {
    
    public List<AxleInfo> axleInfos; // the information about each individual axle
    public float maxMotorTorque; // maximum torque the motor can apply to wheel
    public float maxSteeringAngle;
    DriverAcademy academy;
    public GameObject Target;
    public GameObject Vehicle;
    public float proximityReward;
    public float crashPenalty;
    public float timePenalty;
    public float pickUpReward;
    public float facingReward;
    public float movingTowardsReward;
    public float turningReward;
    int Score;

    List<GameObject> GetClosestWalls()
    {
        List<GameObject> walls = new List<GameObject>();
           foreach (GameObject w in GameObject.FindGameObjectsWithTag("wall")){
            walls.Add(w);
        };
        GameObject nearest = walls[0];//assumes there are two walls bad ruairidh
       
        foreach (GameObject w in walls) {
            
            if (
                Vector3.Distance(Vehicle.transform.position, w.GetComponent<Collider>().ClosestPointOnBounds(Vehicle.transform.position)) <
                Vector3.Distance(Vehicle.transform.position, nearest.GetComponent<Collider>().ClosestPointOnBounds(Vehicle.transform.position)))
            {

                 nearest = w;
                
            }
            

        };
        walls.Remove(nearest);
        GameObject secondnearest = walls[1];
        foreach (GameObject w in walls)
        {
            if (
                Vector3.Distance(Vehicle.transform.position, w.GetComponent<Collider>().ClosestPointOnBounds(Vehicle.transform.position)) <
                Vector3.Distance(Vehicle.transform.position, secondnearest.GetComponent<Collider>().ClosestPointOnBounds(Vehicle.transform.position)))
            {

                secondnearest = w;


            }
        };

        List<GameObject> opt = new List<GameObject>();
        opt.Add(nearest);
        opt.Add(secondnearest);
        return opt;
    }

    public Vector3 directionTo(GameObject Target)
    {
        Vector3 dif = Target.transform.position - Vehicle.transform.position;
        Vector3 dir = dif.normalized;
        
        Debug.DrawRay(Vehicle.transform.position,dir*10);
        return dir;
    }
    public float distanceTo(GameObject Target)
    {
        Vector3 dif = Target.transform.position - this.transform.position;
        float dis =  Mathf.Sqrt(dif.sqrMagnitude);
        return dis;
    }

    public override void CollectObservations()
    {
        AddVectorObs(Mathf.Min(Vehicle.GetComponent<Rigidbody>().velocity.magnitude,10)/10);
        AddVectorObs(directionTo(Target));
        AddVectorObs(Vehicle.transform.forward);
        AddVectorObs(Mathf.Min(distanceTo(Target),10)/10);
        Debug.DrawRay(Vehicle.transform.position, directionTo(Target) * distanceTo(Target), Color.green);
        foreach (GameObject wall in GetClosestWalls())
        {
            
            AddVectorObs((Vehicle.transform.position-wall.GetComponent<Collider>().ClosestPointOnBounds(Vehicle.transform.position)).normalized);
            AddVectorObs(Mathf.Min(
                (Vehicle.transform.position - wall.GetComponent<Collider>().ClosestPointOnBounds(Vehicle.transform.position)).sqrMagnitude,10)/10);
            Debug.DrawLine(Vehicle.transform.position, wall.GetComponent<Collider>().ClosestPointOnBounds(Vehicle.transform.position), Color.blue);
            
        }
    }
    public override void AgentAction(float[] vectorAction, string textAction)
    {   
        CollectObservations();
        
        float steering = Mathf.Clamp(vectorAction[0], -1, 1)*50f;
        float motor = Mathf.Clamp(vectorAction[1], -1, 1)*500f;
     
       
        if (Vehicle.transform.rotation.eulerAngles.z>=90.0f &&
            Vehicle.transform.rotation.eulerAngles.z <= 270.0f)
        {
            Debug.Log("BS crashPenalty");
            AddReward(-crashPenalty);
            Done();
        }

        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                //Debug.Log(steering);
                
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.motor)
            {
                //Debug.Log(motor);
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
        }
        AddReward(-timePenalty);
         AddReward((100 - distanceTo(Target)) * proximityReward/1000);
        RewardFunctionMovingTowards();


         RewardFunctionTurningTowards();

        //float facingDot = Vector3.Dot(directionTo(Target).normalized, Vehicle.transform.forward.normalized);
        //AddReward(facingReward * facingDot);
        
         
    }
    void MovePickup()
    {
        float xpos;
        float ymin;
        float ymax;
        float targetsize;
        academy = FindObjectOfType<DriverAcademy>();
        xpos = academy.resetParameters["target_x_bounds"];
        //Debug.Log(xpos);
        ymin = academy.resetParameters["target_z_min"];
        ymax = academy.resetParameters["target_z_max"];
        targetsize = academy.resetParameters["target_size"];
        Target.transform.position = Target.transform.parent.position + new Vector3(Random.Range(-1 * xpos, xpos), 0.2f, Random.Range(ymin, ymax));
        Target.transform.localScale = new Vector3(targetsize, targetsize, targetsize);

        targetsize = academy.resetParameters["target_size"];


    }
    void OnTriggerEnter(Collider col)
    {
        //Debug.Log("something");

        if (col.gameObject.tag == "Pick Up"){
            
            AddReward(pickUpReward);
            Debug.Log("pickup");
            academy = FindObjectOfType<DriverAcademy>();
            if (academy.resetParameters["maxscore"] > 1.0)
            {
                MovePickup();
            }
            if (academy.resetParameters["maxscore"] == 1.0)
            {
               
                Done();                
                AgentReset();
            }
            //if (currriculum stage == early){
            //Done();
          
            //AgentReset();
            //}
        }
        if (col.gameObject.tag == "wall")
        {

            AddReward(-crashPenalty);
            Debug.Log("agent hit wall");

            academy = FindObjectOfType<DriverAcademy>();

            float rot = academy.resetParameters["veh_rotation"];


            Vehicle.transform.position = Vehicle.transform.parent.position + new Vector3(0f, 2f, 0f);
            Vehicle.transform.eulerAngles = new Vector3(0f, Random.Range(0f, rot), 0f);
        }
    }
    void RewardFunctionMovingTowards()
    {
        float movingTowardsDot;
        movingTowardsDot = Vector3.Dot(Vehicle.GetComponent<Rigidbody>().velocity, directionTo(Target));
        AddReward( movingTowardsReward * movingTowardsDot);
        Debug.Log(movingTowardsDot);
    }
    void RewardFunctionTurningTowards()
    {

        Rigidbody Rb;
        float turningScalar;
        Rb = Vehicle.GetComponent<Rigidbody>();
        turningScalar = Vector3.Dot(Rb.angularVelocity, Vector3.Cross(Vehicle.transform.forward.normalized, directionTo(Target)));
        Debug.Log(turningScalar * turningReward);
        AddReward(turningScalar * turningReward);
    }
    // Update is called once per frame
    public override void  AgentReset() {
        MovePickup();
        academy = FindObjectOfType<DriverAcademy>();
        
        float rot = academy.resetParameters["veh_rotation"];
        
       
        Vehicle.transform.position = Vehicle.transform.parent.position+new Vector3(0f,2f,0f);
        Vehicle.transform.eulerAngles = new Vector3(0f, Random.Range(0f,rot), 0f);
       
	}
}
