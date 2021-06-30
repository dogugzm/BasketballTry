using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using Cinemachine;




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
    public Material orange;


    public GameObject PlayerMaterialGameObject;
    public GameObject EnemyMaterialGameObject;

    public bool isCameraMoving;



    public RawImage inCircle;
    public RawImage outCircle;
    public TextMeshProUGUI Timer;

    public TrailRenderer ballTrail;

    CinemachineVirtualCamera vcam;
    CinemachineBasicMultiChannelPerlin noise;


    private float timeRemaining;

    public float speed = 4f;
    public float ballSpeed = 4f;


    public bool toLeft, toRight,rotateEnemy;

    public Animator animator;
    public Rigidbody rb;

    bool stopTime;
    public bool justOnce;


    private float holdDownStartTime;

    [System.NonSerialized]public PlayerState currentState;
    CameraState cameraState;


    // Start is called before the first frame update
    void Start()
    {
        DOTween.Init(true, true, LogBehaviour.Verbose).SetCapacity(500, 50);
        toLeft = true;
        toRight = false;
        stopTime = false;
        isCameraMoving = false;
        rotateEnemy = false;
        justOnce = false;
        timeRemaining = 5f;
        currentState = PlayerState.PATROL;
        cameraState = CameraState.RIGHT;

        vcam = GameObject.Find("CM vcam1").GetComponent<CinemachineVirtualCamera>();
        noise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

    }

    
    public void TurnCamera() 
    {
        if (cameraState==CameraState.RIGHT && !isCameraMoving)
        {
            
            timeRemaining = 5f;
            justOnce = false;
            vcam.transform.DOMove(newCamPos.transform.position, 2f);
            vcam.transform.DOLocalRotate(newCamPos.transform.rotation.eulerAngles, 2f);
            StartCoroutine(UIManager.instance.UICoroutine(UIManager.instance.Player2, 2f));
            cameraState = CameraState.LEFT;
            ChangeMaterial(orange,blue);
            stopTime = true;
            StartCoroutine(stopTimeCo(2f));
            
        }
        else if (cameraState == CameraState.LEFT&& !isCameraMoving)
        {
           
            timeRemaining = 5f;
            justOnce = false;
            vcam.transform.DOMove(newCamPosRight.transform.position, 2f);
            vcam.transform.DOLocalRotate(newCamPosRight.transform.rotation.eulerAngles, 2f);
            StartCoroutine(UIManager.instance.UICoroutine(UIManager.instance.Player1, 2f));
            cameraState = CameraState.RIGHT;
            ChangeMaterial(blue, orange);
            stopTime = true;
            StartCoroutine(stopTimeCo(2f));
            
        }
    }

    public void ChangeMaterial(Material material1,Material material2)
    {
        PlayerMaterialGameObject.GetComponent<SkinnedMeshRenderer>().material = material1;
        EnemyMaterialGameObject.GetComponent<SkinnedMeshRenderer>().material = material2;


    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(timeRemaining);
        
        Timer.text = timeRemaining.ToString("F1");
       



        if (timeRemaining > 0.1f && !stopTime)
        {
            timeRemaining -= Time.fixedDeltaTime;
        }
        if (timeRemaining < 0.1f)
        {
            if (!justOnce)
            {
                Debug.Log("Süre bitti");
                StartCoroutine(UIManager.instance.UICoroutine(UIManager.instance.MaxTime, 1.5f));
                justOnce = true;
                stopTime = true;
                StartCoroutine(stopTimeCo(2f));
                //üzülme animasyonu olabilir
                TurnCamera();
                currentState = PlayerState.PATROL;
                
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

        if (currentState  == PlayerState.PATROL)
        {
            InCircleJob();
        }

        if (Input.GetMouseButtonDown(0) && timeRemaining < 5f)
        {
            currentState = PlayerState.IDLE;
            holdDownStartTime = Time.time;
            rotateEnemy = true;
            RotateTowardsHoop();
            //currentState = PlayerState.SHOOT;
            stopTime = true;

        }
        if (Input.GetMouseButton(0))
        {
            outCircle.gameObject.SetActive(true); //show ui
            float maxHoldTime = 5f;
            float holdDownTime = Time.time - holdDownStartTime;
            float holdTimeNormalized = Mathf.Clamp01(holdDownTime / maxHoldTime);
            if (holdTimeNormalized<1)
            {
                inCircle.rectTransform.DOScale(4f, 5f);
            }
            if (holdTimeNormalized ==1)
            {
                //1 saniye bekle ve kapat
                InCircleJob();
                StartCoroutine(UIManager.instance.UICoroutine(UIManager.instance.MaxTime,1.5f));
                //justOnce = true;
                stopTime = true;
                StartCoroutine(stopTimeCo(2f));
                //üzülme animasyonu olabilir
                TurnCamera();
                currentState = PlayerState.PATROL;
            }
            Debug.Log("UI gözüktü");
            Debug.Log(holdDownTime);
            
        }
        if (Input.GetMouseButtonUp(0))
        {
            currentState = PlayerState.SHOOT;
            
            //animator.SetTrigger("Running");
            //animator.SetTrigger("Throw");
            float holdDownTime = Time.time - holdDownStartTime;
            CalculateHoldDownTime(holdDownTime);
            InCircleJob();
            timeRemaining = 5f;
            StartCoroutine(stopTimeCo(4f));
        }


        AnimationControl();

    }


    public void InCircleJob()
    {
        inCircle.rectTransform.DOScale(0, 0f);
        outCircle.gameObject.SetActive(false);
    }

    public void CalculateHoldDownTime(float holdTime) 
    {
        float maxHoldTime = 5f;
        float holdTimeNormalized = Mathf.Clamp01(holdTime / maxHoldTime);
        if (holdTimeNormalized < 0.7)
        {
            Debug.Log("Block");
            StartCoroutine(UIManager.instance.UICoroutine(UIManager.instance.Block,2f));
            Invoke("TurnCamera",2f);
            currentState = PlayerState.PATROL;

        }
        else if (holdTimeNormalized >=0.7 && holdTimeNormalized < 0.8)
        {
            
            StartCoroutine("Shoot");
            ballTrail.startColor = Color.grey;
            Debug.Log("1 Puan");
            StartCoroutine(UIManager.instance.UICoroutine(UIManager.instance.Point1, 1.5f));
            Invoke("TurnCamera",2f);

        }
        else if (holdTimeNormalized >= 0.8 && holdTimeNormalized < 0.9)
        {
            
            StartCoroutine("Shoot");
            ballTrail.startColor = Color.grey;
            StartCoroutine(UIManager.instance.UICoroutine(UIManager.instance.Point2, 1.5f));
            Invoke("TurnCamera", 2f);

        }
        else if (holdTimeNormalized >= 0.9 && holdTimeNormalized < 0.96)
        {
            
            StartCoroutine("Shoot");
            ballTrail.startColor = Color.red;
            Debug.Log("3 Puan");
            StartCoroutine(UIManager.instance.UICoroutine(UIManager.instance.Point3, 1.5f));
            StartCoroutine("Confetti");
            Invoke("TurnCamera", 2f);


        }
        else if (holdTimeNormalized >= 0.96 && holdTimeNormalized < 1)
        {
            
            StartCoroutine("Shoot");
            ballTrail.startColor = Color.red;
            Debug.Log("5 Puan");
            StartCoroutine(UIManager.instance.UICoroutine(UIManager.instance.Point5, 1.5f));
            StartCoroutine("Confetti");
            Invoke("TurnCamera", 2f);

        }
        else if (holdTimeNormalized == 1)
        {
            //burayý getmousebutton kýsmýnda yaptýk  
        }
    }


    public void AnimationControl()
    {
        if(currentState == PlayerState.PATROL)
        {
            animator.SetTrigger("Running");     
        }
        if (currentState == PlayerState.IDLE)
        {
            animator.SetTrigger("DynIdle");
        }
        if (currentState == PlayerState.SHOOT)
        {
            animator.SetTrigger("Throw");
        }


    }

    public IEnumerator Shoot()
    {

        BallMove();
        Debug.Log("Shooted");
        yield return new WaitForSeconds(1f);
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

    public void BallMove()
    {   //coroutþne yapýp animasyonla uygun hale getir.
        Ball.transform.DOMove(transform.position, 0f);

        Ball.transform.DOMove(BallTarget.transform.position, 1f); 
    }

    public IEnumerator Confetti()
    {
        yield return new WaitForSeconds(1f);
        Confettis.SetActive(true);
        Noise(0.5f, 0.5f);
        yield return new WaitForSeconds(3f);
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
