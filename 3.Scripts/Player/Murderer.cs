using UnityEngine;
using System.Collections;

public class Murderer : MonoBehaviour ,IListener{

    private enum PlayerState { Idle=0, Walk=1 ,Run=2, Attack=3 ,Die=4 };

    [Header("Animation Settings")]
    public Animator m_Animator;
    private PlayerState m_PlayerState;
    private int m_State;
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
    public bool isDie;
    private bool isAttack;
    private bool m_IsRun;
    private int m_Damage;
    

    [Header("Photon Settings")]
    public GameObject m_Camera;
    public PhotonView m_pv;
    private Vector3 currPos = Vector3.zero;
    private Quaternion currRot = Quaternion.identity;

    public Murder_Audio murder_Audio;
    public Attack attack;
    // Use this for initialization
    void Awake () {

        isDie = false;
        isAttack = false;
        m_IsRun = false;
        m_Damage = 25;
        m_PlayerState = PlayerState.Idle;
        m_State = 0;
        m_pv.synchronization = ViewSynchronization.UnreliableOnChange;

        m_pv.ObservedComponents[0] = this;

        currPos = m_PlayerTr.position;
        currRot = m_PlayerTr.rotation;
    }

	void Start()
    {
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_MOVE, this);
        EventManager.Instance.AddListener(EVENT_TYPE.SURVIVOR_STOP, this);
        if (m_pv.isMine)
        {
            m_Camera.SetActive(true);
            StartCoroutine(PlayerStateCheck());
            StartCoroutine(PlayerAction());
        }
        else
            StartCoroutine(RemotePlayerAction());
       
    }

    #region
    void Update()
    {
        if (!isDie && !isAttack)
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
        if (murder_Audio.GetCheck())
        if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
        {
            murder_Audio.PlayAudio("WALK");
        }
        else if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Run"))
        {
            murder_Audio.PlayAudio("RUN");
        }
        else if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName("Run"))
        {
            murder_Audio.PlayAudio("RUN");
        }
        else
        {
            murder_Audio.PlayAudio("NOT");
        }
        #endregion
    }

    #endregion

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

                m_PlayerState = PlayerState.Idle;
            }
            
            else if(m_horizontal != 0 || m_vertical != 0 || m_rotate != 0)
            {
                m_PlayerState = PlayerState.Walk;
                if(m_IsRun)
                    m_PlayerState = PlayerState.Run;
            }

            if (Input.GetButton("Fire1"))
            {
                m_PlayerState = PlayerState.Attack;
                attack.attack_audio.PlayAudio("ATTACK", true);
                if (murder_Audio.GetCheck())
                    murder_Audio.PlayAudio("ATTACK",true);
            }
            if(isDie)
                m_PlayerState = PlayerState.Die;

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
                    m_speed = m_WalkSpeed;
                    AnimationExcute("Walk");


                    break;

                case PlayerState.Run:
                    m_speed = m_RunSpeed;
                    AnimationExcute("Run");

                    break;
                    
                case PlayerState.Attack:
                    isAttack = true;
                    
                    AnimationExcute("Attack");

                    break; 
                case PlayerState.Die:

                    AnimationExcute("Die");
                    break;
            }
            yield return null;
        }

    }
    #endregion

    #region
    IEnumerator RemotePlayerAction()
    {

        while (true)
        {

            switch (m_State)
            {
                case (int)PlayerState.Idle:

                    AnimationExcute("Idle");

                    break;

                case (int)PlayerState.Walk:
                    m_speed = m_WalkSpeed;
                    AnimationExcute("Walk");
                    //murder_Audio.SetAudio("WALK");

                    break;

                case (int)PlayerState.Run:
                    m_speed = m_RunSpeed;
                    AnimationExcute("Run");
                    //murder_Audio.SetAudio("RUN");
                    break;

                case (int)PlayerState.Attack:
                    isAttack = true;
                    AnimationExcute("Attack");

                    break;
                case (int)PlayerState.Die:

                    AnimationExcute("Die");
                    break;
            }

            //murder_Audio.PlayCurrentAudio();

            yield return null;
        }

    }
    #endregion

    public void OnAttackEnd()
    {

        isAttack = false;
    }
   
    public bool getAttacked()
    {
        return isAttack;
    }

    public int getDamage()
    {
        return m_Damage;
    }

    public void setThroughSystem(bool flag)
    {
        m_Camera.GetComponent<SeeThroughSystem>().checkRenderTypes = flag;
    }

    public void OnEvent(EVENT_TYPE Event_Type, Component Sender, object Param)
    {

        switch (Event_Type)
        {

            case EVENT_TYPE.SURVIVOR_MOVE:

                if(m_pv.isMine)
                {
                    setThroughSystem(true);
                    
                }
               
                break;

            case EVENT_TYPE.SURVIVOR_STOP:
                
                if (m_pv.isMine)
                {

                    setThroughSystem(false);
                }

                break;

        };
    }

    public void AnimationExcute(string name)
    {

        m_Animator.SetBool("Idle", false);
        m_Animator.SetBool("Walk", false);
        m_Animator.SetBool("Run", false);
        m_Animator.SetBool("Attack", false);
        m_Animator.SetBool("Die", false);
        m_Animator.SetBool(name, true);
    }
}
