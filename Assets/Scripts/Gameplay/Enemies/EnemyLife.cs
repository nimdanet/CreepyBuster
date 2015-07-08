﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class EnemyLife : MonoBehaviour 
{
	#region actions
	public static event Action<GameObject> OnDied;
	#endregion

	public float life;
	public int score;
	public bool countAsStreak = true;

	public static float deathTime = 1f;

	[HideInInspector]
	public bool inLight = false;

	private GameObject lightning;
	private SpriteRenderer spriteRenderer;
	private List<SpriteRenderer> brilhos;
	public Color basicColor = Color.yellow;
	public Color damageColor = Color.red;

	public List<EventDelegate> onDeadEvent;

	#region get / set
	public bool IsOffscreen
	{
		get
		{
			Bounds bounds = spriteRenderer.bounds;

			Vector3 max = Camera.main.WorldToViewportPoint(bounds.max);
			Vector3 min = Camera.main.WorldToViewportPoint(bounds.min);

			return max.x < 0 || max.y < 0 || min.x > 1 || min.y > 1;
		}
	}

	public virtual bool IsDamagable
	{
		get { return !IsOffscreen; }
	}

	public bool IsDead
	{
		get
		{
			return life <= 0;
		}
	}
	#endregion

	void OnEnable()
	{
		LevelDesign.OnPlayerLevelUp += UpdateColor;
		GameController.OnLoseStacks += UpdateColor;
	}

	void OnDisable()
	{
		LevelDesign.OnPlayerLevelUp -= UpdateColor;
		GameController.OnLoseStacks -= UpdateColor;
	}

	// Use this for initialization
	void Start () 
	{
		spriteRenderer = transform.FindChild ("Sprite").GetComponent<SpriteRenderer> ();


		brilhos = new List<SpriteRenderer> ();

		//get all spriterenderer children
		foreach (Transform t in spriteRenderer.transform)
			brilhos.Add (t.GetComponent<SpriteRenderer>());

		foreach(SpriteRenderer brilho in brilhos)
			brilho.color = basicColor;

		lightning = transform.FindChild ("Lightning Emitter").gameObject;
		lightning.SetActive (false);
		lightning.GetComponent<LightningBolt> ().target = AttackTargets.Instance.transform;
		lightning.GetComponent<ParticleRenderer>().material.SetColor ("_TintColor", LevelDesign.CurrentColor);

		life += LevelDesign.EnemiesBonusLife;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(inLight)
		{
			life -= AttackTargets.Instance.damage * Time.deltaTime;

			if(life <= 0)
				Dead();
		}
	}

	public void OnLightEnter()
	{
		inLight = true;

		foreach(SpriteRenderer brilho in brilhos)
			brilho.color = damageColor;

		lightning.SetActive (true);
	}

	public void OnLightExit()
	{
		//Debug.Log ("OnLightExit");
		inLight = false;

		foreach(SpriteRenderer brilho in brilhos)
			brilho.color = basicColor;

		lightning.SetActive (false);
	}

	private void UpdateColor()
	{
		//lightning.GetComponent<LightningBolt> ().startLight.color = LevelDesign.CurrentColor;
		lightning.GetComponent<ParticleRenderer>().material.SetColor ("_TintColor", LevelDesign.CurrentColor);
	}

	public void Dead()
	{
		if (OnDied != null)
			OnDied (gameObject);

		foreach(SpriteRenderer brilho in brilhos)
			brilho.color = damageColor;

		foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
			col.enabled = false;

		EventDelegate.Execute (onDeadEvent);

		StartCoroutine (FadeAway (deathTime));
		//Destroy (gameObject);
	}

	private IEnumerator FadeAway (float deathTime)
	{
		float alpha = brilhos[0].color.a;
		Animator animator = spriteRenderer.GetComponent<Animator> ();
		float maxAnimatorSpeed = animator.speed;

		while(alpha > 0)
		{
			foreach(SpriteRenderer brilho in brilhos)
			{
				Color c = brilho.color;
				c.a -= Time.deltaTime * deathTime;
				brilho.color = c;

				if(animator.recorderMode != AnimatorRecorderMode.Offline)
					animator.speed -= Time.deltaTime * deathTime * maxAnimatorSpeed;
			}

			alpha = brilhos[0].color.a;

			yield return null;
		}

		Destroy (gameObject);
	}
}
