using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public abstract class Player : NetworkBehaviour{

    public float radius;
    public float maxSpeed;
    public float acceleration;
    public float airAcceleration;
    public float maxAirSpeed;
    public float maxJumpForce;
    public float jumpMultyplier;
    public int pizzaCounter;

    public abstract void OnSlowSwipe(int direction);
    public abstract void OnFastSwipe(Vector2 dir, float force);
    public abstract void OnSwipeFinish(Vector2 dir);

    public abstract void OnIdleEnter(Player player, float time);
    public abstract void OnIdleExec(Player player, float time);
    public abstract void OnIdleExit(Player player, float time);

    public abstract void OnMovingEnter(Player player, float time);
    public abstract void OnMovingExec(Player player, float time);
    public abstract void OnMovingExit(Player player, float time);

    public abstract void OnJumpingEnter(Player player, float time);
    public abstract void OnJumpingExec(Player player, float time);
    public abstract void OnJumpingExit(Player player, float time);

    public abstract void OnFallingEnter(Player player, float time);
    public abstract void OnFallingExec(Player player, float time);
    public abstract void OnFallingExit(Player player, float time);

    public abstract void OnDieEnter(Player player, float time);
    public abstract void OnDieExec(Player player, float time);
    public abstract void OnDieExit(Player player, float time);

    public abstract void OnThrowedEnter(Player player, float time);
    public abstract void OnThrowedExec(Player player, float time);
    public abstract void OnThrowedExit(Player player, float time);

    public abstract void OnSpecialEnter(Player player, float time);
    public abstract void OnSpecialExec(Player player, float time);
    public abstract void OnSpecialExit(Player player, float time);

    public abstract void PlayerUpdate();

    public FSMObject<Player, PlayerStates> playerState;
    protected Rigidbody2D rigidBody;
    protected Animator playerAnim;
    protected ParticleSystem blood;

    public Sprite[] head;
    public Sprite[] hand;
    public Sprite[] tail;

    public Sprite[] bloodSplatters;
    public Sprite burnedSplatter;

    public LayerMask groundMask;
    public PlayerType playerType;
    public Sprite specialSprite;
    public float specialCoolDownMax;

    Text pizzaCounterUi;

    [HideInInspector]
    public GameInput.PlayerInputs input;

    protected int dieType;
    protected Vector2 lastPos;

    [SyncVar(hook = "RpcCannonShoot")]
    protected Vector3 cannonShotDir;


    [SyncVar(hook = "RpcOnPlayerImpact")]
    protected Vector2 impactForce;

    [SyncVar]
    protected PlayerStates currentPlayerState;

    [SyncVar(hook = "RpcOnPositionUpdate")]
    protected Vector2 currentPosition;

    [SyncVar, HideInInspector]
    public string playerName;

    [SyncVar(hook = "RpcOnPlatipusSpecial")]
    protected PlatipusSpecial platipusSpecial;

    [SyncVar(hook = "RpcOnTRexSpecial")]
    protected TRexSpecial tRexSpecial;

    [SyncVar(hook = "RpcOnCatSpecial")]
    protected CatSpecial catSpecial;

    protected struct PlatipusSpecial
    {
        public float time;
        public Vector2 position;
    }

    protected struct TRexSpecial
    {
        public float time;
        public Vector2 position;
        public Vector2 dir;
    }

    protected struct CatSpecial
    {
        public float time;
        public float duration;
    }

    public enum PlayerType
    {
        Cat,
        Platipus,
        TRex
    }

    public enum PlayerStates
    {
        Idle,
        Moving,
        Jumping,
        Falling,
        Throwed,
        Special,
        Die
    }

    //[SyncVar(hook = "RpcChangeState")]
    //protected PlayerStates currenPlayerState;

    [SyncVar(hook = "RpcChangeDie")]
    protected int currentDieType;

    private Vector3 initialRotation;

    protected Vector2 fireBallInitialPos;
    protected ParticleSystem fireBall;

    protected ParticleSystem dash;


    protected Button buttonSpecial;
    protected Text specialCoodDown;
    protected float coodDownRemaining;

    protected bool specialDisabled;

    protected bool nextWillBeSpecial;

    protected bool autoAdjustRotation = true;

    protected float lastColTime;

    protected void Awake()
    {
        playerState.AddState(PlayerStates.Idle, OnIdleEnter, OnIdleExec, OnIdleExit);
        playerState.AddState(PlayerStates.Moving, OnMovingEnter, OnMovingExec, OnMovingExit);
        playerState.AddState(PlayerStates.Jumping, OnJumpingEnter, OnJumpingExec, OnJumpingExit);
        playerState.AddState(PlayerStates.Falling, OnFallingEnter, OnFallingExec, OnFallingExit);
        playerState.AddState(PlayerStates.Throwed, OnThrowedEnter, OnThrowedExec, OnThrowedExit);
        playerState.AddState(PlayerStates.Die, OnDieEnter, OnDieExec, OnDieExit);
        playerState.AddState(PlayerStates.Special, OnSpecialEnter, OnSpecialExec, OnSpecialExit);


        playerState.State = PlayerStates.Falling;

        input = GetComponent<GameInput.PlayerInputs>();

        rigidBody = GetComponent<Rigidbody2D>();
        playerAnim = GetComponent<Animator>();
        initialRotation = transform.eulerAngles;
        blood = transform.Find("Blood").GetComponent<ParticleSystem>();
        pizzaCounterUi = GameObject.Find("PizzaCounter") ? GameObject.Find("PizzaCounter").GetComponent<Text>() : null;
    }

    void Start()
    {
        if (isLocalPlayer)
        {
            buttonSpecial = GameObject.Find("BtnSpecial").GetComponent<Button>();
            buttonSpecial.transform.Find("Image").GetComponent<Image>().sprite = specialSprite;
            buttonSpecial.onClick.AddListener(TryToActivateSpecial);

            GameObject.Find("BtnHome").GetComponent<Button>().onClick.AddListener(GoHome);

            specialCoodDown = buttonSpecial.transform.Find("Overlay").GetComponentInChildren<Text>();

            pizzaCounter = 0;
            if(pizzaCounterUi) pizzaCounterUi.text = "0";
        }

        if (playerType == PlayerType.TRex)
        {
            fireBall = transform.Find("Player_platy_head").GetChild(0).GetComponent<ParticleSystem>();
            fireBallInitialPos = fireBall.transform.localPosition;
            fireBall.gameObject.SetActive(false);
        }
        else if (playerType == PlayerType.Cat)
        {
            dash = transform.Find("Dash").GetComponent<ParticleSystem>();
            dash.Stop();
        }
    }

    protected void GoHome()
    {
        //SceneManager.LoadScene("ChoosePlayer");
        ((MultiplayerManager)NetworkManager.singleton).Disconnect();
    }
    protected void TryToActivateSpecial()
    {

        if(!specialDisabled && playerState.State != PlayerStates.Die)
        {
            if (playerType == PlayerType.Platipus)
            {
                if(!IsGrounded() && (playerState.State == PlayerStates.Jumping || playerState.State == PlayerStates.Falling || playerState.State == PlayerStates.Throwed))
                {
                    playerState.State = PlayerStates.Special;
                    buttonSpecial.transform.Find("Image").GetComponent<Image>().color = Color.blue;
                }
            }
            else if (playerType == PlayerType.TRex)
            {
                playerState.State = PlayerStates.Special;
                buttonSpecial.transform.Find("Image").GetComponent<Image>().color = Color.red;
            }
            else if (playerType == PlayerType.Cat)
            {
                if(IsGrounded())
                {
                    buttonSpecial.transform.Find("Image").GetComponent<Image>().color = Color.green;
                    playerState.State = PlayerStates.Special;  
                }
            }

        }
    }

    protected void Update()
    {
        if (transform.position.y < -15)
            Respawn();

        if(autoAdjustRotation)
            this.transform.eulerAngles = new Vector3(initialRotation.x, this.transform.eulerAngles.y, initialRotation.z);

        if (this.transform.position.x - lastPos.x > 0.02f)
        {
            this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, 180, this.transform.eulerAngles.z);
            lastPos = this.transform.position;
        }
        else if (this.transform.position.x - lastPos.x < -0.02f)
        {
            this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, 0, this.transform.eulerAngles.z);
            lastPos = this.transform.position;
        }

        if (isLocalPlayer)
        {
            if (playerState.State != currentPlayerState)
                CmdChangeState(playerState.State);
            playerState.Update(Time.deltaTime);
            PlayerUpdate();

            if (specialDisabled) {
                coodDownRemaining -= Time.deltaTime;
                specialCoodDown.text = coodDownRemaining.ToString("F0");
                if (coodDownRemaining < 0)
                {
                    specialDisabled = false;
                    specialCoodDown.transform.parent.gameObject.SetActive(false);
                }
            }
        }
        //Debug.LogError("state - " + playerState.State);
    }

    public bool IsGrounded()
    {
        return Physics2D.Raycast(transform.position, -Vector2.up, radius, groundMask);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawSphere(transform.position, radius);
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (!isLocalPlayer)
            return;

        if (coll.gameObject.name == "Saw" && playerState.State != PlayerStates.Die)
        {
            dieType = 0;
            playerState.State = PlayerStates.Die;
            CmdChangeDie(1);
        }

        if(coll.gameObject.tag == "CannonButton")
        {
            if(!coll.gameObject.GetComponent<Cannon>().hasShot)
                CmdCannonShoot(coll.gameObject.GetComponent<Cannon>().GetFoward());
        }

        if (coll.gameObject.tag == "Player")
        {
            if(Time.realtimeSinceStartup - lastColTime > 0.1f)
            {
                lastColTime = Time.realtimeSinceStartup;
            }
            else
            {
                return;
            }

            Vector2 force = Vector2.zero;
            Rigidbody2D other = coll.gameObject.GetComponent<Rigidbody2D>();
            PlayerStates otherPlayerState = coll.gameObject.GetComponent<Player>().currentPlayerState;

            Debug.LogError("__Collided__");

            CmdUpdatePosition(this.transform.position);

            if (playerState.State == PlayerStates.Moving)
            {
                if (otherPlayerState == PlayerStates.Idle)
                {
                    force = new Vector2(Random.Range(rigidBody.velocity.x - 0.2f, rigidBody.velocity.x + 0.2f) * Random.Range(2.0f, 3.0f), Random.Range(1.5f, 2.5f));
                }
                else if (otherPlayerState == PlayerStates.Moving)
                {
                    force = new Vector2(Random.Range(rigidBody.velocity.x - 0.2f, rigidBody.velocity.x + 0.2f) * Random.Range(2.0f, 3.0f), Random.Range(2f, 3.0f));

                    if(Mathf.Abs(other.velocity.x) > 0.2f)
                    {
                        playerState.State = PlayerStates.Throwed;
                        rigidBody.velocity = new Vector2(Random.Range(other.velocity.x - 0.2f, other.velocity.x + 0.2f) * Random.Range(2.0f, 3.0f), Random.Range(2f, 3.0f));
                    }
                }
                else if (otherPlayerState == PlayerStates.Falling)
                {
                    force = new Vector2(other.velocity.x + rigidBody.velocity.x * Random.Range(0.5f, 0.8f), other.velocity.y * Random.Range(0.5f, 0.6f));

                    playerState.State = PlayerStates.Throwed;
                    rigidBody.velocity = new Vector2(other.velocity.x * Random.Range(2f, 3f), Mathf.Abs(other.velocity.y) * Random.Range(1, 2f));
                }
                else if (otherPlayerState == PlayerStates.Jumping)
                {
                    force = new Vector2(other.velocity.x + rigidBody.velocity.x * Random.Range(0.5f, 0.8f), other.velocity.y * Random.Range(0.8f, 1f));

                    playerState.State = PlayerStates.Throwed;
                    rigidBody.velocity = new Vector2(rigidBody.velocity.x + other.velocity.x * Random.Range(1.5f, 2.5f), Mathf.Abs(other.velocity.y) * 1.5f);
                }
            }
            else if (playerState.State == PlayerStates.Jumping)
            {
                if (otherPlayerState == PlayerStates.Idle)
                {
                    force = new Vector2(rigidBody.velocity.x * Random.Range(1.5f, 2.5f), Mathf.Abs(rigidBody.velocity.y) * Random.Range(1f, 2f));
                }
                else if (otherPlayerState == PlayerStates.Moving)
                {
                    force = new Vector2(other.velocity.x + rigidBody.velocity.x * Random.Range(1.5f, 2.5f), Mathf.Abs(rigidBody.velocity.y) * 1.5f);

                    playerState.State = PlayerStates.Throwed;
                    rigidBody.velocity = new Vector2(rigidBody.velocity.x + other.velocity.x * Random.Range(0.5f, 0.8f), rigidBody.velocity.y * Random.Range(0.8f, 1f));
                 }
                else if (otherPlayerState == PlayerStates.Falling)
                {
                    force = new Vector2(other.velocity.x + rigidBody.velocity.x * Random.Range(1.5f, 2.5f), rigidBody.velocity.y * 1.5f);

                    playerState.State = PlayerStates.Throwed;
                    rigidBody.velocity = new Vector2(rigidBody.velocity.x, rigidBody.velocity.y * 0.8f);
                }
                else if (otherPlayerState == PlayerStates.Jumping)
                {
                    force = new Vector2(other.velocity.x + rigidBody.velocity.x * Random.Range(0.5f, 1.5f), other.velocity.y * 1.5f);

                    playerState.State = PlayerStates.Throwed;
                    rigidBody.velocity = new Vector2(rigidBody.velocity.x + other.velocity.x * Random.Range(0.5f, 1.5f), rigidBody.velocity.y * 1.5f);
                }
            }
            else if (playerState.State == PlayerStates.Falling)
            {
                if (otherPlayerState == PlayerStates.Idle)
                {
                    force = new Vector2(rigidBody.velocity.x * Random.Range(1.5f, 2.5f), Mathf.Abs(rigidBody.velocity.y) * Random.Range(1.5f, 2.5f));
                }
                else if (otherPlayerState == PlayerStates.Moving)
                {
                    force = new Vector2(rigidBody.velocity.x * Random.Range(2f, 3f), Mathf.Abs(rigidBody.velocity.y) * Random.Range(1, 2f));

                    playerState.State = PlayerStates.Throwed;
                    rigidBody.velocity = new Vector2(rigidBody.velocity.x + other.velocity.x * Random.Range(0.5f, 0.8f), rigidBody.velocity.y * Random.Range(0.5f, 0.6f));
                }
                else if (otherPlayerState == PlayerStates.Falling)
                {
                    force = new Vector2(other.velocity.x + rigidBody.velocity.x * Random.Range(1f, 2.0f), other.velocity.y * Random.Range(0.5f, 0.8f));

                    playerState.State = PlayerStates.Throwed;
                    rigidBody.velocity = new Vector2(rigidBody.velocity.x + other.velocity.x * Random.Range(1f, 2.0f), rigidBody.velocity.y * Random.Range(0.5f, 0.8f));
                }
                else if (otherPlayerState == PlayerStates.Jumping)
                {
                    force = new Vector2(rigidBody.velocity.x, rigidBody.velocity.y * 0.8f);
                   
                    playerState.State = PlayerStates.Throwed;
                    rigidBody.velocity = new Vector2(rigidBody.velocity.x + other.velocity.x * Random.Range(1.5f, 2.5f), other.velocity.y * 1.5f);
                }
            }
            rigidBody.velocity *= 2;
            //if(other.velocity.x)
            CmdAddForce(coll.gameObject.GetComponent<Player>().playerName, force * 2);
        }

    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (!isLocalPlayer)
            return;

        if ((coll.gameObject.name == "Explosion" || coll.gameObject.name == "Fire" || (coll.gameObject.name == "FireBall" && (fireBall == null || coll.gameObject != fireBall.gameObject))) && playerState.State != PlayerStates.Die)
        {
            dieType = 1;
            playerState.State = PlayerStates.Die;
            CmdChangeDie(2);
        }
        else if (coll.gameObject.tag == "Pizza")
        {
            pizzaCounter++;
            if(pizzaCounterUi) pizzaCounterUi.text = pizzaCounter.ToString();
            Cmd_DestroyThis(coll.GetComponent<NetworkIdentity>().netId);
            //NetworkServer.Destroy(coll.gameObject);
        }
    }

    [Command]
    public void Cmd_DestroyThis(NetworkInstanceId netID)
    {
         GameObject theObject = NetworkServer.FindLocalObject(netID);
         NetworkServer.Destroy(theObject);
    }

    protected void Respawn()
    {
        if (isLocalPlayer)
        {
            // Set the spawn point to origin as a default value
            Vector3 spawnPoint = Vector3.zero;
            var spawnPoints = FindObjectsOfType<NetworkStartPosition>();

            // If there is a spawn point array and the array is not empty, pick one at random
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
            }

            // Set the player’s position to the chosen spawn point
            transform.position = spawnPoint;
            playerState.State = PlayerStates.Idle;

            CmdUpdatePosition(spawnPoint);
            CmdChangeDie(0);
            //CmdChangeState(PlayerStates.Idle);
        }
    }

    //[Command]
    //protected void CmdChangeState(PlayerStates state)
    //{
    //    currenPlayerState = state;
    //}
    //protected void RpcChangeState(PlayerStates state)
    //{
    //    if (!isLocalPlayer)
    //        playerState.State = state;
    //}

    [Command]
    protected void CmdChangeDie(int type)
    {
        currentDieType = type;
    }

    [Command]
    public void CmdCannonShoot(Vector3 dir)
    {
        cannonShotDir = dir;
    }

    [Command]
    public void CmdAddForce(string uniqueId, Vector2 dir)
    {
        GameObject go = GameObject.Find(uniqueId);
        go.GetComponent<Player>().impactForce = dir;
    }

    [Command]
    protected void CmdChangeState(PlayerStates state)
    {
        currentPlayerState = state;
    }

    [Command]
    protected void CmdUpdatePosition(Vector2 position)
    {
        currentPosition = position;
    }

    [Command]
    protected void CmdOnPlatipusSpecial(PlatipusSpecial special)
    {
        platipusSpecial = special;
    }

    [Command]
    protected void CmdOnTRexSpecial(TRexSpecial special)
    {
        tRexSpecial = special;
    }

    [Command]
    protected void CmdOnCatSpecial(CatSpecial special)
    {
        catSpecial = special;
    }


    protected void RpcOnPlatipusSpecial(PlatipusSpecial special)
    {
        if (isLocalPlayer)
            return;
        this.transform.position = special.position;
        transform.Find("Dirt").GetComponent<ParticleSystem>().Play();

        GetComponent<CameraController>().Shake(0.045f, 0.08f);
    }

    protected void RpcOnTRexSpecial(TRexSpecial special)
    {
        if (isLocalPlayer)
            return;

        fireBall.transform.parent = transform.Find("Player_platy_head");
        fireBall.transform.localPosition = fireBallInitialPos;
        fireBall.transform.parent = null;

        fireBall.gameObject.SetActive(true);
        fireBall.Play();
        fireBall.GetComponent<FireBall>().Shoot(special.dir, special.position);

        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, (special.dir.x > 0 ? 0 : 180));
        lastPos = this.transform.position + new Vector3(0.03f * (special.dir.x > 0 ? -1 : 1), 0, 0);
    }

    protected void RpcOnCatSpecial(CatSpecial special)
    {
        if (isLocalPlayer)
            return;

        dash.Play();
        StartCoroutine(DisableDelayed(dash , special.duration));
    }

    protected void RpcOnPlayerImpact(Vector2 dir)
    {
        if (dir != Vector2.zero && playerState.State != PlayerStates.Die && playerState.State != PlayerStates.Special)
        {
            Debug.LogError("__Message__");

            if (Time.realtimeSinceStartup - lastColTime > 0.1f)
            {
                lastColTime = Time.realtimeSinceStartup;
            }
            else
            {
                return;
            }

            rigidBody.velocity = dir;
            playerState.State = PlayerStates.Throwed;
            impactForce = Vector2.zero;
        }
    }

    protected void RpcOnPositionUpdate(Vector2 pos)
    {
        if (!isLocalPlayer)
            transform.position = pos;//Vector2.Lerp(transform.position, pos, Time.deltaTime);
    }

    protected void RpcChangeDie(int type)
    {
        if (!isLocalPlayer)
        {
            if (type == 1)
                blood.Play();

            transform.Find("Player_platy_head").GetComponent<SpriteRenderer>().sprite = head[type];
            transform.Find("Player_platy_hand").GetComponent<SpriteRenderer>().sprite = hand[type];
            transform.Find("Player_platy_hand2").GetComponent<SpriteRenderer>().sprite = hand[type];
            transform.Find("Player_platy_tail").GetComponent<SpriteRenderer>().sprite = tail[type];

            if (type != 0)
            {
                AudioManager.Instance.Play("death_scream", 0.2f);
            }

            if (type == 1)
            {
                AudioManager.Instance.Play("death_circular_saw", 0.05f);
            }
            else if (type == 2)
            {
                AudioManager.Instance.Play("death_fire", 0.05f);
            }
        }
    }

    protected void RpcCannonShoot(Vector3 dir)
    {
        if (!isLocalPlayer)
        {
            GameObject.FindGameObjectWithTag("CannonButton").GetComponent<Cannon>().Shoot(dir);
        }
    }

    IEnumerator DisableDelayed(ParticleSystem obj, float duration)
    {
        yield return new WaitForSeconds(duration);
        obj.Stop();
    }
}
