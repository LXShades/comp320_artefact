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

    private bool isCharging = false;
    private bool hasCharged = false;

    void Awake()
    {
        animation = GetComponent<Animation>();
    }

    void Update()
    {
        if (isCharging && !animation.isPlaying)
        {
            isCharging = false;
            hasCharged = true;
        }
    }

    /// <summary>
    /// Begins charging a shot
    /// </summary>
    public void ChargeUp()
    {
        if (Time.time - lastFiredTime > fireCooldown)
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
            // Record firing time for cooldowns
            lastFiredTime = Time.time;
            hasCharged = false;

            // Play the shooting animation
            animation.clip = fire;
            animation.Play();

            // Spawn and eject the projectile towards the target
            RockProjectile rock = Instantiate(projectile, transform.position, Quaternion.identity);

            rock.Shoot(player.GetTargetPosition());
        }
    }
}
