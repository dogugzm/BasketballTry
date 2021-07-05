using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using Cinemachine;
using System;
using UnityEngine.SceneManagement;

public enum PlayerState
{
    PATROL,
    SHOOT,
    IDLE
    
}
public enum CameraState
{
    LEFT,
    RIGHT,

}
public class PlayerMove : MonoBehaviour
{

    // put the points from unity interface
    public Transform[] wayPointList;

    public int currentWayPoint = 0;

    [System.NonSerialized]public Transform targetWayPoint;
    public GameObject Hoop;
    public GameObject Ball;
    public GameObject BallTarget;
    public GameObject Confettis;

    public GameObject newCamPos;
    public GameObject newCamPosRight;

    public Material blue;
    public Material red;


    public GameObject PlayerMaterialGameObject;
    public GameObject EnemyMaterialGameObject;

    public GameObject enemyHead;
    public GameObject blockTarget;
    public GameObject field;
    public GameObject hoopUp;
    public GameObject playerHead;

    public RawImage inCircle1;
    public RawImage outCircle1;
    public RawImage inCircle2;
    public RawImage outCircle2;
    public TextMeshProUGUI Timer;
    public TextMeshProUGUI Player1Score;
    public TextMeshProUGUI Player2Score;


    public TrailRenderer ballTrail;

    CinemachineVirtualCamera vcam;
    CinemachineBasicMultiChannelPerlin noise;

    public Animator EnemyAnimator;


    private float timeRemaining;

    public float speed = 4f;
    public float ballSpeed = 4f;


    public bool toLeft, toRight,rotateEnemy;

    public Animator animator;
    public Rigidbody rb;

    bool stopTime;
    public bool justOnce;

    private int score1;
    private int score2;

    bool maxPower;

    private float holdDownStartTime;
    public GameObject PointText;
    public GameObject BlockText;



    public int maxRight;
    public int currentRightPlayer = 0;
    public int currentRightEnemy = 0;
    public GameObject[] elementsPlayer;
    public GameObject[] elementsEnemy;

    public PlayerState currentState;
    public CameraState cameraState;

    //public void ColorUIPlayer()
    //{
    //    elementsPlayer[currentRightPlayer].SetActive(true);
    //    currentRightPlayer++;
    //}
    //public void ColorUIEnemy()
    //{
    //    elementsEnemy[currentRightEnemy].SetActive(true);
    //    currentRightEnemy++;
    //}

