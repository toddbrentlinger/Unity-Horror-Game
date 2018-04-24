using UnityEngine;
using System.Collections;

public class Scare08Controller : MonoBehaviour {

    // private float m_speed = 0.5f;

    private Camera m_mainCam;
    private Transform m_playerTransform;
    private Renderer m_ghostRenderer;
    private UnityEngine.AI.NavMeshAgent m_ghostAgent;
    private Rigidbody m_ghostRB = null;
    private float m_maxRayDistance = 2.0f;
    private float m_minGhostDistace = 1.0f;

    private bool playScare;
    private bool isMoving;

	// Use this for initialization
	void Start ()
    {
        m_mainCam = Camera.main;
        m_playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        m_ghostRenderer = gameObject.GetComponent<Renderer>();
        m_ghostAgent = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (gameObject.GetComponent<Rigidbody>())
            m_ghostRB = gameObject.GetComponent<Rigidbody>();

        playScare = false;
        isMoving = false;

    // Set NavMeshAgent parameters
    // m_ghostAgent.speed = m_speed;
}

	// Update is called once per frame
	void Update ()
    {
        if (!playScare)
            return;

        // if ghost is not visible
        if (!m_ghostRenderer.isVisible)
        {
            if (!isMoving)
            {
                if (m_ghostRB)
                    m_ghostRB.isKinematic = true;

                m_ghostAgent.enabled = true;
                isMoving = true;
            }

            // m_ghostAgent.SetDestination(m_playerTransform.position);
            m_ghostAgent.SetDestination(m_playerTransform.TransformPoint(Vector3.back * m_minGhostDistace));

            // Debug.Log("Ghost Moving");
        }
        // else ghost is visible
        else if (m_ghostRenderer.isVisible)
        {
            if (isMoving)
            {
                m_ghostAgent.Stop();
                m_ghostAgent.ResetPath();
                m_ghostAgent.enabled = false;

                if (m_ghostRB)
                    m_ghostRB.isKinematic = false;

                isMoving = false;
            }

            // Debug.Log("Ghost NOT Moving");
        }
	}

    void OnTriggerStay(Collider other)
    {
        if (!playScare && other.CompareTag("Player"))
        {
            Ray ray = m_mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            Collider ghostCollider = gameObject.GetComponent<Collider>();

            if (ghostCollider.Raycast(ray, out hit, m_maxRayDistance))
            {
                if (m_ghostRB)
                {
                    m_ghostAgent.enabled = false;
                    m_ghostRB.isKinematic = false;
                }

                playScare = true;
            }
        }
    }
}
