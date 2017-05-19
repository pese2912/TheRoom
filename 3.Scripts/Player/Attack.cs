using UnityEngine;
using System.Collections;

public class Attack : MonoBehaviour {

    public Murderer murderer;
    public Attack_Audio attack_audio;

    void OnCollisionEnter(Collision col)
    {
        //Debug.Log("Check");
        //attack_audio.PlayAudio("ATTACK",true);
        if (col.collider.CompareTag("SURVIVOR"))
        {
            
            if (murderer.getAttacked())
            {
                
                EventManager.Instance.PostNotification(EVENT_TYPE.SURVIVOR_HIT, this, murderer.getDamage());
            }
        }

    }

    void Start()
    {
        attack_audio.PlayAudio("START");
    }

    void Update()
    {
        if (attack_audio.GetCheck())
        {
            attack_audio.PlayAudio("NORMAL");
        }
    }
}
