using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    public static AudioManager Instance;

    private AudioSource[] sources;
    public AudioClip[] clips;

	void Awake () {
        Instance = this;

        sources = new AudioSource[20];
        for (int i = 0; i < 20; i++)
        {
            sources[i] = this.gameObject.AddComponent<AudioSource>();
            sources[i].playOnAwake = false;
        }

        DontDestroyOnLoad(this);
	}

    public void Play(AudioClip clip, float volume = 1, bool loop = false)
    {
        for (int j = 0; j < sources.Length; j++)
        {
            if (sources[j].isPlaying && sources[j].clip.name == clip.name)
            {
                sources[j].Stop();
            }
        }

        AudioSource current = GetFreeAudioSource();
        current.clip = clip;
        current.volume = volume;
        current.loop = loop;
        current.Play();
    }

    public void Play(string name, float volume = 1, bool loop = false)
    {
        AudioSource current = GetFreeAudioSource();

        for (int j = 0; j < sources.Length; j++)
        {
            if (sources[j].isPlaying && sources[j].clip.name == name)
            {
                sources[j].Stop();
            }
        }

        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i].name == name)
            {
                current.clip = clips[i];
                current.volume = volume;
                current.loop = loop;
                current.Play();
                return;
            }
        }
    }

    public void Stop(string name)
    {
        for (int i = 0; i < sources.Length; i++)
        {
            if(sources[i].isPlaying && sources[i].clip.name == name){
                sources[i].Stop();
            }
        }
    }

    public void Stop(AudioClip clip)
    {
        for (int i = 0; i < sources.Length; i++)
        {
            if (sources[i].isPlaying && sources[i].clip.name == clip.name)
            {
                sources[i].Stop();
            }
        }
    }

    public void StopAll()
    {
        for (int i = 0; i < sources.Length; i++)
        {
            sources[i].Stop();
        }
    }
  
    AudioSource GetFreeAudioSource()
    {
        for (int i = 0; i < sources.Length; i++)
        {
            if (!sources[i].isPlaying)
            {
                return sources[i];
            }
        }

        return null;
    }
}
