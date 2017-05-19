using UnityEngine;
using System.Collections;

public class Survivor : MonoBehaviour, IListener{

    public enum PlayerState { Idle=0, Walk=1, Run=2, Crouch=3, CrouchWalk=4 ,Hit = 5 ,Die=6, Gram = 7 };

    [Header("Animation Settings")]
    public Animator m_Animator;
	[SerializeField]
    public PlayerState m_PlayerState;
    private int m_State;
    public GameObject[] m_Head; 

    [Header("Controller Settings")]
    public CharacterController m_Controller;
    public Transform m_PlayerTr;
    public float m_speedRotation = 50;
    public float m_WalkSpeed;
    public float m_RunSpeed;
    private float m_speed = 1.5f;

    private Vector3 m_moveDirection = Vector3.zero;
    private float m_horizontal = 0f;
    private float m_vertical = 0f;
    private float m_rotate;

    [Header("Character Settings")]
    private bool isDie;
    private bool m_IsRun;
    private bool m_IsCrouch;
    private bool m_IsHit;
    private bool m_Gram;
    private int m_HP;
    private bool m_IsMoving;

    [Header("Photon Settings")]
    public GameObject m_Camera;
    public PhotonView m_pv;
    private Vector3 currPos = Vector3.zero;
    private Quaternion currRot = Quaternion.identity;

    private int HeartBeat;
    #region Audio

    public Survivor_Audio survivor_audio;
    public Survivor_Audio2 survivor_audio2;
    
    #endregion
    // Use this for initialization
    void Awake()
    {

        isDie = false;
        m_IsRun = false;
        m_IsCrouch = false;
        m_IsHit = false;
        m_IsMoving = false;
        m_Gram = false;
        m_HP = 100;
        
        m_PlayerState = PlayerState.Idle;
        m_State = 0;
        m_pv.synchronization = ViewSynchronization.UnreliableOnChange;

        m_pv.ObservedComponents[0] = this;

        currPos = m_PlayerTr.position;
        currRot = m_PlayerTr.rotation;
    }

