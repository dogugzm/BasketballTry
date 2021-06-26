using System.Collections;
using UnityEngine;
using DG.Tweening;


public enum PlayerState
{
    PATROL,
    SHOOT,
    BLOCK
}
public class PlayerMove : MonoBehaviour
{

    // put the points from unity interface
    public Transform[] wayPointList;

    public int currentWayPoint = 0;

    Transform targetWayPoint;
    public GameObject Hoop;

    public float speed = 4f;

    public bool toLeft, toRight;

    public Animator animator;
    public Rigidbody rb;
    PlayerState currentState;


    // Start is called before the first frame update
    void Start()
    {
        toLeft = true;
        toRight = false;
        currentState = PlayerState.PATROL;
    }

    // Update is called once per frame
    void Update()
    {
        // check if we have somewere to walk
        if (currentWayPoint < this.wayPointList.Length && currentState == PlayerState.PATROL)
        {
            if (targetWayPoint == null)
            {
                targetWayPoint = wayPointList[currentWayPoint];

            }
            //StartCoroutine("Patrol");
            Patrol();
        }


        if (Input.GetMouseButtonDown(0))
        {
            RotateTowardsHoop();
            currentState = PlayerState.SHOOT;
            StartCoroutine("Shoot");

        }
        AnimationControl();

    }

    public void AnimationControl()
    {
       if(currentState == PlayerState.PATROL)
        {
            animator.SetTrigger("Running");
            
        }
        else if (currentState == PlayerState.SHOOT)
        {
            animator.SetTrigger("Throw");
        }

    }

    public IEnumerator Shoot()
    {
        
        Debug.Log("Shooted");
        yield return new WaitForSeconds(2f);
        currentState = PlayerState.PATROL;
    }

    public void Patrol()
    {

        // rotate towards the target
        transform.forward = Vector3.RotateTowards(transform.forward, targetWayPoint.position - transform.position, speed * Time.deltaTime, 0.0f);

        // move towards the target
        transform.position = Vector3.MoveTowards(transform.position, targetWayPoint.position, speed * Time.deltaTime);

        if (transform.position == targetWayPoint.position)
        {
            if (toLeft)
            {
                currentWayPoint++;
            }
            else if (toRight)
            {
                currentWayPoint--;
            }

            targetWayPoint = wayPointList[currentWayPoint];

            if (targetWayPoint == wayPointList[wayPointList.Length - 1])
            {
                toRight = true;
                toLeft = false;
            }
            else if (targetWayPoint == wayPointList[0])
            {
                toLeft = true;
                toRight = false;
            }
        }


        //yield break;
    }

    public void RotateTowardsHoop()
    {
        transform.DORotate(Hoop.transform.position, 1f);
    }

}
