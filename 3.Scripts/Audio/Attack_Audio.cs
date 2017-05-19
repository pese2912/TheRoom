using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack_Audio : AudioController
{

    public AudioClip Audio_Attack_Start;
    public AudioClip Audio_Attack_Normal;
    public AudioClip Audio_Attack_Attack;


    void Start()
    {
        base.Init();
    }

    public void PlayAudio(string audioName, bool Rightly = false)
    {
        if (audioSource != null)
        {
            if (Rightly == false)
            {
                switch (audioName)
                {
                    case "START":
                        audioSource.clip = Audio_Attack_Start;
                        break;
                    case "NORMAL":
                        audioSource.clip = Audio_Attack_Normal;
                        break;
                        /*
                    case "ATTACK":
                        audioSource.clip = Audio_Attack_Attack;
                        break;
                        */
                    default:
                        Debug.LogError("잘못된 오디오 명을 입력하셨습니다.(Attack)");
                        break;
                }
                // PlayCurrentAudioRightly();               
                if (audioSource.clip != null)
                    StartCoroutine(TestAudio(audioSource.clip.length));
            }
            else
            {
                audioSource.clip = Audio_Attack_Attack;
                audioSource.Stop();
                audioSource.Play();
            }
        }
        
    }
}
