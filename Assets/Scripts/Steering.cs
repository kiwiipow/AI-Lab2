using System.Drawing;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class SteeringBehaviours : MonoBehaviour
{
    public GameObject target;
    public GameObject[] HidingSpots;
    public GameObject[] waypoints;
    private NavMeshAgent agent;

    public float stopDistance;
    public float acceleration;
    public float maxSpeed;
    public float turnAcceleration;
    public float maxTurnSpeed;
    private float freq;
    public string SteeringState;
    public float fleeDistance;
    public float slowDownRadius;

    //wander parameters
    public float wanderRadius;
    public float WanderOffset; //never set to more than agent height *2 to avoid performance issues
    private float wanderWaitCounter = 0;
    public float wanderMaxWait = 2040;
    private enum ESTATE { Wandering, Stopped }
    private ESTATE wanderState;


    //hide parameter
    public float awarenesDistance;

    //patrol parameters
    public int destPoint;
    private static ILogger logger = Debug.unityLogger;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
       // agent.transform.position = new Vector3(0, 0, 0); //ChangeEvent for random numbers
        agent.speed = maxSpeed;
        agent.acceleration = acceleration;
        agent.angularSpeed = maxTurnSpeed;
        agent.stoppingDistance = stopDistance;
        wanderState = ESTATE.Stopped;
    }
    void Update()
    {
        //to avoid seeking if the target is within stop distance and then it just skips whole function
        if (Vector3.Distance(target.transform.position, transform.position) < stopDistance) return;

        // Reduce calls to Steering methods
        freq += Time.deltaTime;
        if ((freq > 0.5) && SteeringState != "Wandering")
        {
            switch (SteeringState)
            {
                case "Seek":
                    if (freq > 0.5)
                    {
                        if (Vector3.Distance(target.transform.position, transform.position) < agent.stoppingDistance)
                        {
                            return;
                        }
                        else
                        {
                            seek();
                        }
                    }
                       
                    break;

                case "Flee":
                    flee();
                    break;

               

                case "Pursue":
                    if (Vector3.Distance(target.transform.position, transform.position) < agent.stoppingDistance)
                    {
                        agent.isStopped = true;
                        return;
                    }
                    else
                    {
                        agent.isStopped = false;
                        pursue();
                    }
                    
                    break;

                case "Evade":
                    evade();
                    break;

                case "Hide":
                    hide();
                    break;

                case "Patrol":
                    
                        patrol();
                    

                    break;

                default:
                    break;
            }

            freq -= 0.5f;

        }
        else if(SteeringState == "Wandering")
        {
             
            if (wanderState == ESTATE.Stopped)
            {
                wanderWaitCounter = wanderWaitCounter - Time.deltaTime;
                //logger.Log("counter going down");
                //logger.Log(wanderWaitCounter);
                if (wanderWaitCounter <= 1)
                {
                    //logger.Log("counter has run out");
                    ChangeWanderState(ESTATE.Wandering);
                }

            }
            else if (wanderState == ESTATE.Wandering)
            {
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    //logger.Log("was wandering and arrived to destination");
                    ChangeWanderState(ESTATE.Stopped);
                }
            }
        }
    }

    void seek()//just for reference
    {
            agent.SetDestination(target.transform.position);
    }

    void flee()//just for reference
    {

        Vector3 dir = transform.position - target.transform.position;
        dir.y = 0f;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(dir, out hit, fleeDistance, NavMesh.AllAreas))
        {
            if ((transform.position - target.transform.position).magnitude < fleeDistance)
            {
                agent.SetDestination(transform.position - target.transform.position);
            } 
            else { return; }
        }
       
    }

    void ChangeWanderState(ESTATE s)
    {
        wanderState = s;
        if(wanderState == ESTATE.Wandering)
        {
            //logger.Log("is wandering");
            agent.isStopped = false;
            wander();
        }
        else if(wanderState == ESTATE.Stopped)
        {

            //logger.Log("is stop");
            agent.isStopped = true;
            wanderWaitCounter = wanderMaxWait;
        }
    }
    void wander()
    {
        
            Vector3 localTarget = Random.insideUnitSphere * wanderRadius;
            localTarget.y = 0f;

            Vector3 worldTarget = transform.position + localTarget;
            worldTarget.y = 0f;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(worldTarget, out hit, WanderOffset, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }



    }

    void pursue()
    {
        if ((transform.position - target.transform.position).magnitude < slowDownRadius)
        {
            agent.speed = (maxSpeed / agent.remainingDistance ) - slowDownRadius;
        }

        Vector3 toTarget = target.transform.position - transform.position;
        float lookAhead = toTarget.magnitude / agent.speed;
        agent.SetDestination(target.transform.position + target.transform.forward * lookAhead);
    }
    void evade()
    {
        //maybe addlogic?
        if ((transform.position - target.transform.position).magnitude < fleeDistance) //if there is enemy within a close area
        {
            hide();
        }
        else
        {
            return;
        }
    }

    void hide()
    {

        //check if any of the colliders in the array are near
       

        if (false) //if yes calculate from wich side you aproaching
        {
            //calculate from wich side you aproaching
            // boost speed and move to the oposide side
        }
        else //run away 
        {
            Vector3 dir = transform.position - target.transform.position;
            dir.y = 0f;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(dir, out hit, fleeDistance, NavMesh.AllAreas))
            {
                agent.SetDestination(transform.position - target.transform.position);
            }
        }
        //GameObject targetSpot = HidingSpots[0];

        //if ((target.transform.position - transform.position).magnitude < awarenesDistance)
        //{
        //    for (int i = 0; i < HidingSpots.Length; i++)
        //    {
        //        if (HidingSpots[i] == targetSpot) continue;

        //        if ((HidingSpots[i].transform.position - transform.position).magnitude < (targetSpot.transform.position).magnitude)
        //        {
        //            targetSpot = HidingSpots[i];

        //        }
        //    }
        //}

        //Vector3 dir = (targetSpot.transform.position) - (target.transform.position);
        //Ray backRay = new Ray(targetSpot.transform.position, -dir.normalized);

        //RaycastHit hit;
        //targetSpot.GetComponent<Collider>().Raycast(backRay, out hit, 50f);

        //agent.SetDestination(hit.point + dir.normalized);


        //float distance = Mathf.Infinity;    // Track the shortest distance to a hiding spot
        //Vector3 chosenSpot = Vector3.zero;  // The final hiding position
        //// Iterate over all hiding spots to find the best one
        //for (int i = 0; i < HidingSpots.Length; ++i)
        //{
        //    // Compute the direction from the target to the hiding spot (hideDir) in this case the tree or the house
        //    Vector3 hideDir = HidingSpots[i].transform.position - target.transform.position;
        //    // Calculate the hide position, behind the obstacle (6 units away from the target)
        //    Vector3 hidePos = HidingSpots[i].transform.position + hideDir.normalized * 6; // TODO: try modified the number to see what happens

        //    // Choose the closest hiding spot to the bot
        //    if (Vector3.Distance(this.transform.position, hidePos) < distance)
        //    {
        //        chosenSpot = hidePos;
        //        distance = Vector3.Distance(this.transform.position, hidePos);
        //    }
        //    // Move to the chosen hiding spot
        //    agent.SetDestination(target.transform.position);

        //}
    }

    void patrol()
    {
        // Returns if the waypoints array is empty just in case they are not set in unity
        if (waypoints.Length == 0)
        {
            return;
        }
        else
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                // Set the next waypoint destination
                agent.destination = waypoints[destPoint].transform.position;
                // Choose the next point in the array as the destination and add the %lenght to go back to first in case of it being the last
                destPoint = (destPoint + 1) % waypoints.Length;
            }

        }
    }


}