    // Start is called before the first frame update
    void Start()
    {
        maxRight = elementsPlayer.Length;
        DOTween.Init(true, true, LogBehaviour.Verbose).SetCapacity(1250, 50);
        toLeft = true;
        toRight = false;
        stopTime = false;
        //isCameraMoving = false;
        rotateEnemy = false;
        maxPower = false;
        justOnce = false;
        timeRemaining = 5f;
        score1 = 0;
        score2 = 0;

        currentState = PlayerState.PATROL;
        cameraState = CameraState.RIGHT;

        vcam = GameObject.Find("CM vcam1").GetComponent<CinemachineVirtualCamera>();
        noise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

    }
    public void TurnCamera() 
    {
        if (cameraState==CameraState.RIGHT)
        {            
            timeRemaining = 5f;
            justOnce = false;
            vcam.transform.DOMove(newCamPos.transform.position, 2f);
            vcam.transform.DOLocalRotate(newCamPos.transform.rotation.eulerAngles, 2f);
            //ColorUIPlayer();
            StartCoroutine(UIManager.instance.UICoroutine(UIManager.instance.Player2, 2f));
            cameraState = CameraState.LEFT;
            ChangeMaterial(red,blue);
            stopTime = true;
            StartCoroutine(stopTimeCo(2f));
            
        }
        else if (cameraState == CameraState.LEFT)
        {          
            timeRemaining = 5f;
            justOnce = false;
            vcam.transform.DOMove(newCamPosRight.transform.position, 2f);
            vcam.transform.DOLocalRotate(newCamPosRight.transform.rotation.eulerAngles, 2f);
            //ColorUIEnemy();
            StartCoroutine(UIManager.instance.UICoroutine(UIManager.instance.Player1, 2f));
            cameraState = CameraState.RIGHT;
            ChangeMaterial(blue, red);
            stopTime = true;
            StartCoroutine(stopTimeCo(2f));           
        }
    }
    public void ChangeMaterial(Material material1,Material material2)
    {
        PlayerMaterialGameObject.GetComponent<SkinnedMeshRenderer>().material = material1;
        EnemyMaterialGameObject.GetComponent<SkinnedMeshRenderer>().material = material2;
    }
    void Update()
    {
        //if (elementsPlayer[4].activeSelf && elementsEnemy[4].activeSelf)
        //{
        //    if (score1>score2)
        //    {
        //        SceneManager.LoadScene(1);
        //    }
        //    else if (score2>score1)
        //    {
        //        SceneManager.LoadScene(2);
        //    }
        //    else if (score1==score2)
        //    {
        //        SceneManager.LoadScene(3);
        //    }
        //}

        //if (score1 == 5)
        //{

        //}

        TrailColor();
        AnimationControl();
        

        Timer.text = timeRemaining.ToString("F1");
       
        if (timeRemaining > 0.1f && !stopTime)
        {
            timeRemaining -= Time.deltaTime;
        }
        if (timeRemaining < 0.1f)
        {
            if (!justOnce)
            {               
                justOnce = true;
                stopTime = true;
                StartCoroutine(stopTimeCo(4f));
                StartCoroutine(MaxTime());
                Invoke("TurnCamera",2f);
            }
        }
        
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

        //if (currentState  == PlayerState.PATROL)
        //{
        //    InCircleJob();
        //}

        if (Input.GetMouseButtonDown(0) && timeRemaining < 5f)
        {
            maxPower = false;
            currentState = PlayerState.IDLE;
            holdDownStartTime = Time.time;
            rotateEnemy = true;
            RotateTowardsHoop();      
            stopTime = true;

        }
        if (Input.GetMouseButton(0))
        {
            //outCircle.gameObject.SetActive(true); //show ui
            float maxHoldTime = 1f;
            float holdDownTime = Time.time - holdDownStartTime;
            float holdTimeNormalized = Mathf.Clamp01(holdDownTime / maxHoldTime);
            if (holdTimeNormalized<1)
            {
                if (outCircle1.gameObject.activeSelf)
                {
                    inCircle1.rectTransform.DOScale(2f, 1f);
                }
                else if (outCircle2.gameObject.activeSelf)
                {
                    inCircle2.rectTransform.DOScale(2f, 1f);
                }
            }
            if (holdTimeNormalized ==1)
            {
                maxPower = true;
                //1 saniye bekle ve kapat
                stopTime = true;
                StartCoroutine(stopTimeCo(4f));
                //InCircleJob();
                StartCoroutine(MaxPower());
            }           
            Debug.Log(holdDownTime);           
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (maxPower)
            {
                return;
            }           
            float holdDownTime = Time.time - holdDownStartTime;
            if (cameraState == CameraState.RIGHT)
            {
                CalculateHoldDownTime(holdDownTime);
            }
            else if (cameraState == CameraState.LEFT)
            {
                CalculateHoldDownTime1(holdDownTime);
            }

            StartCoroutine(ActivateOtherCircle());
            timeRemaining = 5f;
            StartCoroutine(stopTimeCo(4f));
        }
        
    }
    public IEnumerator ActivateOtherCircle()
    {
        inCircle1.gameObject.SetActive(false);
        outCircle1.gameObject.SetActive(false);
        yield return new WaitForSeconds(8f);
        outCircle2.gameObject.SetActive(true);
        inCircle2.gameObject.SetActive(true);
    }
    public IEnumerator MaxPower()
    {
        currentState = PlayerState.IDLE;
        RotateTowardsHoop();
        Ball.transform.DOMove(playerHead.transform.position, 0f);
        Ball.transform.DOMove(hoopUp.transform.position, 2f);
        yield return new WaitForSeconds(2f);
        currentState = PlayerState.PATROL;
        Invoke("TurnCamera",2f);


    }
    public IEnumerator UpdateScore(int number1,int number2,float time,String text)
    {
        yield return new WaitForSeconds(time);
        PointText.SetActive(true);
        PointText.GetComponent<TextMesh>().text = text;
        score1 += number1;
        score2 += number2;
        Player1Score.text = "" + score1;
        Player2Score.text = "" + score2;
        
        
    }

