using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
/// <summary>
/// He floats, he pops, he never attacks, he pollutes the environment
/// </summary>
public class BalloonEnemy : MonoBehaviour
{
    // How far up/down the balloon will wobble
    public float wobbleAmount = 30.0f;
    // How rapidly the balloon will wobble
    public float wobbleRate = 0.5f;
    // When the balloon will self-destruct to remove itself from the scene
    public float selfDestructTime = 8.0f;
    // Direction of the balloon
    public Vector3 velocity;
    // Particles to spawn when the balloon pops
    public GameObject popParticles;
    // How the object scales over time
    public AnimationCurve scaleCurve = AnimationCurve.Linear(0, 0, 1, 1);

    // Spawn point of the balloon
    private Vector3 initialPosition;
    private Vector3 initialScale;
    // Time.time at spawn
    private float spawnTime;

    // Components
    private Rigidbody rb;

    // Axis to wobble around
    private Vector3 wobbleAxis;

    void Awake()
    {
        // Store initial spawn ifno
        initialPosition = transform.position;
        initialScale = transform.localScale;
        spawnTime = Time.time;

        wobbleAxis = (transform.position - GameManager.singleton.player.transform.position).normalized;

        rb = GetComponent<Rigidbody>();

        // Don't pop in
        transform.localScale = Vector3.zero;

        // Data collection schtuff
        GameManager.singleton.numTotalBalloons++;
    }

    // Update is called once per frame
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
