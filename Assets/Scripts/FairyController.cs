using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FairyController : MonoBehaviour
{
	private Animator animator;

	void Awake()
	{
		animator = gameObject.GetComponent<Animator>();
	}

	void OnEnable()
	{
		animator.SetTrigger("Jump");
	}
}
