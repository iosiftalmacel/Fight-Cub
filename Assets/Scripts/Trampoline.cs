using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Trampoline : NetworkBehaviour {

    Animator animator;
	void Start () {
        animator = GetComponent<Animator>();
        first.SetActive(true);
        second.SetActive(false);
    }
    public GameObject first;
    public GameObject second;

    void OnTriggerEnter2D(Collider2D coll)
    {
      
        if (coll.gameObject.tag == "Player")
        {
            animator.enabled = true;
            first.SetActive(false);
            second.SetActive(true);
            animator.Play("TrampolineAnim");
            if (coll.gameObject.GetComponent<Player>().isLocalPlayer)
            {
                Rigidbody2D playerRigid = coll.GetComponent<Rigidbody2D>();
                playerRigid.velocity = new Vector2(playerRigid.velocity.x, 8.8f);
                AudioManager.Instance.Play("ig_trampoline_heavy", 0.2f);
            }
            StartCoroutine(Reset());
        }
    }

    IEnumerator Reset()
    {
        yield return new WaitForSeconds(3f);
        first.SetActive(true);
        second.SetActive(false);
        animator.enabled = false;
    }

}
