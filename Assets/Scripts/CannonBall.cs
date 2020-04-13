using UnityEngine;
using System.Collections;

public class CannonBall : MonoBehaviour {

	// Use this for initialization
	void OnEnable () {
        particle.GetComponent<CircleCollider2D>().enabled = false;
	}
    public ParticleSystem particle;
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (!coll.gameObject.name.Equals("Explosion"))
        {
            particle.transform.parent = null;
            particle.transform.position = coll.contacts[0].point;
            particle.Play();
            particle.GetComponent<CircleCollider2D>().enabled = true;
            AudioManager.Instance.Play("ig_cannon_explosion", 1f);
            StartCoroutine(Reset());
        }    
    }

    IEnumerator Reset()
    {
        yield return new WaitForSeconds(0.1f);
        particle.GetComponent<CircleCollider2D>().enabled = false;
        this.gameObject.SetActive(false);
    }

}
