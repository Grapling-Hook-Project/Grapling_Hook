using UnityEngine;
using System.Collections;

public class StopScript : MonoBehaviour
{
	//�@�|�[�Y�������ɕ\������UI
	[SerializeField]
	private GameObject StopPanel;

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown("q"))
		{
			//�@�|�[�YUI�̃A�N�e�B�u�A��A�N�e�B�u��؂�ւ�
			StopPanel.SetActive(!StopPanel.activeSelf);

			//�@�|�[�YUI���\������Ă鎞�͒�~
			if (StopPanel.activeSelf)
			{
				Time.timeScale = 0f;
				//�@�|�[�YUI���\������ĂȂ���Βʏ�ʂ�i�s
			}
			else
			{
				Time.timeScale = 1f;
			}
		}
	}
}
