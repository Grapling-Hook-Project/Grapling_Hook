using UnityEngine;
using System.Collections;

public class StopScript : MonoBehaviour
{
	//　ポーズした時に表示するUI
	[SerializeField]
	private GameObject StopPanel;

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown("q"))
		{
			//　ポーズUIのアクティブ、非アクティブを切り替え
			StopPanel.SetActive(!StopPanel.activeSelf);

			//　ポーズUIが表示されてる時は停止
			if (StopPanel.activeSelf)
			{
				Time.timeScale = 0f;
				//　ポーズUIが表示されてなければ通常通り進行
			}
			else
			{
				Time.timeScale = 1f;
			}
		}
	}
}
