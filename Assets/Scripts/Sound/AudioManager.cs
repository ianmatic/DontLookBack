using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public List<Sound> sounds;

    public static AudioManager instance;
    public List<Sound> wanderGrunts;
    public List<Sound> huntGrunts;

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

        sounds.AddRange(wanderGrunts);
        sounds.AddRange(huntGrunts);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

    }

    /// <summary>
    /// Call this to play a sound, leave sourceObject as null for non-spatial sound
    /// </summary>
    /// <param name="name"></param>
    /// <param name="sourceObject"></param>
    public void Play(string name, GameObject sourceObject = null)
    {
        Sound s = GetSound(name);
        Build3DSound(s, sourceObject);
        if (s != null)
        {
            s.source.Play();
        }
    }

    public Sound GetSound(string name)
    {
        foreach (Sound s in sounds)
        {
            if (s.name == name)
            {
                return s;
            }
        }
        return null;
    }

    /// <summary>
    /// Call this to start looping and to begin a looping sfx, leave sourceObject as null for non-spatial sound
    /// </summary>
    /// <param name="name"></param>
    public void PlayLoopSound(string name, GameObject sourceObject = null)
    {
        Sound s = GetSound(name);
        // remake source at new game object
        Build3DSound(s, sourceObject);

        if (!GetSound(name).source.isPlaying)
        {
            Play(name, sourceObject);
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


    private void Build3DSound(Sound s, GameObject sourceObject)
    {
        // only build if not null
        if (sourceObject && sourceObject != s.source.gameObject)
        {
            if (s.source)
            {
                Destroy(s.source);
            }
            s.source = sourceObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

            s.source.spatialBlend = 1;
            s.source.rolloffMode = AudioRolloffMode.Linear;
            s.source.minDistance = 1;
            s.source.maxDistance = 20;
        }
    }

    /// <summary>
    /// Plays a random wander Grunt, use sourceObject for 3d sound
    /// </summary>
    /// <param name="sourceObject"></param>
    public void PlayRandomWanderGrunt(GameObject sourceObject = null)
    {
        Play(wanderGrunts[Random.Range(0, wanderGrunts.Count)].name, sourceObject);
    }

    /// <summary>
    /// Plays a random hunt Grunt, use sourceObject for 3d sound
    /// </summary>
    /// <param name="sourceObject"></param>
    public void PlayRandomHuntGrunt(GameObject sourceObject = null)
    {
        Play(huntGrunts[Random.Range(0, huntGrunts.Count)].name, sourceObject);
    }
}
