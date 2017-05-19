using UnityEngine;
using System.Collections;

public class GramCtrl : MonoBehaviour, IListener
{
    private Survivor survivor;
    private bool possible;
    public GameObject Object;
    public Animator anim;

    public int time = 15;

    public System.Diagnostics.Stopwatch sw;

    // Use this for initialization
    void Start()
    {
        sw = new System.Diagnostics.Stopwatch();

        EventManager.Instance.AddListener(EVENT_TYPE.GRAM_START, this);
        EventManager.Instance.AddListener(EVENT_TYPE.GRAM_STOP, this);
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_CREATE, this);
        possible = false;

        StartCoroutine(TimeCheck());
    }

    void Update()
    {
        if (possible)
        {
            StartCoroutine(CheckGramCtrl()); //코루틴 실행

        }
    }

    IEnumerator CheckGramCtrl() // 
    {
        yield return null;

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.rotation * Vector3.forward); //레이

        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.rotation * Vector3.forward * 10); // 카메라가 보는 광선을 그려줌

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1 << LayerMask.NameToLayer("GRAM"))) //  레이어 지정
        {

            if (hit.transform.gameObject.name.Equals(Object.name))
            {
                if (possible)
                {

                    if (Input.GetButtonDown("Fire2"))
                    {

                        sw.Start();

                        survivor.GetComponent<GramTrap>().enabled = true;
                        EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_GRAMCTRL, this);

                    }

                    if (Input.GetButtonUp("Fire2"))
                    {
                        sw.Stop();
                        EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_GRAMCTRL, this);

                    }
                }
            }
            else
            {
                if (Input.GetButtonUp("Fire2"))
                {
                    sw.Stop();
                    if (survivor != null && survivor.getGramCtrl())
                        EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_GRAMCTRL, this);

                }
            }

        }

    }

    public IEnumerator TimeCheck()
    {

        string text;
        System.TimeSpan ts;

        while (true)
        {

            ts = sw.Elapsed;

            text = string.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10);


            if (ts.Seconds > time)
            {

                break;
            }

            yield return null;

        }
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("SURVIVOR"))
        {
            possible = true;

        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("SURVIVOR"))
        {
            possible = false;

        }
    }

    public void OnEvent(EVENT_TYPE Event_Type, Component Sender, object Param)
    {

        switch (Event_Type)
        {

            case EVENT_TYPE.GRAM_START:
                StopCoroutine("GramAnim");
                StartCoroutine("GramAnim", true);

                break;

            case EVENT_TYPE.GRAM_STOP:
                StopCoroutine("GramAnim");
                StartCoroutine("GramAnim", false);


                break;
            case EVENT_TYPE.SURVIVOR_CREATE:

                survivor = GameObject.FindGameObjectWithTag("SURVIVOR").GetComponent<Survivor>();
                break;

        };
    }

    public IEnumerator GramAnim(bool flag)
    {

        if (flag)
        {
            yield return new WaitForSeconds(0.7f);
            anim.SetBool("Start", true);
            anim.SetBool("Stop", false);
        }
        else
        {
            anim.SetBool("Stop", true);
            anim.SetBool("Start", false);
        }
        yield return null;
    }

}