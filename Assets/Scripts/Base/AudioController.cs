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
    [SerializeField] private AudioSource WinningSound;

    [Header("clips")]
    [SerializeField] private AudioClip SpinButtonClip;
    [SerializeField] private AudioClip SpinStopClip;
    [SerializeField] private AudioClip Button;
    [SerializeField] private AudioClip SmallWin_Audio;
    [SerializeField] private AudioClip BigWin_Audio;
    [SerializeField] private AudioClip NormalBg_Audio;
    [SerializeField] private AudioClip FreeSpinBg_Audio;
    [SerializeField] private AudioClip sizeup_audio;
    [SerializeField] private AudioClip megaWin;
    [SerializeField] private AudioClip BonusBg_Audio;
    [SerializeField] private AudioClip ByTheOrder;

    private List<AudioSource> allSources;
    private readonly Dictionary<AudioSource, bool> preFocusMuteState = new Dictionary<AudioSource, bool>();
    private bool isForceMuted = false;

    private void Awake()
    {
        allSources = new List<AudioSource> { bg_adudio, audioPlayer_wl, audioPlayer_button, audioPlayer_spin_stop, WinningSound };
        playBgAudio();
        //if (bg_adudio) bg_adudio.Play();
        //audioPlayer_button.clip = clips[clips.Length - 1];
    }

    internal void PlayByTheOrderAudio(){
        WinningSound.clip=ByTheOrder;
        WinningSound.Play();
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
        SetMuteAll(!focus);
    }

    internal void SetMuteAll(bool forceMute)
    {
        if (forceMute == isForceMuted) return;
        isForceMuted = forceMute;

        foreach (var source in allSources)
        {
            if (source == null) continue;
            if (forceMute)
            {
                preFocusMuteState[source] = source.mute;
                source.mute = true;
            }
            else
            {
                source.mute = preFocusMuteState.TryGetValue(source, out bool prevMuted) ? prevMuted : source.mute;
            }
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
            else if(type == "Bonus")
                bg_adudio.clip = BonusBg_Audio;
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
                SetSourceMute(bg_adudio, toggle);
                break;
            case "button":
                SetSourceMute(audioPlayer_button, toggle);
                SetSourceMute(audioPlayer_spin_stop, toggle);
                break;
            case "wl":
                SetSourceMute(audioPlayer_wl, toggle);
                SetSourceMute(WinningSound, toggle);
                break;
            case "all":
                SetSourceMute(bg_adudio, toggle);
                SetSourceMute(audioPlayer_button, toggle);
                SetSourceMute(audioPlayer_spin_stop, toggle);
                SetSourceMute(audioPlayer_wl, toggle);
                SetSourceMute(WinningSound, toggle);
                break;



        }
    }

    private void SetSourceMute(AudioSource source, bool toggle)
    {
        if (source == null) return;
        source.mute = toggle;
        if (isForceMuted) preFocusMuteState[source] = toggle;
    }

}
