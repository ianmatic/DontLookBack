using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager instance;

    void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    public void Play (string name) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s != null)
        {
            s.source.Play();
        }
    }  

    public Sound GetSound(string name)
    {
        return Array.Find(sounds, sound => sound.name == name);
    }

    /// <summary>
    /// Call this to start looping and to begin a looping sfx
    /// </summary>
    /// <param name="name"></param>
    public void PlayLoopSound(string name)
    {
        if (!GetSound(name).source.isPlaying)
        {
            Play(name);
        }
    }

    /// <summary>
    /// Call this to stop looping and to end a looping sfx
    /// </summary>
    /// <param name="name"></param>
    public void EndLoopSound(string name)
    {
        if (GetSound(name).source.isPlaying)
        {
            GetSound(name).source.Stop();
        }
    }
}
