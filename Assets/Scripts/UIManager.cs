using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject Block;
    public GameObject Point1;
    public GameObject Point2;
    public GameObject Point3;
    public GameObject Point5;
    public GameObject MaxTime;
    public GameObject Player1;
    public GameObject Player2;





    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        
    }

    public void UITrue(GameObject gameObject, bool isTrue)
    {
        gameObject.SetActive(isTrue);
    } 
    public IEnumerator UICoroutine(GameObject gameObject,float time)

    {
        gameObject.SetActive(true);
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);

    }





}
