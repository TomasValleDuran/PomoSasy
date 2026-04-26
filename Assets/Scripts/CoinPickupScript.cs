using UnityEngine;
using System.Collections;

public class CoinPickupScript : MonoBehaviour
{
    [SerializeField] private PomoSasyConstants.CoinType coinType;

    [Header("Timings")]
    [SerializeField] private float scatterDuration = 0.35f;
    [SerializeField] private float delayBeforeMagnet = 0.25f;

    [Header("Scatter")]
    [SerializeField] private float scatterRange = 0.75f;

    [Header("Magnet")]
    [SerializeField] private float magnetSpeed = 6f;
    [SerializeField] private float magnetAcceleration = 20f;

    private Transform player;
    private bool isMagnetActive = false;
    private float currentMagnetSpeed;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        StartCoroutine(BehaviorRoutine());
    }

    private IEnumerator BehaviorRoutine()
    {
        // 1. Scatter
        yield return StartCoroutine(Scatter());

        // 2. Delay (coins sit briefly)
        yield return new WaitForSeconds(delayBeforeMagnet);

        // 3. Activate magnet
        isMagnetActive = true;
        currentMagnetSpeed = magnetSpeed;
    }

    private IEnumerator Scatter()
    {
        Vector3 start = transform.position;

        Vector3 target = start + new Vector3(
            Random.Range(-scatterRange, scatterRange),
            Random.Range(-scatterRange, scatterRange),
            0f
        );

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / scatterDuration;
            t = Mathf.Clamp01(t);

            float easedT = 1f - Mathf.Pow(1f - t, 4f);

            transform.position = Vector3.Lerp(start, target, easedT);
            yield return null;
        }

        transform.position = target;
    }

    private void Update()
    {
        if (!isMagnetActive || player == null) return;

        // Accelerating magnet (feels great)
        currentMagnetSpeed += magnetAcceleration * Time.deltaTime;

        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            currentMagnetSpeed * Time.deltaTime
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isMagnetActive) return;

        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        int value = Utils.GetCoinValueFromType(coinType);

        // PlayerWallet.Instance.Add(value);

        Destroy(gameObject);
    }
}