    void Start()
    {
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_HIT, this);
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_GRAMCTRL, this);
        EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_CREATE, this);

        if (m_pv.isMine)
        {
            m_Head[0].GetComponent<SkinnedMeshRenderer>().enabled = false;
            m_Head[1].SetActive(false);
            m_Camera.SetActive(true);
            StartCoroutine(PlayerStateCheck());
            StartCoroutine(PlayerAction());
        }
        else
            StartCoroutine(RemotePlayerAction());
    }

    void Update()
    {
        //Debug.Log(HeartBeat);
        if (!isDie && !m_IsHit && !m_Gram)
        {
            if (m_pv.isMine)
            {
                m_horizontal = Input.GetAxis("Horizontal");
                m_vertical = Input.GetAxis("Vertical");
                m_rotate = Input.GetAxis("Oculus_GearVR_RThumbstickX") * m_speedRotation; // 회전

                if (Input.GetButtonDown("Jump"))
                {
                    
                    m_IsRun = true;

                }

                else if (Input.GetButtonUp("Jump"))
                {
                    
                    m_IsRun = false;

                }

               if (Input.GetButtonDown("Fire1"))
                {
                    m_IsCrouch = !m_IsCrouch;
                    survivor_audio.PlayAudio("CROUCH",true);
                }

                Vector3 desiredMove = transform.forward * m_vertical + transform.right * m_horizontal;


                m_moveDirection.x = desiredMove.x * m_speed;
                m_moveDirection.y -= 9.8f;
                m_moveDirection.z = desiredMove.z * m_speed;

                m_Controller.Move(m_moveDirection * Time.deltaTime);


                transform.Rotate(0, m_rotate * Time.deltaTime, 0);
            }

            else
            {
                m_PlayerTr.position = Vector3.Lerp(m_PlayerTr.position, currPos, Time.deltaTime * 3f);
                m_PlayerTr.rotation = Quaternion.Slerp(m_PlayerTr.rotation, currRot, Time.deltaTime * 3f);

            }
        }

        #region Audio
        if(survivor_audio.GetCheck())
        if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Basic_Walk_01"))
        {
            survivor_audio.PlayAudio("WALK");
        }
        else if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Basic_Run_02"))
        {
            survivor_audio.PlayAudio("RUN");
        }
        else if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("HumanoidCrouchWalk 0"))
        {
            survivor_audio.PlayAudio("CROUCH_WALK");
        }
        else if(m_Animator.GetCurrentAnimatorStateInfo(0).IsName("HumanoidIdle"))
        {
            survivor_audio.PlayAudio("NOT");
        }
        /*
        if (HeartBeat >= 100)
        {
            survivor_audio2.PlayAudio("HEART_SPEED_UP", true);
        }
        else
        {
            survivor_audio2.PlayAudio("HEART_SPEED_DOWN", true);
        }
        */
        #endregion
    }

    #region
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(m_PlayerTr.position);
            stream.SendNext(m_PlayerTr.rotation);
            stream.SendNext((int)m_PlayerState);
        }
        else
        {
            currPos = (Vector3)stream.ReceiveNext();
            currRot = (Quaternion)stream.ReceiveNext();
            m_State = (int)stream.ReceiveNext();
          
        }
    }
    #endregion

    #region
    IEnumerator PlayerStateCheck()
    {
        
        while (true)
        {
         
            if (m_horizontal == 0 && m_vertical == 0 && m_rotate == 0)
            {

                if (m_IsCrouch)
                    m_PlayerState = PlayerState.Crouch;
                else
                {

                    m_PlayerState = PlayerState.Idle;
                }
            }

            else if (m_horizontal != 0 || m_vertical != 0 || m_rotate != 0)
            {
                if (m_IsCrouch)
                {
                    if (m_PlayerState == PlayerState.Crouch)
                        m_PlayerState = PlayerState.CrouchWalk;
                    else if (m_PlayerState != PlayerState.CrouchWalk)
                        m_PlayerState = PlayerState.Crouch;
                }

                else
                {
                    if (m_PlayerState == PlayerState.CrouchWalk)
                        m_PlayerState = PlayerState.Crouch;
                    else if (m_PlayerState == PlayerState.Crouch)
                        m_PlayerState = PlayerState.Idle;
                    else
                    {
                        m_PlayerState = PlayerState.Walk;
                        if (m_IsRun)
                            m_PlayerState = PlayerState.Run;
                    }
                }

            }

            if (isDie)
            {
                //Debug.Log("Death");
                m_PlayerState = PlayerState.Die;
                
            }
            if (m_IsHit)
            {
                m_PlayerState = PlayerState.Hit;
                
            }
            if (m_Gram)
                m_PlayerState = PlayerState.Gram;

            yield return null;
        }
    }
    #endregion

    #region
    IEnumerator PlayerAction()
    {

        while (true)
        {

            switch (m_PlayerState)
            {
                case PlayerState.Idle:

                    AnimationExcute("Idle");

                    break;

                case PlayerState.Walk:
                    --HeartBeat;
                    m_speed = m_WalkSpeed;
                    AnimationExcute("Walk");

                    break;

                case PlayerState.Run:
                    ++HeartBeat;
                    m_speed = m_RunSpeed;
                    AnimationExcute("Run");

                    break;

                case PlayerState.Crouch:
                    AnimationExcute("Crouch");

                    break;
                case PlayerState.CrouchWalk:
                    m_speed = m_WalkSpeed;
                    AnimationExcute("CrouchWalk");

                    break;

                case PlayerState.Hit:
                   
                    AnimationExcute("Hit");

                    break;
                case PlayerState.Die:

                    AnimationExcute("Die");

                    break;
                case PlayerState.Gram:

                    AnimationExcute("Gram");

                    break;

            }
            yield return null;
        }

    }


    public void sendMoveEvent(bool flag)
    {
        if(!m_IsMoving && flag)
        {            
            EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_MOVE, this);
            m_IsMoving = true;
        }

        else if(m_IsMoving && !flag)
        {
            EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_STOP, this);
            m_IsMoving = false;
        }
    }

    public bool getGramCtrl()
    {
        return m_Gram;
    }
    public void setGramCtrl(bool flag)
    {
    
        m_Gram = flag;
    }



    #region
    IEnumerator RemotePlayerAction()
    {

        while (true)
        {

            switch (m_State)
            {
                case (int)PlayerState.Idle:

                    sendMoveEvent(false);
                    AnimationExcute("Idle");

                    break;

                case (int)PlayerState.Walk:
                    sendMoveEvent(false);
                    m_speed = m_WalkSpeed;
                    AnimationExcute("Walk");


                    break;

                case (int)PlayerState.Run:
                    sendMoveEvent(true);
                    m_speed = m_RunSpeed;
                    AnimationExcute("Run");

                    break;

                case (int)PlayerState.Crouch:
                    sendMoveEvent(false);
                    AnimationExcute("Crouch");

                    break;

                case (int)PlayerState.CrouchWalk:

                    sendMoveEvent(false);
                    m_speed = m_WalkSpeed;
                    AnimationExcute("CrouchWalk");

                    break;

                case (int)PlayerState.Hit:

                    sendMoveEvent(false);
                    AnimationExcute("Hit");
                    
                    break;

                case (int)PlayerState.Die:

                    sendMoveEvent(true);
                    AnimationExcute("Die");

                    break;

                case (int)PlayerState.Gram:

                    sendMoveEvent(true);
                    AnimationExcute("Gram");

                    break;

            }
            yield return null;
        }

    }
    #endregion

    public void OnEvent(EVENT_TYPE Event_Type, Component Sender, object Param)
    {

        switch (Event_Type)
        {

            case EVENT_TYPE.SURVIVOR_HIT:
               
                if (m_pv.isMine && !m_IsHit)
                {
                   
                    StartCoroutine("Hit", Param);
                    m_pv.RPC("Hit", PhotonTargets.Others, Param);
                }
                
                break;

            case EVENT_TYPE.SURVIVOR_GRAMCTRL:

                if (m_pv.isMine)
                {
                    GramCtrl();
                    
                   
                }

                break;

        };
    }

   
    public void GramCtrl()
    {
        if (m_PlayerState != PlayerState.Crouch && m_PlayerState != PlayerState.CrouchWalk)
        {

            if (!getGramCtrl())
            {
                setGramCtrl(true);
                EventManager.Instance.PostNotification(EVENT_TYPE.GRAM_START, this);
            }

            else
            {
                setGramCtrl(false);
                EventManager.Instance.PostNotification(EVENT_TYPE.GRAM_STOP, this);
            }
        }
    }

    [PunRPC]
    public IEnumerator Hit(object Param)
    {
        survivor_audio.PlayAudio("ATTACKED", true);
        m_Gram = false;
        int damage = (int)Param;
        m_HP -= damage;
        m_IsHit = true;      
  
        yield return new WaitForSeconds(0.7f);
        m_IsHit = false;

        if (m_HP <= 0)
        {
            survivor_audio.PlayAudio("DEATH", true);
            isDie = true;
        }

    }

    #endregion

    public void AnimationExcute(string name)
    {

        m_Animator.SetBool("Idle", false);
        m_Animator.SetBool("Walk", false);
        m_Animator.SetBool("Run", false);
        m_Animator.SetBool("Crouch", false);
        m_Animator.SetBool("CrouchWalk", false);
        m_Animator.SetBool("Die", false);
        m_Animator.SetBool("Hit", false);
        m_Animator.SetBool("Gram", false);
        m_Animator.SetBool(name, true);
    }
  
}
