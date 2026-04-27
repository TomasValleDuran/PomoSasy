using System;
using UnityEngine;
using System.Collections;
using Controllers;
using Random = UnityEngine.Random;

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
    [SerializeField] private float maxDistance = 5f;

    private Transform player;
    private bool isMagnetActive = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        StartCoroutine(BehaviorRoutine());
    }

    private IEnumerator BehaviorRoutine()
    {
        // Scatter
        yield return StartCoroutine(Scatter());

        // Delay
        yield return new WaitForSeconds(delayBeforeMagnet);

        // Activate magnet
        isMagnetActive = true;
    }

    private IEnumerator Scatter()
    {
        var start = transform.position;

        var target = start + new Vector3(
            Random.Range(-scatterRange, scatterRange),
            Random.Range(-scatterRange, scatterRange),
            0f
        );

        var t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / scatterDuration;
            t = Mathf.Clamp01(t);

            var easedT = 1f - Mathf.Pow(1f - t, 4f);

            transform.position = Vector3.Lerp(start, target, easedT);
            yield return null;
        }

        transform.position = target;
    }

    private void Update()
    {
        if (!isMagnetActive || !player) return;

        var distance = Vector3.Distance(transform.position, player.position);

        if (distance > maxDistance) return;

        var t = 1f - Mathf.Clamp01(distance / maxDistance);
        t *= t;

        var speed = magnetSpeed * t;

        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            speed * Time.deltaTime
        );
        
        if (transform.position == player.position) Collect();
    }

    private void Collect()
    {
        var value = Utils.GetCoinValueFromType(coinType);
        WalletManagerScript.Instance.Add(value);

        Destroy(gameObject); // TODO: Replace with pooling
    }
}