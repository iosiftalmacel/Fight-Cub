using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GenericPlayer : Player {
    private Vector2 futureJump;
    private bool hasToJump;
    private float jumpTime;

    protected int specialDir;

    private bool hasUsedSplatter;

    public override void PlayerUpdate()
    {
        if ((playerState.State == PlayerStates.Idle || playerState.State == PlayerStates.Moving) && !IsGrounded() && !hasToJump)
        {
            playerState.State = PlayerStates.Falling;
        }

        if (!hasToJump && playerState.State == PlayerStates.Jumping && IsGrounded())
        {
            playerState.State = PlayerStates.Idle;
        }
    }


    #region States
    public override void OnIdleEnter(Player player, float time) 
    {
        playerAnim.Play("Platy_Idle");
        //currenPlayerState = PlayerStates.Idle;
    }
    public override void OnIdleExec(Player player, float time)
    {
        rigidBody.velocity *= 0.2f;
    }
    public override void OnIdleExit(Player player, float time)
    {

    }

    public override void OnMovingEnter(Player player, float time)
    {
        playerAnim.Play("Platy_Run");
        AudioManager.Instance.Play("ig_footsteps", 0.1f, true);
        //currenPlayerState = PlayerStates.Moving;
    }
    public override void OnMovingExec(Player player, float time)
    {
    }
    public override void OnMovingExit(Player player, float time)
    {
        AudioManager.Instance.Stop("ig_footsteps");
    }
           
    public override void OnJumpingEnter(Player player, float time)
    {
        jumpTime = Time.timeSinceLevelLoad;
        playerAnim.Play("Platy_Jump");
        AudioManager.Instance.Play("ig_jump_standard", 0.5f);
    }
    public override void OnJumpingExec(Player player, float time)
    {
        if (!hasToJump && rigidBody.velocity.y < 0)
        {
            playerState.State = PlayerStates.Falling;
        }
        if (hasToJump && Time.timeSinceLevelLoad - jumpTime > 0.075f)
        {
            rigidBody.velocity = futureJump;
        }

        if (hasToJump && (Time.timeSinceLevelLoad - jumpTime > 0.4f || !IsGrounded()))
        {
            hasToJump = false;
        }

        this.rigidBody.velocity = new Vector2(Mathf.Clamp(rigidBody.velocity.x, -maxAirSpeed, maxAirSpeed), Mathf.Clamp(this.rigidBody.velocity.y, -100, 100));

        input.isMoving = true;
    }
    public override void OnJumpingExit(Player player, float time)
    {
        hasToJump = false;
    }
           
    public override void OnFallingEnter(Player player, float time)
    {

    }
    public override void OnFallingExec(Player player, float time)
    {
        this.rigidBody.velocity = new Vector2(Mathf.Clamp(rigidBody.velocity.x, -maxAirSpeed, maxAirSpeed), Mathf.Clamp(this.rigidBody.velocity.y, -100, 100));

        if (IsGrounded())
        {
            playerState.State = PlayerStates.Idle;
            //CmdChangeState(PlayerStates.Idle);
        }

        input.isMoving = true;
    }
    public override void OnFallingExit(Player player, float time)
    {

    }

    float throwedTime;
    public override void OnThrowedEnter(Player player, float time)
    {
        throwedTime = Time.timeSinceLevelLoad;
    }
    public override void OnThrowedExec(Player player, float time)
    {
        if(Time.timeSinceLevelLoad - throwedTime > 0.4f)
        {
            if (IsGrounded())
            {
                playerState.State = PlayerStates.Idle;
            }
        }
    }
    public override void OnThrowedExit(Player player, float time)
    {

    }

    public override void OnDieEnter(Player player, float time)
    {
        if(dieType == 0)
            blood.Play();

        transform.Find("Player_platy_head").GetComponent<SpriteRenderer>().sprite = head[dieType + 1];
        transform.Find("Player_platy_hand").GetComponent<SpriteRenderer>().sprite = hand[dieType + 1];
        transform.Find("Player_platy_hand2").GetComponent<SpriteRenderer>().sprite = hand[dieType + 1];
        transform.Find("Player_platy_tail").GetComponent<SpriteRenderer>().sprite = tail[dieType + 1];

        
        AudioManager.Instance.Play("death_scream", 0.8f);

        if (dieType == 0)
        {
            AudioManager.Instance.Play("death_circular_saw", 0.2f);
        }
        else if (dieType == 1)
        {
            AudioManager.Instance.Play("death_fire", 0.4f);
        }

        playerAnim.Play("Platy_Die");
        dieTime = Time.realtimeSinceStartup;

        AudioManager.Instance.Play("death_scream", 0.4f);
    }

    float dieTime;              
    public override void OnDieExec(Player player, float time)
    {
        rigidBody.velocity = new Vector2(rigidBody.velocity.x * 0.6f, rigidBody.velocity.y);

        if (!hasUsedSplatter && Mathf.Abs(rigidBody.velocity.x) < 0.01f && Mathf.Abs(rigidBody.velocity.y) < 0.01f)
        {

            hasUsedSplatter = true;
            if (dieType == 0)
            {
                GameObject bloodSplatter = new GameObject();
                SpriteRenderer spriteRender = bloodSplatter.AddComponent<SpriteRenderer>();
                spriteRender.sprite = bloodSplatters[Random.Range(0, bloodSplatters.Length)];
                spriteRender.sortingOrder = 5;
                bloodSplatter.transform.position = (Vector2)this.transform.position + new Vector2(0, -0.26f);
            }
            else if (dieType == 1)
            {
                GameObject splatter = new GameObject();
                SpriteRenderer spriteRender = splatter.AddComponent<SpriteRenderer>();
                spriteRender.sprite = burnedSplatter;
                spriteRender.sortingOrder = 5;
                splatter.transform.position = (Vector2)this.transform.position + new Vector2(0, -0.12f);
            }
           
        }

        if(Time.realtimeSinceStartup- dieTime > 2)
        {
            Respawn();
        }
    }

    public override void OnDieExit(Player player, float time)
    {
        transform.Find("Player_platy_head").GetComponent<SpriteRenderer>().sprite = head[0];
        transform.Find("Player_platy_hand").GetComponent<SpriteRenderer>().sprite = hand[0];
        transform.Find("Player_platy_hand2").GetComponent<SpriteRenderer>().sprite = hand[0];
        transform.Find("Player_platy_tail").GetComponent<SpriteRenderer>().sprite = tail[0];
    }


    public float specialEnterTime;
    public float specialEndTime;
    public bool specialStarted;

    public override void OnSpecialEnter(Player player, float time)
    {
        specialEnterTime = Time.timeSinceLevelLoad;
        specialEndTime = 0;
        specialStarted = false;

        if (playerType == PlayerType.Platipus)
            playerAnim.Play("Platy_Special");
        else if (playerType == PlayerType.TRex)
            playerAnim.Play("TRex_Special");
        else if (playerType == PlayerType.Cat)
        {
            dash.Play();
            playerAnim.Play("Cat_Special");
        }
    }

    public override void OnSpecialExec(Player player, float time)
    {
        if (playerType == PlayerType.Platipus)
        {
            if (Time.timeSinceLevelLoad - specialEnterTime < 0.3f)
            {
                rigidBody.velocity = Vector2.zero;
            }
            else
            {
                if (!IsGrounded())
                {
                    rigidBody.velocity = new Vector2(0, -15);
                    //specialStarted = true;
                }
                else
                {
                    if (specialEndTime == 0 && IsGrounded())
                    {
                        transform.Find("Dirt").GetComponent<ParticleSystem>().Play();
                        GetComponent<CameraController>().Shake(0.045f, 0.08f);
                        rigidBody.velocity = new Vector2(0, 0);

                        Collider2D[] nearPlayers = Physics2D.OverlapCircleAll(this.transform.position, 1.4f);

                        for (int i = 0; i < nearPlayers.Length; i++)
                        {
                            if (nearPlayers[i].gameObject != this.gameObject && nearPlayers[i].gameObject.layer == gameObject.layer)
                            {
                                Player current = nearPlayers[i].GetComponent<Player>();

                                Vector2 heading = (nearPlayers[i].transform.position - transform.position);
                                float mag = heading.magnitude;
                                Vector2 dir = heading / mag;

                                float force = Mathf.Clamp(0.8f - mag, 0, 0.8f) / 0.8f * 10 + 2;

                                CmdAddForce(current.playerName, dir * force + new Vector2(0, 10 * Mathf.Clamp(0.8f - mag, 0, 0.8f) / 0.8f + 2));

                            }
                        }
                        AudioManager.Instance.Play("baseball_hitbat", 0.4f);


                        PlatipusSpecial special = new PlatipusSpecial();
                        special.time = Time.timeSinceLevelLoad;
                        special.position = this.transform.position;
                        CmdOnPlatipusSpecial(special);

                        specialEndTime = Time.timeSinceLevelLoad;
                    }
                    else if (specialEndTime != 0 && Time.timeSinceLevelLoad - specialEndTime > 0.35f)
                    {
                        playerState.State = PlayerStates.Idle;
                    }
                }

            }
        }
        else if (playerType == PlayerType.TRex)
        {
            if (Time.timeSinceLevelLoad - specialEnterTime < 0.2f)
            {
                rigidBody.velocity = Vector2.zero;
            }
            else
            {
                if (!specialStarted)
                {
                    specialStarted = true;
                    fireBall.transform.parent = transform.Find("Player_platy_head");
                    fireBall.transform.localPosition = fireBallInitialPos;
                    fireBall.transform.parent = null;
                    fireBall.gameObject.SetActive(true);
                    fireBall.Play();
                }
                else if (specialStarted && specialEndTime == 0)
                {
                    GetComponent<CameraController>().Shake(0.025f, 0.06f);
                    fireBall.GetComponent<FireBall>().Shoot(new Vector2(transform.localEulerAngles.y == 180 ? 1 : -1, 0));
                    specialEndTime = Time.timeSinceLevelLoad;

                    TRexSpecial special = new TRexSpecial();
                    special.time = Time.timeSinceLevelLoad;
                    special.position = this.transform.position;
                    special.dir = new Vector2(transform.eulerAngles.y == 180 ? 1 : -1, 0);
                    CmdOnTRexSpecial(special);
                    AudioManager.Instance.Play("death_fire2", 0.4f);
                }
                else if (specialEndTime != 0 && Time.timeSinceLevelLoad - specialEndTime > 0.5f)
                {
                    if (IsGrounded())
                        playerState.State = PlayerStates.Idle;
                    else
                        playerState.State = PlayerStates.Falling;
                }
            }
        }
        else if (playerType == PlayerType.Cat)
        {
            if (Time.timeSinceLevelLoad - specialEnterTime < 0.2f)
            {
                rigidBody.velocity = Vector2.zero;
                specialDir = transform.localEulerAngles.y == 180 ? 1 : -1;
            }
            else
            {
                //float rotation = Mathf.Atan2(specialDir.y, Mathf.Abs(specialDir.x)) * 180 / Mathf.PI;
                //this.transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, -rotation);
                //rigidBody.velocity = specialDir * 15;
                rigidBody.velocity = specialDir * Vector2.right * 15;

                if (!specialStarted)
                {
                    specialStarted = true;
                    specialEndTime = Time.timeSinceLevelLoad;
                    autoAdjustRotation = false;
                    AudioManager.Instance.Play("ig_cat", 0.5f);

                    CatSpecial special = new CatSpecial();
                    special.time = Time.timeSinceLevelLoad;
                    special.duration = 0.2f;
                    CmdOnCatSpecial(special);
                }
                else if (specialStarted)
                {
                    if (Time.timeSinceLevelLoad - specialEndTime > 0.12f)
                    {
                        rigidBody.velocity = Vector2.zero;
                        autoAdjustRotation = true;
                        dash.Stop();

                        if (IsGrounded())
                            playerState.State = PlayerStates.Idle;
                        else
                            playerState.State = PlayerStates.Falling;
                    }
                    else
                    {
                        Collider2D[] nearPlayers = Physics2D.OverlapCircleAll(this.transform.position, 0.45f);

                        for (int i = 0; i < nearPlayers.Length; i++)
                        {
                            if (nearPlayers[i].gameObject != this.gameObject && nearPlayers[i].gameObject.layer == gameObject.layer)
                            {
                                Player current = nearPlayers[i].GetComponent<Player>();

                                CmdAddForce(current.playerName, rigidBody.velocity + new Vector2(0, 2));
                            }
                        }
                    }
                }
            }
        }
    }
    public override void OnSpecialExit(Player player, float time)
    {
        specialCoodDown.transform.parent.gameObject.SetActive(true);
        specialDisabled = true;
        autoAdjustRotation = true;
        if(dash != null)
            dash.Stop();
        coodDownRemaining = specialCoolDownMax;
        buttonSpecial.transform.Find("Image").GetComponent<Image>().color = Color.black;
    }
    #endregion

    #region Swipe
    public override void OnSlowSwipe(int direction)
    {
        if (hasToJump)
            return;

        if (IsGrounded() && !nextWillBeSpecial && playerState.State != PlayerStates.Die && playerState.State != PlayerStates.Jumping && playerState.State != PlayerStates.Falling)
        {
            if (playerState.State == PlayerStates.Moving){
                float velocityX = rigidBody.velocity.x;
                if (velocityX > 0 && direction == -1 || velocityX < 0 && direction == 1)
                {
                    rigidBody.velocity = new Vector2(acceleration * direction, rigidBody.velocity.y);
                }
                else
                {
                    velocityX += acceleration * direction;
                    velocityX = Mathf.Clamp(velocityX, -maxSpeed, maxSpeed);
                    rigidBody.velocity = new Vector2(velocityX, rigidBody.velocity.y);
                }
            }
            else
            {
                playerState.State = PlayerStates.Moving;
                //CmdChangeState(PlayerStates.Moving);
            }
        }
        else if (!IsGrounded() && !nextWillBeSpecial)
        {
            if (playerState.State == PlayerStates.Jumping || playerState.State == PlayerStates.Falling)
            {
                rigidBody.velocity = new Vector2(Mathf.Clamp(rigidBody.velocity.x + airAcceleration * direction, -maxAirSpeed, maxAirSpeed), Mathf.Clamp(this.rigidBody.velocity.y, -100, 100));
            }
        }
    }

    public override void OnFastSwipe(Vector2 dir, float force)
    {
        if (IsGrounded() && !hasToJump && !nextWillBeSpecial && playerState.State != PlayerStates.Jumping && playerState.State != PlayerStates.Die && playerState.State != PlayerStates.Falling && (!float.IsNaN(dir.x) && !float.IsNaN(dir.y)))
        {
            //this.transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x) * (dir.x < 0 ? 1 : -1), transform.localScale.y);

            float futureJumpPower = force * jumpMultyplier;
            futureJumpPower = Mathf.Clamp(futureJumpPower, -maxJumpForce, maxJumpForce);
            futureJump = dir * futureJumpPower;

            Debug.LogError("fututreJump - " + futureJump);
            playerState.State = PlayerStates.Jumping;
            hasToJump = true;
        }
    }

    public override void OnSwipeFinish(Vector2 dir)
    {
        if (IsGrounded() && !hasToJump && playerState.State != PlayerStates.Die && !nextWillBeSpecial)
        {
            playerState.State = PlayerStates.Idle;
            //CmdChangeState(PlayerStates.Idle);
        }
        //Special Cat
        //if (nextWillBeSpecial)
        //{
        //    if(playerType == PlayerType.Cat && IsGrounded())
        //    {
        //        playerState.State = PlayerStates.Special;
        //        specialDir = dir;
        //        nextWillBeSpecial = false;
        //        transform.eulerAngles = new Vector3(transform.eulerAngles.x, (dir.x > 0 ? 180 : 0), transform.eulerAngles.z);
        //        lastPos = this.transform.position + new Vector3(0.03f * (dir.x > 0 ? -1 : 1), 0, 0);
        //    }
        //}
    }
    #endregion
}
