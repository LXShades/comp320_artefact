using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The player's weapon object
/// </summary>
public class Slingshot : MonoBehaviour
{
    [Header("Hierarchy")]
    // The player holding the slingshot
    public Player player;
    // Where the projectiles should spawn from
    public Transform projectileSpawnPoint;

    [Header("Shooting")]
    // Cooldown time between shots, in seconds
    public float fireCooldown;
    // Prefab of the projectile to fire
    public RockProjectile projectile;

    [Header("Animations")]
    // Clip for charge-up animation
    public AnimationClip chargeUp;
    public AnimationClip fire;

    // Components
    private new Animation animation;

    private float lastFiredTime;

    // Whether the slingshot has begun charging
    private bool isCharging = false;
    private bool hasCharged = false;
    // Causes the slingshot to fire on the next frame that it's not charged
    private bool doFireWhenCharged = false;

    // Data recording
    [HideInInspector] public int numShotsFired = 0;

    void Awake()
    {
        animation = GetComponent<Animation>();
    }

    void Update()
    {
        // show default pose when not using it
        if (!animation.isPlaying && !isCharging && !hasCharged)
        {
            animation.clip = chargeUp;
            animation.clip.SampleAnimation(gameObject, 0);
        }

        // Determine whether charge sequence has finished
        if (isCharging && !animation.isPlaying)
        {
            isCharging = false;
            hasCharged = true;
        }

        // Fire automatically if the mouse was released while charging
        if (doFireWhenCharged && hasCharged)
        {
            ReleaseProjectile();
        }
    }

    /// <summary>
    /// Begins charging a shot
    /// </summary>
    public void ChargeUp()
    {
        if (!isCharging && !hasCharged && Time.time - lastFiredTime > fireCooldown)
        {
            animation.clip = chargeUp;
            animation.Play();

            isCharging = true;
        }
    }

    /// <summary>
    /// Releases a charged shot
    /// </summary>
    public void Fire()
    {
        if (hasCharged)
        {
            // fire immediately
            ReleaseProjectile();
        }
        else if (isCharging)
        {
            // postpone until the animation is finished
            doFireWhenCharged = true;
        }
    }

    private void ReleaseProjectile()
    {
        // Record firing time for cooldowns
        lastFiredTime = Time.time;
        hasCharged = false;
        isCharging = false;
        doFireWhenCharged = false;

        // Play the shooting animation
        animation.clip = fire;
        animation.Play();

        // shots fired!
        numShotsFired++;
    }

    public void OnProjectileDetach()
    {
        // Spawn and eject the projectile towards the target
        RockProjectile rock = Instantiate(projectile, projectileSpawnPoint.position, Quaternion.identity);

        rock.Shoot(player.GetTargetPosition());
    }
}
