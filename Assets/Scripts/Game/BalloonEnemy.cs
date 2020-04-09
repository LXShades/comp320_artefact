using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
/// <summary>
/// He floats, he pops, he never attacks, he pollutes the environment
/// </summary>
public class BalloonEnemy : MonoBehaviour
{
    [Tooltip("How far up/down the balloon will wobble")]
    public float wobbleAmount = 30.0f;
    [Tooltip("How rapidly the balloon will wobble")]
    public float wobbleRate = 0.5f;
    [Tooltip("When the balloon will self-destruct to remove itself from the scene")]
    public float selfDestructTime = 8.0f;
    [Tooltip("Direction of the balloon")]
    public Vector3 velocity;
    [Tooltip("Particles to spawn when the balloon pops")]
    public GameObject popParticles;
    [Tooltip("How the object scales over time")]
    public AnimationCurve scaleCurve = AnimationCurve.Linear(0, 0, 1, 1);

    /// <summary>
    /// Position that we initially spawned at
    /// </summary>
    private Vector3 initialPosition;
    /// <summary>
    /// Scale that we initially spawned at
    /// </summary>
    private Vector3 initialScale;

    /// <summary>
    /// Game Time.time at spawn
    /// </summary>
    private float spawnTime;

    /// <summary>
    /// The attached Rigidbody component
    /// </summary>
    private Rigidbody rb;

    /// <summary>
    /// Axis to wobble around on
    /// </summary>
    private Vector3 wobbleAxis;

    /// <summary>
    /// Called early on by Unity upon creation. Initialises variables and stores initial states
    /// </summary>
    void Awake()
    {
        // Store initial spawn info
        initialPosition = transform.position;
        initialScale = transform.localScale;
        spawnTime = Time.time;

        wobbleAxis = (transform.position - GameManager.singleton.player.transform.position).normalized;

        // Get components
        rb = GetComponent<Rigidbody>();

        // Don't pop in
        transform.localScale = Vector3.zero;

        // Data collection schtuff
        GameManager.singleton.numTotalBalloons++;
    }

    /// <summary>
    /// Called by Unity upon a frame. Moves the balloon along its movement direction.
    /// </summary>
    void Update()
    {
        float aliveTime = Time.time - spawnTime;

        // Move the balloon along the path
        transform.position = initialPosition + velocity * aliveTime;
        // and bob
        transform.localRotation = Quaternion.AngleAxis(Mathf.Sin(wobbleRate * Mathf.PI * aliveTime) * wobbleAmount, wobbleAxis);

        // Scale up (so we can legally spawn objects in plain sight)
        transform.localScale = initialScale * scaleCurve.Evaluate(aliveTime);

        // Pass the momentum onto the rigidbody so collisions can be detected
        rb.velocity = velocity;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;

        // Destroy the balloon after a period
        if (aliveTime > selfDestructTime)
        {
            Pop();
        }
    }

    /// <summary>
    /// Called by Unity during physics updates. Stops the balloon from spinning if it hits other balloons
    /// </summary>
    void FixedUpdate()
    {
        rb.angularVelocity = Vector3.zero;
    }

    /// <summary>
    /// Pops the balloon on contact with anything
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponent<RockProjectile>() != null)
        {
            // The player hit!
            // Add to their score?
            GameManager.singleton.numPoppedBalloons++;
        }

        if (collision.collider.GetComponent<BalloonEnemy>() == null)
        {
            // Die
            Pop();
        }
    }

    /// <summary>
    /// Causes the balloon to burst and delete itself
    /// </summary>
    public void Pop()
    {
        // Record the data (but not if self-destructed; that would give an unuseful value of 5 every time)
        if (Time.time - spawnTime < selfDestructTime)
        {
            GameManager.singleton.balloonPopLifetimes.Add(Time.time - spawnTime);
        }

        Instantiate(popParticles, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
