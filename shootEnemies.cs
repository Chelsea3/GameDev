﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;

public class shootEnemies : MonoBehaviour {

	public List<GameObject> enemiesInRanges;
	private float lastShotTime;
	private TowerData towerData;
	// Use this for initialization
	void Start () {
		enemiesInRanges = new List<GameObject> ();
	}

	void OnEnemyDestroy (GameObject enemy) {
		enemiesInRanges.Remove (enemy);
		lastShotTime = Time.time;
		towerData = gameObject.GetComponentInChildren<TowerData>();
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.tag.Equals ("Enemy")) {
			enemiesInRanges.Add (other.gameObject);
			EnemyDestructionDelegate del = other.gameObject.GetComponent<EnemyDestructionDelegate> ();
			del.enemyDelegate += OnEnemyDestroy;
		}
	}

	void OnTriggerExit2D(Collider2D other) {
		if (other.gameObject.tag.Equals("Enemy")) {
			enemiesInRanges.Remove(other.gameObject);
			EnemyDestructionDelegate del = other.gameObject.GetComponent<EnemyDestructionDelegate>();
			del.enemyDelegate -= OnEnemyDestroy;
		} 
	}

	void Shoot(Collider2D target)
	{
		GameObject bulletPrefab = towerData.CurrentLevel.bullet;
		// 1
		Vector3 startPosition = gameObject.transform.position;
		Vector3 targetPosition = target.transform.position;
		startPosition.z = bulletPrefab.transform.position.z;
		targetPosition.z = bulletPrefab.transform.position.z;

		// 2
		GameObject newBullet = (GameObject)Instantiate(bulletPrefab);
		newBullet.transform.position = startPosition;
		BulletBehavior bulletComp = newBullet.GetComponent<BulletBehavior>();
		bulletComp.target = target.gameObject;
		bulletComp.startPosition = startPosition;
		bulletComp.targetPosition = targetPosition;

		// 3
		Animator animator = towerData.CurrentLevel.visualization.GetComponent<Animator>();
		animator.SetTrigger("fireShot");
		AudioSource audioSource = gameObject.GetComponent<AudioSource>();
		audioSource.PlayOneShot(audioSource.clip);
	}
	// Update is called once per frame
	void Update () {
		GameObject target = null;
		// 1
		float minimalEnemyDistance = float.MaxValue;
		foreach (GameObject enemy in enemiesInRanges)
		{
			float distanceToGoal = enemy.GetComponent<MoveEnemy>().distanceToGoal();
			if (distanceToGoal < minimalEnemyDistance) 
			{
				target = enemy;
				minimalEnemyDistance = distanceToGoal;
			}
		}
		// 2
		if (target != null) 
		{
			if (Time.time - lastShotTime > towerData.CurrentLevel.fireRate){
				Shoot(target.GetComponent<Collider2D>());
				lastShotTime = Time.time;
			}
			// 3
			Vector3 direction = gameObject.transform.position - target.transform.position;
			gameObject.transform.rotation = Quaternion.AngleAxis(
				Mathf.Atan2(direction.y, direction.x) * 180 / Mathf.PI,
				new Vector3(0, 0, 1));
		}
	}
}