    //public void InCircleJob()
    //{
    //    inCircle1.rectTransform.DOScale(0, 0f);
    //    //outCircle.gameObject.SetActive(false);
    //}
    public void CalculateHoldDownTime(float holdTime) 
    {
        float maxHoldTime = 1f;
        float holdTimeNormalized = Mathf.Clamp01(holdTime / maxHoldTime);
        if (holdTimeNormalized < 0.7)
        {          
            StartCoroutine(Block());

            Invoke("TurnCamera",2f);
           
        }
        //else if (holdTimeNormalized >=0.7 && holdTimeNormalized < 0.8)
        //{
            
        //    StartCoroutine("Shoot");
        //    StartCoroutine(UpdateScore(1, 0, 1f,"+1"));
            
        //    Invoke("TurnCamera",2f);

        //}
        //else if (holdTimeNormalized >= 0.8 && holdTimeNormalized < 0.9)
        //{
        //    StartCoroutine("Shoot");

        //    StartCoroutine(UpdateScore(2, 0, 1f,"+2"));
           


        //    Invoke("TurnCamera", 2f);

        //}
        //else if (holdTimeNormalized >= 0.9 && holdTimeNormalized < 0.96)
        //{
            
        //    StartCoroutine("Shoot");

        //    StartCoroutine(Confetti(1f,2,2));

        //    StartCoroutine(UpdateScore(3, 0, 1f,"+3"));

        //    StartCoroutine(ActivateDanceAnim());
        //    Invoke("TurnCamera", 7f);

        //}
        else if (holdTimeNormalized >= 0.7 && holdTimeNormalized < 1)
        {
            //ballTrail.startColor = Color.red;
            StartCoroutine("Shoot");
            StartCoroutine(Confetti(1f,1f,1f));
            StartCoroutine(UpdateScore(3,0,1f,"+3"));
            
            StartCoroutine(ActivateDanceAnim());
            StartCoroutine(ActivateEnemySadAnim());
            Invoke("TurnCamera", 7f);
            


        }
        else if (holdTimeNormalized == 1)
        {
            //burayý getmousebutton kýsmýnda yaptýk  
        }
    }

