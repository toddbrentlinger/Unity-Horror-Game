using UnityEngine;
using System.Collections;

public class Scare06Controller : MonoBehaviour {

    [SerializeField] private GameObject m_ball;
    private float m_throwForce = 0.8f;

    private float m_xOffset = 0.5f;
    private float m_yMinOffset = -0.3f;
    private float m_yMaxOffset = 0.6f;
    private float m_xTopAngle = 15.0f;
    private float m_xBottomAngle = 40.0f;
    private float m_yAngle = 8.0f;

    [SerializeField] private AudioClip m_childLaughClip;
    private float m_childLaughDistance = 1.5f;

    private enum ballScareState { Default, RollBallLook, RollBallNoLook, FloatBall, KickBall, ThrowBall };
    private ballScareState currentScareState;
    private float[] scareStateStartTime = { 0.0f, 20.0f, 120.0f, 180.0f, 240.0f, 300.0f };

    private Transform m_player;
    private Camera m_mainCamera;
    private Collider m_ballCollider;
    private Rigidbody m_ballRB;
    private Renderer m_ballRenderer;
    private UnityEngine.AI.NavMeshAgent m_ballNavMeshAgent;
    private float m_ballMoveDistance = 2.0f;
    private float m_ballRollForce = 1.0f;

    private float m_scareTimer;
    private float timeToNextAction;

    private bool m_throwBall;
    private bool m_enterTrigger;
    private bool m_playScareEvent;
    private bool m_playScare;

	// Use this for initialization
	void Start ()
    {
        currentScareState = ballScareState.Default;

        m_player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        m_mainCamera = Camera.main;
        m_ballCollider = m_ball.GetComponent<Collider>();
        m_ballRB = m_ball.GetComponent<Rigidbody>();
        m_ballRenderer = m_ball.GetComponent<Renderer>();
        m_ballNavMeshAgent = m_ball.GetComponent<UnityEngine.AI.NavMeshAgent>();
        m_ballNavMeshAgent.enabled = false;

        timeToNextAction = 0.0f;

        m_throwBall = true;
        m_enterTrigger = false;
        m_playScareEvent = false;
	}
	
	// Update is called once per frame
	void Update ()
    {
        // Debug
        //Debug.Log(currentScareState + " " + m_scareTimer + " " + timeToNextAction);

        if (Input.GetKeyDown(KeyCode.Keypad1))
            ThrowBall();

        // Check if scare is playing
        if (m_playScareEvent)
        {
            // Increase scareTimer by deltaTiime
            m_scareTimer += Time.deltaTime;

            // Set currentScareState
            SetScareState();

            switch (currentScareState)
            {
                case ballScareState.Default:
                    break;

                case ballScareState.RollBallLook:
                    RollBallLook();
                    break;

                case ballScareState.RollBallNoLook:
                    break;

                case ballScareState.FloatBall:
                    break;

                case ballScareState.ThrowBall:
                    break;
            }

            return;
        }

        /*
        // if scare is not playing, check if player is in trigger
        if (!m_enterTrigger)
           return;
        */

        // if player is in trigger, check if E key is pressed
        if (!Input.GetKeyDown(KeyCode.E))
            return;

        // if E key is pressed, check if player is looking at ball
        RaycastHit hit;
        Ray ray = m_mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        float maxRayDistance = 3.0f;
        if (m_ballCollider.Raycast(ray, out hit, maxRayDistance))
        {
            m_scareTimer = 0.0f;
            m_playScareEvent = true;
        }
        else
            m_playScareEvent = false;
	}

    private void SetScareState()
    {
        if (m_scareTimer > scareStateStartTime[5])
        {
            // Throw ball from behind player
            currentScareState = ballScareState.ThrowBall;
        }
        else if (m_scareTimer > scareStateStartTime[4])
        {
            // Kick ball if looking
            currentScareState = ballScareState.KickBall;
        }
        else if (m_scareTimer > scareStateStartTime[3])
        {
            // Float ball when not looking
            currentScareState = ballScareState.FloatBall;
        }
        else if (m_scareTimer > scareStateStartTime[2])
        {
            // Roll ball when not looking
            currentScareState = ballScareState.RollBallNoLook;
        }
        else if (m_scareTimer > scareStateStartTime[1])
        {
            // Roll ball if looking
            currentScareState = ballScareState.RollBallLook;
        }
        else
            // Default
            currentScareState = ballScareState.Default;
    }
    /*
    IEnumerator PlayScare()
    {
        m_scareTimer = 0.0f;

        float timeToNextAction = 0.0f;
        float minTime = 10.0f;
        float maxTime = 30.0f;
        while (m_scareTimer < scareStateStartTime[1])
        {
            if (m_scareTimer > timeToNextAction)
            {
                timeToNextAction = m_scareTimer + Random.Range(minTime, maxTime);
            }

            yield return null;
        }
    }
    */
    void CheckThrowBall()
    {

    }

