using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerCollision : MonoBehaviour {

	public int playerNumber = 1;
	public Vector2 stackOffset = Vector2.zero;
	public float stackDist = .2f;
	public UnityEvent<int> onItemsChanged;
	public AudioSource soundOnHit;
	public AudioSource pickUpLoot;
	private List<GameObject> hoardedItems = new();

	void start (){
	}

	
	private void Update() {
		for (int i = 0; i < hoardedItems.Count; i++) {
			Vector3 itemPos = (Vector3) stackOffset + i * stackDist * Vector3.up;
			
			if (playerNumber == 2) {
				itemPos.x *= -1;
			}
			hoardedItems[i].transform.position = transform.position + itemPos;
		}
	}
	
	public int GetItemCount() {
		return hoardedItems.Count;
	}

	private void PickupItem(GameObject item) {
		item.GetComponentInChildren<SpriteRenderer>().sortingLayerName = "Front";
		Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
		rb.velocity = Vector2.zero;
		item.transform.parent = transform;
		item.layer = LayerMask.NameToLayer("Hoarded" + playerNumber);
		ItemLogic itemLogic = item.GetComponent<ItemLogic>();
		itemLogic.onCannonBallHit.AddListener(OnItemHit);
		
		hoardedItems.Add(item);
		pickUpLoot.Play();
		onItemsChanged.Invoke(hoardedItems.Count);
	}

	public void UnloadItems(Transform target) {
		foreach (GameObject item in hoardedItems) {
			item.GetComponent<ItemLogic>().Unload(target);
		}
		hoardedItems.Clear();
		onItemsChanged.Invoke(hoardedItems.Count);
	}
	
	public void TakeDamage() {
		soundOnHit.Play();
		foreach (GameObject item in hoardedItems) {
			item.GetComponent<ItemLogic>().Drop();
		}
		hoardedItems.Clear();
		onItemsChanged.Invoke(hoardedItems.Count);
	}
	
	private void OnItemHit(GameObject item) {
		int index = hoardedItems.IndexOf(item.gameObject);
		
		if (index == -1) {
			return;
		}
		soundOnHit.Play();
		for (int i = index; i < hoardedItems.Count; i++) {
			hoardedItems[i].GetComponent<ItemLogic>().Drop();
		}
		hoardedItems.RemoveRange(index, hoardedItems.Count - index);
		onItemsChanged.Invoke(hoardedItems.Count);
	}
	
	private void OnCollisionEnter2D(Collision2D collision) {
		if (collision.gameObject.CompareTag("CannonBall")) {
			TakeDamage();
		} else if (collision.gameObject.CompareTag("Collectable")) {
			PickupItem(collision.gameObject);
		}
	}
}