    public void CalculateHoldDownTime1(float holdTime)
    {
        float maxHoldTime = 1f;
        float holdTimeNormalized = Mathf.Clamp01(holdTime / maxHoldTime);
        if (holdTimeNormalized < 0.8)
        {         
            StartCoroutine("Shoot");
            Invoke("TurnCamera", 2f);
            PointText.GetComponent<TextMesh>().color = Color.red;
            StartCoroutine(UpdateScore(0, 2, 1f,"+2"));
            StartCoroutine(GiveAnim(1));
        }
        else if (holdTimeNormalized >= 0.7 && holdTimeNormalized < 1)
        {

            StartCoroutine(Block());
            StartCoroutine(Confetti(0.5f,1,1));
            StartCoroutine(GiveAnim(4));



        }

    }
    public IEnumerator GiveAnim(int scene)
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(scene);
    }
    public IEnumerator ActivateDanceAnim()
    {
        yield return new WaitForSeconds(1f);
        animator.SetBool("Dance", true);
        yield return new WaitForSeconds(5f);
        animator.SetBool("Dance", false);
        animator.SetBool("Idle", true);

    }
    public IEnumerator ActivateEnemySadAnim()
    {
        yield return new WaitForSeconds(1f);
        EnemyAnimator.SetBool("Sad", true);
        yield return new WaitForSeconds(5f);
        EnemyAnimator.SetBool("Sad", false);
        EnemyAnimator.SetBool("Idle", true);

    }


    public void TrailColor()
    {

        if (cameraState == CameraState.RIGHT)
        {
            Color color1 = new Color(0.4f, 0.95f, 1);
            ballTrail.startColor = color1;

        }
        else if (cameraState == CameraState.LEFT)
        {
            Color color2 = new Color(1, 0.5f, 0.02f);
            ballTrail.startColor = color2;
        }
    }

    public void AnimationControl()
    {
        if(currentState == PlayerState.PATROL)
        {
            animator.SetBool("Idle",false);
            animator.SetBool("Shoot",false);
            animator.SetBool("Walk",true);
        }
        else if (currentState == PlayerState.IDLE)
        {
            animator.SetBool("Idle", true);
            animator.SetBool("Shoot", false);
            animator.SetBool("Walk", false);
        }
        else if (currentState == PlayerState.SHOOT)
        {
            animator.SetBool("Idle", false);
            animator.SetBool("Shoot", true);
            animator.SetBool("Walk", false);
        }
    }
    public IEnumerator MaxTime()
    {
        currentState = PlayerState.IDLE;
        RotateTowardsHoop();
        //yield return new WaitForSeconds(1f);
        Ball.transform.DOMove(transform.position, 0f);
        Ball.transform.DOMove(field.transform.position, 3f);
        yield return new WaitForSeconds(2f);
        currentState = PlayerState.PATROL;

    }

    public IEnumerator BlockCamera()
    {
        
        vcam.transform.DOLocalMoveZ(vcam.transform.localPosition.z + 2f, 0.5f);
        yield return new WaitForSeconds(0.5f);
        vcam.transform.DOMoveZ(vcam.transform.localPosition.z -6f, 0.5f);
        yield return new WaitForSeconds(0.5f);
        vcam.transform.DOMoveZ(vcam.transform.localPosition.z +4f, 0.5f);
      

    }

    public IEnumerator Block()
    {
        currentState = PlayerState.SHOOT;
        StartCoroutine(BlockCamera());
        Ball.transform.DOMove(transform.position, 0f);
        Ball.transform.DOMove(enemyHead.transform.position, 0.3f);
        yield return new WaitForSeconds(0.3f);
        BlockText.SetActive(true);
        Ball.transform.DOMove(blockTarget.transform.position, 1f);
        yield return new WaitForSeconds(2f);
        
        //currentState = PlayerState.IDLE;
        //yield return new WaitForSeconds(2f);
        //currentState = PlayerState.PATROL;
    }
    public IEnumerator Shoot()
    {
        currentState = PlayerState.SHOOT;
        StartCoroutine(BallMove());  //1sn     
        yield return new WaitForSeconds(1f);
        currentState = PlayerState.IDLE;
        yield return new WaitForSeconds(6f);
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
        transform.DOLookAt(Hoop.transform.position,0.5f);
        
    }

    public IEnumerator BallMove()
    {   
        Ball.transform.DOMove(transform.position, 0f);
        Ball.transform.DOMove(BallTarget.transform.position, 1f);
        Ball.transform.DOScale(0.8f, 0.5f);
        
        yield return new WaitForSeconds(0.5f);
        Ball.transform.DOScale(0.5f, 0.5f);

        
    }

    public IEnumerator Confetti(float time,float n1,float n2)
    {
        yield return new WaitForSeconds(time);
        Confettis.SetActive(true);
        Noise(n1, n2);
        yield return new WaitForSeconds(5f);
        Confettis.SetActive(false);
        Noise(0f, 0f);

    }

    public void Noise(float amplitudeGain, float frequencyGain)
    {
        noise.m_AmplitudeGain = amplitudeGain;
        noise.m_FrequencyGain = frequencyGain;
    }

    public IEnumerator stopTimeCo(float time)
    {
        
        yield return new WaitForSeconds(time);
        stopTime = false;
        
    }

    public IEnumerable JustWait(float time)
    {
        yield return new WaitForSeconds(time);
    }
}
