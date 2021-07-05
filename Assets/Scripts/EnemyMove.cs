using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum EnemyState 
{
    PATROL,
    BLOCK,
    IDLE
}

public class EnemyMove : MonoBehaviour
{

    public GameObject Player;
    public float speed;
    public bool toLeft, toRight;
    public GameObject[] wayPointListEnemy;

    public int currentWayPointEnemy = 0;

    GameObject targetWayPointEnemy;
    EnemyState currentState;

    public Animator animator;


    // Start is called before the first frame update
    void Start()
    {
        toLeft = true;
        toRight = false;
        currentState = EnemyState.PATROL;
        
        
    }
    public IEnumerator Block()
    {
        yield return new WaitForSeconds(1f);
        currentState = EnemyState.PATROL;
    }

    // Update is called once per frame
    void Update()
    {
        //if (Player.GetComponent<PlayerMove>().rotateEnemy ==true)
        //{
        //    RotateTowardsPlayer();
        //    Player.GetComponent<PlayerMove>().rotateEnemy = false;
        //}

        if (Player.GetComponent<PlayerMove>().currentState == PlayerState.PATROL)
        {
            currentState = EnemyState.PATROL;
        }
        else if (Player.GetComponent<PlayerMove>().currentState == PlayerState.IDLE)
        {
            currentState = EnemyState.IDLE;

        }
        else if (Player.GetComponent<PlayerMove>().currentState == PlayerState.SHOOT)
        {
            currentState = EnemyState.BLOCK;
            StartCoroutine("Block");

        }
        
        // check if we have somewere to walk
        if (Player.GetComponent<PlayerMove>().currentWayPoint < this.wayPointListEnemy.Length && currentState == EnemyState.PATROL)
        {
            if (targetWayPointEnemy == null)
            {
                
                targetWayPointEnemy = wayPointListEnemy[Player.GetComponent<PlayerMove>().currentWayPoint];

            }
            
            Fallow();
        }

        AnimationControl();
    }
    public void RotateTowardsPlayer()
    {
        
        transform.DOLookAt(Player.transform.position,0.5f);
    }

    public void Fallow()
    {
        // rotate towards the target
       transform.forward = Vector3.RotateTowards(transform.forward, targetWayPointEnemy.transform.position - transform.position, speed * Time.deltaTime, 0.0f);
       

        // move towards the target
       transform.position = Vector3.MoveTowards(transform.position, targetWayPointEnemy.transform.position, speed * Time.deltaTime);
        

        if (transform.position == targetWayPointEnemy.transform.position)
        {
            //if (toLeft)
            //{
            //    currentWayPointEnemy++;
            //}
            //else if (toRight)
            //{
            //    currentWayPointEnemy--;
            //}

            targetWayPointEnemy = wayPointListEnemy[Player.GetComponent<PlayerMove>().currentWayPoint];

            //if (targetWayPointEnemy == wayPointListEnemy[wayPointListEnemy.Length - 1])
            //{
            //    toRight = true;
            //    toLeft = false;
            //}
            //else if (targetWayPointEnemy == wayPointListEnemy[0])
            //{
            //    toLeft = true;
            //    toRight = false;
            //}
        }
    }

    public void AnimationControl()
    {
        if (currentState == EnemyState.PATROL)
        {
            animator.SetBool("Walk", true);
            animator.SetBool("Idle", false);
            animator.SetBool("Jump", false);

        }
        else if (currentState == EnemyState.IDLE)
        {
            RotateTowardsPlayer();
            animator.SetBool("Idle", true);
            animator.SetBool("Walk", false);
            animator.SetBool("Jump", false);

        }
        else if (currentState == EnemyState.BLOCK)
        {
            animator.SetBool("Jump", true);
            animator.SetBool("Walk", false);
            animator.SetBool("Idle", false);
        }

    }


}