    void RollBallLook()
    {
        float minTime = 10.0f;
        float maxTime = 30.0f;

        if (m_scareTimer > timeToNextAction)
        {
            if (m_ballRenderer.isVisible)
            {
                // Roll Ball

                // Get target vector from ball position to player position
                Vector3 target = m_player.transform.TransformPoint(Vector3.forward * m_ballMoveDistance) -
                    m_ball.transform.position;

                // Add torque in direction of target
                target.y = 0;
                target = Quaternion.Euler(0, 90, 0) * target;
                m_ballRB.velocity = Vector3.zero;
                m_ballRB.AddTorque(target.normalized * m_ballRollForce);
                

                // Add force in direction of target vector
                // m_ballRB.AddForce(target.normalized * m_ballRollForce, ForceMode.Impulse);

                // Find new timeToNextAction
                timeToNextAction = m_scareTimer + Random.Range(minTime, maxTime);
            }
        }

        if (m_scareTimer > scareStateStartTime[2])
        {
            currentScareState = ballScareState.RollBallNoLook;
            timeToNextAction = 0.0f;
        }
    }

    void MoveBall()
    {
        // Enable ball NavMeshAgent
        m_ballNavMeshAgent.enabled = true;

        // Set destination of NavMeshAgent to front of player
        m_ballNavMeshAgent.SetDestination(m_player.transform.TransformPoint(Vector3.forward * m_ballMoveDistance));
    }

    void ThrowBall()
    {
        // Disable renderer of ball
        // ball.GetComponent<Renderer>().enabled = false;

        // Make rigidboy of ball kinematic
        m_ballRB.isKinematic = true;

        // Randomly choose one of four possible spawn positions
        int spawnPositionIndex = Random.Range(1, 5);

        // Set up position and direction of ball
        float xOffsetSpawn = 0;
        float yOffsetSpawn = 0;
        float xAngleSpawn = 0;
        float yAngleSpawn = 0;
        switch (spawnPositionIndex)
        {
            case 1:
                xOffsetSpawn = m_xOffset;
                yOffsetSpawn = m_yMaxOffset;
                xAngleSpawn = -m_xTopAngle;
                yAngleSpawn = -m_yAngle;
                break;
            case 2:
                xOffsetSpawn = m_xOffset;
                yOffsetSpawn = m_yMinOffset;
                xAngleSpawn = -m_xBottomAngle;
                yAngleSpawn = -m_yAngle;
                break;
            case 3:
                xOffsetSpawn = -m_xOffset;
                yOffsetSpawn = m_yMinOffset;
                xAngleSpawn = -m_xBottomAngle;
                yAngleSpawn = m_yAngle;
                break;
            case 4:
                xOffsetSpawn = -m_xOffset;
                yOffsetSpawn = m_yMaxOffset;
                xAngleSpawn = -m_xTopAngle;
                yAngleSpawn = m_yAngle;
                break;
        }

        // Move ball to spawn position
        m_ball.transform.position = m_player.TransformPoint(new Vector3(xOffsetSpawn, yOffsetSpawn, 0));

        // Set ball direction
        m_ball.transform.rotation = m_player.rotation * Quaternion.Euler(xAngleSpawn, yAngleSpawn, 0f);

        // Debug.Log("spawnPositionIndex: " + spawnPositionIndex);

        // Enable renderer of ball
        // ball.GetComponent<Renderer>().enabled = true;

        // Make rigidbody not kinematic
        m_ballRB.isKinematic = false;

        // Play childLaughClip at a position directly behind player
        AudioSource.PlayClipAtPoint(m_childLaughClip, m_player.TransformPoint(Vector3.back * m_childLaughDistance));

        // Add force to throw ball in forward direction
        m_ballRB.velocity = Vector3.zero;
        m_ballRB.AddForce(m_ball.transform.TransformDirection(Vector3.forward) * m_throwForce, 
            ForceMode.Impulse);
    }

    void OnTriggerEnter()
    {
        m_enterTrigger = true;
    }

    void OnTriggerExit()
    {
        m_enterTrigger = false;
    }
}
