using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Murder_Audio : AudioController
{
    public AudioClip Audio_Run;
    public AudioClip Audio_Walk;
    public AudioClip Audio_Attack_cry;


    void Start()
    {
        base.Init();
    }

    public void PlayAudio(string audioName, bool isOneSound = false)
    {
        if (audioSource != null)
        {
            if (isOneSound == false)
            {
                switch (audioName)
                {
                    case "RUN":
                        audioSource.clip = Audio_Run;
                        break;
                    case "WALK":
                        audioSource.clip = Audio_Walk;
                        break;
                    case "NOT":
                        audioSource.clip = null;
                        audioSource.Stop();
                        break;
                    default:
                        Debug.LogError("잘못된 오디오 명을 입력하셨습니다.(Murder)");
                        break;
                }
                PlayCurrentAudio();
            }
            else
            {
                switch (audioName)
                {
                    case "ATTACK":
                        audioSource.clip = Audio_Attack_cry;
                        break;
                    default:
                        Debug.LogError("잘못된 오디오 명을 입력하셨습니다.(Survivor)");
                        break;
                }
                // PlayCurrentAudioRightly();               
                if (audioSource.clip != null)
                    StartCoroutine(TestAudio(audioSource.clip.length));
            }
        }
    }
}
