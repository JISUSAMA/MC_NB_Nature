using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineRunner : MonoBehaviour
{
    public static CoroutineRunner instance { get; private set; }

    private Dictionary<string, Coroutine> coroutines = new Dictionary<string, Coroutine>();

    public bool corutineRunning = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Run(string key, IEnumerator coroutine)
    {
        Stop(key); // ���� �� ���߱�
        coroutines[key] = StartCoroutine(InternalRun(key, coroutine));
    }

    private IEnumerator InternalRun(string key, IEnumerator coroutine)
    {
        yield return StartCoroutine(coroutine);

        coroutines.Remove(key);
    }

    // Ư�� Ű�� �ڷ�ƾ ����
    public void Stop(string key)
    {
        if (coroutines.ContainsKey(key))
        {
            StopCoroutine(coroutines[key]);
            coroutines.Remove(key);
   
            if (key == "narration")
            {
                NarrationManager.isTyping = false; // ������ ����
            }
        }
    }


    // ��� �ڷ�ƾ ����
    public void StopAll()
    {
        foreach (var coroutine in coroutines.Values)
        {
            StopCoroutine(coroutine);
        }

        coroutines.Clear();
    }

    // Wait�� Timeout ���� �߰�
    public IEnumerator RunAndWait(string key, IEnumerator coroutine, float timeout = 10f)
    {
        Run(key, coroutine);
        corutineRunning = true;

        float elapsed = 0f;
        while (coroutines.ContainsKey(key))
        {
            yield return null;

            elapsed += Time.deltaTime;
            if (elapsed > timeout)
            {
                Debug.LogWarning($"CoroutineRunner: Coroutine with key '{key}' timed out after {timeout} seconds!");
                NarrationManager.isTyping = false;
                Stop(key); // ���� ����
                break;
            }
        }

        corutineRunning = false;
    }
}
