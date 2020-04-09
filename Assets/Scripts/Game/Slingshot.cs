using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The player's weapon
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
    [Tooltip("The charge-up animation")]
    public AnimationClip chargeUp;
    [Tooltip("The release/fire animation")]
    public AnimationClip fire;

    // Components
    [Tooltip("The Animation component playing active animations")]
    private new Animation animation;

    /// <summary>
    /// The last time the weapon was fired. Used for cooldowns
    /// </summary>
    private float lastFiredTime;

    /// <summary>
    /// Whether the slingshot has begun charging
    /// </summary>
    private bool isCharging = false;

    /// <summary>
    /// Whether the swingshot is charged and ready to fire
    /// </summary>
    private bool hasCharged = false;

    /// <summary>
    /// Causes the slingshot to fire as soon as it's ready (player released the mouse button early)
    /// </summary>
    private bool doFireWhenCharged = false;

    /// <summary>
    /// Number of shots fired in total. For data recording
    /// </summary>
    [HideInInspector] public int numShotsFired = 0;

    /// <summary>
    /// Called by Unity upon creation. Initialises components.
    /// </summary>
    void Awake()
    {
        animation = GetComponent<Animation>();
    }

    /// <summary>
    /// Called by Unity upon a frame. Handles weapon game logic.
    /// </summary>
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

    /// <summary>
    /// Plays the projectile release animation and begins cooldown
    /// </summary>
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

    /// <summary>
    /// Called by the release animation when the rock should detach and release from the elastic
    /// </summary>
    public void OnProjectileDetach()
    {
        // Spawn and eject the projectile towards the target
        RockProjectile rock = Instantiate(projectile, projectileSpawnPoint.position, Quaternion.identity);

        rock.Shoot(player.GetTargetPosition());
    }
}
