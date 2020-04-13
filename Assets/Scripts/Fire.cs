using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Fire : NetworkBehaviour
{
    public ParticleSystem[] particles;

    float lastToggleTime;
    [SyncVar(hook = "RpcToggleFire")]
    public bool enabled;

	void Start () {
        lastToggleTime = Time.timeSinceLevelLoad;
    }

    // Update is called once per frame
    void Update () {

		if(isServer && Time.timeSinceLevelLoad - lastToggleTime > 6)
        {
            foreach(ParticleSystem system in particles)
            {
                if (enabled)
                {
                    AudioManager.Instance.Stop("ig_loop_fire");
                    system.Stop();
                    system.Clear();
                    system.GetComponent<BoxCollider2D>().enabled = false;
                }
                else
                {
                    AudioManager.Instance.Play("ig_loop_fire", 0.15f, true);
                    system.Play();
                    system.Clear();
                    system.GetComponent<BoxCollider2D>().enabled = true;
                }
            }
            enabled = !enabled;
            lastToggleTime = Time.timeSinceLevelLoad;
        }

        if (!isServer)
        {
           
        }
	}

    void RpcToggleFire(bool enabled)
    {
        foreach (ParticleSystem system in particles)
        {
            if (!enabled)
            {
                AudioManager.Instance.Stop("ig_loop_fire");
                system.Stop();
                system.Clear();
                system.GetComponent<BoxCollider2D>().enabled = false;
            }
            else
            {
                AudioManager.Instance.Play("ig_loop_fire", 0.1f, true);
                system.Play();
                system.Clear();
                system.GetComponent<BoxCollider2D>().enabled = true;
            }
        }
    }
}
