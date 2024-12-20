using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioSource bg_adudio;
    [SerializeField] private AudioSource audioPlayer_wl;
    [SerializeField] private AudioSource audioPlayer_button;
    [SerializeField] private AudioSource audioPlayer_spin_stop;
    [SerializeField] private AudioSource sidebar_sound;


    [Header("clips")]
    [SerializeField] private AudioClip SpinButtonClip;
    [SerializeField] private AudioClip SpinStopClip;
    [SerializeField] private AudioClip GambleSpinClip;
    [SerializeField] private AudioClip Button;
    [SerializeField] private AudioClip SmallWin_Audio;
    [SerializeField] private AudioClip BigWin_Audio;
    [SerializeField] private AudioClip NormalBg_Audio;
    [SerializeField] private AudioClip FreeSpinBg_Audio;
    [SerializeField] private AudioClip sizeup_audio;
    [SerializeField] private AudioClip megaWin;


    private void Awake()
    {
        sidebar_sound.clip=sizeup_audio;
        playBgAudio();

        //if (bg_adudio) bg_adudio.Play();
        //audioPlayer_button.clip = clips[clips.Length - 1];
    }

    internal void PlayWLAudio(string type = "default")
    {
        StopWLAaudio();
        // audioPlayer_wl.loop=loop;
        if (type == "big"){
            audioPlayer_wl.clip = BigWin_Audio;
            audioPlayer_wl.pitch=1.2f;
        } else if(type == "maega"){
                audioPlayer_wl.clip=megaWin;
        }
        else
        {
            audioPlayer_wl.clip = SmallWin_Audio;

        }

        audioPlayer_wl.Play();

    }

    internal void PlaySpinStopAudio( )
    {

        audioPlayer_spin_stop.clip = SpinStopClip;
        audioPlayer_spin_stop.Play();

    }

    internal void StopSpinAudio()
    {

        if (audioPlayer_spin_stop) audioPlayer_spin_stop.Stop();

    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {

            bg_adudio.Pause();
            audioPlayer_wl.Pause();
            audioPlayer_button.Pause();
            audioPlayer_spin_stop.Pause();

        }
        else
        {
            bg_adudio.UnPause();
            audioPlayer_wl.UnPause();
            audioPlayer_button.UnPause();
            audioPlayer_spin_stop.UnPause();


        }
    }



    internal void playBgAudio(string type = "default")
    {


        //int randomIndex = UnityEngine.Random.Range(0, Bg_Audio.Length);
        StopBgAudio();
        bg_adudio.loop = true;
        if (bg_adudio)
        {
            if (type == "FP")
                bg_adudio.clip = FreeSpinBg_Audio;
            else
                bg_adudio.clip = NormalBg_Audio;


            bg_adudio.Play();
        }

    }

    internal void PlayButtonAudio(string type = "default")
    {

        if (type == "spin")
            audioPlayer_button.clip = SpinButtonClip;
        else
            audioPlayer_button.clip = Button;

        //StopButtonAudio();
        audioPlayer_button.Play();
        // Invoke("StopButtonAudio", audioPlayer_button.clip.length);

    }

    internal void StopWLAaudio()
    {
        audioPlayer_wl.Stop();
        audioPlayer_wl.loop = false;
        audioPlayer_wl.pitch = 1f;

    }

    internal void PlaySizeUpSound(bool play){
        if(play)
        sidebar_sound.Play();
        else
        sidebar_sound.Stop();

    }


    internal void StopButtonAudio()
    {

        audioPlayer_button.Stop();

    }


    internal void StopBgAudio()
    {
        bg_adudio.Stop();

    }



    internal void ToggleMute(bool toggle, string type)
    {

        switch (type)
        {
            case "bg":
                bg_adudio.mute = toggle;
                break;
            case "button":
                audioPlayer_button.mute = toggle;
                audioPlayer_spin_stop.mute = toggle;
                break;
            case "wl":
                audioPlayer_wl.mute = toggle;
                break;
            case "all":
                bg_adudio.mute = toggle;
                audioPlayer_button.mute = toggle;
                audioPlayer_spin_stop.mute = toggle;
                audioPlayer_wl.mute = toggle;
                break;



        }
    }

}
