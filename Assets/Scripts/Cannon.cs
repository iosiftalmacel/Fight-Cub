using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Cannon : NetworkBehaviour
{

    public Transform cannonContainer;
    public Transform bomb;

    public Transform boom;

    public bool hasShot;

    public Vector3 startRotation;
    public Vector3 endRotation;

    protected Vector3 nextRotation;

    [SyncVar(hook = "RpcChangePos")]
    protected Vector3 currentPos;

	// Use this for initialization
	void Start () {
        nextRotation = endRotation;
	}

    void RpcChangePos(Vector3 current)
    {
        cannonContainer.transform.eulerAngles = current;
    }
    Vector2 next;
    // Update is called once per frame
    void Update () {
        cannonContainer.transform.eulerAngles = new Vector3 (0, 0, Mathf.MoveTowardsAngle(cannonContainer.transform.eulerAngles.z, nextRotation.z, Time.deltaTime * 30));

        float rotZ = cannonContainer.transform.eulerAngles.z > 180 ? cannonContainer.transform.eulerAngles.z - 360 : cannonContainer.transform.eulerAngles.z;
        if (Mathf.Abs(rotZ - nextRotation.z) < 5)
        {
            nextRotation = nextRotation == startRotation ? endRotation : startRotation;
            if (isServer)
                currentPos = cannonContainer.transform.eulerAngles;
        }
	}


    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Player" && coll.gameObject.GetComponent<Player>().isLocalPlayer && !hasShot)
        {
            Vector2 foward = new Vector2(cannonContainer.transform.right.y, -cannonContainer.transform.right.x);
            bomb.gameObject.SetActive(true);
            bomb.transform.position = (Vector2) cannonContainer.transform.position + foward * 0.5f;
            bomb.GetComponent<Rigidbody2D>().AddForce(foward * 200);
            boom.gameObject.SetActive(true);
            this.transform.position = (Vector2)this.transform.position - new Vector2(0, 0.037f);

            AudioManager.Instance.Play("ig_cannon_button", 0.4f);
            AudioManager.Instance.Play("ig_cannon_shot", 0.3f);

            StartCoroutine(Reset());
        }
    }

    public void Shoot(Vector2 foward)
    {
        bomb.gameObject.SetActive(true);
        bomb.transform.position = (Vector2)cannonContainer.transform.position + foward * 0.5f;
        bomb.GetComponent<Rigidbody2D>().AddForce(foward * 200);
        boom.gameObject.SetActive(true);
        this.transform.position = (Vector2)this.transform.position - new Vector2(0, 0.037f);
        hasShot = true;
        AudioManager.Instance.Play("ig_cannon_button", 0.4f);
        AudioManager.Instance.Play("ig_cannon_shot", 0.3f);

        StartCoroutine(Reset());
    }


    public Vector2 GetFoward()
    {
        return new Vector2(cannonContainer.transform.right.y, -cannonContainer.transform.right.x);
    }

    IEnumerator Reset()
    {
        yield return new WaitForEndOfFrame();
        hasShot = true;
        yield return new WaitForSeconds( 0.2f);
        boom.gameObject.SetActive(false);

        yield return new WaitForSeconds(5);
        this.transform.position = (Vector2)this.transform.position + new Vector2(0, 0.037f);
        hasShot = false;
    }
    
}
