using DevLocker.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityMagicMethodsLogger : MonoBehaviour
{
	private void Awake()
	{
		LogMagic("Awake ");
	}

	void Start()
	{
		LogMagic("Start");
	}

	private void OnEnable()
	{
		LogMagic("OnEnable");
	}

	private void OnDisable()
	{
		LogMagic("OnDisable");
	}

	private void OnDestroy()
	{
		LogMagic("OnDestroy");
	}

	private void LogMagic(string message)
	{
		Debug.Log($"{Time.frameCount} {name} - {message}", this);
	}
}
