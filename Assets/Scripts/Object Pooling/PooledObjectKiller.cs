using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledObjectKiller : MonoBehaviour {

	// Variables
	public enum killObjectEnum {
		OnSleep,
		OnTimeout
	}
	public killObjectEnum killObjectOn;
	public float timeoutAfterSleep = 2;
	public float killTimeout = 10;
	public enum killSequenceEnum {
		ShrinkAndDescend,
		Shrink,
		Descend,
		None
	};
	public GameObject[] disableChildrenOnKill;
	public bool disableColliderOnKill = true;
	public bool disableRigidbodyOnKill = true;
	public killSequenceEnum killSequenceType;
	public float killSequenceDuration = 1;
	public AnimationCurve killSequenceRate;
	public float descendDistance = 0.5f;
	public float maxLife = 20;

	// Private variables
	private Rigidbody _rb;
	private Collider _col;
	private bool _waitToDie = false;
	private bool _isDying = false;
	private Vector3 _initPos;
	private Vector3 _initScale;
	private float _killSequenceRateEval;
	private float _killSequenceElapsed;
	private float _killSequenceStart;

	// Use this for initialization
	void Awake () {
		_rb = GetComponent<Rigidbody> ();
		_col = GetComponent<Collider> ();
		_initScale = transform.localScale;
		ResetAll ();
	}

	void OnEnable() {
		ResetAll ();

		if (maxLife > 0) {
			StartCoroutine (KillObjectTimeout (maxLife));
		}
		if (killObjectOn == killObjectEnum.OnTimeout) {
			StartCoroutine (KillObjectTimeout (killTimeout));
		}

	}

	void OnDisable() {
//		Debug.Log ("Stop all coroutines");
		StopAllCoroutines ();
	}

	// Update is called once per frame
	void Update () {
		if (!_isDying) {
//			Debug.Log ("Still alive");
			CheckForSleepingObject ();
		} else {
			if (killSequenceType != killSequenceEnum.None) {
				DoKillSequence ();
			} else {
//				Debug.Log ("Kill object now");
				KillObject ();
			}
		}
	}

	private void CheckForSleepingObject() {
		if (killObjectOn == killObjectEnum.OnSleep
			&& _rb.IsSleeping ()) {

			if (timeoutAfterSleep <= 0) {
//				Debug.Log ("Kill sleeping object");
				PreObjectKill ();
			} else if (!_waitToDie) {
				_waitToDie = true;
				StartCoroutine (KillObjectTimeout (timeoutAfterSleep));
			}
		}
	}

	private void ResetAll() {
		_waitToDie = false;
		_isDying = false;

		if (disableColliderOnKill) {
			_col.enabled = true;
		}

		if (disableRigidbodyOnKill) {
			_rb.isKinematic = false;
		}

		ResetScale ();
		ResetChildren ();
	}

	private void ResetScale() {
		transform.localScale = _initScale;
	}

	private void ResetChildren() {
		foreach (GameObject child in disableChildrenOnKill) {
			child.SetActive (true);
		}
	}

	private void DoKillSequence() {
//		Debug.Log ("Kill sequence");
		_killSequenceElapsed = Time.time - _killSequenceStart;
		_killSequenceRateEval = killSequenceRate.Evaluate(_killSequenceElapsed / killSequenceDuration);

		switch (killSequenceType) {
		case killSequenceEnum.Shrink:
			AdjustScale ();				
			break;
		case killSequenceEnum.Descend:
			Descend ();
			break;
		case killSequenceEnum.ShrinkAndDescend:
			AdjustScale ();
			Descend ();
			break;
		case killSequenceEnum.None:
			break;
		}

		if (_killSequenceElapsed >= killSequenceDuration) {
			KillObject ();
		}
	}

	private void AdjustScale() {
		
		transform.localScale = _initScale * _killSequenceRateEval;
	}

	private void Descend() {
//		Debug.Log ("Descend");
		transform.position = Vector3.Lerp(_initPos,
			new Vector3(
				_initPos.x,
				_initPos.y - descendDistance,
				_initPos.z
			),
			_killSequenceElapsed / killSequenceDuration
		);
	}

	private void DisableCollision() {
//		Debug.Log ("Disable collision");

		if (disableColliderOnKill) {
			_col.enabled = false;
		}

		if (disableRigidbodyOnKill) {
			_rb.isKinematic = true;
		}
	}

	private void PreObjectKill() {
		foreach (GameObject child in disableChildrenOnKill) {
			child.SetActive (false);
		}
		_initPos = transform.position;
		_killSequenceStart = Time.time;
		_isDying = true;
		DisableCollision ();
	}

	private IEnumerator KillObjectTimeout(float _killTime) {
//		Debug.Log ("Kill object after " + _killTime + " seconds.");
		yield return new WaitForSeconds (_killTime);
//		Debug.Log ("Kill object timeout");
		PreObjectKill();
	}

	private void KillObject() {
//		Debug.Log ("Kill object");
		gameObject.SetActive (false);
	}
		
}
