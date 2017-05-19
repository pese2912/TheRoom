using UnityEngine;
using System.Collections;

public class Murderer_AI : MonoBehaviour {
	[SerializeField]
	Transform[] patrolPos;

	UnityEngine.AI.NavMeshAgent _naviAgnt;
	Animator _animator;
	public bool isAttacking = false;
	public Transform currentPatPos;
	public Transform tracePos;

    // Use this for initialization
    void Start () {
		_naviAgnt = GetComponent<UnityEngine.AI.NavMeshAgent> ();
		_animator = GetComponent<Animator> ();
	}
	IEnumerator Walk(){

		_naviAgnt.Stop ();
		_naviAgnt.Resume();
		_animator.SetTrigger ("Walk");

        int index = (int)Random.Range (0, patrolPos.Length);
		currentPatPos = patrolPos [index];
		_naviAgnt.SetDestination (patrolPos [index].position);

        yield return null;
	}
	IEnumerator Run(){
		_naviAgnt.Stop ();
		_naviAgnt.Resume();
		_animator.SetTrigger ("Run");


		_naviAgnt.SetDestination (tracePos.position);

        yield return null;
	}
	IEnumerator Attack1(){
		_naviAgnt.Stop ();
		_animator.SetTrigger ("Attack1");
		yield return null;
	}
	IEnumerator Attack2(){
		_naviAgnt.Stop ();
		_animator.SetTrigger ("Attack2");
		yield return null;
	}
	IEnumerator Attack3(){
		_naviAgnt.Stop ();
		_animator.SetTrigger ("Attack3");
		yield return null;
	}
	IEnumerator Idle(){
		_naviAgnt.Stop ();
		_animator.SetTrigger ("Idle");
		yield return null;

	}
	public void Patrol(){
		
		StartCoroutine (Walk ());
	}
	public void Trace(){
		StartCoroutine (Run ());
	}
	public void Attack(Transform _survivor){
		if (!isAttacking) {
			isAttacking = true;
			Debug.Log ("Attack");
			int tmpRandomAttackIndex = (int)Random.Range (1, 4);
			this.transform.LookAt (_survivor.position);
			StartCoroutine ("Attack" + tmpRandomAttackIndex);
		}
	}
	public void Stop(){
		
		StartCoroutine (Idle ());
	}
	public void StopAIRoutine(){
		
		StopAllCoroutines ();
	}
    public Animator GetAni()
    {
        return _animator;
    }
}
